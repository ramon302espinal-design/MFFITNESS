using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.14 — alertas de productos.</summary>
public class ProductAlertDecisionRuleTests
{
    private static SalesStarContributionRow Star(
        int id, string name, decimal revenue, bool stockout)
        => new()
        {
            ProductId = id,
            ProductName = name,
            RevenueTotal = revenue,
            FlagStockoutRisk = stockout
        };

    private static ProductClassificationRow ClassRow(
        int id,
        string name,
        ProductPerformanceClass cls,
        decimal capital = 5_000m,
        ProductTrendDirection? trend = null)
        => new()
        {
            ProductId = id,
            ProductName = name,
            Class = cls,
            Trend = trend,
            Reasons = ["test"],
            Performance = new ProductPerformanceRow
            {
                ProductId = id,
                ProductName = name,
                InventoryCapital = capital,
                RevenueTotal = 10_000m
            }
        };

    [Fact]
    public void Star_Stockout_Emits_Immediate()
    {
        // TEST 4
        var mix = new SalesStarMixReport
        {
            StarsWithStockoutRisk = [Star(1, "Estrella", 50_000m, true)]
        };

        var candidates = ProductAlertRuleComposer.FromStarMix(mix, "p");
        Assert.Single(candidates);
        Assert.Equal("product.star_stockout", candidates[0].EventType);
        Assert.True(candidates[0].TimeSensitiveStockout);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Priority >= DecisionPriority.High);
    }

    [Fact]
    public void Critical_Class_Emits()
    {
        var report = new ProductClassificationReport
        {
            Rows =
            [
                ClassRow(2, "Crit", ProductPerformanceClass.Critical, 12_000m,
                    ProductTrendDirection.Declining)
            ]
        };

        var candidates = ProductAlertRuleComposer.FromClassification(report, "p");
        Assert.Single(candidates);
        Assert.Equal("product.critical_class", candidates[0].EventType);
    }

    [Fact]
    public void Opportunity_Class_Emits()
    {
        // TEST 5 conceptual
        var report = new ProductClassificationReport
        {
            Rows =
            [
                ClassRow(3, "Opp", ProductPerformanceClass.Opportunity, 2_000m,
                    ProductTrendDirection.Growing)
            ]
        };

        var candidates = ProductAlertRuleComposer.FromClassification(report, "p");
        Assert.Single(candidates);
        Assert.Equal("product.growth_opportunity", candidates[0].EventType);
        Assert.True(candidates[0].OpportunityWindow);
    }

    [Fact]
    public void Insufficient_And_New_Do_Not_Emit_Advanced_Alerts()
    {
        // TEST 7 / 13
        Assert.True(ProductAlertRuleComposer.ShouldSuppressAdvancedAlert(
            ProductPerformanceClass.InsufficientData));
        Assert.True(ProductAlertRuleComposer.ShouldSuppressAdvancedAlert(
            ProductPerformanceClass.New));

        var report = new ProductClassificationReport
        {
            Rows =
            [
                ClassRow(4, "Nuevo", ProductPerformanceClass.New),
                ClassRow(5, "Insuf", ProductPerformanceClass.InsufficientData)
            ]
        };

        Assert.Empty(ProductAlertRuleComposer.FromClassification(report, "p"));
    }

    [Fact]
    public void HealthyGrowth_From_StockRisk_Emits_Opportunity()
    {
        var stock = new SalesStockRiskReport
        {
            Rows =
            [
                new SalesStockSignalRow
                {
                    ProductId = 6,
                    ProductName = "Grow",
                    PrimarySignal = SalesStockSignalKind.HealthyGrowth,
                    Signals = [SalesStockSignalKind.HealthyGrowth],
                    DaysOfCover = 25m,
                    UnitsPerDay = 3m,
                    Reason = "Crece con stock saludable"
                }
            ]
        };

        var candidates = ProductAlertRuleComposer.FromStockRiskGrowth(stock, "p");
        Assert.Single(candidates);
        Assert.Equal("product.growth_opportunity", candidates[0].EventType);
    }

    [Fact]
    public void Dedup_Opportunity_From_Class_And_Stock()
    {
        var rule = new ProductAlertDecisionRule(
            (_, _) => new ProductClassificationReport
            {
                Rows = [ClassRow(7, "Same", ProductPerformanceClass.Opportunity)]
            },
            (_, _) => new SalesStarMixReport(),
            (_, _) => new SalesStockRiskReport
            {
                Rows =
                [
                    new SalesStockSignalRow
                    {
                        ProductId = 7,
                        ProductName = "Same",
                        PrimarySignal = SalesStockSignalKind.HealthyGrowth,
                        Signals = [SalesStockSignalKind.HealthyGrowth],
                        DaysOfCover = 20m
                    }
                ]
            });

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth });

        Assert.Equal(1, report.EmittedCount);
        Assert.Equal("product.growth_opportunity", report.Events[0].EventType);
    }

    [Fact]
    public void Injected_Rule_Combines_Star_And_Critical()
    {
        var rule = new ProductAlertDecisionRule(
            (_, _) => new ProductClassificationReport
            {
                Rows = [ClassRow(8, "Bad", ProductPerformanceClass.Critical, 20_000m)]
            },
            (_, _) => new SalesStarMixReport
            {
                StarsWithStockoutRisk = [Star(9, "StarOut", 80_000m, true)]
            },
            (_, _) => new SalesStockRiskReport());

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.Last30Days });

        Assert.Equal(2, report.EmittedCount);
        Assert.Contains(report.Events, e => e.EventType == "product.critical_class");
        Assert.Contains(report.Events, e => e.EventType == "product.star_stockout");
    }

    [Fact]
    public void Registry_Includes_Product_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "product.alerts.v1");
        Assert.Contains("10.14", ProductAlertRulePolicy.Definition);
        Assert.Contains("TEST 7/13", ProductAlertRulePolicy.Definition);
    }
}
