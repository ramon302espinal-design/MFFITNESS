using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.17 — alertas de inversiones.</summary>
public class InvestmentAlertDecisionRuleTests
{
    private static InvestmentSummary Summary(
        int id,
        string name,
        decimal invested,
        decimal frozen,
        decimal? roi,
        bool isLoss = false,
        bool reliable = true,
        InvestmentStatus status = InvestmentStatus.Activa,
        decimal? recoveryPct = 20m)
        => new()
        {
            InvestmentId = id,
            Name = name,
            CapitalInvested = invested,
            FrozenCapital = frozen,
            RoiRealizedPct = roi,
            IsLoss = isLoss,
            HasReliableCost = reliable,
            Status = status,
            RecoveryPct = recoveryPct,
            RealizedProfit = isLoss ? -500m : 100m
        };

    private static InvestmentTrappedCapitalRow Trapped(
        InvestmentSummary s,
        int frozenCrit = 1)
        => new()
        {
            Summary = s,
            TrappedCapital = s.FrozenCapital,
            ProductsFrozenOrCritical = frozenCrit,
            ProductsLinked = frozenCrit + 1
        };

    [Fact]
    public void Material_Frozen_Capital_Emits()
    {
        var bridge = new InvestmentCapitalBridgeReport
        {
            Investments =
            [
                Trapped(Summary(1, "Inv-A", 20_000m, 8_000m, 5m))
            ]
        };

        var candidates = InvestmentAlertRuleComposer.FromTrappedCapital(bridge, "p");
        Assert.Single(candidates);
        Assert.Equal("invst.frozen_capital", candidates[0].EventType);
        Assert.Equal(DecisionEntityType.Investment, candidates[0].EntityType);
        Assert.Contains("≠ InventoryCapital", candidates[0].Reason);
        Assert.Contains("no liquidar", candidates[0].Recommendation, StringComparison.OrdinalIgnoreCase);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Below_MinMaterial_Frozen_Silent()
    {
        var bridge = new InvestmentCapitalBridgeReport
        {
            Investments =
            [
                Trapped(Summary(2, "Tiny", 500m, 400m, 10m))
            ]
        };

        Assert.Empty(InvestmentAlertRuleComposer.FromTrappedCapital(bridge, "p"));
    }

    [Fact]
    public void Negative_Roi_Emits_Weak()
    {
        var candidates = InvestmentAlertRuleComposer.FromSummaries(
        [
            Summary(3, "Lossy", 15_000m, 2_000m, -12m, isLoss: true)
        ], "p");

        Assert.Single(candidates);
        Assert.Equal("invst.roi_weak", candidates[0].EventType);
        Assert.Contains("FASE 6", candidates[0].Reason);
    }

    [Fact]
    public void Positive_Roi_Not_Weak()
    {
        Assert.Empty(InvestmentAlertRuleComposer.FromSummaries(
        [
            Summary(4, "Ok", 10_000m, 1_000m, 18m)
        ], "p"));
    }

    [Fact]
    public void Unreliable_Or_Planificada_Skipped()
    {
        Assert.False(InvestmentAlertRuleComposer.IsRoiWeak(
            Summary(5, "U", 20_000m, 0m, -5m, reliable: false)));
        Assert.False(InvestmentAlertRuleComposer.IsRoiWeak(
            Summary(6, "P", 20_000m, 0m, -5m, status: InvestmentStatus.Planificada)));
    }

    [Fact]
    public void Injected_Rule_Emits_Frozen_And_Roi()
    {
        var rule = new InvestmentAlertDecisionRule(
            () => new InvestmentCapitalBridgeReport
            {
                Investments =
                [
                    Trapped(Summary(10, "Trap", 30_000m, 12_000m, 2m), frozenCrit: 2)
                ]
            },
            () =>
            [
                Summary(11, "Neg", 25_000m, 500m, -8m, isLoss: true)
            ]);

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth });

        Assert.Equal(2, report.EmittedCount);
        Assert.Contains(report.Events, e => e.EventType == "invst.frozen_capital");
        Assert.Contains(report.Events, e => e.EventType == "invst.roi_weak");
    }

    [Fact]
    public void Registry_Includes_Investment_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "invst.alerts.v1");
        Assert.Contains("10.17", InvestmentAlertRulePolicy.Definition);
        Assert.Contains("≠ InventoryCapital", InvestmentAlertRulePolicy.Definition);
    }

    [Fact]
    public void MinMaterial_Matches_Inventory_Ssot()
    {
        Assert.Equal(
            InventoryHealthThresholds.Default.MinMaterialCapital,
            InvestmentAlertRuleComposer.MinMaterialCapital);
    }
}
