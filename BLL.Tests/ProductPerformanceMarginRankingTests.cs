using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.6 — ranking TOP MARGEN (≠ ganancia absoluta ≠ ROI ≠ estrella).</summary>
public class ProductPerformanceMarginRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, decimal revenueWithCost, decimal profit)
    {
        decimal? margin = InventoryFinancialMath.MarginPct(profit, revenueWithCost);
        return ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = id,
                ProductName = name,
                GroupName = name,
                UnitsSold = 10,
                RevenueTotal = revenueWithCost,
                RevenueWithCost = revenueWithCost,
                Cogs = revenueWithCost - profit,
                RealizedProfit = profit,
                MarginPct = margin,
                RoiPct = InventoryFinancialMath.RoiPct(profit, revenueWithCost - profit),
                HasReliableRealizedProfit = true
            },
            null);
    }

    [Fact]
    public void High_Margin_Low_Profit_Beats_Low_Margin_High_Profit_On_Margin_Rank()
    {
        // Brief TEST 2: margen alto + ganancia baja vs volumen
        var highMargin = Row(1, "Niche", revenueWithCost: 10_000m, profit: 4_500m); // 45%
        var highProfit = Row(2, "Volume", revenueWithCost: 200_000m, profit: 20_000m); // 10%

        var byMargin = ProductPerformanceRanker.Rank(
            new[] { highMargin, highProfit }, ProductPerformanceMetricKind.MarginPct);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { highMargin, highProfit }, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Equal("Niche", byMargin[0].Row.ProductName);
        Assert.Equal(45.00m, byMargin[0].MetricValue);
        Assert.Equal("Volume", byProfit[0].Row.ProductName);
    }

    [Fact]
    public void Excludes_Null_Margin()
    {
        var ok = Row(1, "Ok", 1000m, 200m);
        var bad = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 2,
                ProductName = "NoCost",
                GroupName = "NoCost",
                UnitsSold = 100,
                RevenueTotal = 5000m,
                HasReliableRealizedProfit = false,
                MarginPct = null
            },
            null);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { ok, bad }, ProductPerformanceMetricKind.MarginPct);

        Assert.Single(ranked);
        Assert.Equal("Ok", ranked[0].Row.ProductName);
    }

    [Fact]
    public void Policy_Separates_Margin_From_Absolute_Profit()
    {
        Assert.Contains("TOP MARGEN", ProductPerformancePolicy.MarginRankingDefinition);
        Assert.Contains("≠ ganancia absoluta", ProductPerformancePolicy.MarginRankingDefinition);
        Assert.Contains("eficiencia", ProductPerformancePolicy.MarginRankingDefinition);
    }
}
