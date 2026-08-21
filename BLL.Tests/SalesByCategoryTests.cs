using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.11 — ventas por categoría.</summary>
public class SalesByCategoryTests
{
    private static ProfitGroupRow Cat(
        int id, string name, int units, decimal revenue, decimal profit, int txns)
        => new()
        {
            CategoryId = id,
            GroupName = name,
            UnitsSold = units,
            RevenueTotal = revenue,
            RealizedProfit = profit,
            TransactionCount = txns,
            RevenueWithCost = revenue,
            Cogs = revenue - profit,
            MarginPct = InventoryFinancialMath.MarginPct(profit, revenue),
            RoiPct = InventoryFinancialMath.RoiPct(profit, revenue - profit),
            HasReliableRealizedProfit = true
        };

    private static ProfitGroupRow Prod(int catId, int units)
        => new()
        {
            ProductId = units,
            CategoryId = catId,
            GroupName = "P",
            UnitsSold = units,
            RevenueTotal = units * 100m,
            TransactionCount = 1
        };

    [Fact]
    public void Best_Revenue_Category_May_Differ_From_Best_Profit()
    {
        var cats = new[]
        {
            Cat(1, "Suplementos", 100, 200_000m, 20_000m, 80),
            Cat(2, "Accesorios", 40, 80_000m, 32_000m, 30)
        };

        var report = SalesByCategoryComposer.Build(cats, periodKind: ProfitPeriodKind.ThisMonth);

        Assert.Equal("Suplementos", report.Categories[0].CategoryName);
        Assert.True(report.Categories[1].MarginPct > report.Categories[0].MarginPct);
        Assert.Equal(200_000m / 280_000m * 100m, report.Categories[0].RevenueSharePct!.Value, 2);
    }

    [Fact]
    public void Category_Trend_Growing()
    {
        var current = new[] { Prod(1, 80), Prod(1, 20) };  // 100 uds
        var previous = new[] { Prod(1, 40), Prod(1, 10) }; // 50 uds

        var (dir, pct) = SalesByCategoryComposer.CategoryTrend(1, current, previous);
        Assert.Equal(ProductTrendDirection.Growing, dir);
        Assert.Equal(100.00m, pct);
    }

    [Fact]
    public void Build_Attaches_Trend()
    {
        var cats = new[] { Cat(1, "Bebidas", 50, 50_000m, 10_000m, 25) };
        var cur = new[] { Prod(1, 50) };
        var prev = new[] { Prod(1, 100) };

        var report = SalesByCategoryComposer.Build(cats, cur, prev, ProfitPeriodKind.Last30Days);
        Assert.Equal(ProductTrendDirection.Declining, report.Categories[0].Trend);
        Assert.True(report.Categories[0].UnitsChangePct < 0);
    }

    [Fact]
    public void Ticket_Per_Category()
    {
        var row = SalesByCategoryComposer.Compose(
            Cat(1, "X", 30, 30_000m, 6_000m, 15),
            totalRevenue: 100_000m,
            totalProfit: 20_000m,
            ProductTrendDirection.Stable,
            0m,
            rank: 1);

        Assert.Equal(2_000m, row.AverageTicket);
        Assert.Equal(30.00m, row.RevenueSharePct);
    }

    [Fact]
    public void Policy_Separates_Revenue_From_Profit()
    {
        Assert.Contains("≠", SalesByCategoryPolicy.Definition);
        Assert.Contains("MoM", SalesByCategoryPolicy.TrendNote);
    }
}
