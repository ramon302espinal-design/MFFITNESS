using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.15 — producto en riesgo (Critical).</summary>
public class ProductRiskTests
{
    [Fact]
    public void Brief_Section29_High_Capital_Idle_Weak_Profit()
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Risk",
                GroupName = "Risk",
                UnitsSold = 2,
                RevenueTotal = 5_000m,
                RevenueWithCost = 5_000m,
                Cogs = 4_000m,
                RealizedProfit = 1_000m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Risk",
                Stock = 100,
                InventoryCapital = 50_000m,
                PotentialProfit = 2_000m,
                HealthStatus = InventoryHealthStatus.Healthy,
                IdleDays = 45
            });

        // idle 45 < 60 — should NOT trip idle rule; force 60+
        perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Risk",
                GroupName = "Risk",
                UnitsSold = 2,
                RevenueTotal = 5_000m,
                RevenueWithCost = 5_000m,
                Cogs = 4_500m,
                RealizedProfit = 500m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Risk",
                Stock = 100,
                InventoryCapital = 50_000m,
                PotentialProfit = 2_000m,
                HealthStatus = InventoryHealthStatus.Healthy,
                IdleDays = 70
            });

        var row = ProductClassificationMath.Classify(perf);
        Assert.Equal(ProductPerformanceClass.Critical, row.Class);
        Assert.Contains(row.Reasons, r => r.Contains("idle") || r.Contains("Capital"));
    }

    [Fact]
    public void Frozen_Positive_Potential_Is_Slow_Not_Critical()
    {
        var perf = ProductPerformanceComposer.Compose(
            null,
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "FrozenOk",
                Stock = 10,
                InventoryCapital = 5_000m,
                PotentialProfit = 1_000m,
                HealthStatus = InventoryHealthStatus.Frozen,
                IdleDays = 40
            });

        var row = ProductClassificationMath.Classify(perf);
        Assert.Equal(ProductPerformanceClass.Slow, row.Class);
    }

    [Fact]
    public void Stockout_Alone_Is_Not_Critical()
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Hot",
                GroupName = "Hot",
                UnitsSold = 100,
                RevenueTotal = 50_000m,
                HasReliableRealizedProfit = false
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Hot",
                Stock = 2,
                InventoryCapital = 500m,
                PotentialProfit = 200m,
                HealthStatus = InventoryHealthStatus.Healthy,
                FlagStockoutRisk = true,
                UnitsPerDay = 10m
            });

        Assert.False(ProductRiskMath.TryBuildRisk(perf, null, out _));
        var row = ProductClassificationMath.Classify(perf);
        Assert.NotEqual(ProductPerformanceClass.Critical, row.Class);
    }

    [Fact]
    public void Declining_Material_Weak_Profit_Is_Risk()
    {
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Fall",
                GroupName = "Fall",
                UnitsSold = 5,
                RevenueTotal = 3_000m,
                RevenueWithCost = 3_000m,
                Cogs = 2_800m,
                RealizedProfit = 200m,
                HasReliableRealizedProfit = true
            },
            new InventoryFinancialRow
            {
                ProductId = 1,
                ProductName = "Fall",
                Stock = 40,
                InventoryCapital = 20_000m,
                PotentialProfit = 1_000m,
                HealthStatus = InventoryHealthStatus.Healthy
            });
        var trend = ProductTrendMath.Compose(1, "Fall", "", 50, 100, 3000m, 10000m);
        var row = ProductClassificationMath.Classify(perf, trend);
        Assert.Equal(ProductPerformanceClass.Critical, row.Class);
    }

    [Fact]
    public void Policy_Separates_Stockout_And_Frozen()
    {
        Assert.Contains("≠ quiebre", ProductRiskPolicy.Definition);
        Assert.Contains("Frozen sin agravantes → Slow", ProductRiskPolicy.VsFrozen);
    }
}
