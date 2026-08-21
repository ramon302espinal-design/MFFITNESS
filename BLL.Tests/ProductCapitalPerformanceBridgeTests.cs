using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.17 — puente capital / clasificación de performance.</summary>
public class ProductCapitalPerformanceBridgeTests
{
    private static ProductPerformanceRow Perf(
        int id, string name, decimal capital, InventoryHealthStatus health,
        decimal profit = 0m, int? idle = null)
        => ProductPerformanceComposer.Compose(
            profit != 0m
                ? new ProfitGroupRow
                {
                    ProductId = id,
                    ProductName = name,
                    GroupName = name,
                    UnitsSold = 10,
                    RevenueTotal = Math.Abs(profit) * 3m,
                    RealizedProfit = profit,
                    HasReliableRealizedProfit = true
                }
                : null,
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = 5,
                InventoryCapital = capital,
                PotentialProfit = capital * 0.2m,
                HealthStatus = health,
                IdleDays = idle
            });

    private static ProductClassificationRow Cls(int id, string name, ProductPerformanceClass c)
        => new()
        {
            ProductId = id,
            ProductName = name,
            Class = c,
            Reasons = new[] { c.ToString() }
        };

    [Fact]
    public void Star_Capital_Is_Not_Immobilized_When_Healthy()
    {
        var perf = Perf(1, "StarA", 40_000m, InventoryHealthStatus.Healthy, profit: 12_000m);
        var row = ProductCapitalPerformanceComposer.Compose(perf, Cls(1, "StarA", ProductPerformanceClass.Star));

        Assert.Equal(40_000m, row.InventoryCapital);
        Assert.Equal(0m, row.ImmobilizedCapital);
        Assert.False(row.IsImmobilized);
        Assert.Equal(ProductPerformanceClass.Star, row.Class);
    }

    [Fact]
    public void Frozen_Slow_Counts_In_Immobilized_And_Slow_Bucket()
    {
        var frozen = ProductCapitalPerformanceComposer.Compose(
            Perf(1, "FrozenSlow", 25_000m, InventoryHealthStatus.Frozen, idle: 45),
            Cls(1, "FrozenSlow", ProductPerformanceClass.Slow));
        var healthy = ProductCapitalPerformanceComposer.Compose(
            Perf(2, "Healthy", 60_000m, InventoryHealthStatus.Healthy, profit: 5_000m),
            Cls(2, "Healthy", ProductPerformanceClass.Healthy));

        var report = ProductCapitalPerformanceComposer.BuildReport(
            new[] { frozen, healthy }, ProfitPeriodKind.ThisMonth);

        Assert.Equal(85_000m, report.TotalInventoryCapital);
        Assert.Equal(25_000m, report.TotalImmobilizedCapital);
        Assert.NotEqual(report.TotalInventoryCapital, report.TotalImmobilizedCapital);
        Assert.Equal(25_000m, report.SlowCapital);
        Assert.Equal(60_000m, report.HealthyCapital);
        Assert.Single(report.TopImmobilized);
        Assert.Equal("FrozenSlow", report.TopImmobilized[0].ProductName);
        Assert.Equal(ProductPerformanceClass.Slow, report.TopImmobilized[0].Class);
    }

    [Fact]
    public void Critical_Class_Capital_Separate_From_Star()
    {
        var crit = ProductCapitalPerformanceComposer.Compose(
            Perf(1, "Risk", 30_000m, InventoryHealthStatus.Critical, idle: 80),
            Cls(1, "Risk", ProductPerformanceClass.Critical));
        var star = ProductCapitalPerformanceComposer.Compose(
            Perf(2, "Star", 30_000m, InventoryHealthStatus.Healthy, profit: 9_000m),
            Cls(2, "Star", ProductPerformanceClass.Star));
        var opp = ProductCapitalPerformanceComposer.Compose(
            Perf(3, "Opp", 8_000m, InventoryHealthStatus.Healthy, profit: 2_000m),
            Cls(3, "Opp", ProductPerformanceClass.Opportunity));

        var report = ProductCapitalPerformanceComposer.BuildReport(
            new[] { crit, star, opp }, ProfitPeriodKind.ThisMonth);

        Assert.Equal(30_000m, report.CriticalClassCapital);
        Assert.Equal(30_000m, report.StarCapital);
        Assert.Equal(8_000m, report.OpportunityCapital);
        Assert.Equal(30_000m, report.TotalImmobilizedCapital);
        Assert.Equal(3, report.Buckets.Count);
    }

    [Fact]
    public void Policy_Separates_Immobilized_And_Classes()
    {
        Assert.Contains("≠", ProductCapitalPerformancePolicy.Definition);
        Assert.Contains("Star", ProductCapitalPerformancePolicy.Definition);
        Assert.Contains("HealthStatus", ProductCapitalPerformancePolicy.Separation);
        Assert.Contains("Sin score", ProductCapitalPerformancePolicy.NoScore);
        Assert.Contains("estrellas", ProductCapitalPerformancePolicy.Question);
    }
}
