using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.12 — alertas de inventario operativo.</summary>
public class InventoryAlertDecisionRuleTests
{
    private static InventoryAlert Alert(
        InventoryAlertKind kind,
        int productId,
        string name,
        decimal capital,
        InventoryAlertPriority priority = InventoryAlertPriority.High,
        decimal? cover = null,
        int? idle = null)
        => new()
        {
            Kind = kind,
            ProductId = productId,
            ProductName = name,
            CapitalAmount = capital,
            Priority = priority,
            DaysOfCover = cover,
            IdleDays = idle,
            Message = $"{name}: {kind}"
        };

    private static InventoryAlertReport AlertReport(params InventoryAlert[] alerts)
        => new() { Alerts = alerts, TotalAlerts = alerts.Length };

    private static SalesStockSignalRow StockRow(
        int id,
        string name,
        SalesStockSignalKind signal,
        decimal? cover,
        int stock = 5)
        => new()
        {
            ProductId = id,
            ProductName = name,
            PrimarySignal = signal,
            Signals = [signal],
            DaysOfCover = cover,
            Stock = stock,
            ProjectedDemandUnits = 20m,
            UnitsPerDay = 2m,
            Reason = "Crece con cobertura baja"
        };

    private static SalesStockRiskReport StockReport(params SalesStockSignalRow[] rows)
        => new()
        {
            PeriodKind = ProfitPeriodKind.Last30Days,
            Rows = rows,
            ReplenishmentOpportunityCount = rows.Count(r =>
                r.PrimarySignal == SalesStockSignalKind.ReplenishmentOpportunity)
        };

    [Fact]
    public void Stockout_Emits_High_Urgency()
    {
        var candidates = InventoryAlertRuleComposer.FromInventoryAlerts(
            AlertReport(Alert(InventoryAlertKind.StockoutRisk, 1, "A", 500m, cover: 3m)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("inv.stockout_risk", candidates[0].EventType);
        Assert.True(candidates[0].TimeSensitiveStockout);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Priority >= DecisionPriority.High);
    }

    [Fact]
    public void Overstock_And_NeverSold_Emit()
    {
        var candidates = InventoryAlertRuleComposer.FromInventoryAlerts(
            AlertReport(
                Alert(InventoryAlertKind.Overstock, 2, "B", 3_000m, cover: 120m),
                Alert(InventoryAlertKind.NeverSold, 3, "C", 2_500m, idle: 45)),
            "p");

        Assert.Contains(candidates, c => c.EventType == "inv.overstock");
        Assert.Contains(candidates, c => c.EventType == "inv.never_sold");
    }

    [Fact]
    public void Capital_Kinds_Are_Ignored_In_10_12()
    {
        // Critical/Frozen = 10.13
        var candidates = InventoryAlertRuleComposer.FromInventoryAlerts(
            AlertReport(
                Alert(InventoryAlertKind.CriticalCapital, 9, "X", 20_000m),
                Alert(InventoryAlertKind.FrozenCapital, 8, "Y", 5_000m),
                Alert(InventoryAlertKind.HighImmobilizedShare, 0, "", 50_000m)),
            "p");

        Assert.Empty(candidates);
    }

    [Fact]
    public void Replenishment_From_StockRisk()
    {
        var candidates = InventoryAlertRuleComposer.FromStockRisk(
            StockReport(StockRow(4, "D", SalesStockSignalKind.ReplenishmentOpportunity, 7m)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("inv.replenishment", candidates[0].EventType);
        Assert.True(candidates[0].OpportunityWindow);
        Assert.Contains("auto", candidates[0].Recommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Caps_Per_Kind_Anti_Fatigue()
    {
        var alerts = Enumerable.Range(1, 20)
            .Select(i => Alert(InventoryAlertKind.Overstock, i, "P" + i, 1_000m * i, cover: 100m))
            .ToArray();

        var candidates = InventoryAlertRuleComposer.FromInventoryAlerts(
            AlertReport(alerts), "p", maxPerKind: 10);
        Assert.Equal(10, candidates.Count);
    }

    [Fact]
    public void Low_Capital_Stockout_Still_Emits()
    {
        var candidates = InventoryAlertRuleComposer.FromInventoryAlerts(
            AlertReport(Alert(InventoryAlertKind.StockoutRisk, 1, "Star", 100m)),
            "p");

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Injected_Rule_Combines_Sources()
    {
        var rule = new InventoryAlertDecisionRule(
            _ => AlertReport(
                Alert(InventoryAlertKind.StockoutRisk, 1, "A", 800m),
                Alert(InventoryAlertKind.NeverSold, 2, "B", 2_000m, idle: 60)),
            (_, _) => StockReport(
                StockRow(3, "C", SalesStockSignalKind.ReplenishmentOpportunity, 5m)));

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth });

        Assert.Equal(3, report.EmittedCount);
        Assert.Contains(report.Events, e => e.EventType == "inv.stockout_risk");
        Assert.Contains(report.Events, e => e.EventType == "inv.never_sold");
        Assert.Contains(report.Events, e => e.EventType == "inv.replenishment");
    }

    [Fact]
    public void Registry_Includes_Inventory_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "inv.alerts.v1");
        Assert.Contains("10.12", InventoryAlertRulePolicy.Definition);
        Assert.Contains("10.13", InventoryAlertRulePolicy.Definition);
    }
}
