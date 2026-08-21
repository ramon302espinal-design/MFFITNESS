using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.3 — períodos extendidos (sin BD).</summary>
public class SalesPeriodTests
{
    private static readonly DateTime AsOf = new(2026, 8, 20);

    [Theory]
    [InlineData(ProfitPeriodKind.Last14Days)]
    [InlineData(ProfitPeriodKind.ThisQuarter)]
    [InlineData(ProfitPeriodKind.ThisSemester)]
    [InlineData(ProfitPeriodKind.PreviousYear)]
    public void New_Presets_Have_HalfOpen_Range(ProfitPeriodKind kind)
    {
        ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(kind, AsOf);
        Assert.NotNull(range.From);
        Assert.NotNull(range.ToExclusive);
        Assert.True(range.ToExclusive > range.From);
    }

    [Fact]
    public void Last14Days_Includes_AsOf()
    {
        var r = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.Last14Days, AsOf);
        Assert.Equal(new DateTime(2026, 8, 7), r.From);
        Assert.Equal(new DateTime(2026, 8, 21), r.ToExclusive);
        Assert.Equal(14, SalesAnalyticsMath.CalendarDays(r.From, r.ToExclusive));
    }

    [Fact]
    public void ThisQuarter_August_Is_Q3()
    {
        var r = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.ThisQuarter, AsOf);
        Assert.Equal(new DateTime(2026, 7, 1), r.From);
        Assert.Equal(new DateTime(2026, 10, 1), r.ToExclusive);
    }

    [Fact]
    public void ThisSemester_August_Is_H2()
    {
        var r = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.ThisSemester, AsOf);
        Assert.Equal(new DateTime(2026, 7, 1), r.From);
        Assert.Equal(new DateTime(2027, 1, 1), r.ToExclusive);
    }

    [Fact]
    public void PreviousYear_Is_Full_Prior_Calendar_Year()
    {
        var r = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.PreviousYear, AsOf);
        Assert.Equal(new DateTime(2025, 1, 1), r.From);
        Assert.Equal(new DateTime(2026, 1, 1), r.ToExclusive);
    }

    [Fact]
    public void Period_Pairs_Exist_For_New_Presets()
    {
        Assert.NotNull(ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.Last14Days, AsOf));
        Assert.NotNull(ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisQuarter, AsOf));
        Assert.NotNull(ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisSemester, AsOf));
        Assert.NotNull(ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisYear, AsOf));

        var q = ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisQuarter, AsOf)!.Value;
        Assert.Equal(new DateTime(2026, 4, 1), q.Previous.From);
        Assert.Equal(new DateTime(2026, 7, 1), q.Previous.ToExclusive);
    }

    [Fact]
    public void Policy_Lists_New_Periods()
    {
        Assert.Contains("14d", SalesAnalyticsPolicy.PeriodsDefinition);
        Assert.Contains("Trimestre", SalesAnalyticsPolicy.PeriodsDefinition);
        Assert.Contains("9.16", SalesAnalyticsPolicy.PeriodsDefinition);
    }
}
