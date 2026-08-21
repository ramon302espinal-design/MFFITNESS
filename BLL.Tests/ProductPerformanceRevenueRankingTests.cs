using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.4 — ranking TOP INGRESOS (≠ unidades ≠ ganancia ≠ estrella).</summary>
public class ProductPerformanceRevenueRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, int units, decimal revenue, decimal profit = 0m)
        => ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = id,
                ProductName = name,
                GroupName = name,
                UnitsSold = units,
                RevenueTotal = revenue,
                RealizedProfit = profit,
                HasReliableRealizedProfit = profit != 0
            },
            null);

    [Fact]
    public void Rank_By_Revenue_Not_By_Units_Or_Profit()
    {
        // A: más unidades; B: más ingresos y ganancia
        var rows = new[]
        {
            Row(1, "A", units: 500, revenue: 200_000m, profit: 20_000m),
            Row(2, "B", units: 100, revenue: 300_000m, profit: 40_000m),
        };

        var byRevenue = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.Revenue);
        var byUnits = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.UnitsSold);

        Assert.Equal("B", byRevenue[0].Row.ProductName);
        Assert.Equal(300_000m, byRevenue[0].MetricValue);
        Assert.Equal(ProductPerformanceMetricKind.Revenue, byRevenue[0].Kind);
        Assert.Equal("A", byUnits[0].Row.ProductName);
    }

    [Fact]
    public void Excludes_Zero_Revenue()
    {
        var rows = new[]
        {
            Row(1, "Paid", 1, 100m),
            Row(2, "Free", 5, 0m),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.Revenue);

        Assert.Single(ranked);
        Assert.Equal("Paid", ranked[0].Row.ProductName);
    }

    [Fact]
    public void All_MetricKinds_Accepted()
    {
        foreach (ProductPerformanceMetricKind kind in Enum.GetValues<ProductPerformanceMetricKind>())
            Assert.Empty(ProductPerformanceRanker.Rank(Array.Empty<ProductPerformanceRow>(), kind));
    }

    [Fact]
    public void Policy_Separates_Revenue_From_Units()
    {
        Assert.Contains("TOP INGRESOS", ProductPerformancePolicy.RevenueRankingDefinition);
        Assert.Contains("≠ unidades", ProductPerformancePolicy.RevenueRankingDefinition);
        Assert.Contains("Subtotal", ProductPerformancePolicy.RevenueRankingDefinition);
    }
}
