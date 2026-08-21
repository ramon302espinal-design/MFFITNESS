using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.2 — contrato capital inventario ≠ valor potencial ≠ congelado clasificado.</summary>
public class InventoryCapitalContractTests
{
    [Fact]
    public void Test1_Capital_Is_Stock_Times_Cost()
    {
        Assert.Equal(50_000m, InventoryFinancialMath.InventoryCapital(100, 500m));
        Assert.Equal(InventoryFinancialMath.InventoryCost(100, 500m),
            InventoryFinancialMath.InventoryCapital(100, 500m));
    }

    [Fact]
    public void Test2_Potential_Is_Not_Capital()
    {
        Assert.Equal(80_000m, InventoryFinancialMath.PotentialSalesValue(100, 800m));
        Assert.Equal(30_000m, InventoryFinancialMath.PotentialProfit(100, 500m, 800m));
        Assert.NotEqual(
            InventoryFinancialMath.InventoryCapital(100, 500m),
            InventoryFinancialMath.PotentialSalesValue(100, 800m));
    }

    [Fact]
    public void Zero_Stock_Or_Cost_Yields_Zero_Capital()
    {
        Assert.Equal(0m, InventoryFinancialMath.InventoryCapital(0, 500m));
        Assert.Equal(0m, InventoryFinancialMath.InventoryCapital(10, 0m));
        Assert.Equal(0m, InventoryFinancialMath.InventoryCapital(-1, 500m));
    }

    [Fact]
    public void FrozenShare_Requires_Inventory_Base()
    {
        Assert.Null(InventoryFinancialMath.FrozenShareOfInventoryPct(1000m, 0m));
        Assert.Equal(13.33m, InventoryFinancialMath.FrozenShareOfInventoryPct(20_000m, 150_000m));
    }

    [Fact]
    public void Policy_Documents_Legacy_Alias()
    {
        Assert.Contains("PrecioCompra", InventoryCapitalPolicy.InventoryCapitalDefinition);
        Assert.Contains("7.9", InventoryCapitalPolicy.FrozenVsInventoryNote);
        Assert.Contains("FASE 6", InventoryCapitalPolicy.InvestmentFrozenNote);
    }
}
