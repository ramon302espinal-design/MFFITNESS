using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.7 — ventas por día / mejor / peor.</summary>
public class SalesByDayTests
{
    private static ProfitDayRow Day(
        int day, decimal revenue, decimal profit, int units = 10, int txns = 5)
        => new()
        {
            Date = new DateTime(2026, 8, day),
            RevenueTotal = revenue,
            RealizedProfit = profit,
            UnitsSold = units,
            TransactionCount = txns,
            MarginPct = revenue > 0 ? InventoryFinancialMath.MarginPct(profit, revenue) : null,
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Best_Revenue_And_Best_Profit_Can_Differ()
    {
        var source = new[]
        {
            Day(1, 50_000m, 5_000m),   // alto ingreso, margen bajo
            Day(2, 30_000m, 12_000m),  // menor ingreso, más ganancia
            Day(3, 20_000m, 4_000m)
        };

        var report = SalesByDayComposer.Build(
            source, ProfitPeriodKind.ThisMonth,
            new DateTime(2026, 8, 1), new DateTime(2026, 9, 1));

        Assert.Equal(3, report.OperatingDayCount);
        Assert.Equal(new DateTime(2026, 8, 1), report.BestDayByRevenue!.Date);
        Assert.Equal(50_000m, report.BestDayByRevenue.RevenueTotal);
        Assert.Equal(new DateTime(2026, 8, 2), report.BestDayByProfit!.Date);
        Assert.Equal(12_000m, report.BestDayByProfit.RealizedProfit);
        Assert.NotEqual(report.BestDayByRevenue.Date, report.BestDayByProfit.Date);
    }

    [Fact]
    public void Worst_Excludes_Zero_Txn_Days()
    {
        var source = new[]
        {
            Day(1, 40_000m, 8_000m),
            new ProfitDayRow
            {
                Date = new DateTime(2026, 8, 2),
                RevenueTotal = 0m,
                RealizedProfit = 0m,
                TransactionCount = 0,
                UnitsSold = 0
            },
            Day(3, 15_000m, 3_000m)
        };

        var report = SalesByDayComposer.Build(
            source, ProfitPeriodKind.Custom,
            new DateTime(2026, 8, 1), new DateTime(2026, 8, 4));

        Assert.Equal(2, report.OperatingDayCount);
        Assert.Equal(new DateTime(2026, 8, 3), report.WorstDayByRevenue!.Date);
        Assert.Equal(15_000m, report.WorstDayByRevenue.RevenueTotal);
        Assert.DoesNotContain(report.Days, d => d.TransactionCount == 0);
    }

    [Fact]
    public void Day_Includes_Ticket_And_Units()
    {
        var row = SalesByDayComposer.FromProfitDay(Day(5, 10_000m, 2_500m, units: 40, txns: 10));
        Assert.Equal(1_000m, row.AverageTicket);
        Assert.Equal(40, row.UnitsSold);
        Assert.Equal(10, row.TransactionCount);
    }

    [Fact]
    public void Empty_Has_Null_Extremes()
    {
        var report = SalesByDayComposer.Build(
            Array.Empty<ProfitDayRow>(), ProfitPeriodKind.Today,
            new DateTime(2026, 8, 20), new DateTime(2026, 8, 21));

        Assert.Equal(0, report.OperatingDayCount);
        Assert.Null(report.BestDayByRevenue);
        Assert.Null(report.WorstDayByProfit);
    }

    [Fact]
    public void Policy_Separates_Revenue_From_Profit_Best()
    {
        Assert.Contains("por separado", SalesByDayPolicy.Definition);
        Assert.Contains("excluyen", SalesByDayPolicy.ExcludeEmpty);
        Assert.Contains("GetByDay", SalesByDayPolicy.Source);
    }
}
