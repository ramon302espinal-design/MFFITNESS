using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.21 — dashboard de ventas (señales).</summary>
public class SalesDashboardTests
{
    private static SalesSummary Summary()
        => new()
        {
            TransactionCount = 100,
            UnitsSold = 500,
            RevenueTotal = 300_000m,
            RealizedProfit = 80_000m,
            MarginPct = 26m,
            AverageTicket = 3_000m,
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Build_Exposes_Kpis_And_Forecast_As_Estimate()
    {
        var forecast = SalesForecastMath.Build(
            Enumerable.Repeat(10_000m, 20).ToArray(),
            30,
            new SalesSeriesTrendResult
            {
                Kind = SalesSeriesTrendKind.Stable,
                PointCount = 20,
                CoefficientOfVariationPct = 5m
            });

        var dash = SalesDashboardComposer.Build(
            ProfitPeriodKind.ThisMonth,
            Summary(),
            forecast: forecast);

        Assert.Equal(300_000m, dash.RevenueTotal);
        Assert.Equal(100, dash.TransactionCount);
        Assert.NotNull(dash.ForecastBaseRevenue);
        Assert.Contains("ESTIMACIÓN", dash.ForecastNote);
    }

    [Fact]
    public void Alerts_Strong_Growth_And_Cross_Signals()
    {
        var cmp = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            new SalesSummary
            {
                RevenueTotal = 120_000m,
                RealizedProfit = 19_000m,
                UnitsSold = 100,
                TransactionCount = 50,
                AverageTicket = 2_400m,
                MarginPct = 15m,
                HasReliableRealizedProfit = true
            },
            new SalesSummary
            {
                RevenueTotal = 100_000m,
                RealizedProfit = 20_000m,
                UnitsSold = 90,
                TransactionCount = 50,
                AverageTicket = 2_000m,
                MarginPct = 20m,
                HasReliableRealizedProfit = true
            });

        var variations = SalesVariationMath.FromComparison(cmp);
        var alerts = SalesDashboardComposer.BuildAlerts(variations, null, null, null, null);

        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.StrongGrowth);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.RevenueUpProfitDown);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.RevenueUpMarginDown);
    }

    [Fact]
    public void Alerts_Deceleration_And_Stockout()
    {
        var accel = SalesAccelerationMath.ClassifyFromChangePcts(new[] { 40m, 20m, 5m });
        var stock = SalesStockRiskMath.Compose(
            new[]
            {
                SalesStockRiskMath.Classify(
                    1, "A", "", 5, 10, 2m, 1m, true, false, false)
            },
            ProfitPeriodKind.ThisMonth);

        var alerts = SalesDashboardComposer.BuildAlerts(null, accel, stock, null, null);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.Deceleration);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.StockoutRisk);
    }

    [Fact]
    public void Alerts_Roi_Down_From_Capital_Bridge()
    {
        var capital = SalesCapitalBridgeMath.BuildReport(
            new[]
            {
                SalesCapitalBridgeMath.Compose(
                    new ProductPerformanceRow
                    {
                        ProductId = 1,
                        ProductName = "X",
                        RevenueTotal = 10_000m,
                        InventoryCapital = 5_000m,
                        RoiPct = 10m
                    },
                    revenueChangePct: 20m,
                    roiChangePct: -10m)
            },
            ProfitPeriodKind.ThisMonth);

        var alerts = SalesDashboardComposer.BuildAlerts(null, null, null, capital, null);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.RoiDown);
    }

    [Fact]
    public void Tops_Pass_Through()
    {
        var tops = new[]
        {
            new SalesDashboardTopItem { Rank = 1, Name = "Whey", Amount = 50_000m, SharePct = 25m }
        };
        var dash = SalesDashboardComposer.Build(
            ProfitPeriodKind.Last30Days,
            Summary(),
            topProducts: tops);

        Assert.Single(dash.TopProducts);
        Assert.Equal("Whey", dash.TopProducts[0].Name);
    }

    [Fact]
    public void Policy_No_Logic_In_Forms()
    {
        Assert.Contains("Sin lógica financiera en Forms", SalesDashboardPolicy.Definition);
        Assert.Contains("No ejecutan acciones", SalesDashboardPolicy.Alerts);
        Assert.Contains("ESTIMACIÓN", SalesDashboardPolicy.Forecast);
    }
}
