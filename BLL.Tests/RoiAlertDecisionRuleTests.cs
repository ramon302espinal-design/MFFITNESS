using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.11 — alertas ROI de línea (≠ inversión).</summary>
public class RoiAlertDecisionRuleTests
{
    private static SalesCapitalBridgeRow Row(
        int id,
        string name,
        decimal? revCh,
        decimal? roiCh,
        decimal capital,
        bool revUpRoiDown)
    {
        var signals = new List<SalesCapitalSignal>();
        if (revUpRoiDown)
        {
            signals.Add(new SalesCapitalSignal
            {
                Kind = SalesCapitalSignalKind.RevenueUpRoiDown,
                Message = "Ventas ↑ + ROI ↓"
            });
        }

        return new SalesCapitalBridgeRow
        {
            ProductId = id,
            ProductName = name,
            RevenueChangePct = revCh,
            RoiChangePct = roiCh,
            RoiPct = 40m + (roiCh ?? 0m),
            InventoryCapital = capital,
            Signals = signals,
            PrimarySignal = revUpRoiDown
                ? SalesCapitalSignalKind.RevenueUpRoiDown
                : SalesCapitalSignalKind.None
        };
    }

    private static SalesCapitalBridgeReport Bridge(params SalesCapitalBridgeRow[] rows)
        => new()
        {
            PeriodKind = ProfitPeriodKind.ThisMonth,
            Rows = rows,
            RevenueUpRoiDownCount = rows.Count(r =>
                r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown))
        };

    [Fact]
    public void Section52_RevUp_RoiDown_Emits()
    {
        // TEST 6 conceptual
        var candidates = RoiAlertRuleComposer.FromBridge(
            Bridge(Row(1, "SKU-A", 20m, -8m, 5_000m, revUpRoiDown: true)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("roi.rev_up_roi_down", candidates[0].EventType);
        Assert.Equal(DecisionEventArea.Roi, candidates[0].Area);
        Assert.Contains("FASE 6", candidates[0].Reason);
        Assert.True(candidates[0].Materiality.CrossSignal);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Strong_Roi_Decline_Alone_Emits_Deterioration()
    {
        var candidates = RoiAlertRuleComposer.FromBridge(
            Bridge(Row(2, "SKU-B", -5m, -18m, 12_000m, revUpRoiDown: false)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("roi.deterioration", candidates[0].EventType);
    }

    [Fact]
    public void Mild_Roi_Change_Without_Section52_Silent()
    {
        Assert.Empty(RoiAlertRuleComposer.FromBridge(
            Bridge(Row(3, "SKU-C", -1m, -5m, 2_000m, revUpRoiDown: false)),
            "p"));
    }

    [Fact]
    public void Prefers_Section52_Over_Deterioration_When_Both()
    {
        // Señal §52 presente → no duplicar como deterioration
        var candidates = RoiAlertRuleComposer.FromBridge(
            Bridge(Row(4, "SKU-D", 25m, -20m, 8_000m, revUpRoiDown: true)),
            "p");

        Assert.Single(candidates);
        Assert.Equal("roi.rev_up_roi_down", candidates[0].EventType);
    }

    [Fact]
    public void Caps_Product_Alerts_Anti_Fatigue()
    {
        var rows = Enumerable.Range(1, 20)
            .Select(i => Row(i, "P" + i, 10m, -10m - i, 1_000m * i, revUpRoiDown: true))
            .ToArray();

        var candidates = RoiAlertRuleComposer.FromBridge(Bridge(rows), "p", maxProductAlerts: 10);
        Assert.Equal(10, candidates.Count);
    }

    [Fact]
    public void Low_Capital_Section52_Still_Emits_Via_CrossSignal()
    {
        var candidates = RoiAlertRuleComposer.FromBridge(
            Bridge(Row(5, "SKU-E", 15m, -3m, 200m, revUpRoiDown: true)),
            "p");

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Injected_Rule_Through_Engine()
    {
        var rule = new RoiAlertDecisionRule((_, _) => Bridge(
            Row(1, "A", 20m, -6m, 4_000m, true),
            Row(2, "B", 0m, -22m, 15_000m, false)));

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.Last30Days });

        Assert.Equal(2, report.EmittedCount);
        Assert.Contains(report.Events, e => e.EventType == "roi.rev_up_roi_down");
        Assert.Contains(report.Events, e => e.EventType == "roi.deterioration");
    }

    [Fact]
    public void Registry_Includes_Roi_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "roi.alerts.v1");
        Assert.Contains("10.11", RoiAlertRulePolicy.Definition);
        Assert.Contains("≠ ROI de inversión", RoiAlertRulePolicy.Definition);
    }

    [Fact]
    public void Empty_Bridge_Silent()
    {
        Assert.Empty(RoiAlertRuleComposer.FromBridge(
            Bridge(), "p"));
        Assert.Empty(RoiAlertRuleComposer.FromBridge(null, "p"));
    }
}
