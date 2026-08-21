using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.4 — comparaciones período vs equivalente.</summary>
public class SalesComparisonTests
{
    private static SalesSummary Summary(
        decimal revenue, decimal profit, int units, int txns, decimal? margin = null)
        => new()
        {
            RevenueTotal = revenue,
            RealizedProfit = profit,
            UnitsSold = units,
            TransactionCount = txns,
            AverageTicket = SalesAnalyticsMath.AverageTicket(revenue, txns),
            MarginPct = margin,
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Brief_Test1_Revenue_Plus20()
    {
        var current = Summary(300_000m, 80_000m, 1000, 200, margin: 26m);
        var previous = Summary(250_000m, 70_000m, 900, 180, margin: 28m);

        var report = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(new DateTime(2026, 8, 1), new DateTime(2026, 9, 1)),
            new ProfitPeriodRange(new DateTime(2026, 7, 1), new DateTime(2026, 8, 1)),
            current,
            previous);

        Assert.Equal(20.00m, report.Revenue.VariationPct);
        Assert.True(report.Revenue.HasComparableBase);
        Assert.True(report.HasComparablePeriod);
    }

    [Fact]
    public void Separate_Metrics_Can_Diverge()
    {
        // Brief TEST 2 precursor: ventas +20%, ganancia -5%
        var current = Summary(120_000m, 19_000m, 500, 100);
        var previous = Summary(100_000m, 20_000m, 400, 90);

        var report = SalesComparisonComposer.Build(
            ProfitPeriodKind.Last30Days,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            current,
            previous);

        Assert.Equal(20.00m, report.Revenue.VariationPct);
        Assert.Equal(-5.00m, report.RealizedProfit.VariationPct);
        Assert.NotEqual(report.Revenue.VariationPct, report.RealizedProfit.VariationPct);
    }

    [Fact]
    public void Previous_Zero_Is_Not_Comparable()
    {
        var delta = SalesComparisonComposer.Delta(50_000m, 0m);
        Assert.Null(delta.VariationPct);
        Assert.False(delta.HasComparableBase);
    }

    [Fact]
    public void Ticket_And_Margin_Deltas()
    {
        var current = Summary(120_000m, 30_000m, 300, 100, margin: 25m);
        var previous = Summary(100_000m, 28_000m, 280, 100, margin: 28m);

        var report = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            current,
            previous);

        Assert.Equal(20.00m, report.Ticket.VariationPct); // 1200 vs 1000
        Assert.NotNull(report.Margin);
        Assert.True(report.Margin!.VariationPct < 0);
    }

    [Fact]
    public void Service_Returns_Null_When_No_Pair()
    {
        var svc = new SalesComparisonService();
        // AllTime no tiene par — no llama BD si corta en TryResolvePeriodPair
        Assert.Null(svc.GetComparison(ProfitPeriodKind.AllTime));
        Assert.Null(svc.GetComparison(ProfitPeriodKind.PreviousYear));
        Assert.Null(svc.GetComparison(ProfitPeriodKind.Custom));
    }

    [Fact]
    public void Policy_Separates_From_Seasonal_Yoy()
    {
        Assert.Contains("9.16", SalesComparisonPolicy.Definition);
        Assert.Contains("Ingresos", SalesComparisonPolicy.Metrics);
        Assert.Contains("HasComparablePeriod=false", SalesComparisonPolicy.NoPair);
    }
}
