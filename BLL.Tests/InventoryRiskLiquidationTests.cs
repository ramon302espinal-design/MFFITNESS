using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.10 — riesgo + simulación liquidación.</summary>
public class InventoryRiskLiquidationTests
{
    [Fact]
    public void Simulate_10pct_Discount()
    {
        // 100 uds, costo 500, PVP 800 → lista 80k; -10% = 72k; capital 50k; PnL +22k
        var s = InventoryFinancialMath.SimulateLiquidation(100, 500m, 800m, 10m);
        Assert.Equal(10m, s.DiscountPct);
        Assert.Equal(720m, s.SimulatedUnitPrice);
        Assert.Equal(72_000m, s.SimulatedRevenue);
        Assert.Equal(50_000m, s.CapitalAtCost);
        Assert.Equal(22_000m, s.ProfitOrLoss);
        Assert.Equal(50_000m, s.CapitalLiberable);
    }

    [Fact]
    public void Simulate_50pct_Can_Be_Loss()
    {
        var s = InventoryFinancialMath.SimulateLiquidation(100, 500m, 800m, 50m);
        Assert.Equal(40_000m, s.SimulatedRevenue);
        Assert.Equal(-10_000m, s.ProfitOrLoss);
    }

    [Fact]
    public void Aggregate_Simulation_From_Totals()
    {
        var s = InventoryFinancialMath.SimulateLiquidationFromTotals(20_000m, 30_000m, 20m);
        Assert.Equal(24_000m, s.SimulatedRevenue);
        Assert.Equal(4_000m, s.ProfitOrLoss);
        Assert.Equal(20_000m, s.CapitalLiberable);
    }

    [Fact]
    public void Default_Discounts_Include_Brief_Set()
    {
        Assert.Contains(0m, InventoryFinancialMath.DefaultLiquidationDiscounts);
        Assert.Contains(50m, InventoryFinancialMath.DefaultLiquidationDiscounts);
        Assert.Equal(6, InventoryFinancialMath.DefaultLiquidationDiscounts.Count);
    }

    [Fact]
    public void Policy_Forbids_Price_Mutation()
    {
        Assert.Contains("NUNCA modifica PrecioVenta", InventoryCapitalPolicy.RiskAndLiberableDefinition);
        Assert.Contains("AtRisk", InventoryCapitalPolicy.RiskAndLiberableDefinition);
    }
}
