using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.6 — rotación PROXY (no inventario promedio).</summary>
public class InventoryTurnoverProxyTests
{
    [Fact]
    public void TurnoverProxy_Cogs_Over_Capital()
    {
        // COGS 25,000 / capital 50,000 = 0.5 vueltas en la ventana
        Assert.Equal(0.50m, InventoryFinancialMath.TurnoverProxy(25_000m, 50_000m));
    }

    [Fact]
    public void TurnoverProxy_Null_Without_Capital()
    {
        Assert.Null(InventoryFinancialMath.TurnoverProxy(1000m, 0m));
    }

    [Fact]
    public void TurnoverProxy_Zero_When_No_Cogs()
    {
        Assert.Equal(0m, InventoryFinancialMath.TurnoverProxy(0m, 10_000m));
    }

    [Fact]
    public void UnitTurnover_Stock_Based()
    {
        Assert.Equal(0.50m, InventoryFinancialMath.UnitTurnoverProxy(50, 100));
        Assert.Null(InventoryFinancialMath.UnitTurnoverProxy(10, 0));
    }

    [Fact]
    public void Policy_Labels_As_Proxy()
    {
        Assert.Contains("PROXY", InventoryCapitalPolicy.TurnoverProxyDefinition);
        Assert.Contains("promedio", InventoryCapitalPolicy.TurnoverProxyDefinition);
    }
}
