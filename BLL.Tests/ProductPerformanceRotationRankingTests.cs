using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.8 — ranking TOP ROTACIÓN (TurnoverProxy / UnitsPerDay).</summary>
public class ProductPerformanceRotationRankingTests
{
    private static ProductPerformanceRow WithRotation(
        int id, string name, decimal? turnover, decimal? unitsPerDay, int unitsSold = 0)
        => ProductPerformanceComposer.Compose(
            unitsSold > 0
                ? new ProfitGroupRow
                {
                    ProductId = id,
                    ProductName = name,
                    GroupName = name,
                    UnitsSold = unitsSold,
                    HasReliableRealizedProfit = false
                }
                : null,
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = 10,
                InventoryCapital = 5_000m,
                TurnoverProxy = turnover,
                UnitsPerDay = unitsPerDay,
                HealthStatus = InventoryHealthStatus.Healthy
            });

    [Fact]
    public void Rank_By_TurnoverProxy_Desc()
    {
        var fast = WithRotation(1, "Fast", turnover: 2.5m, unitsPerDay: 1m);
        var slow = WithRotation(2, "Slow", turnover: 0.2m, unitsPerDay: 0.1m);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { slow, fast }, ProductPerformanceMetricKind.TurnoverProxy);

        Assert.Equal("Fast", ranked[0].Row.ProductName);
        Assert.Equal(2.5m, ranked[0].MetricValue);
        Assert.Equal(ProductPerformanceMetricKind.TurnoverProxy, ranked[0].Kind);
    }

    [Fact]
    public void Rank_By_UnitsPerDay_Parallel()
    {
        var a = WithRotation(1, "A", turnover: 1m, unitsPerDay: 5m, unitsSold: 50);
        var b = WithRotation(2, "B", turnover: 3m, unitsPerDay: 1m, unitsSold: 10);

        var byVel = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.UnitsPerDay);
        var byTurn = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.TurnoverProxy);

        Assert.Equal("A", byVel[0].Row.ProductName);
        Assert.Equal("B", byTurn[0].Row.ProductName);
    }

    [Fact]
    public void Excludes_Null_Turnover_And_Zero_Velocity()
    {
        var ok = WithRotation(1, "Ok", 1.0m, 2m);
        var noTurn = WithRotation(2, "NoTurn", null, 3m);
        var zeroVel = WithRotation(3, "ZeroVel", 0.5m, 0m);

        Assert.Single(ProductPerformanceRanker.Rank(
            new[] { ok, noTurn }, ProductPerformanceMetricKind.TurnoverProxy));
        Assert.Single(ProductPerformanceRanker.Rank(
            new[] { ok, zeroVel }, ProductPerformanceMetricKind.UnitsPerDay));
    }

    [Fact]
    public void Policy_Marks_Proxy_Not_Accounting_Turnover()
    {
        Assert.Contains("TOP ROTACIÓN", ProductPerformancePolicy.RotationRankingDefinition);
        Assert.Contains("PROXY", ProductPerformancePolicy.RotationRankingDefinition);
        Assert.Contains("≠ rotación contable", ProductPerformancePolicy.RotationRankingDefinition);
    }
}
