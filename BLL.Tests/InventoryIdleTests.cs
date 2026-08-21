using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.4 — última venta / NeverSold / IdleDays.</summary>
public class InventoryIdleTests
{
    private static readonly DateTime AsOf = new(2026, 8, 20);

    [Fact]
    public void HasSales_Idle_From_LastSale()
    {
        var r = InventoryFinancialMath.ResolveIdle(
            lastSaleDate: new DateTime(2026, 8, 1),
            firstEntryDate: new DateTime(2026, 7, 1),
            asOf: AsOf);

        Assert.Equal(InventoryIdleKind.HasSales, r.Kind);
        Assert.Equal(19, r.IdleDays);
        Assert.Equal(19, r.DaysWithoutSale);
    }

    [Fact]
    public void NeverSold_Uses_Entry_As_Reference()
    {
        var r = InventoryFinancialMath.ResolveIdle(
            lastSaleDate: null,
            firstEntryDate: new DateTime(2026, 8, 1),
            asOf: AsOf);

        Assert.Equal(InventoryIdleKind.NeverSold, r.Kind);
        Assert.Equal(19, r.IdleDays);
        Assert.Null(r.DaysWithoutSale);
    }

    [Fact]
    public void Unknown_Without_Sale_Or_Entry()
    {
        var r = InventoryFinancialMath.ResolveIdle(null, null, AsOf);
        Assert.Equal(InventoryIdleKind.Unknown, r.Kind);
        Assert.Null(r.IdleDays);
        Assert.Null(r.DaysWithoutSale);
    }

    [Fact]
    public void Idle_Differs_From_Age_When_Sold()
    {
        // Entrada 1 jul; última venta 5 ago; hoy 20 ago
        int age = InventoryFinancialMath.DaysSince(new DateTime(2026, 7, 1), AsOf)!.Value;
        var idle = InventoryFinancialMath.ResolveIdle(
            new DateTime(2026, 8, 5), new DateTime(2026, 7, 1), AsOf);

        Assert.Equal(50, age);
        Assert.Equal(15, idle.IdleDays);
        Assert.NotEqual(age, idle.IdleDays);
    }

    [Fact]
    public void Policy_Documents_NeverSold()
    {
        Assert.Contains("NUNCA VENDIDO", InventoryCapitalPolicy.IdleDefinition);
        Assert.Contains("FirstEntry", InventoryCapitalPolicy.IdleDefinition);
    }
}
