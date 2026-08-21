using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.5 — velocidad de venta.</summary>
public class InventoryVelocityTests
{
    [Fact]
    public void Test_Brief_30_Units_Over_30_Days()
    {
        var v = InventoryFinancialMath.ResolveVelocity(30, 30);
        Assert.Equal(1.00m, v.UnitsPerDay);
        Assert.Equal(7.00m, v.UnitsPerWeek);
        Assert.Equal(30.00m, v.UnitsPerMonth);
    }

    [Fact]
    public void Zero_Sales_Yields_Zero_Velocity_Not_Null()
    {
        var v = InventoryFinancialMath.ResolveVelocity(0, 30);
        Assert.Equal(0m, v.UnitsPerDay);
        Assert.Equal(0m, v.UnitsPerWeek);
        Assert.Equal(0m, v.UnitsPerMonth);
    }

    [Fact]
    public void Invalid_Window_Yields_Null()
    {
        var v = InventoryFinancialMath.ResolveVelocity(10, 0);
        Assert.Null(v.UnitsPerDay);
    }

    [Fact]
    public void Policy_Documents_Commercial_Month()
    {
        Assert.Contains("30", InventoryCapitalPolicy.VelocityDefinition);
        Assert.Equal(30, InventoryCapitalPolicy.DefaultVelocityWindowDays);
    }
}
