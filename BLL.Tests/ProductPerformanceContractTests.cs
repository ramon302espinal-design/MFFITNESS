using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.2 — contrato métricas base (impacto ≠ eficiencia ≠ riesgo; sin score).</summary>
public class ProductPerformanceContractTests
{
    [Fact]
    public void Brief_Test1_Volume_Vs_Profit_Vs_Roi_Separated()
    {
        // A: más unidades e ingresos; B: más ganancia y ROI
        var aPeriod = new ProfitGroupRow
        {
            ProductId = 1,
            ProductName = "A",
            GroupName = "A",
            UnitsSold = 100,
            RevenueTotal = 100_000m,
            RevenueWithCost = 100_000m,
            Cogs = 70_000m,
            RealizedProfit = 30_000m,
            MarginPct = InventoryFinancialMath.MarginPct(30_000m, 100_000m),
            RoiPct = InventoryFinancialMath.RoiPct(30_000m, 70_000m),
            HasReliableRealizedProfit = true
        };
        var bPeriod = new ProfitGroupRow
        {
            ProductId = 2,
            ProductName = "B",
            GroupName = "B",
            UnitsSold = 50,
            RevenueTotal = 80_000m,
            RevenueWithCost = 80_000m,
            Cogs = 40_000m,
            RealizedProfit = 40_000m,
            MarginPct = InventoryFinancialMath.MarginPct(40_000m, 80_000m),
            RoiPct = InventoryFinancialMath.RoiPct(40_000m, 40_000m),
            HasReliableRealizedProfit = true
        };

        var a = ProductPerformanceComposer.Compose(aPeriod, null);
        var b = ProductPerformanceComposer.Compose(bPeriod, null);

        Assert.True(a.UnitsSold > b.UnitsSold);
        Assert.True(a.RevenueTotal > b.RevenueTotal);
        Assert.True(b.RealizedProfit > a.RealizedProfit);
        Assert.True(b.RoiPct > a.RoiPct);
    }

    [Fact]
    public void High_Roi_Low_Profit_Is_Efficiency_Not_Impact()
    {
        // Brief §21 / TEST 10: ROI 200% ganancia 2k vs ROI 40% ganancia 100k
        decimal roiSmall = InventoryFinancialMath.RoiPct(2_000m, 1_000m)!.Value;
        decimal roiBig = InventoryFinancialMath.RoiPct(100_000m, 250_000m)!.Value;

        Assert.True(roiSmall > roiBig);
        Assert.True(100_000m > 2_000m);
        Assert.Contains("ROI alto con ganancia mínima", ProductPerformancePolicy.EfficiencyDefinition);
    }

    [Fact]
    public void Immobilized_Only_When_Frozen_Or_Critical()
    {
        var healthy = new InventoryFinancialRow
        {
            ProductId = 1,
            ProductName = "H",
            InventoryCapital = 50_000m,
            HealthStatus = InventoryHealthStatus.Healthy
        };
        var frozen = new InventoryFinancialRow
        {
            ProductId = 2,
            ProductName = "F",
            InventoryCapital = 30_000m,
            HealthStatus = InventoryHealthStatus.Frozen
        };

        Assert.Equal(0m, ProductPerformanceComposer.ImmobilizedCapitalOf(healthy));
        Assert.Equal(30_000m, ProductPerformanceComposer.ImmobilizedCapitalOf(frozen));
        Assert.False(ProductPerformanceComposer.Compose(null, healthy).IsImmobilized);
        Assert.True(ProductPerformanceComposer.Compose(null, frozen).IsImmobilized);
    }

    [Fact]
    public void ComposeAll_Unions_Period_And_Inventory()
    {
        var period = new[]
        {
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "Sold",
                GroupName = "Sold",
                UnitsSold = 10,
                RevenueTotal = 1000m,
                HasReliableRealizedProfit = false
            }
        };
        var inv = new[]
        {
            new InventoryFinancialRow
            {
                ProductId = 2,
                ProductName = "StockOnly",
                Category = "Cat",
                Stock = 5,
                InventoryCapital = 500m,
                HealthStatus = InventoryHealthStatus.New
            }
        };

        var rows = ProductPerformanceComposer.ComposeAll(period, inv);
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.ProductId == 1 && r.HasPeriodActivity);
        Assert.Contains(rows, r => r.ProductId == 2 && r.HasInventorySnapshot && !r.HasPeriodActivity);
    }

    [Fact]
    public void Policy_Forbids_Star_Equals_Most_Sold_And_Score()
    {
        Assert.Contains("≠", ProductPerformancePolicy.StarRule);
        Assert.Contains("más vendido", ProductPerformancePolicy.StarRule);
        Assert.Contains("NO implementar pesos", ProductPerformancePolicy.ScoreNote);
        Assert.Contains("IMPACTO", ProductPerformancePolicy.ImpactDefinition);
        Assert.Contains("EFICIENCIA", ProductPerformancePolicy.EfficiencyDefinition);
        Assert.Contains("RIESGO", ProductPerformancePolicy.RiskDefinition);
        Assert.Contains("≠ ROI inversión", ProductPerformancePolicy.RoiProductDefinition);
    }

    [Fact]
    public void MetricKind_Contract_Lists_Independent_Rankings()
    {
        Assert.Equal(0, (int)ProductPerformanceMetricKind.UnitsSold);
        Assert.Equal(7, (int)ProductPerformanceMetricKind.ImmobilizedCapital);
        Assert.NotEqual(
            ProductPerformanceMetricKind.RealizedProfit,
            ProductPerformanceMetricKind.RoiPct);
    }
}
