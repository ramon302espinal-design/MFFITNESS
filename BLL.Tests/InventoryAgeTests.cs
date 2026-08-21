using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.3 — antigüedad ≠ días sin venta.</summary>
public class InventoryAgeTests
{
    [Fact]
    public void DaysSince_Counts_Calendar_Days()
    {
        var entry = new DateTime(2026, 8, 1);
        var asOf = new DateTime(2026, 8, 20);
        Assert.Equal(19, InventoryFinancialMath.DaysSince(entry, asOf));
    }

    [Fact]
    public void DaysSince_Null_When_No_Entry()
    {
        Assert.Null(InventoryFinancialMath.DaysSince(null, DateTime.Today));
    }

    [Fact]
    public void DaysSince_Zero_On_Same_Day()
    {
        var d = new DateTime(2026, 8, 20, 15, 0, 0);
        Assert.Equal(0, InventoryFinancialMath.DaysSince(d, new DateTime(2026, 8, 20)));
    }

    [Fact]
    public void Age_Is_Not_DaysWithoutSale()
    {
        // Entrada 1 ago; última venta 5 ago; hoy 20 ago
        int age = InventoryFinancialMath.DaysSince(new DateTime(2026, 8, 1), new DateTime(2026, 8, 20))!.Value;
        int idle = InventoryFinancialMath.DaysSince(new DateTime(2026, 8, 5), new DateTime(2026, 8, 20))!.Value;
        Assert.Equal(19, age);
        Assert.Equal(15, idle);
        Assert.NotEqual(age, idle);
    }

    [Fact]
    public void Policy_Documents_Entry_Source()
    {
        Assert.Contains("ENTRADA", InventoryCapitalPolicy.AgeDefinition);
        Assert.Contains("LastSale", InventoryCapitalPolicy.AgeDefinition);
    }
}
