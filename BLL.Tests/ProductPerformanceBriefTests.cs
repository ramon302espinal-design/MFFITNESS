using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 8.19 — batería del brief (TEST 1–10).
/// Sin BD: composición + rankings + clasificación + tendencia.
/// Devoluciones = N/A en POS (igual que brief FASE 6/7).
/// </summary>
public class ProductPerformanceBriefTests
{
    private static ProfitGroupRow Profit(
        int id, string name, int units, decimal revenue, decimal profit, decimal cogs)
    {
        decimal revWithCost = revenue;
        return new ProfitGroupRow
        {
            ProductId = id,
            ProductName = name,
            GroupName = name,
            UnitsSold = units,
            RevenueTotal = revenue,
            RealizedProfit = profit,
            Cogs = cogs,
            RevenueWithCost = revWithCost,
            MarginPct = InventoryFinancialMath.MarginPct(profit, revWithCost),
            RoiPct = InventoryFinancialMath.RoiPct(profit, cogs),
            HasReliableRealizedProfit = cogs > 0
        };
    }

    private static InventoryFinancialRow Inv(
        int id, string name, decimal capital,
        InventoryHealthStatus health = InventoryHealthStatus.Healthy,
        bool stockout = false, int stock = 10, int? idle = null,
        decimal? turnover = 1.0m, decimal? upd = 1.0m)
        => new()
        {
            ProductId = id,
            ProductName = name,
            Stock = stock,
            InventoryCapital = capital,
            PotentialProfit = Math.Max(0m, capital * 0.25m),
            PotentialSalesValue = capital * 1.25m,
            HealthStatus = health,
            IdleDays = idle,
            FlagStockoutRisk = stockout,
            TurnoverProxy = turnover,
            UnitsPerDay = upd
        };

    private static ProductPerformanceRow Row(
        ProfitGroupRow? p, InventoryFinancialRow? i)
        => ProductPerformanceComposer.Compose(p, i);

