using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.10 — ranking TOP CONGELADO (≠ capital inventario ≠ estrella).</summary>
public class ProductPerformanceImmobilizedRankingTests
{
    private static ProductPerformanceRow Row(
        int id, string name, decimal capital, InventoryHealthStatus health,
        int? idleDays = null, decimal potential = 0m)
        => ProductPerformanceComposer.Compose(
            null,
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = 10,
                InventoryCapital = capital,
                PotentialProfit = potential,
                PotentialSalesValue = capital + potential,
                HealthStatus = health,
                IdleDays = idleDays
            });

    [Fact]
    public void Brief_Section11_Frozen_Ranks_Above_Healthy()
    {
        var frozen = Row(1, "FrozenA", 30_000m, InventoryHealthStatus.Frozen, idleDays: 40);
        var healthy = Row(2, "HealthyB", 50_000m, InventoryHealthStatus.Healthy);

        var byFrozen = ProductPerformanceRanker.Rank(
            new[] { healthy, frozen }, ProductPerformanceMetricKind.ImmobilizedCapital);
        var byCapital = ProductPerformanceRanker.Rank(
            new[] { healthy, frozen }, ProductPerformanceMetricKind.InventoryCapital);

        Assert.Single(byFrozen);
        Assert.Equal("FrozenA", byFrozen[0].Row.ProductName);
        Assert.Equal(30_000m, byFrozen[0].MetricValue);
        Assert.Equal("HealthyB", byCapital[0].Row.ProductName);
    }

    [Fact]
    public void Critical_Included_In_Immobilized()
    {
        var critical = Row(1, "Crit", 10_000m, InventoryHealthStatus.Critical, idleDays: 70);
        var slow = Row(2, "Slow", 8_000m, InventoryHealthStatus.Slow, idleDays: 20);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { slow, critical }, ProductPerformanceMetricKind.ImmobilizedCapital);

        Assert.Single(ranked);
        Assert.Equal("Crit", ranked[0].Row.ProductName);
    }

    [Fact]
    public void TieBreak_IdleDays()
    {
        var older = Row(1, "Older", 5_000m, InventoryHealthStatus.Frozen, idleDays: 90);
        var newer = Row(2, "Newer", 5_000m, InventoryHealthStatus.Frozen, idleDays: 35);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { newer, older }, ProductPerformanceMetricKind.ImmobilizedCapital);

        Assert.Equal("Older", ranked[0].Row.ProductName);
    }

    [Fact]
    public void Potential_Ranking_Parallel()
    {
        var high = Row(1, "HighPot", 10_000m, InventoryHealthStatus.Healthy, potential: 8_000m);
        var low = Row(2, "LowPot", 20_000m, InventoryHealthStatus.Healthy, potential: 1_000m);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { low, high }, ProductPerformanceMetricKind.PotentialProfit);

        Assert.Equal("HighPot", ranked[0].Row.ProductName);
        Assert.Equal(8_000m, ranked[0].MetricValue);
    }

    [Fact]
    public void Policy_Documents_Frozen_Or_Critical()
    {
        Assert.Contains("TOP CONGELADO", ProductPerformancePolicy.ImmobilizedRankingDefinition);
        Assert.Contains("Frozen o Critical", ProductPerformancePolicy.ImmobilizedRankingDefinition);
        Assert.Contains("≠ InventoryCapital", ProductPerformancePolicy.ImmobilizedRankingDefinition);
    }
}
