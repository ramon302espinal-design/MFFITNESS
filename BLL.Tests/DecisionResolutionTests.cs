using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.22 — resolución / ignorado.</summary>
public class DecisionResolutionTests
{
    private static (DecisionHistoryService hist, DecisionResolutionService res, Guid eventId) Seed()
    {
        var store = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(store);
        var res = new DecisionResolutionService(store);

        var engine = new DecisionEngine().Evaluate(
        [
            new DecisionRuleCandidate
            {
                RuleId = "r",
                EventType = "capital.at_risk",
                Area = DecisionEventArea.Capital,
                EntityType = DecisionEntityType.Product,
                EntityId = "42",
                EntityName = "SKU",
                PeriodKey = "p",
                Title = "Riesgo",
                Description = "x",
                Recommendation = "Revisar estrategia de salida.",
                Materiality = new DecisionMaterialityInput { CapitalAmount = 8_000m },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.High
            }
        ]);

        hist.Capture(engine);
        return (hist, res, engine.Events[0].EventId);
    }

    [Fact]
    public void Resolve_From_Active_Sets_Resolved_And_Actor()
    {
        var (_, res, id) = Seed();
        DecisionResolutionResult r = res.Resolve(id, "ana", "revisado en piso");
        Assert.True(r.Success);
        Assert.Equal(DecisionEventStatus.Active, r.PreviousStatus);
        Assert.Equal(DecisionEventStatus.Resolved, r.NewStatus);
        Assert.Equal("ana", r.Record!.ResolvedBy);
        Assert.Equal("revisado en piso", r.Record.ResolutionNote);
        Assert.NotNull(r.Record.ResolvedAt);
    }

    [Fact]
    public void Ignore_From_Active()
    {
        var (_, res, id) = Seed();
        DecisionResolutionResult r = res.Ignore(id, "ana", "ruido estacional");
        Assert.True(r.Success);
        Assert.Equal(DecisionEventStatus.Ignored, r.NewStatus);
    }

    [Fact]
    public void StartReview_Then_Resolve()
    {
        var (_, res, id) = Seed();
        Assert.True(res.StartReview(id, "ana").Success);
        DecisionResolutionResult mid = res.Apply(new DecisionResolutionRequest
        {
            EventId = id,
            Action = DecisionResolutionAction.Resolve,
            Actor = "ana",
            Note = "cerrado"
        });
        Assert.True(mid.Success);
        Assert.Equal(DecisionEventStatus.InReview, mid.PreviousStatus);
        Assert.Equal(DecisionEventStatus.Resolved, mid.NewStatus);
    }

    [Fact]
    public void Reopen_From_InReview_Back_To_Active()
    {
        var (_, res, id) = Seed();
        res.StartReview(id, "ana");
        DecisionResolutionResult r = res.Reopen(id);
        Assert.True(r.Success);
        Assert.Equal(DecisionEventStatus.Active, r.NewStatus);
        Assert.Null(r.Record!.ResolvedAt);
    }

    [Fact]
    public void Cannot_Resolve_Twice()
    {
        var (_, res, id) = Seed();
        Assert.True(res.Resolve(id, "a").Success);
        DecisionResolutionResult again = res.Resolve(id, "b");
        Assert.False(again.Success);
        Assert.Contains("cerrado", again.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void After_Resolve_Capture_Allows_New_Detection()
    {
        var store = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(store);
        var res = new DecisionResolutionService(hist);

        DecisionRuleCandidate Cand(string period) => new()
        {
            RuleId = "r",
            EventType = "inv.stockout_risk",
            Area = DecisionEventArea.Inventory,
            EntityType = DecisionEntityType.Product,
            EntityId = "9",
            EntityName = "Star",
            PeriodKey = period,
            Title = "Quiebre",
            Description = "x",
            Recommendation = "Evaluar reposición — no comprar automáticamente.",
            Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Inventory = DecisionImpactLevel.Critical
            },
            TimeSensitiveStockout = true,
            Urgency = DecisionUrgencyLevel.Immediate,
            RequiresImmediateReview = true
        };

        // Same period → same fingerprint
        var e1 = new DecisionEngine().Evaluate([Cand("p")]);
        hist.Capture(e1);
        res.Resolve(e1.Events[0].EventId, "ana");

        var e2 = new DecisionEngine().Evaluate([Cand("p")]);
        DecisionHistoryCaptureResult cap = hist.Capture(e2);
        Assert.Equal(1, cap.Inserted);
    }

    [Fact]
    public void InReview_Blocks_Duplicate_Capture()
    {
        var store = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(store);
        var res = new DecisionResolutionService(store);

        var engine = new DecisionEngine().Evaluate(
        [
            new DecisionRuleCandidate
            {
                RuleId = "r",
                EventType = "sales.strong_decline",
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = "p",
                Title = "Caída",
                Description = "x",
                Recommendation = "Revisar demanda.",
                Materiality = new DecisionMaterialityInput { VariationPct = -25m },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.High
            }
        ]);
        hist.Capture(engine);
        res.StartReview(engine.Events[0].EventId);

        var again = new DecisionEngine().Evaluate(
        [
            new DecisionRuleCandidate
            {
                RuleId = "r",
                EventType = "sales.strong_decline",
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = "p",
                Title = "Caída",
                Description = "x",
                Recommendation = "Revisar demanda.",
                Materiality = new DecisionMaterialityInput { VariationPct = -25m },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.High
            }
        ]);
        Assert.Equal(0, hist.Capture(again).Inserted);
        Assert.Equal(1, hist.Capture(again).SkippedActiveDuplicate);
    }

    [Fact]
    public void Resolve_By_Fingerprint()
    {
        var (hist, res, id) = Seed();
        string fp = hist.GetHistory()[0].Fingerprint;
        DecisionResolutionResult r = res.Apply(new DecisionResolutionRequest
        {
            Fingerprint = fp,
            Action = DecisionResolutionAction.Resolve,
            Actor = "bot"
        });
        Assert.True(r.Success);
        Assert.Equal(id, r.Record!.EventId);
    }

    [Fact]
    public void Policy_Mentions_Transitions()
    {
        Assert.Contains("10.22", DecisionResolutionPolicy.Definition);
        Assert.Contains("InReview", DecisionResolutionPolicy.Transitions);
        Assert.Contains("completa", DecisionResolutionPolicy.Deferred);
    }
}
