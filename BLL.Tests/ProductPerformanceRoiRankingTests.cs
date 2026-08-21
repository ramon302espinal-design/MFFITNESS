using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.7 — ranking TOP ROI producto (≠ margen ≠ ganancia ≠ ROI inversión).</summary>
public class ProductPerformanceRoiRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, decimal cogs, decimal profit)
    {
        decimal revenue = cogs + profit;
        return ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = id,
                ProductName = name,
                GroupName = name,
                UnitsSold = 10,
                RevenueTotal = revenue,
                RevenueWithCost = revenue,
                Cogs = cogs,
                RealizedProfit = profit,
                MarginPct = InventoryFinancialMath.MarginPct(profit, revenue),
                RoiPct = InventoryFinancialMath.RoiPct(profit, cogs),
                HasReliableRealizedProfit = true
            },
            null);
    }

    [Fact]
    public void Brief_Test1_B_Has_Higher_Roi_Than_A()
    {
        // A: 30k / 70k ≈ 42.86% · B: 40k / 40k = 100%
        var a = Row(1, "A", cogs: 70_000m, profit: 30_000m);
        var b = Row(2, "B", cogs: 40_000m, profit: 40_000m);

        var byRoi = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.RoiPct);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Equal("B", byRoi[0].Row.ProductName);
        Assert.Equal(100.00m, byRoi[0].MetricValue);
        Assert.Equal("B", byProfit[0].Row.ProductName);
    }

    [Fact]
    public void Brief_Test10_High_Roi_Low_Profit_Vs_High_Impact()
    {
        var efficient = Row(1, "Efficient", cogs: 1_000m, profit: 2_000m); // ROI 200%
        var impact = Row(2, "Impact", cogs: 250_000m, profit: 100_000m); // ROI 40%

        var byRoi = ProductPerformanceRanker.Rank(
            new[] { efficient, impact }, ProductPerformanceMetricKind.RoiPct);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { efficient, impact }, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Equal("Efficient", byRoi[0].Row.ProductName);
        Assert.Equal(200.00m, byRoi[0].MetricValue);
        Assert.Equal("Impact", byProfit[0].Row.ProductName);
    }

    [Fact]
    public void Excludes_Zero_Cogs_Or_Null_Roi()
    {
        var ok = Row(1, "Ok", 500m, 100m);
        var bad = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 2,
                ProductName = "NoCogs",
                GroupName = "NoCogs",
                UnitsSold = 1,
                RevenueTotal = 100m,
                Cogs = 0m,
                RealizedProfit = 0m,
                RoiPct = null,
                HasReliableRealizedProfit = false
            },
            null);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { ok, bad }, ProductPerformanceMetricKind.RoiPct);

        Assert.Single(ranked);
        Assert.Equal("Ok", ranked[0].Row.ProductName);
    }

    [Fact]
    public void Policy_Separates_Product_Roi_From_Investment()
    {
        Assert.Contains("TOP ROI", ProductPerformancePolicy.RoiRankingDefinition);
        Assert.Contains("≠ ROI inversión", ProductPerformancePolicy.RoiRankingDefinition);
        Assert.Contains("COGS > 0", ProductPerformancePolicy.RoiRankingDefinition);
    }
}
