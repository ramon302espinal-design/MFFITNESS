using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 9.23 — batería del brief (TEST 1–10).
/// Sin BD: comparación, variaciones, series, estacionalidad, stock, forecast.
/// Devoluciones = N/A en POS (anulación = DELETE).
/// </summary>
public class SalesAnalyticsBriefTests
{
    private static SalesSummary Summary(
        decimal revenue, decimal profit, int units = 100, int txns = 50,
        decimal? margin = null)
        => new()
        {
            RevenueTotal = revenue,
            RealizedProfit = profit,
            UnitsSold = units,
            TransactionCount = txns,
            AverageTicket = SalesAnalyticsMath.AverageTicket(revenue, txns),
            MarginPct = margin,
            HasReliableRealizedProfit = true
        };

    // TEST 1: 300k vs 250k → +20%
    [Fact]
    public void Test1_Variation_Plus20()
    {
        var report = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(new DateTime(2026, 8, 1), new DateTime(2026, 9, 1)),
            new ProfitPeriodRange(new DateTime(2026, 7, 1), new DateTime(2026, 8, 1)),
            Summary(300_000m, 80_000m),
            Summary(250_000m, 70_000m));

        Assert.Equal(20.00m, report.Revenue.VariationPct);
        Assert.Equal("+20.00 %", SalesVariationMath.FromComparison(report).Revenue.Display);
    }

    // TEST 2: Ventas +20% / Ganancia -5% → alerta
    [Fact]
    public void Test2_RevenueUp_ProfitDown_Alert()
    {
        var cmp = SalesComparisonComposer.Build(
            ProfitPeriodKind.Last30Days,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            Summary(120_000m, 19_000m),
            Summary(100_000m, 20_000m));

        var variations = SalesVariationMath.FromComparison(cmp);
        Assert.Contains(variations.CrossSignals,
            s => s.Kind == SalesCrossSignalKind.RevenueUpProfitDown);

        var alerts = SalesDashboardComposer.BuildAlerts(variations, null, null, null, null);
        Assert.Contains(alerts, a => a.Kind == SalesDashboardAlertKind.RevenueUpProfitDown);
    }

    // TEST 3: Ventas +20% / Margen -10% → señal
    [Fact]
    public void Test3_RevenueUp_MarginDown_Signal()
    {
        var cmp = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            Summary(120_000m, 18_000m, margin: 15m),
            Summary(100_000m, 25_000m, margin: 25m));

        // margin 15 vs 25 = -40% relative; brief says -10pp conceptually — signal on direction
        var variations = SalesVariationMath.FromComparison(cmp);
        Assert.Equal(20.00m, variations.Revenue.VariationPct);
        Assert.True(variations.Margin!.VariationPct < 0);
        Assert.Contains(variations.CrossSignals,
            s => s.Kind == SalesCrossSignalKind.RevenueUpMarginDown);

        var decision = SalesDecisionMath.FromRevenueUpMarginDown(20m, -10m);
        Assert.NotNull(decision);
        Assert.Contains("margen cayó", decision!.Message);
    }

    // TEST 4: Ventas -30% + stock alto → riesgo de capital
    [Fact]
    public void Test4_Decline_High_Stock_Capital_Risk()
    {
        var row = SalesStockRiskMath.Classify(
            1, "SobreInv", "Cat",
            stock: 200, stockMinimo: 5,
            unitsPerDay: 1m, turnoverProxy: 0.1m,
            flagStockoutRisk: false, flagOverstock: true, isImmobilized: true,
            trend: ProductTrendDirection.Declining);

        Assert.Equal(SalesStockSignalKind.CapitalRisk, row.PrimarySignal);

        var capital = SalesCapitalBridgeMath.Compose(
            new ProductPerformanceRow
            {
                ProductId = 1,
                ProductName = "SobreInv",
                RevenueTotal = 7_000m,
                InventoryCapital = 40_000m,
                ImmobilizedCapital = 40_000m,
                FlagOverstock = true,
                HealthStatus = InventoryHealthStatus.Frozen
            },
            revenueChangePct: -30m,
            trend: ProductTrendDirection.Declining);

        Assert.Equal(SalesCapitalSignalKind.CapitalRisk, capital.PrimarySignal);
    }

    // TEST 5: Ventas crecientes + stock bajo → quiebre
    [Fact]
    public void Test5_Growing_Low_Stock_Stockout()
    {
        var row = SalesStockRiskMath.Classify(
            1, "Hot", "Cat",
            stock: 5, stockMinimo: 15,
            unitsPerDay: 2m, turnoverProxy: 2m,
            flagStockoutRisk: true, flagOverstock: false, isImmobilized: false,
            trend: ProductTrendDirection.Growing);

        Assert.Contains(SalesStockSignalKind.StockoutRisk, row.Signals);
        Assert.True(row.DemandExceedsStock || row.FlagStockoutRisk);
        Assert.Contains(SalesStockSignalKind.ReplenishmentOpportunity, row.Signals);
    }

    // TEST 6: Datos insuficientes → no tendencia inventada
    [Fact]
    public void Test6_Insufficient_No_Trend()
    {
        var trend = SalesSeriesTrendMath.Classify(new[] { 10_000m, 12_000m });
        Assert.Equal(SalesSeriesTrendKind.InsufficientData, trend.Kind);

        var mom = ProductTrendMath.Classify(0, 0);
        Assert.Equal(ProductTrendDirection.InsufficientData, mom);
    }

    // TEST 7: Días extremos — promedio vs mediana
    [Fact]
    public void Test7_Average_Vs_Median_Extreme_Days()
    {
        // 1k, 1k, 1k, 100k → promedio distorsionado, mediana estable
        var values = new[] { 1_000m, 1_000m, 1_000m, 100_000m };
        var stats = SalesSeriesStatsComposer.FromValues(values);

        Assert.Equal(25_750m, stats.Average);
        Assert.Equal(1_000m, stats.Median);
        Assert.NotEqual(stats.Average, stats.Median);
    }

    // TEST 8: Mes actual vs mismo mes año anterior (estacional)
    [Fact]
    public void Test8_Seasonal_Same_Month_YoY()
    {
        var asOf = new DateTime(2026, 8, 20);
        var (cur, prior) = SalesSeasonalityMath.ResolveSameMonthYoY(asOf);

        Assert.Equal(new DateTime(2026, 8, 1), cur.From);
        Assert.Equal(new DateTime(2025, 8, 1), prior.From);

        var mom = ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisMonth, asOf)!.Value;
        Assert.NotEqual(prior.From, mom.Previous.From);
        Assert.Equal(new DateTime(2026, 7, 1), mom.Previous.From);
    }

    // TEST 9: Producto ventas↑ margen↓
    [Fact]
    public void Test9_Product_Revenue_Up_Margin_Down()
    {
        var signal = SalesCapitalBridgeMath.Compose(
            new ProductPerformanceRow
            {
                ProductId = 1,
                ProductName = "SKU",
                RevenueTotal = 120_000m,
                RealizedProfit = 18_000m,
                MarginPct = 15m,
                RoiPct = 20m,
                InventoryCapital = 30_000m
            },
            revenueChangePct: 25m,
            profitChangePct: -5m,
            roiChangePct: -8m);

        Assert.Contains(signal.Signals, s => s.Kind == SalesCapitalSignalKind.RevenueUpProfitDown);
        Assert.Contains(signal.Signals, s => s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown);

        var decision = SalesDecisionMath.FromRevenueUpMarginDown(20m, -6m);
        Assert.NotNull(decision);
        Assert.Contains("aumentaron +20%", decision!.Message);
    }

    // TEST 10: Forecast low / base / high
    [Fact]
    public void Test10_Forecast_Low_Base_High()
    {
        var report = SalesForecastMath.Build(
            Enumerable.Repeat(10_000m, 20).ToArray(),
            horizonDays: 30,
            new SalesSeriesTrendResult
            {
                Kind = SalesSeriesTrendKind.Stable,
                PointCount = 20,
                CoefficientOfVariationPct = 5m
            });

        Assert.True(report.Low.EstimatedRevenue < report.Base.EstimatedRevenue);
        Assert.True(report.Base.EstimatedRevenue < report.High.EstimatedRevenue);
        Assert.Contains("estimación", report.Base.Label, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ESTIMACIÓN", report.LanguageNote);
        Assert.DoesNotContain("certeza", report.Base.Label, StringComparison.OrdinalIgnoreCase);
    }
}
