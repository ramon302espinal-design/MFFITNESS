using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.14 — oportunidad emergente ≠ estrella ≠ auto-compra.</summary>
public class ProductOpportunityTests
{
    private static ProductPerformanceRow Row(
        decimal capital, int stock, decimal margin, decimal roi, decimal profit = 500m)
        => ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Opp",
                GroupName = "Opp",
                UnitsSold = 15,
                RevenueTotal = 2000m,
                RevenueWithCost = 2000m,
                Cogs = 2000m - profit,
                RealizedProfit = profit,
                MarginPct = margin,
                RoiPct = roi,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Opp",
                Stock = stock,
                InventoryCapital = capital,
                PotentialProfit = 400m,
                HealthStatus = InventoryHealthStatus.Healthy
            });

    [Fact]
    public void Growing_Low_Capital_Is_Opportunity()
    {
        var perf = Row(capital: 8_000m, stock: 20, margin: 25m, roi: 40m);
        var trend = ProductTrendMath.Compose(1, "Opp", "", 40, 20, 4000m, 2000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.Equal(ProductPerformanceClass.Opportunity, row.Class);
        Assert.Contains(row.Reasons, r => r.Contains("Growing"));
        Assert.Contains(row.Reasons, r => r.Contains("no es orden de compra"));
    }

    [Fact]
    public void High_Capital_Growing_Is_Not_Opportunity()
    {
        var perf = Row(capital: 80_000m, stock: 20, margin: 25m, roi: 40m);
        var trend = ProductTrendMath.Compose(1, "Opp", "", 40, 20, 4000m, 2000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.NotEqual(ProductPerformanceClass.Opportunity, row.Class);
    }

    [Fact]
    public void Stable_Trend_Is_Not_Opportunity()
    {
        var perf = Row(capital: 5_000m, stock: 10, margin: 30m, roi: 50m);
        var trend = ProductTrendMath.Compose(1, "Opp", "", 100, 100, 1000m, 1000m);
        Assert.True(ProductOpportunityMath.TryBuildOpportunity(perf, trend, out _) == false);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.NotEqual(ProductPerformanceClass.Opportunity, row.Class);
    }

    [Fact]
    public void Star_Takes_Priority_Over_Opportunity()
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Both",
                GroupName = "Both",
                UnitsSold = 50,
                RevenueTotal = 100_000m,
                RevenueWithCost = 100_000m,
                Cogs = 65_000m,
                RealizedProfit = 35_000m,
                MarginPct = 35m,
                RoiPct = 53.85m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Both",
                Stock = 20,
                InventoryCapital = 10_000m,
                PotentialProfit = 5_000m,
                UnitsPerDay = 2m,
                TurnoverProxy = 1.5m,
                HealthStatus = InventoryHealthStatus.Healthy
            });
        var trend = ProductTrendMath.Compose(1, "Both", "", 50, 30, 100_000m, 60_000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.Equal(ProductPerformanceClass.Star, row.Class);
    }

    [Fact]
    public void Policy_Forbids_Auto_Buy()
    {
        Assert.Contains("NO recomienda comprar", ProductOpportunityPolicy.NoAutoBuy);
        Assert.Contains("≠ estrella", ProductOpportunityPolicy.Definition);
    }
}
