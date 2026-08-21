using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.18 — señales Dashboard (sin Forms).</summary>
public class ProductPerformanceDashboardTests
{
    private static ProductClassificationRow Row(
        int id, string name, ProductPerformanceClass cls,
        decimal capital = 10_000m, InventoryHealthStatus health = InventoryHealthStatus.Healthy,
        decimal profit = 1_000m, int units = 10)
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = id,
                ProductName = name,
                GroupName = name,
                UnitsSold = units,
                RevenueTotal = Math.Abs(profit) * 3m,
                RealizedProfit = profit,
                Cogs = Math.Abs(profit) * 2m,
                RevenueWithCost = Math.Abs(profit) * 3m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = 5,
                InventoryCapital = capital,
                PotentialProfit = 500m,
                HealthStatus = health,
                IdleDays = health is InventoryHealthStatus.Frozen or InventoryHealthStatus.Critical
                    ? 60
                    : 5,
                TurnoverProxy = 1.2m,
                UnitsPerDay = 1m
            });

        return new ProductClassificationRow
        {
            ProductId = id,
            ProductName = name,
            Class = cls,
            Reasons = new[] { cls.ToString() },
            Performance = perf
        };
    }

    [Fact]
    public void Build_Maps_Buckets_And_Capital()
    {
        var classification = new ProductClassificationReport
        {
            PeriodKind = ProfitPeriodKind.ThisMonth,
            StarCount = 1,
            HealthyCount = 1,
            OpportunityCount = 1,
            SlowCount = 1,
            CriticalCount = 1,
            NewCount = 0,
            InsufficientCount = 0,
            ProductCount = 5,
            Rows = new[]
            {
                Row(1, "Star", ProductPerformanceClass.Star, 20_000m, profit: 8_000m, units: 50),
                Row(2, "Ok", ProductPerformanceClass.Healthy, 15_000m, profit: 2_000m),
                Row(3, "Opp", ProductPerformanceClass.Opportunity, 5_000m, profit: 1_500m),
                Row(4, "Slow", ProductPerformanceClass.Slow, 12_000m,
                    InventoryHealthStatus.Frozen, profit: 100m),
                Row(5, "Risk", ProductPerformanceClass.Critical, 30_000m,
                    InventoryHealthStatus.Critical, profit: -200m)
            }
        };

        var dash = ProductPerformanceDashboardComposer.Build(classification, topLists: 3);

        Assert.Equal(1, dash.StarCount);
        Assert.Equal(1, dash.OpportunityCount);
        Assert.Equal(1, dash.CriticalCount);
        Assert.Equal(20_000m, dash.StarCapital);
        Assert.Equal(30_000m, dash.CriticalClassCapital);
        Assert.Equal(42_000m, dash.TotalImmobilizedCapital); // Frozen Slow 12k + Critical 30k
        Assert.Equal("Star", dash.TopStars[0].ProductName);
        Assert.Equal("Risk", dash.TopRisks[0].ProductName);
        Assert.NotEmpty(dash.TopUnits);
        Assert.NotEmpty(dash.TopProfit);
        Assert.InRange(dash.PortfolioHealthScore, 0, 100);
    }

    [Fact]
    public void PortfolioScore_Penalizes_Critical()
    {
        int healthy = ProductPerformanceDashboardComposer.PortfolioHealthScore(
            star: 2, healthy: 3, opportunity: 1, neu: 0, slow: 0, critical: 0, insufficient: 0);
        int risky = ProductPerformanceDashboardComposer.PortfolioHealthScore(
            star: 2, healthy: 3, opportunity: 1, neu: 0, slow: 0, critical: 3, insufficient: 0);

        Assert.True(healthy > risky);
        Assert.Equal(100, healthy);
    }

    [Fact]
    public void Policy_Separates_From_Fase7_Health()
    {
        Assert.Contains("≠", ProductPerformanceDashboardPolicy.Buckets);
        Assert.Contains("Forms", ProductPerformanceDashboardPolicy.Definition);
        Assert.Contains("una métrica", ProductPerformanceDashboardPolicy.Tops);
        Assert.Contains("PortfolioHealthScore", ProductPerformanceDashboardPolicy.PortfolioScore);
    }
}
