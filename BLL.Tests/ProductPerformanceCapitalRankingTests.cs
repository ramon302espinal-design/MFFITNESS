using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.9 — ranking TOP CAPITAL inventario (≠ congelado ≠ estrella).</summary>
public class ProductPerformanceCapitalRankingTests
{
    private static ProductPerformanceRow WithCapital(
        int id, string name, decimal capital, int stock = 10,
        InventoryHealthStatus health = InventoryHealthStatus.Healthy)
        => ProductPerformanceComposer.Compose(
            null,
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = stock,
                InventoryCapital = capital,
                HealthStatus = health
            });

    [Fact]
    public void Rank_By_InventoryCapital_Not_Quality()
    {
        // Brief §10: más capital ≠ mejor producto
        var heavy = WithCapital(1, "Heavy", 50_000m);
        var light = WithCapital(2, "Light", 20_000m);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { light, heavy }, ProductPerformanceMetricKind.InventoryCapital);

        Assert.Equal("Heavy", ranked[0].Row.ProductName);
        Assert.Equal(50_000m, ranked[0].MetricValue);
        Assert.Equal(ProductPerformanceMetricKind.InventoryCapital, ranked[0].Kind);
    }

    [Fact]
    public void Healthy_High_Capital_Still_Ranks_Above_Frozen_Low()
    {
        var healthyBig = WithCapital(1, "HealthyBig", 40_000m, health: InventoryHealthStatus.Healthy);
        var frozenSmall = WithCapital(2, "FrozenSmall", 5_000m, health: InventoryHealthStatus.Frozen);

        var byCapital = ProductPerformanceRanker.Rank(
            new[] { healthyBig, frozenSmall }, ProductPerformanceMetricKind.InventoryCapital);

        Assert.Equal("HealthyBig", byCapital[0].Row.ProductName);
        Assert.Equal(0m, ProductPerformanceComposer.ImmobilizedCapitalOf(
            new InventoryFinancialRow
            {
                InventoryCapital = 40_000m,
                HealthStatus = InventoryHealthStatus.Healthy
            }));
    }

    [Fact]
    public void Excludes_Zero_Capital()
    {
        var ok = WithCapital(1, "Ok", 1000m);
        var zero = WithCapital(2, "Zero", 0m, stock: 0);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { ok, zero }, ProductPerformanceMetricKind.InventoryCapital);

        Assert.Single(ranked);
        Assert.Equal("Ok", ranked[0].Row.ProductName);
    }

    [Fact]
    public void Policy_Separates_Capital_From_Frozen()
    {
        Assert.Contains("TOP CAPITAL", ProductPerformancePolicy.CapitalRankingDefinition);
        Assert.Contains("≠ capital congelado", ProductPerformancePolicy.CapitalRankingDefinition);
        Assert.Contains("snapshot", ProductPerformancePolicy.CapitalRankingDefinition.ToLowerInvariant());
    }
}
