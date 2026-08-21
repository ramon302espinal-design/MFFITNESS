using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.10 — alertas de ganancia / margen.</summary>
public class ProfitAlertDecisionRuleTests
{
    private static SalesVariationReport Report(
        decimal? revenueVar,
        decimal? profitVar,
        decimal? marginVar = null)
    {
        return new SalesVariationReport
        {
            Revenue = SalesVariationMath.Label(revenueVar),
            RealizedProfit = SalesVariationMath.Label(profitVar),
            Units = SalesVariationMath.Label(null),
            Transactions = SalesVariationMath.Label(null),
            Ticket = SalesVariationMath.Label(null),
            Margin = marginVar.HasValue ? SalesVariationMath.Label(marginVar) : null,
            CrossSignals = Array.Empty<SalesCrossSignal>()
        };
    }

    [Fact]
    public void Strong_Profit_Decline_Emits()
    {
        var candidates = ProfitAlertRuleComposer.FromVariation(
            Report(-5m, -28m), "p", "el mes");

        Assert.Single(candidates);
        Assert.Equal("profit.decline", candidates[0].EventType);
        Assert.Equal(DecisionEventArea.Profit, candidates[0].Area);
        Assert.Contains("Revisar", candidates[0].Recommendation);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Severity >= DecisionSeverity.High);
    }

    [Fact]
    public void Mild_Profit_Decline_Silent()
    {
        Assert.Empty(ProfitAlertRuleComposer.FromVariation(
            Report(0m, -8m), "p"));
    }

    [Fact]
    public void Strong_Margin_Decline_Emits()
    {
        var candidates = ProfitAlertRuleComposer.FromVariation(
            Report(10m, 5m, -20m), "p");

        Assert.Contains(candidates, c => c.EventType == "margin.deterioration");
        Assert.Equal(DecisionEventArea.Margin,
            candidates.First(c => c.EventType == "margin.deterioration").Area);
    }

    [Fact]
    public void Profit_And_Margin_Can_Both_Emit()
    {
        var candidates = ProfitAlertRuleComposer.FromVariation(
            Report(-2m, -25m, -18m), "p");

        Assert.Contains(candidates, c => c.EventType == "profit.decline");
        Assert.Contains(candidates, c => c.EventType == "margin.deterioration");

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(2, report.EmittedCount);
    }

    [Fact]
    public void Does_Not_Emit_Sales_Events()
    {
        // Caída fuerte de ingresos sola → no es responsabilidad de 10.10
        var candidates = ProfitAlertRuleComposer.FromVariation(
            Report(-30m, 2m), "p");
        Assert.Empty(candidates);
    }

    [Fact]
    public void Injected_Rule_Through_Engine()
    {
        var rule = new ProfitAlertDecisionRule((_, _) => Report(5m, -30m, -16m));
        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth });

        Assert.True(report.EmittedCount >= 2);
        Assert.Contains(report.Events, e => e.EventType == "profit.decline");
        Assert.Contains(report.Events, e => e.EventType == "margin.deterioration");
    }

    [Fact]
    public void Registry_Includes_Profit_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "profit.alerts.v1");
        Assert.Contains("10.10", ProfitAlertRulePolicy.Definition);
        Assert.Contains("Ganancia ≠ ingresos", ProfitAlertRulePolicy.Definition);
    }

    [Fact]
    public void NoComparable_Silent()
    {
        Assert.Empty(ProfitAlertRuleComposer.FromVariation(
            Report(null, null, null), "p"));
    }
}
