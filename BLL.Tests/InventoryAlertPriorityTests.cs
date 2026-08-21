using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.11 — prioridades de alerta.</summary>
public class InventoryAlertPriorityTests
{
    [Fact]
    public void Small_Capital_Is_Low()
    {
        Assert.Equal(
            InventoryAlertPriority.Low,
            InventoryAlertService.ResolvePriorityByCapital(100m));
    }

    [Fact]
    public void Material_Capital_Is_High()
    {
        Assert.Equal(
            InventoryAlertPriority.High,
            InventoryAlertService.ResolvePriorityByCapital(5_000m));
    }

    [Fact]
    public void Critical_Capital_Threshold()
    {
        Assert.Equal(
            InventoryAlertPriority.Critical,
            InventoryAlertService.ResolvePriorityByCapital(50_000m));
    }

    [Fact]
    public void Floor_Raises_Priority()
    {
        Assert.Equal(
            InventoryAlertPriority.High,
            InventoryAlertService.ResolvePriorityByCapital(
                100m, floor: InventoryAlertPriority.High));
    }

    [Fact]
    public void Policy_Separates_Integrity_Anomalies()
    {
        Assert.Contains("4.8", InventoryCapitalPolicy.AlertDefinition);
        Assert.Contains("StockoutRisk", InventoryCapitalPolicy.AlertDefinition);
    }
}
