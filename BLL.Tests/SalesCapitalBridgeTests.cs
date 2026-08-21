using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.19 — puente ventas ↔ capital.</summary>
public class SalesCapitalBridgeTests
{
    private static ProductPerformanceRow Perf(
        int id, string name,
        decimal revenue, decimal profit, decimal? roi,
        decimal capital, decimal immobilized = 0m,
        InventoryHealthStatus health = InventoryHealthStatus.Healthy,
        bool overstock = false, bool stockout = false)
        => new()
        {
            ProductId = id,
            ProductName = name,
            RevenueTotal = revenue,
            RealizedProfit = profit,
            RoiPct = roi,
            InventoryCapital = capital,
            ImmobilizedCapital = immobilized,
            HealthStatus = health,
            FlagOverstock = overstock,
            FlagStockoutRisk = stockout,
            Stock = 10,
            HasInventorySnapshot = true
        };

    [Fact]
    public void Brief_Revenue_Up_Roi_Down()
    {
        var row = SalesCapitalBridgeMath.Compose(
            Perf(1, "SKU", revenue: 120_000m, profit: 20_000m, roi: 15m, capital: 50_000m),
            revenueChangePct: 20m,
            profitChangePct: 5m,
            roiChangePct: -8m);

        Assert.Contains(row.Signals, s => s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown);
        Assert.Equal(SalesCapitalSignalKind.RevenueUpRoiDown, row.PrimarySignal);
        Assert.Contains("ROI", row.Signals[0].Message);
    }

    [Fact]
    public void Revenue_Up_Profit_Down()
    {
        var row = SalesCapitalBridgeMath.Compose(
            Perf(1, "SKU", 120_000m, 19_000m, 20m, 40_000m),
            revenueChangePct: 20m,
            profitChangePct: -5m,
            roiChangePct: 0m);

        Assert.Contains(row.Signals, s => s.Kind == SalesCapitalSignalKind.RevenueUpProfitDown);
    }

    [Fact]
    public void Capital_Risk_Declining_Immobilized()
    {
        var row = SalesCapitalBridgeMath.Compose(
            Perf(1, "Lento", 5_000m, 500m, 5m, 40_000m, immobilized: 40_000m,
                health: InventoryHealthStatus.Frozen, overstock: true),
            trend: ProductTrendDirection.Declining);

        Assert.Equal(SalesCapitalSignalKind.CapitalRisk, row.PrimarySignal);
        Assert.True(row.IsImmobilized);
    }

    [Fact]
    public void Stockout_With_Capital()
    {
        var row = SalesCapitalBridgeMath.Compose(
            Perf(1, "Quiebre", 8_000m, 2_000m, 30m, 12_000m, stockout: true),
            stockSignal: SalesStockSignalKind.StockoutRisk);

        Assert.Contains(row.Signals, s => s.Kind == SalesCapitalSignalKind.StockoutWithCapital);
    }

    [Fact]
    public void Report_CapitalAtRisk_And_Efficiency()
    {
        var ok = SalesCapitalBridgeMath.Compose(
            Perf(1, "Ok", 50_000m, 10_000m, 25m, 20_000m),
            trend: ProductTrendDirection.Growing);
        var risk = SalesCapitalBridgeMath.Compose(
            Perf(2, "Risk", 2_000m, 100m, 2m, 30_000m, immobilized: 30_000m, overstock: true),
            trend: ProductTrendDirection.Declining);

        var report = SalesCapitalBridgeMath.BuildReport(new[] { ok, risk }, ProfitPeriodKind.ThisMonth);

        Assert.Equal(50_000m, report.TotalInventoryCapital);
        Assert.Equal(30_000m, report.TotalImmobilizedCapital);
        Assert.Equal(30_000m, report.CapitalAtRisk);
        Assert.Equal(1, report.CapitalRiskCount);
        // profit = 10000+100=10100 → 10100/50000*100 = 20.20
        Assert.Equal(20.20m, report.PeriodProfitOnInventoryCapitalPct);
        Assert.Contains("≠ RoiPct", report.Caution);
    }

    [Fact]
    public void Policy_Separates_Capitals()
    {
        Assert.Contains("InventoryCapital ≠ ImmobilizedCapital", SalesCapitalBridgePolicy.Definition);
        Assert.Contains("§52", SalesCapitalBridgePolicy.RevenueUpRoi);
        Assert.Contains("CapitalAtRisk", SalesCapitalBridgePolicy.CapitalRisk);
        Assert.Contains("≠ ROI de inversión", SalesCapitalBridgePolicy.Separation);
    }
}
