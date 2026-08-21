using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.9 — alertas de ventas (reglas de dominio).</summary>
public class SalesAlertDecisionRuleTests
{
    private static SalesVariationReport Report(
        decimal? revenueVar,
        decimal? profitVar = null,
        decimal? marginVar = null,
        IReadOnlyList<SalesCrossSignal>? crosses = null)
    {
        SalesVariationLabel rev = SalesVariationMath.Label(revenueVar);
        SalesVariationLabel profit = SalesVariationMath.Label(profitVar);
        SalesVariationLabel? margin = marginVar.HasValue
            ? SalesVariationMath.Label(marginVar)
            : null;

        return new SalesVariationReport
        {
            Revenue = rev,
            RealizedProfit = profit,
            Units = SalesVariationMath.Label(null),
            Transactions = SalesVariationMath.Label(null),
            Ticket = SalesVariationMath.Label(null),
            Margin = margin,
            CrossSignals = crosses ?? Array.Empty<SalesCrossSignal>()
        };
    }

    [Fact]
    public void Strong_Decline_30pct_Emits_Through_Engine()
    {
        // TEST 1
        var variations = Report(-30m);
        var rule = new SalesAlertDecisionRule(
            (_, _) => variations,
            (_, _) => null);

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext
            {
                PeriodKind = ProfitPeriodKind.ThisMonth,
                PeriodKey = "ThisMonth|test"
            });

        Assert.Equal(1, report.EmittedCount);
        Assert.Equal("sales.strong_decline", report.Events[0].EventType);
        Assert.Contains("Revisar", report.Events[0].Recommendation);
        Assert.True(report.Events[0].Severity >= DecisionSeverity.High);
    }

    [Fact]
    public void Flat_Variation_Produces_No_Candidate()
    {
        var candidates = SalesAlertRuleComposer.FromVariation(
            Report(1.5m), "p", "el mes");
        Assert.Empty(candidates);
    }

    [Fact]
    public void Mild_Decline_Does_Not_Emit_Strong_Alert()
    {
        // Solo Strong (15%+) — anti-fatiga alineado a dashboard
        var candidates = SalesAlertRuleComposer.FromVariation(
            Report(-8m), "p");
        Assert.Empty(candidates);
    }

    [Fact]
    public void Strong_Growth_Emits_Opportunity_Flag()
    {
        var candidates = SalesAlertRuleComposer.FromVariation(
            Report(20m), "p");
        Assert.Single(candidates);
        Assert.Equal("sales.strong_growth", candidates[0].EventType);
        Assert.True(candidates[0].OpportunityWindow);
    }

    [Fact]
    public void RevenueUp_ProfitDown_Emits_Cross_Alert()
    {
        // TEST 2
        var variations = Report(
            20m,
            -10m,
            crosses:
            [
                new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpProfitDown,
                    Message = "Crecimiento de ventas sin crecimiento de ganancia"
                }
            ]);

        var candidates = SalesAlertRuleComposer.FromVariation(variations, "p");
        Assert.Contains(candidates, c => c.EventType == "sales.strong_growth");
        Assert.Contains(candidates, c => c.EventType == "sales.rev_up_profit_down");
        Assert.All(
            candidates.Where(c => c.EventType == "sales.rev_up_profit_down"),
            c => Assert.True(c.Materiality.CrossSignal));

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.True(report.EmittedCount >= 2);
        Assert.Contains(report.Events, e => e.EventType == "sales.rev_up_profit_down");
    }

    [Fact]
    public void RevenueUp_MarginDown_Emits()
    {
        var variations = Report(
            18m,
            3m,
            -8m,
            crosses:
            [
                new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpMarginDown,
                    Message = "Ventas↑ margen↓"
                }
            ]);

        var candidates = SalesAlertRuleComposer.FromVariation(variations, "p");
        Assert.Contains(candidates, c => c.EventType == "sales.rev_up_margin_down");
    }

    [Fact]
    public void Concentration_Above_50_Emits()
    {
        var share = new SalesShareReport
        {
            TopN = 3,
            TopNSharePct = 62m,
            TotalAmount = 100_000m
        };

        var candidates = SalesAlertRuleComposer.FromConcentration(share, "p");
        Assert.Single(candidates);
        Assert.Equal("sales.concentration", candidates[0].EventType);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
    }

    [Fact]
    public void Concentration_Below_Threshold_Silent()
    {
        var share = new SalesShareReport
        {
            TopN = 3,
            TopNSharePct = 40m
        };
        Assert.Empty(SalesAlertRuleComposer.FromConcentration(share, "p"));
    }

    [Fact]
    public void NoComparable_Base_Silent()
    {
        var candidates = SalesAlertRuleComposer.FromVariation(
            Report(null), "p");
        Assert.Empty(candidates);
    }

    [Fact]
    public void Registry_Includes_Sales_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "sales.alerts.v1");
        Assert.Contains("10.9", SalesAlertRulePolicy.Definition);
        Assert.Contains("Strong", SalesAlertRulePolicy.StrongOnly);
    }

    [Fact]
    public void Injected_Rule_Does_Not_Hit_Db()
    {
        var rule = new SalesAlertDecisionRule(
            (_, _) => Report(-25m),
            (_, _) => new SalesShareReport { TopN = 3, TopNSharePct = 55m });

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.Last30Days });

        Assert.True(report.EmittedCount >= 2);
        Assert.Contains(report.Events, e => e.EventType == "sales.strong_decline");
        Assert.Contains(report.Events, e => e.EventType == "sales.concentration");
    }
}
