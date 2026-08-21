using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.20 — mix ventas ↔ estrellas / clases.</summary>
public class SalesStarMixTests
{
    private static ProductClassificationRow Row(
        int id, string name, ProductPerformanceClass cls,
        decimal revenue, decimal profit = 0m, int units = 0,
        string category = "Cat", bool stockout = false)
        => new()
        {
            ProductId = id,
            ProductName = name,
            Category = category,
            Class = cls,
            Trend = ProductTrendDirection.Growing,
            Performance = new ProductPerformanceRow
            {
                ProductId = id,
                ProductName = name,
                Category = category,
                RevenueTotal = revenue,
                RealizedProfit = profit,
                UnitsSold = units,
                FlagStockoutRisk = stockout,
                HasPeriodActivity = revenue > 0
            }
        };

    [Fact]
    public void Brief_Mix_Star_Healthy_Slow_Critical()
    {
        var rows = new[]
        {
            Row(1, "StarA", ProductPerformanceClass.Star, 40_000m, 12_000m, 40),
            Row(2, "HealthyA", ProductPerformanceClass.Healthy, 30_000m, 8_000m, 30),
            Row(3, "SlowA", ProductPerformanceClass.Slow, 20_000m, 2_000m, 10),
            Row(4, "CritA", ProductPerformanceClass.Critical, 10_000m, -500m, 5)
        };

        var report = SalesStarMixMath.BuildReport(rows, ProfitPeriodKind.ThisMonth);

        Assert.Equal(100_000m, report.TotalRevenue);
        Assert.Equal(40.00m, report.StarRevenueSharePct);
        Assert.Equal(30.00m, report.HealthyRevenueSharePct);
        Assert.Equal(20.00m, report.SlowRevenueSharePct);
        Assert.Equal(10.00m, report.CriticalRevenueSharePct);
        Assert.Equal(1, report.StarCount);
    }

    [Fact]
    public void TopStars_Ordered_By_Revenue()
    {
        var rows = new[]
        {
            Row(1, "StarLow", ProductPerformanceClass.Star, 10_000m),
            Row(2, "StarHigh", ProductPerformanceClass.Star, 50_000m),
            Row(3, "NotStar", ProductPerformanceClass.Healthy, 80_000m)
        };

        var report = SalesStarMixMath.BuildReport(rows, ProfitPeriodKind.ThisMonth);
        Assert.Equal(2, report.TopStars.Count);
        Assert.Equal("StarHigh", report.TopStars[0].ProductName);
        Assert.Equal(1, report.TopStars[0].Rank);
        // 50k / 140k
        Assert.Equal(35.71m, report.TopStars[0].RevenueSharePct);
    }

    [Fact]
    public void Star_With_Stockout_Listed()
    {
        var rows = new[]
        {
            Row(1, "StarRisk", ProductPerformanceClass.Star, 25_000m, stockout: true),
            Row(2, "StarOk", ProductPerformanceClass.Star, 15_000m, stockout: false)
        };

        var report = SalesStarMixMath.BuildReport(rows, ProfitPeriodKind.ThisMonth);
        Assert.Single(report.StarsWithStockoutRisk);
        Assert.Equal("StarRisk", report.StarsWithStockoutRisk[0].ProductName);
    }

    [Fact]
    public void Category_Mix_Shares()
    {
        var rows = new[]
        {
            Row(1, "A", ProductPerformanceClass.Star, 45_000m, category: "Suplementos"),
            Row(2, "B", ProductPerformanceClass.Healthy, 25_000m, category: "Bebidas"),
            Row(3, "C", ProductPerformanceClass.Slow, 20_000m, category: "Accesorios"),
            Row(4, "D", ProductPerformanceClass.New, 10_000m, category: "Otros")
        };

        var report = SalesStarMixMath.BuildReport(rows, ProfitPeriodKind.ThisMonth);
        Assert.Equal("Suplementos", report.CategoryMix[0].CategoryName);
        Assert.Equal(45.00m, report.CategoryMix[0].RevenueSharePct);
        Assert.Equal(4, report.CategoryMix.Count);
    }

    [Fact]
    public void Top_Seller_Not_Automatically_Star()
    {
        Assert.Contains("no ser Star", SalesStarMixPolicy.Separation);
        var rows = new[]
        {
            Row(1, "Volume", ProductPerformanceClass.Healthy, 90_000m),
            Row(2, "Star", ProductPerformanceClass.Star, 10_000m)
        };
        var report = SalesStarMixMath.BuildReport(rows, ProfitPeriodKind.ThisMonth);
        Assert.Equal(10.00m, report.StarRevenueSharePct);
        Assert.DoesNotContain(report.TopStars, s => s.ProductName == "Volume");
    }

    [Fact]
    public void Policy_Requires_Class_Mix()
    {
        Assert.Contains("§53", SalesStarMixPolicy.Definition);
        Assert.Contains("ProductClassification", SalesStarMixPolicy.Definition);
        Assert.Contains("§54", SalesStarMixPolicy.Category);
    }
}
