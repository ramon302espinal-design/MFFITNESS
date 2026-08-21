using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.5 — ranking TOP GANANCIA (≠ ROI ≠ ingresos ≠ estrella).</summary>
public class ProductPerformanceProfitRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, int units, decimal revenue, decimal profit, bool reliable = true)
        => ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = id,
                ProductName = name,
                GroupName = name,
                UnitsSold = units,
                RevenueTotal = revenue,
                RevenueWithCost = reliable ? revenue : 0m,
                Cogs = reliable ? revenue - profit : 0m,
                RealizedProfit = profit,
                HasReliableRealizedProfit = reliable
            },
            null);

    [Fact]
    public void Brief_Test1_B_Wins_Profit_A_Wins_Units()
    {
        var rows = new[]
        {
            Row(1, "A", units: 100, revenue: 100_000m, profit: 30_000m),
            Row(2, "B", units: 50, revenue: 80_000m, profit: 40_000m),
        };

        var byProfit = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.RealizedProfit);
        var byUnits = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.UnitsSold);

        Assert.Equal("B", byProfit[0].Row.ProductName);
        Assert.Equal(40_000m, byProfit[0].MetricValue);
        Assert.Equal(ProductPerformanceMetricKind.RealizedProfit, byProfit[0].Kind);
        Assert.Equal("A", byUnits[0].Row.ProductName);
    }

    [Fact]
    public void Excludes_Unreliable_Cost()
    {
        var rows = new[]
        {
            Row(1, "Ok", 10, 1000m, 200m, reliable: true),
            Row(2, "NoCost", 50, 5000m, 999m, reliable: false),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Single(ranked);
        Assert.Equal("Ok", ranked[0].Row.ProductName);
    }

    [Fact]
    public void Loss_Can_Rank_Below_Profit()
    {
        var rows = new[]
        {
            Row(1, "Win", 5, 1000m, 200m),
            Row(2, "Loss", 5, 1000m, -100m),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Equal("Win", ranked[0].Row.ProductName);
        Assert.Equal("Loss", ranked[1].Row.ProductName);
        Assert.Equal(-100m, ranked[1].MetricValue);
    }

    [Fact]
    public void Policy_Separates_Profit_From_Roi()
    {
        Assert.Contains("TOP GANANCIA", ProductPerformancePolicy.ProfitRankingDefinition);
        Assert.Contains("≠ ROI", ProductPerformancePolicy.ProfitRankingDefinition);
        Assert.Contains("HasReliableRealizedProfit", ProductPerformancePolicy.ProfitRankingDefinition);
    }
}
