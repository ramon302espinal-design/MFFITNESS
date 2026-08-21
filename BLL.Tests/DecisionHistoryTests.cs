using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.21 — historial de DecisionEvents.</summary>
public class DecisionHistoryTests
{
    private static DecisionRuleCandidate Cand(
        string type,
        DecisionEventArea area,
        string? entityId,
        string entityName,
        bool stockout = false)
        => new()
        {
            RuleId = "hist.test",
            EventType = type,
            Area = area,
            EntityType = string.IsNullOrEmpty(entityId)
                ? DecisionEntityType.Portfolio
                : DecisionEntityType.Product,
            EntityId = entityId,
            EntityName = entityName,
            PeriodKey = "p",
            Title = type,
            Description = "hist",
            Recommendation = "Revisar la señal.",
            Materiality = new DecisionMaterialityInput
            {
                TimeSensitiveStockout = stockout,
                VariationPct = stockout ? null : -20m,
                CapitalAmount = stockout ? null : 5_000m
            },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Financial = DecisionImpactLevel.High,
                Inventory = stockout ? DecisionImpactLevel.Critical : DecisionImpactLevel.None
            },
            Urgency = stockout ? DecisionUrgencyLevel.Immediate : DecisionUrgencyLevel.High,
            TimeSensitiveStockout = stockout,
            RequiresImmediateReview = stockout
        };

    [Fact]
    public void Capture_Inserts_And_Skips_Active_Duplicate()
    {
        // TEST 8
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("capital.at_risk", DecisionEventArea.Capital, "42", "SKU-X")
        ]);

        var first = history.Capture(engine);
        Assert.Equal(1, first.Inserted);
        Assert.Equal(0, first.SkippedActiveDuplicate);

        var second = history.Capture(engine);
        Assert.Equal(0, second.Inserted);
        Assert.Equal(1, second.SkippedActiveDuplicate);

        var rows = history.GetHistory(new DecisionHistoryQuery { Top = 10 });
        Assert.Single(rows);
        Assert.Equal("capital.at_risk", rows[0].EventType);
        Assert.False(string.IsNullOrWhiteSpace(rows[0].Fingerprint));
        Assert.Equal(DecisionEventStatus.Active, rows[0].Status);
    }

    [Fact]
    public void After_Resolved_Same_Fingerprint_Can_Reappear()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory, "9", "Star", stockout: true)
        ]);

        history.Capture(engine);
        Guid eventId = engine.Events[0].EventId;
        var resolution = new DecisionResolutionService(store);
        Assert.True(resolution.Resolve(eventId, "tester", "ok").Success);

        // New emission (new EventId, same fingerprint)
        var engine2 = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory, "9", "Star", stockout: true)
        ]);
        var cap = history.Capture(engine2);
        Assert.Equal(1, cap.Inserted);

        var all = history.GetHistory(new DecisionHistoryQuery { Fingerprint = engine.Events[0].Fingerprint });
        Assert.Equal(2, all.Count);
        Assert.Contains(all, r => r.Status == DecisionEventStatus.Resolved);
        Assert.Contains(all, r => r.Status == DecisionEventStatus.Active);
    }

    [Fact]
    public void Query_Filters_By_EventType_And_Entity()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("capital.frozen", DecisionEventArea.Capital, "1", "A"),
            Cand("sales.strong_decline", DecisionEventArea.Sales, null, "")
        ]);
        history.Capture(engine);

        var capital = history.GetHistory(new DecisionHistoryQuery
        {
            EventType = "capital.frozen",
            EntityId = "1"
        });
        Assert.Single(capital);
        Assert.Equal("A", capital[0].EntityName);
    }

    [Fact]
    public void Recurrence_Detects_Repeated_EventType()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);

        for (int i = 0; i < 3; i++)
        {
            // Distinct period keys → distinct fingerprints → all insert
            var engine = new DecisionEngine().Evaluate(
            [
                new DecisionRuleCandidate
                {
                    RuleId = "r",
                    EventType = "capital.frozen",
                    Area = DecisionEventArea.Capital,
                    EntityType = DecisionEntityType.Product,
                    EntityId = "7",
                    EntityName = "FrozenSKU",
                    PeriodKey = "p" + i,
                    Title = "Frozen",
                    Description = "x",
                    Recommendation = "Revisar rotación.",
                    Materiality = new DecisionMaterialityInput { CapitalAmount = 12_000m },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Capital = DecisionImpactLevel.High
                    },
                    Urgency = DecisionUrgencyLevel.Medium
                }
            ]);
            history.Capture(engine);
        }

        var recurrent = history.GetRecurrentProblems(lookbackDays: 90, minOccurrences: 3);
        Assert.Contains(recurrent, r =>
            r.EventType == "capital.frozen"
            && r.EntityId == "7"
            && r.OccurrenceCount >= 3
            && r.Message.Contains("RECURRENTE"));
    }

    [Fact]
    public void Metrics_Count_Generated_And_Critical()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory, "1", "X", stockout: true),
            Cand("capital.at_risk", DecisionEventArea.Capital, "2", "Y")
        ]);
        history.Capture(engine);

        DecisionHistoryMetrics m = history.GetMetrics();
        Assert.Equal(2, m.GeneratedCount);
        Assert.True(m.CriticalCount >= 1);
        Assert.Equal(2, m.ActiveCount);
        Assert.Equal(0, m.ResolvedCount);
    }

    [Fact]
    public void Capture_Attaches_GroupKey()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory, "42", "X", stockout: true),
            Cand("capital.at_risk", DecisionEventArea.Capital, "42", "X")
        ]);

        history.Capture(engine);
        var rows = history.GetHistory();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.False(string.IsNullOrWhiteSpace(r.GroupKey)));
        Assert.Equal(rows[0].GroupKey, rows[1].GroupKey);
    }

    [Fact]
    public void SourceMap_Includes_History_Service()
    {
        var s = DecisionSourceMap.Find("DecisionHistoryService");
        Assert.NotNull(s);
        Assert.Contains("10.21", s!.Phase);
    }

    [Fact]
    public void Policy_Defers_Audit()
    {
        Assert.Contains("10.21", DecisionHistoryPolicy.Definition);
        Assert.Contains("TEST 8", DecisionHistoryPolicy.Dedup);
        Assert.Contains("TEST 9", DecisionHistoryPolicy.Reconcile);
        Assert.Contains("completa", DecisionHistoryPolicy.Deferred);
    }
}
