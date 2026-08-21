using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.17 — forecast / estimación (escenarios).</summary>
public class SalesForecastTests
{
    [Fact]
    public void Brief_Simple_Avg_Times_Horizon()
    {
        // 4 días a 10,000 → promedio 10k × 30 = 300k (simple); stable → base = 300k
        var revenues = new[] { 10_000m, 10_000m, 10_000m, 10_000m };
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Stable,
            PointCount = 4,
            CoefficientOfVariationPct = 0m
        };

        var report = SalesForecastMath.Build(revenues, horizonDays: 30, trendResult: trend);

        Assert.Equal(10_000m, report.HistoricalDailyAverageRevenue);
        Assert.Equal(300_000m, report.SimpleProjectionRevenue);
        Assert.Equal(300_000m, report.Base.EstimatedRevenue);
        Assert.True(report.HasEstimate);
    }

    [Fact]
    public void Brief_Test10_Low_Base_High()
    {
        var revenues = Enumerable.Repeat(10_000m, 20).ToArray();
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Stable,
            PointCount = 20,
            CoefficientOfVariationPct = 5m
        };

        var report = SalesForecastMath.Build(revenues, 30, trend);

        Assert.True(report.Low.EstimatedRevenue < report.Base.EstimatedRevenue);
        Assert.True(report.Base.EstimatedRevenue < report.High.EstimatedRevenue);
        Assert.Contains("bajo", report.Low.Label, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("base", report.Base.Label, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alto", report.High.Label, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("estimación", report.Base.Label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Growing_Raises_Base_Above_Simple()
    {
        var revenues = new[] { 8_000m, 9_000m, 10_000m, 11_000m };
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Growing,
            PointCount = 4,
            CoefficientOfVariationPct = 15m
        };

        var report = SalesForecastMath.Build(revenues, 30, trend);
        Assert.True(report.Base.EstimatedRevenue > report.SimpleProjectionRevenue);
        Assert.Equal(1.10m, report.TrendAdjustmentFactor);
    }

    [Fact]
    public void Declining_Lowers_Base()
    {
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Declining,
            PointCount = 4,
            CoefficientOfVariationPct = 15m
        };
        var report = SalesForecastMath.Build(
            new[] { 12_000m, 11_000m, 10_000m, 9_000m }, 30, trend);

        Assert.True(report.Base.EstimatedRevenue < report.SimpleProjectionRevenue);
    }

    [Fact]
    public void Profit_Estimated_From_Margin()
    {
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Stable,
            PointCount = 10,
            CoefficientOfVariationPct = 10m
        };
        var report = SalesForecastMath.Build(
            Enumerable.Repeat(10_000m, 10).ToArray(),
            30,
            trend,
            historicalMarginPct: 25m);

        Assert.Equal(75_000m, report.Base.EstimatedProfit); // 300k × 25%
        Assert.NotNull(report.Low.EstimatedProfit);
        Assert.NotNull(report.High.EstimatedProfit);
    }

    [Fact]
    public void Language_Is_Estimate_Not_Certainty()
    {
        Assert.Contains("ESTIMACIÓN", SalesForecastPolicy.Language);
        Assert.Contains("certeza", SalesForecastPolicy.Language);
        Assert.Contains("Nunca presentar como certeza", SalesForecastPolicy.Language);

        var report = SalesForecastMath.Build(
            new[] { 10_000m, 10_000m, 10_000m, 10_000m }, 30,
            new SalesSeriesTrendResult { Kind = SalesSeriesTrendKind.Stable, PointCount = 4 });

        Assert.Contains("ESTIMACIÓN", report.LanguageNote);
        Assert.Contains("estimación", report.Base.Label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confidence_No_Probabilities_High_With_Stable_Data()
    {
        var trend = new SalesSeriesTrendResult
        {
            Kind = SalesSeriesTrendKind.Stable,
            PointCount = 20,
            CoefficientOfVariationPct = 10m
        };
        var report = SalesForecastMath.Build(
            Enumerable.Repeat(10_000m, 20).ToArray(), 30, trend);

        Assert.Equal(SalesForecastConfidence.High, report.Confidence);
        Assert.Contains("ALTA", report.ConfidenceReason);
        Assert.DoesNotContain("probabilidad", report.ConfidenceReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Insufficient_Days_Marks_Low_Or_Insufficient_Confidence()
    {
        var report = SalesForecastMath.Build(new[] { 10_000m, 12_000m }, 30);
        Assert.Equal(SalesForecastConfidence.InsufficientData, report.Confidence);
        Assert.Equal(2, report.OperatingDaysUsed);
        Assert.True(report.HasEstimate);
    }

    [Fact]
    public void Policy_Requires_Three_Scenarios()
    {
        Assert.Contains("Low / Base / High", SalesForecastPolicy.Scenarios);
        Assert.Contains("NO inventar probabilidades", SalesForecastPolicy.Confidence);
        Assert.Contains("ESTIMADA", SalesForecastPolicy.Profit);
    }
}