    // TEST 1: A volumen; B ganancia y ROI
    [Fact]
    public void Test1_A_Volume_B_Profit_And_Roi()
    {
        // A: 100 uds, 100k ventas, 30k ganancia, ROI 40% → COGS 75k
        var a = Row(
            Profit(1, "A", 100, 100_000m, 30_000m, 75_000m),
            Inv(1, "A", 20_000m));
        // B: 50 uds, 80k ventas, 40k ganancia, ROI 80% → COGS 50k
        var b = Row(
            Profit(2, "B", 50, 80_000m, 40_000m, 50_000m),
            Inv(2, "B", 15_000m));

        Assert.Equal(40.00m, a.RoiPct);
        Assert.Equal(80.00m, b.RoiPct);

        var byUnits = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.UnitsSold);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.RealizedProfit);
        var byRoi = ProductPerformanceRanker.Rank(
            new[] { a, b }, ProductPerformanceMetricKind.RoiPct);

        Assert.Equal("A", byUnits[0].Row.ProductName);
        Assert.Equal("B", byProfit[0].Row.ProductName);
        Assert.Equal("B", byRoi[0].Row.ProductName);
    }

    // TEST 2: margen alto, ganancia baja ≠ top ganancia
    [Fact]
    public void Test2_HighMargin_LowProfit_Not_Top_Profit()
    {
        var highMargin = Row(
            Profit(1, "Niche", 5, 10_000m, 5_000m, 5_000m), // margen 50%
            Inv(1, "Niche", 2_000m));
        var lowMargin = Row(
            Profit(2, "Volume", 80, 100_000m, 15_000m, 85_000m), // margen 15%
            Inv(2, "Volume", 25_000m));

        Assert.True(highMargin.MarginPct > lowMargin.MarginPct);
        Assert.True(lowMargin.RealizedProfit > highMargin.RealizedProfit);

        var byMargin = ProductPerformanceRanker.Rank(
            new[] { highMargin, lowMargin }, ProductPerformanceMetricKind.MarginPct);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { highMargin, lowMargin }, ProductPerformanceMetricKind.RealizedProfit);

        Assert.Equal("Niche", byMargin[0].Row.ProductName);
        Assert.Equal("Volume", byProfit[0].Row.ProductName);
    }

    // TEST 3: alto volumen, bajo margen ≠ top margen
    [Fact]
    public void Test3_HighVolume_LowMargin_Not_Top_Margin()
    {
        var highVol = Row(
            Profit(1, "Bulk", 200, 200_000m, 10_000m, 190_000m), // margen ~5%
            Inv(1, "Bulk", 40_000m));
        var lowVol = Row(
            Profit(2, "Premium", 20, 40_000m, 16_000m, 24_000m), // margen 40%
            Inv(2, "Premium", 8_000m));

        var byUnits = ProductPerformanceRanker.Rank(
            new[] { highVol, lowVol }, ProductPerformanceMetricKind.UnitsSold);
        var byMargin = ProductPerformanceRanker.Rank(
            new[] { highVol, lowVol }, ProductPerformanceMetricKind.MarginPct);

        Assert.Equal("Bulk", byUnits[0].Row.ProductName);
        Assert.Equal("Premium", byMargin[0].Row.ProductName);
    }

    // TEST 4: producto nuevo — no Critical/Slow/Star
    [Fact]
    public void Test4_New_Product_Classified_As_New()
    {
        var neu = Row(
            null,
            Inv(1, "Nuevo", 8_000m, InventoryHealthStatus.New, idle: 3));

        var cls = ProductClassificationMath.Classify(neu, trend: null);
        Assert.Equal(ProductPerformanceClass.New, cls.Class);
        Assert.NotEqual(ProductPerformanceClass.Critical, cls.Class);
        Assert.NotEqual(ProductPerformanceClass.Slow, cls.Class);
        Assert.NotEqual(ProductPerformanceClass.Star, cls.Class);
    }

    // TEST 5: sin ventas — no inventar actividad de período
    [Fact]
    public void Test5_NoSales_No_Invented_Period_Activity()
    {
        var idle = Row(
            null,
            Inv(1, "SinVenta", 12_000m, InventoryHealthStatus.Healthy, idle: 10));

        Assert.Equal(0, idle.UnitsSold);
        Assert.False(idle.HasPeriodActivity);
        Assert.Equal(0m, idle.RealizedProfit);
        Assert.True(idle.HasInventorySnapshot);

        var cls = ProductClassificationMath.Classify(idle);
        Assert.NotEqual(ProductPerformanceClass.Star, cls.Class);
        Assert.NotEqual(ProductPerformanceClass.Opportunity, cls.Class);
    }

    // TEST 6: alto capital congelado
    [Fact]
    public void Test6_High_Immobilized_Capital_Ranks_First()
    {
        var frozen = Row(
            null,
            Inv(1, "Congelado", 50_000m, InventoryHealthStatus.Frozen, idle: 55));
        var healthy = Row(
            Profit(2, "Activo", 40, 60_000m, 12_000m, 48_000m),
            Inv(2, "Activo", 80_000m, InventoryHealthStatus.Healthy));

        Assert.True(frozen.IsImmobilized);
        Assert.Equal(50_000m, frozen.ImmobilizedCapital);
        Assert.False(healthy.IsImmobilized);

        var ranked = ProductPerformanceRanker.Rank(
            new[] { healthy, frozen }, ProductPerformanceMetricKind.ImmobilizedCapital);
        Assert.Single(ranked);
        Assert.Equal("Congelado", ranked[0].Row.ProductName);
    }

    // TEST 7: tendencia creciente
    [Fact]
    public void Test7_Growing_Trend()
    {
        var trend = ProductTrendMath.Compose(
            1, "Crece", "", unitsCurrent: 80, unitsPrevious: 40,
            revenueCurrent: 80_000m, revenuePrevious: 40_000m);

        Assert.Equal(ProductTrendDirection.Growing, trend.PrimaryTrend);
        Assert.Equal(100.00m, trend.UnitsChangePct);
    }

    // TEST 8: tendencia decreciente
    [Fact]
    public void Test8_Declining_Trend()
    {
        var trend = ProductTrendMath.Compose(
            1, "Cae", "", unitsCurrent: 20, unitsPrevious: 80,
            revenueCurrent: 20_000m, revenuePrevious: 80_000m);

        Assert.Equal(ProductTrendDirection.Declining, trend.PrimaryTrend);
        Assert.True(trend.UnitsChangePct < 0);
    }

    // TEST 9: estrella con stock crítico (quiebre) — alerta, no bloquea
    [Fact]
    public void Test9_Star_With_StockoutRisk_Warns_Does_Not_Block()
    {
        var starish = Row(
            Profit(1, "EstrellaStock", 60, 80_000m, 25_000m, 55_000m),
            Inv(1, "EstrellaStock", 5_000m, InventoryHealthStatus.Healthy,
                stockout: true, stock: 2, turnover: 2m, upd: 3m));

        Assert.True(ProductStarMath.TryBuildStar(starish, null, out var star));
        Assert.Equal(ProductPerformanceClass.Star, star.Class);
        Assert.Contains(star.Reasons, r => r.Contains("StockoutRisk", StringComparison.Ordinal));
        Assert.Contains("reabastecer", string.Join(" ", star.Reasons), StringComparison.OrdinalIgnoreCase);
    }

    // TEST 10: ROI alto + capital mínimo ≠ alto impacto
    [Fact]
    public void Test10_HighRoi_MinimalCapital_Is_Efficiency_Not_Impact()
    {
        // ROI 200% sobre COGS 500 → ganancia 1k; capital inventario mínimo
        var efficient = Row(
            Profit(1, "Micro", 3, 1_500m, 1_000m, 500m),
            Inv(1, "Micro", 500m));
        // ROI ~25%, ganancia 20k — más impacto
        var impact = Row(
            Profit(2, "Impacto", 100, 100_000m, 20_000m, 80_000m),
            Inv(2, "Impacto", 40_000m));

        Assert.True(efficient.RoiPct > impact.RoiPct);
        Assert.True(impact.RealizedProfit > efficient.RealizedProfit);
        Assert.True(impact.InventoryCapital > efficient.InventoryCapital);

        var byRoi = ProductPerformanceRanker.Rank(
            new[] { efficient, impact }, ProductPerformanceMetricKind.RoiPct);
        var byProfit = ProductPerformanceRanker.Rank(
            new[] { efficient, impact }, ProductPerformanceMetricKind.RealizedProfit);
        var byCapital = ProductPerformanceRanker.Rank(
            new[] { efficient, impact }, ProductPerformanceMetricKind.InventoryCapital);

        Assert.Equal("Micro", byRoi[0].Row.ProductName);
        Assert.Equal("Impacto", byProfit[0].Row.ProductName);
        Assert.Equal("Impacto", byCapital[0].Row.ProductName);

        Assert.Contains("≠", ProductPerformancePolicy.RoiProductDefinition);
        Assert.Contains("eficiencia", ProductPerformancePolicy.RoiRankingDefinition,
            StringComparison.OrdinalIgnoreCase);
    }
}
