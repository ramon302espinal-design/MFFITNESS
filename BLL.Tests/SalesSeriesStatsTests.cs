using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.6 — promedios y medianas (serie diaria).</summary>
public class SalesSeriesStatsTests
{
    private static ProfitDayRow Day(DateTime date, decimal revenue, decimal profit, int units, int txns)
        => new()
        {
            Date = date,
            RevenueTotal = revenue,
            RealizedProfit = profit,
            UnitsSold = units,
            TransactionCount = txns,
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Brief_Test7_Average_Vs_Median_ExtremeDay()
    {
        var days = new[]
        {
            Day(new DateTime(2026, 8, 1), 10_000m, 2_000m, 10, 5),
            Day(new DateTime(2026, 8, 2), 10_000m, 2_000m, 10, 5),
            Day(new DateTime(2026, 8, 3), 10_000m, 2_000m, 10, 5),
            Day(new DateTime(2026, 8, 4), 10_000m, 2_000m, 10, 5),
            Day(new DateTime(2026, 8, 5), 100_000m, 20_000m, 100, 40)
        };

        var report = SalesSeriesStatsComposer.FromDays(
            days,
            ProfitPeriodKind.Custom,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 6));

        Assert.Equal(5, report.OperatingDays);
        Assert.Equal(28_000m, report.Revenue.Average);
        Assert.Equal(10_000m, report.Revenue.Median);
        Assert.True(report.Revenue.Average > report.Revenue.Median);
        Assert.Equal(100_000m, report.Revenue.Max);
        Assert.Equal(10_000m, report.Revenue.Min);
    }

    [Fact]
    public void Calendar_Vs_Operating_Average()
    {
        // 2 días con venta en rango de 10 días calendario
        var days = new[]
        {
            Day(new DateTime(2026, 8, 1), 50_000m, 10_000m, 50, 20),
            Day(new DateTime(2026, 8, 10), 50_000m, 10_000m, 50, 20)
        };

        var report = SalesSeriesStatsComposer.FromDays(
            days,
            ProfitPeriodKind.Custom,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 11));

        Assert.Equal(10, report.CalendarDays);
        Assert.Equal(2, report.OperatingDays);
        Assert.Equal(10_000m, report.AverageRevenuePerCalendarDay);  // 100k/10
        Assert.Equal(50_000m, report.AverageRevenuePerOperatingDay); // 100k/2
    }

    [Fact]
    public void Empty_Series_No_Invented_Average()
    {
        var report = SalesSeriesStatsComposer.FromDays(
            Array.Empty<ProfitDayRow>(),
            ProfitPeriodKind.ThisMonth,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 9, 1));

        Assert.Equal(0, report.OperatingDays);
        Assert.Null(report.Revenue.Average);
        Assert.Null(report.Revenue.Median);
        Assert.Null(report.AverageRevenuePerOperatingDay);
    }

    [Fact]
    public void Zero_Txn_Days_Excluded_From_Operating_Series()
    {
        var days = new[]
        {
            Day(new DateTime(2026, 8, 1), 20_000m, 4_000m, 20, 8),
            Day(new DateTime(2026, 8, 2), 0m, 0m, 0, 0),
            Day(new DateTime(2026, 8, 3), 30_000m, 6_000m, 30, 10)
        };

        var report = SalesSeriesStatsComposer.FromDays(
            days, ProfitPeriodKind.Custom,
            new DateTime(2026, 8, 1), new DateTime(2026, 8, 4));

        Assert.Equal(2, report.OperatingDays);
        Assert.Equal(25_000m, report.Revenue.Average);
        Assert.Equal(25_000m, report.Revenue.Median);
    }

    [Fact]
    public void Policy_Separates_Average_From_Median()
    {
        Assert.Contains("Mediana", SalesSeriesStatsPolicy.Definition);
        Assert.Contains("OperatingDays", SalesSeriesStatsPolicy.OperatingDays);
        Assert.Contains("N=0", SalesSeriesStatsPolicy.NoInvent);
    }
}
