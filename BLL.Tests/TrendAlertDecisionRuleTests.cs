using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.15 — alertas de tendencias.</summary>
public class TrendAlertDecisionRuleTests
{
    private static SalesAccelerationReport Accel(SalesAccelerationKind kind, decimal delta = -15m)
        => new()
        {
            PeriodKind = ProfitPeriodKind.Last30Days,
            Revenue = new SalesAccelerationResult
            {
                Kind = kind,
                AccelerationDeltaPp = delta,
                FirstChangePct = 40m,
                LastChangePct = 40m + delta,
                Reason = kind == SalesAccelerationKind.Decelerating
                    ? "Tasas +40% → +25% — desacelera (puede seguir creciendo)"
                    : "ok",
                ChangeCount = 3
            },
            RealizedProfit = new SalesAccelerationResult
            {
                Kind = SalesAccelerationKind.Steady
            }
        };

    private static SalesSeriesTrendReport Trend(SalesSeriesTrendKind kind, decimal cv = 45m)
        => new()
        {
            PeriodKind = ProfitPeriodKind.Last30Days,
            Revenue = new SalesSeriesTrendResult
            {
                Kind = kind,
                CoefficientOfVariationPct = cv,
                PointCount = 12,
                Reason = kind == SalesSeriesTrendKind.Volatile
                    ? $"CV {cv:N0}% ≥ 40% — no clasificar solo por pendiente"
                    : "ok"
            },
            RealizedProfit = new SalesSeriesTrendResult { Kind = SalesSeriesTrendKind.Stable },
            Units = new SalesSeriesTrendResult { Kind = SalesSeriesTrendKind.Stable },
            Transactions = new SalesSeriesTrendResult { Kind = SalesSeriesTrendKind.Stable }
        };

    [Fact]
    public void Deceleration_Emits_Not_As_Decline()
    {
        var candidates = TrendAlertRuleComposer.FromAcceleration(
            Accel(SalesAccelerationKind.Decelerating), "p");

        Assert.Single(candidates);
        Assert.Equal("trend.deceleration", candidates[0].EventType);
        Assert.Contains("≠ caída", candidates[0].Reason);
        Assert.Contains("creciendo", candidates[0].Reason);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Severity <= DecisionSeverity.High);
    }

    [Fact]
    public void Accelerating_And_Steady_Silent()
    {
        Assert.Empty(TrendAlertRuleComposer.FromAcceleration(
            Accel(SalesAccelerationKind.Accelerating, 12m), "p"));
        Assert.Empty(TrendAlertRuleComposer.FromAcceleration(
            Accel(SalesAccelerationKind.Steady, 2m), "p"));
    }

    [Fact]
    public void Insufficient_Acceleration_Silent()
    {
        Assert.Empty(TrendAlertRuleComposer.FromAcceleration(
            Accel(SalesAccelerationKind.InsufficientData), "p"));
    }

    [Fact]
    public void Volatile_Emits()
    {
        var candidates = TrendAlertRuleComposer.FromSeriesTrend(
            Trend(SalesSeriesTrendKind.Volatile, 55m), "p");

        Assert.Single(candidates);
        Assert.Equal("trend.volatile", candidates[0].EventType);
        Assert.Contains("pendiente", candidates[0].Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Growing_Declining_Stable_Not_Volatile_Alert()
    {
        Assert.Empty(TrendAlertRuleComposer.FromSeriesTrend(
            Trend(SalesSeriesTrendKind.Growing), "p"));
        Assert.Empty(TrendAlertRuleComposer.FromSeriesTrend(
            Trend(SalesSeriesTrendKind.Declining), "p"));
        Assert.Empty(TrendAlertRuleComposer.FromSeriesTrend(
            Trend(SalesSeriesTrendKind.Stable), "p"));
    }

    [Fact]
    public void Injected_Rule_Emits_Both()
    {
        var rule = new TrendAlertDecisionRule(
            (_, _) => Accel(SalesAccelerationKind.Decelerating, -18m),
            (_, _) => Trend(SalesSeriesTrendKind.Volatile, 50m));

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.Last30Days });

        Assert.Equal(2, report.EmittedCount);
        Assert.Contains(report.Events, e => e.EventType == "trend.deceleration");
        Assert.Contains(report.Events, e => e.EventType == "trend.volatile");
    }

    [Fact]
    public void Registry_Includes_Trend_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "trend.alerts.v1");
        Assert.Contains("10.15", TrendAlertRulePolicy.Definition);
        Assert.Contains("≠ MoM", TrendAlertRulePolicy.Definition);
    }
}
