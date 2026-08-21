using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.2 — contrato métricas base de ventas (sin BD).</summary>
public class SalesAnalyticsContractTests
{
    [Fact]
    public void Brief_Test1_Variation_Plus20()
    {
        Assert.Equal(20.00m, SalesAnalyticsMath.VariationPct(300_000m, 250_000m));
    }

    [Fact]
    public void Variation_PreviousZero_Is_Null()
    {
        Assert.Null(SalesAnalyticsMath.VariationPct(100m, 0m));
        Assert.Contains("Previous = 0", SalesAnalyticsPolicy.VariationDefinition);
    }

    [Fact]
    public void Ticket_And_UnitsPerTxn()
    {
        Assert.Equal(1_000m, SalesAnalyticsMath.AverageTicket(100_000m, 100));
        Assert.Equal(3.00m, SalesAnalyticsMath.UnitsPerTransaction(300, 100));
        Assert.Null(SalesAnalyticsMath.AverageTicket(50_000m, 0));
    }

    [Fact]
    public void Average_Vs_Median_ExtremeDay()
    {
        // Brief TEST 7 precursor: extremos distorsionan promedio, no mediana
        var days = new[] { 10_000m, 10_000m, 10_000m, 10_000m, 100_000m };
        Assert.Equal(28_000m, SalesAnalyticsMath.Average(days));
        Assert.Equal(10_000m, SalesAnalyticsMath.Median(days));
    }

    [Fact]
    public void Composer_Separates_Metrics_From_ProfitSummary()
    {
        var profit = new ProfitSummary
        {
            TransactionCount = 100,
            UnitsSold = 300,
            RevenueTotal = 100_000m,
            SalesHeaderTotal = 100_000m,
            RealizedProfit = 24_000m,
            Cogs = 76_000m,
            RevenueWithCost = 100_000m,
            MarginPct = 24m,
            RoiPct = InventoryFinancialMath.RoiPct(24_000m, 76_000m),
            HasReliableRealizedProfit = true,
            CollectedAtSale = 90_000m
        };

        var sales = SalesAnalyticsComposer.FromProfitSummary(profit, ProfitPeriodKind.ThisMonth);

        Assert.Equal(100, sales.TransactionCount);
        Assert.Equal(300, sales.UnitsSold);
        Assert.Equal(100_000m, sales.RevenueTotal);
        Assert.Equal(24_000m, sales.RealizedProfit);
        Assert.Equal(1_000m, sales.AverageTicket);
        Assert.Equal(3.00m, sales.UnitsPerTransaction);
        Assert.Equal(24m, sales.MarginPct);
        Assert.NotEqual(sales.RevenueTotal, sales.CollectedAtSale);
    }

    [Fact]
    public void Policy_Separates_All_Six_Concepts()
    {
        Assert.Contains("≠", SalesAnalyticsPolicy.SeparationRule);
        Assert.Contains("TRANSACCIONES", SalesAnalyticsPolicy.TransactionsDefinition);
        Assert.Contains("Subtotal", SalesAnalyticsPolicy.RevenueDefinition);
        Assert.Contains("≠ ROI", SalesAnalyticsPolicy.MarginDefinition);
        Assert.Contains("≠ ROI inversión", SalesAnalyticsPolicy.RoiProductDefinition);
        Assert.Contains("DELETE", SalesAnalyticsPolicy.VoidReturnNote);
        Assert.Contains("ESTIMACIÓN", SalesAnalyticsPolicy.ForecastLanguage);
    }

    [Fact]
    public void Share_And_AveragePerDay()
    {
        Assert.Equal(20.00m, SalesAnalyticsMath.SharePct(100_000m, 500_000m));
        Assert.Equal(10_000m, SalesAnalyticsMath.AveragePerDay(300_000m, 30));
        Assert.Null(SalesAnalyticsMath.SharePct(10m, 0m));
    }
}
