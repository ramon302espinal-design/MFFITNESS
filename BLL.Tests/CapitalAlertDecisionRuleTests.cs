using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.13 — alertas de capital.</summary>
public class CapitalAlertDecisionRuleTests
{
    private static InventoryAlert Alert(
        InventoryAlertKind kind,
        int? productId,
        string name,
        decimal capital,
        InventoryAlertPriority priority = InventoryAlertPriority.High)
        => new()
        {
            Kind = kind,
            ProductId = productId,
            ProductName = name,
            CapitalAmount = capital,
            Priority = priority,
            Message = $"{name}: {kind}"
        };

    private static InventoryAlertReport AlertReport(
        decimal? frozenShare,
        params InventoryAlert[] alerts)
        => new()
        {
            Alerts = alerts,
            TotalAlerts = alerts.Length,
            FrozenSharePct = frozenShare,
            ImmobilizedCapital = alerts.Sum(a => a.CapitalAmount ?? 0m)
        };

    private static SalesCapitalBridgeRow BridgeRow(
        int id,
        string name,
        decimal capital,
        ProductTrendDirection trend,
        decimal? revCh = -20m)
        => new()
        {
            ProductId = id,
            ProductName = name,
            InventoryCapital = capital,
            RevenueChangePct = revCh,
            Trend = trend,
            Signals =
            [
                new SalesCapitalSignal
                {
                    Kind = SalesCapitalSignalKind.CapitalRisk,
                    Message = "Capital risk"
                }
            ],
            PrimarySignal = SalesCapitalSignalKind.CapitalRisk
        };

    [Fact]
    public void Critical_And_Frozen_Emit()
    {
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            AlertReport(null,
                Alert(InventoryAlertKind.CriticalCapital, 1, "A", 15_000m, InventoryAlertPriority.Critical),
                Alert(InventoryAlertKind.FrozenCapital, 2, "B", 4_000m)),
            "p");

        Assert.Contains(candidates, c => c.EventType == "capital.critical");
        Assert.Contains(candidates, c => c.EventType == "capital.frozen");
        Assert.All(candidates, c => Assert.Equal(DecisionEventArea.Capital, c.Area));
    }

    [Fact]
    public void High_Immobilized_Share_Portfolio_Event()
    {
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            AlertReport(32m,
                Alert(InventoryAlertKind.HighImmobilizedShare, null, "", 80_000m,
                    InventoryAlertPriority.High)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("capital.high_immobilized_share", candidates[0].EventType);
        Assert.Equal(DecisionEntityType.Portfolio, candidates[0].EntityType);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Inventory_Operational_Kinds_Ignored()
    {
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            AlertReport(null,
                Alert(InventoryAlertKind.StockoutRisk, 1, "A", 500m),
                Alert(InventoryAlertKind.Overstock, 2, "B", 3_000m),
                Alert(InventoryAlertKind.NeverSold, 3, "C", 2_000m)),
            "p");

        Assert.Empty(candidates);
    }

    [Fact]
    public void Frozen_With_Unknown_Sales_Does_Not_Force_Critical_Priority()
    {
        // TEST 11 — ProductStillSelling amortigua
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            AlertReport(null,
                Alert(InventoryAlertKind.FrozenCapital, 1, "X", 50_000m,
                    InventoryAlertPriority.Critical)),
            "p");

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Priority <= DecisionPriority.Medium
                    || report.Events[0].Severity <= DecisionSeverity.High);
        Assert.True(candidates[0].ProductStillSelling);
    }

    [Fact]
    public void Bridge_CapitalRisk_Emits_AtRisk()
    {
        var bridge = new SalesCapitalBridgeReport
        {
            Rows =
            [
                BridgeRow(5, "Risk", 25_000m, ProductTrendDirection.Declining)
            ]
        };

        var candidates = CapitalAlertRuleComposer.FromCapitalBridge(bridge, "p");
        Assert.Single(candidates);
        Assert.Equal("capital.at_risk", candidates[0].EventType);
        Assert.False(candidates[0].ProductStillSelling);
    }

    [Fact]
    public void Bridge_Growing_Dampens_Urgency()
    {
        var bridge = new SalesCapitalBridgeReport
        {
            Rows =
            [
                BridgeRow(6, "StillSells", 30_000m, ProductTrendDirection.Growing, revCh: 5m)
            ]
        };

        var c = CapitalAlertRuleComposer.FromCapitalBridge(bridge, "p").Single();
        Assert.True(c.ProductStillSelling);
        Assert.Equal(DecisionUrgencyLevel.Medium, c.Urgency);
    }

    [Fact]
    public void Dedup_Alert_And_Bridge_Same_Product()
    {
        var rule = new CapitalAlertDecisionRule(
            _ => AlertReport(null,
                Alert(InventoryAlertKind.AtRiskLoss, 7, "Dup", 12_000m)),
            (_, _) => new SalesCapitalBridgeReport
            {
                Rows = [BridgeRow(7, "Dup", 12_000m, ProductTrendDirection.Declining)]
            });

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth });

        Assert.Equal(1, report.EmittedCount);
        Assert.Equal("capital.at_risk", report.Events[0].EventType);
    }

    [Fact]
    public void Slow_Capital_Emits_Low_Urgency()
    {
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            AlertReport(null,
                Alert(InventoryAlertKind.SlowCapital, 8, "Slow", 1_500m,
                    InventoryAlertPriority.Low)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("capital.slow", candidates[0].EventType);
        Assert.Equal(DecisionUrgencyLevel.Low, candidates[0].Urgency);
    }

    [Fact]
    public void Registry_Includes_Capital_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "capital.alerts.v1");
        Assert.Contains("10.13", CapitalAlertRulePolicy.Definition);
        Assert.Contains("TEST 11", CapitalAlertRulePolicy.Definition);
    }
}
