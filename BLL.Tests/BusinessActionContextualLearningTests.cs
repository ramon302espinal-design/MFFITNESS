using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.14 — Learning producto / problema / recurrencia.</summary>
public class BusinessActionContextualLearningTests
{
    private static BusinessActionRecord Action(
        BusinessActionType type,
        BusinessActionOutcome? outcome,
        DecisionEntityType entityType = DecisionEntityType.Portfolio,
        string? entityId = null,
        string entityName = "",
        Guid? decisionEventId = null,
        DecisionEventArea area = DecisionEventArea.Capital,
        BusinessActionStatus status = BusinessActionStatus.Completed)
    {
        BusinessActionActualImpact? impact = outcome == null
            ? null
            : new BusinessActionActualImpact
            {
                Outcome = outcome.Value,
                Confidence = BusinessActionConfidence.Medium,
                Summary = "obs"
            };

        return new BusinessActionRecord
        {
            ActionId = Guid.NewGuid(),
            ActionType = type,
            Status = status,
            Area = area,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            DecisionEventId = decisionEventId,
            Description = "t",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = status == BusinessActionStatus.Completed ? DateTime.UtcNow : null,
            ActualImpact = impact
        };
    }

    private static DecisionHistoryRecord Decision(
        Guid eventId,
        string eventType,
        string? entityId = null,
        DateTime? at = null)
        => new()
        {
            EventId = eventId,
            EventType = eventType,
            Area = DecisionEventArea.Capital,
            EntityType = DecisionEntityType.Product,
            EntityId = entityId,
            Title = eventType,
            DetectedAt = at ?? DateTime.UtcNow,
            CreatedAt = at ?? DateTime.UtcNow,
            Status = DecisionEventStatus.Active,
            Severity = DecisionSeverity.Medium,
            Priority = DecisionPriority.Medium
        };

