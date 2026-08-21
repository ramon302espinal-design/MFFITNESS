using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.16 — alertas de forecast (estimación).</summary>
public class ForecastAlertDecisionRuleTests
{
    private static SalesForecastReport Forecast(
        SalesForecastConfidence confidence,
        string reason = "Pocos días / alta volatilidad")
        => new()
        {
            SourcePeriodKind = ProfitPeriodKind.Last30Days,
            HorizonDays = 30,
            OperatingDaysUsed = 10,
            TrendUsed = SalesSeriesTrendKind.Volatile,
            Confidence = confidence,
            ConfidenceReason = reason,
            LanguageNote = "ESTIMACIÓN",
            Low = new SalesForecastScenario
            {
                Key = "low",
                Label = "Escenario bajo (estimación)",
                EstimatedRevenue = 80_000m
            },
            Base = new SalesForecastScenario
            {
                Key = "base",
                Label = "Escenario base (estimación)",
                EstimatedRevenue = 100_000m
            },
            High = new SalesForecastScenario
            {
                Key = "high",
                Label = "Escenario alto (estimación)",
                EstimatedRevenue = 120_000m
            }
        };

    [Fact]
    public void Low_Confidence_Emits_Estimate_Language()
    {
        var candidates = ForecastAlertRuleComposer.FromForecast(
            Forecast(SalesForecastConfidence.Low), "p");

        Assert.Single(candidates);
        Assert.Equal("forecast.low_confidence", candidates[0].EventType);
        Assert.Contains("ESTIMACIÓN", candidates[0].Reason);
        Assert.Contains("no es certeza", candidates[0].Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("probabilidad 80", candidates[0].Reason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("va a vender", candidates[0].Description, StringComparison.OrdinalIgnoreCase);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.Contains("estimación", report.Events[0].Recommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Medium_And_High_Silent()
    {
        Assert.Empty(ForecastAlertRuleComposer.FromForecast(
            Forecast(SalesForecastConfidence.Medium), "p"));
        Assert.Empty(ForecastAlertRuleComposer.FromForecast(
            Forecast(SalesForecastConfidence.High), "p"));
    }

    [Fact]
    public void Insufficient_Data_Silent()
    {
        // TEST 7
        Assert.Empty(ForecastAlertRuleComposer.FromForecast(
            Forecast(SalesForecastConfidence.InsufficientData), "p"));
    }

    [Fact]
    public void Null_Forecast_Silent()
    {
        Assert.Empty(ForecastAlertRuleComposer.FromForecast(null, "p"));
    }

    [Fact]
    public void Injected_Rule_Through_Engine()
    {
        var rule = new ForecastAlertDecisionRule(
            (_, _) => Forecast(SalesForecastConfidence.Low, "CV alto"));

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKind = ProfitPeriodKind.Last30Days });

        Assert.Equal(1, report.EmittedCount);
        Assert.Equal(DecisionEventArea.Forecast, report.Events[0].Area);
        Assert.True(report.Events[0].Priority <= DecisionPriority.Medium);
    }

    [Fact]
    public void Registry_Includes_Forecast_Rule()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "forecast.alerts.v1");
        Assert.Contains("10.16", ForecastAlertRulePolicy.Definition);
        Assert.Contains("nunca certeza", ForecastAlertRulePolicy.Definition);
    }
}
