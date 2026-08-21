using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.8 — clasificación saludable/lento/congelado/crítico.</summary>
public class InventoryHealthClassifierTests
{
    [Fact]
    public void New_Product_Not_Frozen()
    {
        var s = InventoryHealthClassifier.Classify(
            stock: 100,
            inventoryCapital: 50_000m,
            potentialProfit: 10_000m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 5,
            daysSinceFirstEntry: 5,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.Equal(InventoryHealthStatus.New, s);
    }

    [Fact]
    public void High_Rotation_High_Capital_Is_Healthy()
    {
        var s = InventoryHealthClassifier.Classify(
            stock: 60,
            inventoryCapital: 50_000m,
            potentialProfit: 20_000m,
            idleKind: InventoryIdleKind.HasSales,
            idleDays: 2,
            daysSinceFirstEntry: 90,
            daysOfCover: 20m,
            unitsPerDay: 3m);

        Assert.Equal(InventoryHealthStatus.Healthy, s);
    }

    [Fact]
    public void Material_Idle_Is_Frozen()
    {
        var s = InventoryHealthClassifier.Classify(
            stock: 50,
            inventoryCapital: 5_000m,
            potentialProfit: 1_000m,
            idleKind: InventoryIdleKind.HasSales,
            idleDays: 40,
            daysSinceFirstEntry: 100,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.Equal(InventoryHealthStatus.Frozen, s);
    }

    [Fact]
    public void High_Capital_Frozen_Is_Critical()
    {
        var s = InventoryHealthClassifier.Classify(
            stock: 100,
            inventoryCapital: 50_000m,
            potentialProfit: 5_000m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 70,
            daysSinceFirstEntry: 80,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.Equal(InventoryHealthStatus.Critical, s);
    }

    [Fact]
    public void Low_Capital_No_Sales_Is_Not_Critical()
    {
        var s = InventoryHealthClassifier.Classify(
            stock: 2,
            inventoryCapital: 400m,
            potentialProfit: 100m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 45,
            daysSinceFirstEntry: 50,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.NotEqual(InventoryHealthStatus.Critical, s);
        Assert.Equal(InventoryHealthStatus.Slow, s);
    }

    [Fact]
    public void Policy_Documents_Materiality()
    {
        Assert.Contains("MinMaterialCapital", InventoryCapitalPolicy.HealthClassificationDefinition);
    }
}
