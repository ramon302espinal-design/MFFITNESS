using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.8 — ventas por hora / confiabilidad / picos.</summary>
public class SalesByHourTests
{
    private static SalesHourRow H(int hour, decimal revenue, int txns, int units = 10, decimal profit = 1_000m)
        => SalesByHourComposer.Compose(hour, txns, units, revenue, profit, reliableProfit: true);

    [Fact]
    public void Peaks_Differ_By_Metric()
    {
        var hours = new[]
        {
            H(10, 50_000m, txns: 20, units: 40),
            H(14, 30_000m, txns: 45, units: 90),
            H(18, 40_000m, txns: 25, units: 200)
        };

        var report = SalesByHourComposer.Build(
            hours, ProfitPeriodKind.ThisMonth,
            new DateTime(2026, 8, 1), new DateTime(2026, 9, 1));

        Assert.True(report.HourDataReliable);
        Assert.Equal(10, report.PeakByRevenue!.Hour);
        Assert.Equal(14, report.PeakByTransactions!.Hour);
        Assert.Equal(18, report.PeakByUnits!.Hour);
    }

    [Fact]
    public void All_Midnight_Is_Unreliable_No_Peaks()
    {
        var hours = new[]
        {
            H(0, 100_000m, txns: 80, units: 200)
        };

        var report = SalesByHourComposer.Build(
            hours, ProfitPeriodKind.Last7Days, null, null);

        Assert.False(report.HourDataReliable);
        Assert.Null(report.PeakByRevenue);
        Assert.Null(report.PeakByTransactions);
        Assert.Contains("00:00", report.ReliabilityNote);
    }

    [Fact]
    public void Empty_Is_Unreliable()
    {
        var report = SalesByHourComposer.Build(
            Array.Empty<SalesHourRow>(), ProfitPeriodKind.Today, null, null);

        Assert.False(report.HourDataReliable);
        Assert.Contains("insuficientes", report.ReliabilityNote, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Mixed_Hours_With_Some_Midnight_Still_Reliable()
    {
        var hours = new[]
        {
            H(0, 5_000m, txns: 2),
            H(11, 40_000m, txns: 30)
        };

        var report = SalesByHourComposer.Build(hours, ProfitPeriodKind.ThisMonth, null, null);
        Assert.True(report.HourDataReliable);
        Assert.Equal(11, report.PeakByRevenue!.Hour);
    }

    [Fact]
    public void Policy_Forbids_Inventing_Hours()
    {
        Assert.Contains("NO inventar", SalesByHourPolicy.Definition);
        Assert.Contains("HourDataReliable=false", SalesByHourPolicy.Reliability);
        Assert.Contains("≠", SalesByHourPolicy.Peaks);
    }
}
