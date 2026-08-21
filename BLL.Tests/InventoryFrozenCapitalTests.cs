using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.9 — capital congelado = Frozen + Critical (≠ inventario total).</summary>
public class InventoryFrozenCapitalTests
{
    [Fact]
    public void Share_Is_Immobilized_Over_Inventory()
    {
        // Brief: 20,000 / 150,000 = 13.33%
        Assert.Equal(13.33m, InventoryFinancialMath.FrozenShareOfInventoryPct(20_000m, 150_000m));
    }

    [Fact]
    public void Healthy_Capital_Not_In_Frozen_Bucket()
    {
        Assert.Equal(
            InventoryHealthStatus.Healthy,
            InventoryHealthClassifier.Classify(
                60, 50_000m, 20_000m,
                InventoryIdleKind.HasSales, 2, 90, 20m, 3m));

        Assert.Equal(
            InventoryHealthStatus.Frozen,
            InventoryHealthClassifier.Classify(
                50, 5_000m, 1_000m,
                InventoryIdleKind.HasSales, 40, 100, null, 0m));
    }

    [Fact]
    public void Policy_States_Not_Equal_Inventory()
    {
        Assert.Contains("Frozen o Critical", InventoryCapitalPolicy.FrozenVsInventoryNote);
        Assert.Contains("≠", InventoryCapitalPolicy.FrozenVsInventoryNote);
    }
}
