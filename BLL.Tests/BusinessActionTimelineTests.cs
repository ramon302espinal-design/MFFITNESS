using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.11 — Timeline decisión→acción→resultado.</summary>
public class BusinessActionTimelineTests
{
    [Fact]
    public void Composer_Orders_Decision_Then_Action_Then_Result()
    {
        var detected = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);
        var registered = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);
        var completed = new DateTime(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);

        var decision = new DecisionHistoryRecord
        {
            Id = 41,
            EventId = Guid.NewGuid(),
            EventType = "capital.immobilized",
            Title = "Capital inmovilizado alto",
            DetectedAt = detected,
            CreatedAt = detected,
            Status = DecisionEventStatus.Active
        };

        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest
            {
                CapturedAt = registered.AddHours(1)
            },
            new Dictionary<string, decimal?> { ["capital.immobilized"] = 9_000m });

        var action = BusinessActionRecordFactory.Create(
            BusinessActionType.StockReduction,
            "Liquidar exceso",
            decisionEventId: decision.EventId,
            decisionHistoryId: decision.Id,
            createdBy: "ana",
            createdAt: registered,
            baseline: baseline,
            status: BusinessActionStatus.Pending);

        // Completar + deltas + outcome
        action = new BusinessActionRecord
        {
            ActionId = action.ActionId,
            DecisionEventId = action.DecisionEventId,
            DecisionHistoryId = action.DecisionHistoryId,
            ActionType = action.ActionType,
            Area = action.Area,
            EntityType = action.EntityType,
            Description = action.Description,
            CreatedAt = action.CreatedAt,
            CreatedBy = action.CreatedBy,
            Status = BusinessActionStatus.Completed,
            StartedAt = registered,
            CompletedAt = completed,
            CompletedBy = "ana",
            EvaluationDays = 7,
            EvaluationDueAt = completed.AddDays(7),
            Baseline = baseline,
            ActualImpact = new BusinessActionActualImpact
            {
                Outcome = BusinessActionOutcome.Successful,
                Confidence = BusinessActionConfidence.Medium,
                Summary = "Se observó mejora.",
                Deltas =
                [
                    BusinessActionMetricDeltaMath.Compute("capital.immobilized", null, 9_000m, 6_000m)!
                ]
            }
        };

        BusinessActionTimeline timeline = BusinessActionTimelineComposer.Build(action, decision);

        Assert.Equal(action.ActionId, timeline.ActionId);
        Assert.Equal(decision.EventId, timeline.DecisionEventId);
        Assert.True(timeline.Steps.Count >= 5);
        Assert.Equal(BusinessActionTimelineStepKind.DecisionDetected, timeline.Steps[0].Kind);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.ActionRegistered);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.BaselineCaptured);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.ActionCompleted);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.OutcomeEvaluated);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.CapitalImpactNoted);
        Assert.Equal(BusinessActionOutcome.Successful, timeline.Outcome);

        // Orden cronológico
        for (int i = 1; i < timeline.Steps.Count; i++)
            Assert.True(timeline.Steps[i].AtUtc >= timeline.Steps[i - 1].AtUtc);

        Assert.DoesNotContain("causó", string.Join(' ', timeline.Steps.Select(s => s.Detail)),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Composer_Cancelled_Adds_Cancel_Step_Not_Outcome()
    {
        var action = BusinessActionRecordFactory.Create(
            BusinessActionType.Other,
            "Cancelar",
            status: BusinessActionStatus.Cancelled,
            createdAt: new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc));

        action = new BusinessActionRecord
        {
            ActionId = action.ActionId,
            ActionType = action.ActionType,
            Area = action.Area,
            EntityType = action.EntityType,
            Description = action.Description,
            CreatedAt = action.CreatedAt,
            Status = BusinessActionStatus.Cancelled,
            CompletedAt = action.CreatedAt.AddHours(2),
            CompletedBy = "bob"
        };

        var timeline = BusinessActionTimelineComposer.Build(action);
        Assert.Contains(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.ActionCancelled);
        Assert.DoesNotContain(timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.OutcomeEvaluated);
        Assert.Contains("TEST 8", timeline.Steps.First(s => s.Kind == BusinessActionTimelineStepKind.ActionCancelled).Detail);
    }

    [Fact]
    public void Service_GetByDecisionEventId_Returns_Linked_Actions()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var decisionStore = new InMemoryDecisionHistoryStore();
        var actions = new BusinessActionService(actionStore);
        var timelineSvc = new BusinessActionTimelineService(actionStore, decisionStore);

        Guid eventId = Guid.NewGuid();
        decisionStore.Append(new DecisionHistoryRecord
        {
            EventId = eventId,
            EventType = "sales.decline",
            Title = "Caída ventas",
            DetectedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = DecisionEventStatus.Active
        });

        actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Campaña A",
            DecisionEventId = eventId,
            CreatedBy = "ana"
        });
        actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promo B",
            DecisionEventId = eventId
        });

        IReadOnlyList<BusinessActionTimeline> list = timelineSvc.GetByDecisionEventId(eventId);
        Assert.Equal(2, list.Count);
        Assert.All(list, t => Assert.Equal(eventId, t.DecisionEventId));
        Assert.All(list, t => Assert.Contains(
            t.Steps, s => s.Kind == BusinessActionTimelineStepKind.DecisionDetected));
    }

    [Fact]
    public void Service_GetByActionId_Null_When_Missing()
    {
        var svc = new BusinessActionTimelineService(
            new InMemoryBusinessActionStore(),
            new InMemoryDecisionHistoryStore());
        Assert.Null(svc.GetByActionId(Guid.NewGuid()));
    }

    [Fact]
    public void ListRecentBatch_Prefetches_Decisions_Without_N_Plus_1()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var decisionStore = new InMemoryDecisionHistoryStore();
        var actions = new BusinessActionService(actionStore);
        var timelineSvc = new BusinessActionTimelineService(actionStore, decisionStore);

        for (int i = 0; i < 12; i++)
        {
            Guid eventId = Guid.NewGuid();
            long historyId = decisionStore.Append(new DecisionHistoryRecord
            {
                EventId = eventId,
                EventType = "sales.decline",
                Title = $"Señal {i}",
                Fingerprint = $"fp-{i}",
                DetectedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(i),
                CreatedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(i),
                Status = DecisionEventStatus.Active
            });

            actions.Register(new BusinessActionRegisterRequest
            {
                ActionType = BusinessActionType.Campaign,
                Description = $"Acción {i}",
                DecisionEventId = eventId,
                DecisionHistoryId = historyId,
                CreatedBy = "ana"
            });
        }

        BusinessActionTimelineBatch batch = timelineSvc.ListRecentBatch(top: 20);

        Assert.Equal(12, batch.Items.Count);
        Assert.Equal(1, batch.Stats.ActionStoreCalls);
        Assert.True(batch.Stats.DecisionStoreCalls <= 2,
            $"Expected ≤2 decision calls, got {batch.Stats.DecisionStoreCalls}");
        Assert.True(batch.Stats.DecisionsPrefetched >= 12);
        Assert.Contains("11.21", batch.Stats.PolicyNote);
        Assert.All(batch.Items, t =>
            Assert.Contains(t.Steps, s => s.Kind == BusinessActionTimelineStepKind.DecisionDetected));
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.11", BusinessActionTimelinePolicy.Definition);
        Assert.Contains("11.21", BusinessActionTimelinePolicy.Definition);
        Assert.Contains("completa", BusinessActionTimelinePolicy.Deferred);
        Assert.DoesNotContain("11.21", BusinessActionTimelinePolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionTimelineComposer"));
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionTimelineService"));
    }
}