    [Fact]
    public void ByEntity_Groups_Product_With_Rates()
    {
        var records = new[]
        {
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Successful,
                DecisionEntityType.Product, "10", "Cool Heaven"),
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Ineffective,
                DecisionEntityType.Product, "10", "Cool Heaven"),
            Action(BusinessActionType.StockReduction, BusinessActionOutcome.Successful,
                DecisionEntityType.Product, "20", "Red Bull")
        };

        var byEntity = BusinessActionContextualLearningComposer.ComposeByEntity(records);
        Assert.Equal(2, byEntity.Count);

        BusinessActionEntityLearningStats cool = byEntity.Single(e => e.EntityId == "10");
        Assert.Equal("Cool Heaven", cool.EntityName);
        Assert.Equal(2, cool.TotalCount);
        Assert.Equal(50m, cool.SuccessRatePct);
        Assert.Contains("Histórico", cool.Summary);
    }

    [Fact]
    public void ByProblem_Ranks_Best_Historical_ActionType()
    {
        // Brief §49: capital congelado — Promo 2/3 éxito; Liquidación 2/2 éxito.
        Guid e1 = Guid.NewGuid(), e2 = Guid.NewGuid(), e3 = Guid.NewGuid();
        Guid e4 = Guid.NewGuid(), e5 = Guid.NewGuid();

        var decisions = new Dictionary<Guid, DecisionHistoryRecord>
        {
            [e1] = Decision(e1, "capital.immobilized"),
            [e2] = Decision(e2, "capital.immobilized"),
            [e3] = Decision(e3, "capital.immobilized"),
            [e4] = Decision(e4, "capital.immobilized"),
            [e5] = Decision(e5, "capital.immobilized")
        };

        var records = new[]
        {
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Successful, decisionEventId: e1),
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Successful, decisionEventId: e2),
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Ineffective, decisionEventId: e3),
            Action(BusinessActionType.StockReduction, BusinessActionOutcome.Successful, decisionEventId: e4),
            Action(BusinessActionType.StockReduction, BusinessActionOutcome.Successful, decisionEventId: e5)
        };

        var byProblem = BusinessActionContextualLearningComposer.ComposeByProblem(records, decisions);
        BusinessActionProblemLearningStats problem = Assert.Single(byProblem);
        Assert.Equal("capital.immobilized", problem.ProblemKey);
        Assert.Equal(BusinessActionType.StockReduction, problem.BestHistoricalActionType);
        Assert.NotNull(problem.BestHistoricalHint);
        Assert.Contains("mejores resultados históricos", problem.BestHistoricalHint!);
        Assert.Contains("no es una garantía", problem.BestHistoricalHint, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("funcionará", problem.BestHistoricalHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DetectSignals_RecurrentProblem_TEST10()
    {
        var decisions = Enumerable.Range(0, 3)
            .Select(_ => Decision(Guid.NewGuid(), "capital.immobilized", "10"))
            .ToList();

        var signals = BusinessActionContextualLearningComposer.DetectSignals(
            Array.Empty<BusinessActionRecord>(), decisions, minOccurrences: 3);

        BusinessActionLearningSignal signal = Assert.Single(signals);
        Assert.Equal(BusinessActionLearningSignalKind.RecurrentProblem, signal.Kind);
        Assert.Contains("PROBLEMA RECURRENTE", signal.Message);
        Assert.Equal(3, signal.OccurrenceCount);
    }

    [Fact]
    public void DetectSignals_HistoricallyEffective_TEST11_SoftLanguage()
    {
        var records = Enumerable.Range(0, 3)
            .Select(_ => Action(BusinessActionType.Campaign, BusinessActionOutcome.Successful))
            .ToList();

        var signals = BusinessActionContextualLearningComposer.DetectSignals(
            records, Array.Empty<DecisionHistoryRecord>(), minOccurrences: 3);

        BusinessActionLearningSignal signal = Assert.Single(
            signals, s => s.Kind == BusinessActionLearningSignalKind.HistoricallyEffectiveAction);

        Assert.Contains("históric", signal.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("garantía", signal.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("funcionará", signal.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(BusinessActionSoftLanguageGuard.ContainsForbidden(signal.Message));
    }

    [Fact]
    public void DetectSignals_IneffectivePattern()
    {
        var records = new[]
        {
            Action(BusinessActionType.PriceChange, BusinessActionOutcome.Ineffective),
            Action(BusinessActionType.PriceChange, BusinessActionOutcome.Ineffective),
            Action(BusinessActionType.PriceChange, BusinessActionOutcome.Ineffective)
        };

        var signals = BusinessActionContextualLearningComposer.DetectSignals(
            records, Array.Empty<DecisionHistoryRecord>(), minOccurrences: 3);

        Assert.Contains(signals, s =>
            s.Kind == BusinessActionLearningSignalKind.IneffectiveActionPattern
            && s.Message.Contains("POCO EFECTIVA"));
    }

    [Fact]
    public void Cancelled_Excluded_From_Entity_Rates()
    {
        var records = new[]
        {
            Action(BusinessActionType.Promotion, BusinessActionOutcome.Successful,
                DecisionEntityType.Product, "1", "P"),
            Action(BusinessActionType.Promotion, null,
                DecisionEntityType.Product, "1", "P",
                status: BusinessActionStatus.Cancelled)
        };

        BusinessActionEntityLearningStats stats =
            Assert.Single(BusinessActionContextualLearningComposer.ComposeByEntity(records));

        Assert.Equal(2, stats.TotalCount);
        Assert.Equal(1, stats.ClassifiedCount);
        Assert.Equal(100m, stats.SuccessRatePct);
    }

    [Fact]
    public void Service_GetContextual_Wires_Stores()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var decisionStore = new InMemoryDecisionHistoryStore();
        var learning = new BusinessActionLearningService(actionStore, decisionStore);

        Guid eventId = Guid.NewGuid();
        decisionStore.Append(Decision(eventId, "capital.immobilized", "99"));

        var record = Action(
            BusinessActionType.Promotion,
            BusinessActionOutcome.Successful,
            DecisionEntityType.Product,
            "99",
            "Prod",
            eventId);
        actionStore.Append(record);

        BusinessActionContextualLearning ctx = learning.GetContextual(minOccurrences: 2);
        Assert.NotEmpty(ctx.ByEntity);
        Assert.NotEmpty(ctx.ByProblem);
        Assert.Equal(BusinessActionContextualLearningPolicy.Caution, ctx.Caution);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.14", BusinessActionContextualLearningPolicy.Definition);
        Assert.Contains("completa", BusinessActionContextualLearningPolicy.Deferred);
        Assert.Contains("histórica", BusinessActionContextualLearningPolicy.Caution, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionContextualLearningComposer"));
    }
}
