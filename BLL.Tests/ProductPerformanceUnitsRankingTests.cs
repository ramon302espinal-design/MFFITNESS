using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.3 — ranking TOP UNIDADES (≠ ganancia ≠ estrella).</summary>
public class ProductPerformanceUnitsRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, int units, decimal revenue = 0m, decimal profit = 0m)
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
    public void Rank_By_Units_Not_By_Profit()
    {
        // Brief TEST 1: A más unidades; B más ganancia
        var rows = new[]
        {
            Row(1, "A", units: 100, revenue: 100_000m, profit: 30_000m),
            Row(2, "B", units: 50, revenue: 80_000m, profit: 40_000m),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.UnitsSold, top: 10);

        Assert.Equal(2, ranked.Count);
        Assert.Equal("A", ranked[0].Row.ProductName);
        Assert.Equal(1, ranked[0].Rank);
        Assert.Equal(100m, ranked[0].MetricValue);
        Assert.Equal(ProductPerformanceMetricKind.UnitsSold, ranked[0].Kind);
        Assert.Equal("B", ranked[1].Row.ProductName);
    }

    [Fact]
    public void Excludes_Zero_Units()
    {
        var rows = new[]
        {
            Row(1, "Active", 5),
            Row(2, "Idle", 0),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.UnitsSold);

        Assert.Single(ranked);
        Assert.Equal("Active", ranked[0].Row.ProductName);
    }

    [Fact]
    public void TieBreak_Uses_Revenue_Then_Name()
    {
        var rows = new[]
        {
            Row(1, "Zed", units: 10, revenue: 100m),
            Row(2, "Amy", units: 10, revenue: 500m),
            Row(3, "Bob", units: 10, revenue: 500m),
        };

        var ranked = ProductPerformanceRanker.Rank(
            rows, ProductPerformanceMetricKind.UnitsSold);

        Assert.Equal("Amy", ranked[0].Row.ProductName);
        Assert.Equal("Bob", ranked[1].Row.ProductName);
        Assert.Equal("Zed", ranked[2].Row.ProductName);
    }

    [Fact]
    public void Other_Metrics_Not_Supported_Yet()
    {
        // 8.10 completa todos los MetricKind del enum — no queda NotSupported.
        Assert.Equal(10, Enum.GetValues<ProductPerformanceMetricKind>().Length);
        foreach (ProductPerformanceMetricKind kind in Enum.GetValues<ProductPerformanceMetricKind>())
        {
            var result = ProductPerformanceRanker.Rank(
                Array.Empty<ProductPerformanceRow>(), kind);
            Assert.Empty(result);
        }
    }

    [Fact]
    public void Policy_Labels_Top_Units_Not_Star()
    {
        Assert.Contains("TOP UNIDADES", ProductPerformancePolicy.UnitsRankingDefinition);
        Assert.Contains("≠", ProductPerformancePolicy.UnitsRankingDefinition);
        Assert.Contains("estrella", ProductPerformancePolicy.UnitsRankingDefinition);
    }
}
