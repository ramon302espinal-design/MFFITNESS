using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.7 — cobertura / días de inventario.</summary>
public class InventoryCoverageTests
{
    [Fact]
    public void Brief_Stock60_Vel2_Is_30_Days()
    {
        Assert.Equal(30.00m, InventoryFinancialMath.DaysOfCover(60, 2m));
    }

    [Fact]
    public void Brief_Stock100_Vel5_Is_20_Days()
    {
        Assert.Equal(20.00m, InventoryFinancialMath.DaysOfCover(100, 5m));
    }

    [Fact]
    public void No_Velocity_No_Invented_Infinite_Cover()
    {
        Assert.Null(InventoryFinancialMath.DaysOfCover(100, 0m));
        Assert.Null(InventoryFinancialMath.DaysOfCover(100, null));
    }

    [Fact]
    public void Overstock_Threshold_Defaults()
    {
        Assert.Equal(90, InventoryFinancialMath.DefaultOverstockCoverDays);
        Assert.Equal(30, InventoryFinancialMath.DefaultHealthyCoverDays);
        decimal? cover = InventoryFinancialMath.DaysOfCover(300, 2m); // 150 días
        Assert.True(cover >= InventoryFinancialMath.DefaultOverstockCoverDays);
    }

    [Fact]
    public void Policy_Separates_Overstock_From_Frozen()
    {
        Assert.Contains("Stock / UnitsPerDay", InventoryCapitalPolicy.CoverageDefinition);
        Assert.Contains("congelado", InventoryCapitalPolicy.CoverageDefinition);
    }
}
