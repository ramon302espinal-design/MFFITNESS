using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.18 — agrupación de eventos relacionados.</summary>
public class DecisionEventGrouperTests
{
    private static DecisionEvent Ev(
        string type,
        DecisionEventArea area,
        DecisionEntityType entityType,
        string? entityId,
        string entityName,
        DecisionPriority priority,
        DecisionSeverity severity,
        string period = "p")
    {
        var draft = DecisionEventFactory.Create(
            type, area, entityType, entityId, entityName, period,
            "UnitTest", type, "desc");

        return new DecisionEvent
        {
            EventId = draft.EventId,
            EventType = draft.EventType,
            Area = draft.Area,
            EntityType = draft.EntityType,
            EntityId = draft.EntityId,
            EntityName = draft.EntityName,
            PeriodKey = draft.PeriodKey,
            DetectedAt = draft.DetectedAt,
            CreatedAt = draft.CreatedAt,
            Severity = severity,
            Priority = priority,
            Title = draft.Title,
            Description = draft.Description,
            Reason = draft.Reason,
            Impact = draft.Impact,
            Recommendation = draft.Recommendation,
            Status = draft.Status,
            Source = draft.Source,
            Fingerprint = draft.Fingerprint,
            Evidence = draft.Evidence,
            MetricKeys = draft.MetricKeys
        };
    }

    [Fact]
    public void Same_Product_Events_Form_One_Group()
    {
        // TEST 10
        var events = new[]
        {
            Ev("inv.stockout_risk", DecisionEventArea.Inventory,
                DecisionEntityType.Product, "42", "SKU-X",
                DecisionPriority.Critical, DecisionSeverity.Critical),
            Ev("capital.at_risk", DecisionEventArea.Capital,
                DecisionEntityType.Product, "42", "SKU-X",
                DecisionPriority.High, DecisionSeverity.High),
            Ev("product.critical_class", DecisionEventArea.Product,
                DecisionEntityType.Product, "42", "SKU-X",
                DecisionPriority.High, DecisionSeverity.High)
        };

        var groups = DecisionEventGrouper.Group(events);
        Assert.Single(groups);
        Assert.Equal(3, groups[0].EventCount);
        Assert.Equal("SKU-X", groups[0].Title);
        Assert.Equal(DecisionPriority.Critical, groups[0].Priority);
        Assert.Equal("inv.stockout_risk", groups[0].Primary!.EventType);
        Assert.Contains("3 señales", groups[0].Summary);
    }

    [Fact]
    public void Different_Products_Are_Separate_Groups()
    {
        var groups = DecisionEventGrouper.Group(
        [
            Ev("inv.overstock", DecisionEventArea.Inventory,
                DecisionEntityType.Product, "1", "A",
                DecisionPriority.Medium, DecisionSeverity.Medium),
            Ev("inv.overstock", DecisionEventArea.Inventory,
                DecisionEntityType.Product, "2", "B",
                DecisionPriority.Medium, DecisionSeverity.Medium)
        ]);

        Assert.Equal(2, groups.Count);
    }

    [Fact]
    public void Portfolio_Sales_And_Profit_Group_Together()
    {
        var groups = DecisionEventGrouper.Group(
        [
            Ev("sales.strong_decline", DecisionEventArea.Sales,
                DecisionEntityType.Portfolio, null, "",
                DecisionPriority.High, DecisionSeverity.High),
            Ev("profit.decline", DecisionEventArea.Profit,
                DecisionEntityType.Portfolio, null, "",
                DecisionPriority.Medium, DecisionSeverity.High),
            Ev("margin.deterioration", DecisionEventArea.Margin,
                DecisionEntityType.Portfolio, null, "",
                DecisionPriority.Medium, DecisionSeverity.Medium)
        ]);

        Assert.Single(groups);
        Assert.Equal("Ventas y rentabilidad", groups[0].Title);
        Assert.Equal(3, groups[0].EventCount);
    }

    [Fact]
    public void Trend_And_Sales_Are_Different_Portfolio_Themes()
    {
        var groups = DecisionEventGrouper.Group(
        [
            Ev("sales.strong_decline", DecisionEventArea.Sales,
                DecisionEntityType.Portfolio, null, "",
                DecisionPriority.High, DecisionSeverity.High),
            Ev("trend.deceleration", DecisionEventArea.Trend,
                DecisionEntityType.Portfolio, null, "",
                DecisionPriority.Medium, DecisionSeverity.Medium)
        ]);

        Assert.Equal(2, groups.Count);
    }

    [Fact]
    public void Engine_Evaluate_Fills_Groups()
    {
        var candidates = new[]
        {
            new DecisionRuleCandidate
            {
                RuleId = "t1",
                EventType = "inv.stockout_risk",
                Area = DecisionEventArea.Inventory,
                EntityType = DecisionEntityType.Product,
                EntityId = "9",
                EntityName = "Star",
                PeriodKey = "p",
                Title = "Quiebre",
                Description = "Stock bajo",
                Recommendation = "Evaluar reposición.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate
            },
            new DecisionRuleCandidate
            {
                RuleId = "t2",
                EventType = "product.star_stockout",
                Area = DecisionEventArea.Product,
                EntityType = DecisionEntityType.Product,
                EntityId = "9",
                EntityName = "Star",
                PeriodKey = "p",
                Title = "Estrella quiebre",
                Description = "Star",
                Recommendation = "Evaluar reposición.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical,
                    Sales = DecisionImpactLevel.High
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate
            }
        };

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(2, report.EmittedCount);
        Assert.Equal(1, report.GroupCount);
        Assert.Equal("Star", report.PrimaryGroup!.Title);
    }

    [Fact]
    public void GroupId_Is_Stable_For_Same_Key()
    {
        var e = Ev("capital.frozen", DecisionEventArea.Capital,
            DecisionEntityType.Product, "7", "X",
            DecisionPriority.Medium, DecisionSeverity.High);

        string key = DecisionEventGrouper.ResolveGroupKey(e);
        var g1 = DecisionEventGrouper.Group([e])[0];
        var g2 = DecisionEventGrouper.Group([e])[0];
        Assert.Equal(key, g1.GroupKey);
        Assert.Equal(g1.GroupId, g2.GroupId);
    }

    [Fact]
    public void Policy_Mentions_Centro()
    {
        Assert.Contains("TEST 10", DecisionGroupPolicy.Definition);
        Assert.Contains("completa", DecisionGroupPolicy.Deferred);
    }
}
