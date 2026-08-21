using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.9 — ticket promedio y variación.</summary>
public class SalesTicketTests
{
    private static SalesSummary S(decimal revenue, int txns, int units)
        => new()
        {
            RevenueTotal = revenue,
            TransactionCount = txns,
            UnitsSold = units,
            AverageTicket = SalesAnalyticsMath.AverageTicket(revenue, txns),
            UnitsPerTransaction = SalesAnalyticsMath.UnitsPerTransaction(units, txns)
        };

    [Fact]
    public void Brief_Example_Ticket_1000()
    {
        Assert.Equal(1_000m, SalesAnalyticsMath.AverageTicket(100_000m, 100));
        Assert.Equal(3.00m, SalesAnalyticsMath.UnitsPerTransaction(300, 100));
    }

    [Fact]
    public void Ticket_Variation_Plus20()
    {
        // Brief §23: 1200 vs 1000 → +20%
        var current = S(120_000m, 100, 300);
        var previous = S(100_000m, 100, 250);

        var report = SalesTicketComposer.Build(
            ProfitPeriodKind.ThisMonth,
            current,
            previous,
            new ProfitPeriodRange(new DateTime(2026, 8, 1), new DateTime(2026, 9, 1)),
            new ProfitPeriodRange(new DateTime(2026, 7, 1), new DateTime(2026, 8, 1)));

        Assert.Equal(1_200m, report.CurrentTicket);
        Assert.Equal(1_000m, report.PreviousTicket);
        Assert.Equal(20.00m, report.TicketVariationPct);
        Assert.Equal("+20.00 %", report.TicketLabel.Display);
        Assert.True(report.HasComparablePrevious);
    }

    [Fact]
    public void No_Previous_Is_Not_Comparable()
    {
        var report = SalesTicketComposer.Build(
            ProfitPeriodKind.AllTime,
            S(50_000m, 40, 80),
            previous: null,
            new ProfitPeriodRange(null, null),
            previousRange: null);

        Assert.False(report.HasComparablePrevious);
        Assert.Null(report.TicketVariationPct);
        Assert.Equal("N/D", report.TicketLabel.Display);
    }

    [Fact]
    public void Zero_Transactions_Null_Ticket()
    {
        var s = S(0m, 0, 0);
        Assert.Null(s.AverageTicket);
        Assert.Null(s.UnitsPerTransaction);
    }

    [Fact]
    public void Policy_Separates_Ticket_From_Collected()
    {
        Assert.Contains("≠ MontoPagado", SalesTicketPolicy.Definition);
        Assert.Contains("cross-sell", SalesTicketPolicy.UnitsPerTxn);
        Assert.Contains("N/D", SalesTicketPolicy.Comparison);
    }
}
