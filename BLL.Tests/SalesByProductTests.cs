using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.10 — ventas por producto + FASE 8.</summary>
public class SalesByProductTests
{
    private static ProfitGroupRow P(
        int id, string name, int units, decimal revenue, decimal profit,
        int txns, decimal cogs)
        => new()
        {
            ProductId = id,
            ProductName = name,
            GroupName = name,
            UnitsSold = units,
            RevenueTotal = revenue,
            RealizedProfit = profit,
            TransactionCount = txns,
            Cogs = cogs,
            RevenueWithCost = revenue,
            MarginPct = InventoryFinancialMath.MarginPct(profit, revenue),
            RoiPct = InventoryFinancialMath.RoiPct(profit, cogs),
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Compose_Ticket_Share_Trend_Class()
    {
        var profit = P(1, "Whey", 30, 60_000m, 18_000m, txns: 20, cogs: 42_000m);
        var trend = ProductTrendMath.Compose(
            1, "Whey", "Suplementos", 30, 20, 60_000m, 40_000m);
        var cls = new ProductClassificationRow
        {
            ProductId = 1,
            ProductName = "Whey",
            Category = "Suplementos",
            Class = ProductPerformanceClass.Star,
            Reasons = new[] { "Impacto", "Eficiencia", "Bajo riesgo" }
        };

        var row = SalesByProductComposer.Compose(
            profit, totalRevenue: 300_000m, trend, cls, rank: 1);

        Assert.Equal(3_000m, row.AverageTicket);
        Assert.Equal(1.50m, row.UnitsPerTransaction);
        Assert.Equal(20.00m, row.RevenueSharePct);
        Assert.Equal(ProductTrendDirection.Growing, row.Trend);
        Assert.Equal(ProductPerformanceClass.Star, row.PerformanceClass);
        Assert.Equal("Suplementos", row.Category);
    }

    [Fact]
    public void Build_Orders_By_Revenue()
    {
        var products = new[]
        {
            P(1, "A", 10, 40_000m, 8_000m, 5, 32_000m),
            P(2, "B", 20, 80_000m, 10_000m, 15, 70_000m)
        };

        var report = SalesByProductComposer.Build(
            products,
            new Dictionary<int, ProductTrendRow>(),
            new Dictionary<int, ProductClassificationRow>(),
            ProfitPeriodKind.ThisMonth);

        Assert.Equal(2, report.ProductCount);
        Assert.Equal("B", report.Products[0].ProductName);
        Assert.Equal(1, report.Products[0].Rank);
        Assert.Equal(120_000m, report.TotalRevenue);
    }

    [Fact]
    public void Metrics_Remain_Separate()
    {
        var a = P(1, "Volumen", 100, 100_000m, 10_000m, 50, 90_000m);
        var b = P(2, "Margen", 20, 40_000m, 16_000m, 15, 24_000m);

        var report = SalesByProductComposer.Build(
            new[] { a, b },
            new Dictionary<int, ProductTrendRow>(),
            new Dictionary<int, ProductClassificationRow>(),
            ProfitPeriodKind.Last30Days);

        Assert.Equal("Volumen", report.Products[0].ProductName); // top revenue
        Assert.True(report.Products[1].MarginPct > report.Products[0].MarginPct);
    }

    [Fact]
    public void Policy_Integrates_Fase8()
    {
        Assert.Contains("Clase FASE 8", SalesByProductPolicy.Definition);
        Assert.Contains("DISTINCT VentaId", SalesByProductPolicy.TicketNote);
        Assert.Contains("no recalcular estrella", SalesByProductPolicy.Fase8);
    }
}
