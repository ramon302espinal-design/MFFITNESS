using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.20 — Centro de decisiones (resumen + prioridades).</summary>
public class DecisionCenterTests
{
    private static DecisionRuleCandidate Cand(
        string type,
        DecisionEventArea area,
        DecisionEntityType entityType,
        string? entityId,
        string entityName,
        DecisionImpactLevel impact,
        DecisionUrgencyLevel urgency = DecisionUrgencyLevel.None,
        bool stockout = false,
        bool opportunity = false,
        string recommendation = "Revisar la señal.")
        => new()
        {
            RuleId = "test",
            EventType = type,
            Area = area,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            PeriodKey = "p",
            Title = type,
            Description = type,
            Recommendation = recommendation,
            Materiality = new DecisionMaterialityInput
            {
                TimeSensitiveStockout = stockout,
                OpportunitySignal = opportunity,
                VariationPct = opportunity ? 20m : (stockout ? null : -20m),
                CapitalAmount = stockout || opportunity ? null : 5_000m
            },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Financial = impact,
                Inventory = stockout ? DecisionImpactLevel.Critical : DecisionImpactLevel.None,
                Capital = area == DecisionEventArea.Capital ? impact : DecisionImpactLevel.None,
                Sales = area == DecisionEventArea.Sales ? impact : DecisionImpactLevel.None
            },
            Urgency = urgency,
            TimeSensitiveStockout = stockout,
            OpportunityWindow = opportunity,
            RequiresImmediateReview = stockout || urgency == DecisionUrgencyLevel.Immediate
        };

    [Fact]
    public void Compose_Builds_Executive_Summary_Buckets()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory,
                DecisionEntityType.Product, "1", "SKU-A",
                DecisionImpactLevel.Critical, DecisionUrgencyLevel.Immediate, stockout: true),
            Cand("capital.at_risk", DecisionEventArea.Capital,
                DecisionEntityType.Product, "2", "SKU-B",
                DecisionImpactLevel.High, DecisionUrgencyLevel.High),
            Cand("product.growth_opportunity", DecisionEventArea.Product,
                DecisionEntityType.Product, "3", "SKU-C",
                DecisionImpactLevel.Medium, opportunity: true,
                recommendation: "Evaluar oportunidad de crecimiento / cobertura."),
            Cand("trend.deceleration", DecisionEventArea.Trend,
                DecisionEntityType.Portfolio, null, "",
                DecisionImpactLevel.Low, DecisionUrgencyLevel.Low)
        ]);

        var center = DecisionCenterComposer.Compose(engine, periodKey: "p");

        Assert.True(center.Summary.CriticalCount >= 1);
        Assert.True(center.Summary.OpportunityCount >= 1);
        Assert.Contains("HOY", center.Summary.Headline);
        Assert.Contains("críticos", center.Summary.Headline);
        Assert.Equal(engine.GroupCount, center.Summary.TotalGroups);
        Assert.Equal(engine.EmittedCount, center.Summary.TotalEvents);
    }

    [Fact]
    public void PrioritiesToday_Capped_At_Max()
    {
        var candidates = new List<DecisionRuleCandidate>();
        for (int i = 0; i < 8; i++)
        {
            candidates.Add(Cand(
                "capital.slow", DecisionEventArea.Capital,
                DecisionEntityType.Product, i.ToString(), $"P{i}",
                DecisionImpactLevel.Medium, DecisionUrgencyLevel.Medium));
        }

        var engine = new DecisionEngine().Evaluate(candidates);
        var center = DecisionCenterComposer.Compose(engine, maxPriorities: 5);

        Assert.True(engine.GroupCount >= 5);
        Assert.Equal(5, center.PrioritiesToday.Count);
        Assert.Equal(1, center.PrioritiesToday[0].Rank);
        Assert.Equal(5, center.PrioritiesToday[4].Rank);
    }

    [Fact]
    public void TopPriority_Is_Critical_Before_Review()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("trend.volatile", DecisionEventArea.Trend,
                DecisionEntityType.Portfolio, null, "",
                DecisionImpactLevel.Low),
            Cand("product.star_stockout", DecisionEventArea.Product,
                DecisionEntityType.Product, "9", "Star",
                DecisionImpactLevel.Critical, DecisionUrgencyLevel.Immediate, stockout: true,
                recommendation: "Evaluar reposición — no comprar automáticamente.")
        ]);

        var center = new DecisionCenterService().FromEngine(engine);
        Assert.NotNull(center.TopPriority);
        Assert.Equal(DecisionCenterBucket.Critical, center.TopPriority!.Bucket);
        Assert.Contains("Star", center.TopPriority.Title);
        Assert.True(DecisionSoftLanguageGuard.IsCompliant(center.TopPriority.Recommendation)
            || center.TopPriority.Recommendation.StartsWith("Evaluar", StringComparison.OrdinalIgnoreCase)
            || center.TopPriority.Recommendation.StartsWith("Revisar", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Opportunity_Bucket_For_Growth()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("product.growth_opportunity", DecisionEventArea.Product,
                DecisionEntityType.Product, "7", "Rising",
                DecisionImpactLevel.Medium, opportunity: true,
                recommendation: "Evaluar oportunidad de crecimiento / cobertura.")
        ]);

        var group = engine.Groups[0];
        var rec = engine.Recommendations.First(r => r.GroupId == group.GroupId);
        Assert.Equal(DecisionCenterBucket.Opportunity,
            DecisionCenterComposer.Classify(group, rec));
    }

    [Fact]
    public void Snapshot_Lines_Are_Exposure_Not_Guarantee()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("sales.strong_decline", DecisionEventArea.Sales,
                DecisionEntityType.Portfolio, null, "",
                DecisionImpactLevel.High, DecisionUrgencyLevel.High)
        ]);

        var center = DecisionCenterComposer.Compose(
            engine,
            new DecisionCenterSnapshot
            {
                SalesVariationPct = -18m,
                ProfitVariationPct = 11m,
                FrozenCapitalAmount = 31500m
            });

        Assert.Contains(center.Summary.SnapshotLines, l => l.Contains("Ventas"));
        Assert.Contains(center.Summary.SnapshotLines, l => l.Contains("Ganancia"));
        Assert.Contains(center.Summary.SnapshotLines,
            l => l.Contains("31500") || l.Contains("31,500"));
        Assert.Contains(center.Summary.SnapshotLines,
            l => l.Contains("expuesto", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(center.Summary.SnapshotLines,
            l => l.Contains("vas a perder", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Priority_Item_Carries_SubSignals()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            Cand("inv.stockout_risk", DecisionEventArea.Inventory,
                DecisionEntityType.Product, "42", "X",
                DecisionImpactLevel.Critical, DecisionUrgencyLevel.Immediate, stockout: true),
            Cand("capital.at_risk", DecisionEventArea.Capital,
                DecisionEntityType.Product, "42", "X",
                DecisionImpactLevel.High, DecisionUrgencyLevel.High)
        ]);

        var center = DecisionCenterComposer.Compose(engine);
        Assert.Single(center.Groups);
        Assert.True(center.TopPriority!.SubSignals.Count >= 2);
        Assert.Contains("inv.stockout_risk", center.TopPriority.SubSignals);
    }

    [Fact]
    public void SourceMap_Includes_DecisionCenterService()
    {
        var s = DecisionSourceMap.Find("DecisionCenterService");
        Assert.NotNull(s);
        Assert.Contains("10.20", s!.Phase);
        Assert.Contains("Auto-acciones", s.MustNot);
    }

    [Fact]
    public void Policy_Mentions_AntiFatigue()
    {
        Assert.Contains("10.20", DecisionCenterPolicy.Definition);
        Assert.Contains("5", DecisionCenterPolicy.AntiFatigue);
        Assert.Contains("completa", DecisionCenterPolicy.Deferred);
        Assert.Equal(5, DecisionCenterPolicy.DefaultMaxPriorities);
    }
}
