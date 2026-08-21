using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.12–8.13 — clasificación + estrella checklist.</summary>
public class ProductClassificationTests
{
    private static ProductPerformanceRow Perf(
        int id, string name,
        InventoryHealthStatus health,
        decimal capital = 5_000m,
        decimal potential = 1_000m,
        int? idle = null,
        bool periodActivity = true,
        decimal? margin = 20m,
        decimal? roi = 30m,
        bool reliable = true)
        => ProductPerformanceComposer.Compose(
            periodActivity
                ? new ProfitGroupRow
                {
                    ProductId = id,
                    ProductName = name,
                    GroupName = name,
                    UnitsSold = 10,
                    RevenueTotal = 1000m,
                    RevenueWithCost = 1000m,
                    RealizedProfit = 200m,
                    MarginPct = margin,
                    RoiPct = roi,
                    HasReliableRealizedProfit = reliable
                }
                : null,
            new InventoryFinancialRow
            {
                ProductId = id,
                ProductName = name,
                Stock = 10,
                InventoryCapital = capital,
                PotentialProfit = potential,
                HealthStatus = health,
                IdleDays = idle
            });

    [Fact]
    public void Critical_Health_Is_Critical()
    {
        var row = ProductClassificationMath.Classify(
            Perf(1, "C", InventoryHealthStatus.Critical, idle: 70));
        Assert.Equal(ProductPerformanceClass.Critical, row.Class);
        Assert.Contains(row.Reasons, r => r.Contains("Critical"));
    }

    [Fact]
    public void New_Not_Slow()
    {
        var row = ProductClassificationMath.Classify(
            Perf(1, "N", InventoryHealthStatus.New, periodActivity: false));
        Assert.Equal(ProductPerformanceClass.New, row.Class);
    }

    [Fact]
    public void Frozen_Is_Slow_Unless_Critical_Aggravation()
    {
        var slow = ProductClassificationMath.Classify(
            Perf(1, "F", InventoryHealthStatus.Frozen, potential: 500m, idle: 40));
        Assert.Equal(ProductPerformanceClass.Slow, slow.Class);

        var crit = ProductClassificationMath.Classify(
            Perf(2, "F2", InventoryHealthStatus.Frozen, potential: -100m, idle: 40));
        Assert.Equal(ProductPerformanceClass.Critical, crit.Class);
    }

    [Fact]
    public void Growing_With_Margin_Is_Opportunity()
    {
        var perf = Perf(1, "O", InventoryHealthStatus.Healthy);
        var trend = ProductTrendMath.Compose(1, "O", "", 40, 20, 4000m, 2000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.Equal(ProductPerformanceClass.Opportunity, row.Class);
    }

    [Fact]
    public void Healthy_Without_Adverse_Signals()
    {
        var row = ProductClassificationMath.Classify(
            Perf(1, "H", InventoryHealthStatus.Healthy),
            ProductTrendMath.Compose(1, "H", "", 100, 100, 1000m, 1000m));
        Assert.Equal(ProductPerformanceClass.Healthy, row.Class);
    }

    [Fact]
    public void Star_Assigned_When_Checklist_Met()
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "StarProd",
                GroupName = "StarProd",
                UnitsSold = 50,
                RevenueTotal = 100_000m,
                RevenueWithCost = 100_000m,
                Cogs = 65_000m,
                RealizedProfit = 35_000m,
                MarginPct = 35m,
                RoiPct = 53.85m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "StarProd",
                Stock = 20,
                InventoryCapital = 10_000m,
                PotentialProfit = 5_000m,
                UnitsPerDay = 2m,
                TurnoverProxy = 1.5m,
                HealthStatus = InventoryHealthStatus.Healthy
            });

        var trend = ProductTrendMath.Compose(1, "StarProd", "", 50, 40, 100_000m, 80_000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.Equal(ProductPerformanceClass.Star, row.Class);
        Assert.Contains(row.Reasons, r => r.Contains("Impacto"));
        Assert.Contains(row.Reasons, r => r.Contains("Eficiencia"));
        Assert.Contains(row.Reasons, r => r.Contains("Bajo riesgo"));
    }

    [Fact]
    public void Most_Sold_Alone_Is_Not_Star()
    {
        var volumeOnly = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Volume",
                GroupName = "Volume",
                UnitsSold = 500,
                RevenueTotal = 50_000m,
                RevenueWithCost = 50_000m,
                Cogs = 48_000m,
                RealizedProfit = 2_000m,
                MarginPct = 4m,
                RoiPct = 4.17m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Volume",
                Stock = 100,
                InventoryCapital = 20_000m,
                PotentialProfit = 1_000m,
                UnitsPerDay = 5m,
                HealthStatus = InventoryHealthStatus.Healthy
            });

        var row = ProductClassificationMath.Classify(volumeOnly);
        Assert.NotEqual(ProductPerformanceClass.Star, row.Class);
    }

    [Fact]
    public void Low_Impact_Healthy_Is_Not_Star()
    {
        var row = ProductClassificationMath.Classify(
            Perf(1, "S", InventoryHealthStatus.Healthy, capital: 50_000m));
        Assert.NotEqual(ProductPerformanceClass.Star, row.Class);
        Assert.Contains("8.13", ProductClassificationPolicy.StarRule);
    }

    [Fact]
    public void Policy_Documents_Taxonomy_And_Star_Checklist()
    {
        Assert.Contains("Star", ProductClassificationPolicy.TaxonomyDefinition);
        Assert.Contains("Reasons", ProductClassificationPolicy.ExplainabilityNote);
        Assert.Contains("tres pilares", ProductStarPolicy.Definition);
        Assert.Contains("≠ más vendido", ProductStarPolicy.Definition);
    }
}
