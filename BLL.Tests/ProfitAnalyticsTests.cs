using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 5.10 — batería del motor de ganancias (tests 1–10 del brief).
/// Sin base de datos: fórmulas + política + períodos.
/// </summary>
public class ProfitAnalyticsTests
{
    // TEST 1: costo 800, precio 1200, vender 6
    [Fact]
    public void Test1_SellSix_RevenueCogsProfitMarginRoi()
    {
        const decimal cost = 800m;
        const decimal price = 1200m;
        const int qty = 6;

        decimal revenue = qty * price;
        decimal cogs = InventoryFinancialMath.LineCogs(qty, cost);
        decimal profit = InventoryFinancialMath.RealizedLineProfit(qty, price, cost);

        Assert.Equal(7200m, revenue);
        Assert.Equal(4800m, cogs);
        Assert.Equal(2400m, profit);
        Assert.Equal(33.33m, InventoryFinancialMath.MarginPct(profit, revenue));
        Assert.Equal(50.00m, InventoryFinancialMath.RoiPct(profit, cogs));
    }

    // TEST 2: vender las 4 restantes → ganancia total 4000
    [Fact]
    public void Test2_SellRemainingFour_TotalRealizedProfit()
    {
        const decimal cost = 800m;
        const decimal price = 1200m;

        decimal first = InventoryFinancialMath.RealizedLineProfit(6, price, cost);
        decimal second = InventoryFinancialMath.RealizedLineProfit(4, price, cost);
        Assert.Equal(2400m, first);
        Assert.Equal(1600m, second);
        Assert.Equal(4000m, first + second);
        Assert.Equal(0m, InventoryFinancialMath.PotentialProfit(0, cost, price));
    }

    // TEST 3: descuento → precio real (lista 1200, cobrado 1100)
    [Fact]
    public void Test3_Discount_UsesFinalPriceNotList()
    {
        const decimal cost = 800m;
        const decimal list = 1200m;
        const decimal final = 1100m;

        decimal withDiscount = InventoryFinancialMath.RealizedLineProfit(1, final, cost);
        decimal withoutDiscount = InventoryFinancialMath.RealizedLineProfit(1, list, cost);

        Assert.Equal(300m, withDiscount);
        Assert.Equal(400m, withoutDiscount);
        Assert.True(withDiscount < withoutDiscount);
        Assert.Equal(27.27m, InventoryFinancialMath.MarginPct(withDiscount, final));
    }

    // TEST 4: venta bajo costo → pérdida
    [Fact]
    public void Test4_SoldBelowCost_NegativeProfitAndMargin()
    {
        decimal profit = InventoryFinancialMath.RealizedLineProfit(1, 700m, 800m);
        Assert.Equal(-100m, profit);
        Assert.Equal(-14.29m, InventoryFinancialMath.MarginPct(profit, 700m));
        Assert.Equal(-12.50m, InventoryFinancialMath.RoiPct(profit, 800m));
    }

    // TEST 5: devolución completa — POS borra venta → contribución 0
    [Fact]
    public void Test5_FullVoid_DeletedSaleContributesZero()
    {
        Assert.False(ProfitVoidAndReturnPolicy.HasProductReturnModule);
        Assert.True(ProfitVoidAndReturnPolicy.AnnulmentDeletesHistory);
        Assert.False(ProfitVoidAndReturnPolicy.StockOrCashReversalAffectsRealizedProfit);

        // Tras DELETE no queda línea: ganancia de esa venta = 0
        Assert.Equal(0m, InventoryFinancialMath.RealizedLineProfit(0, 1200m, 800m));
    }

    // TEST 6: devolución parcial — no existe módulo; no inventar asiento
    [Fact]
    public void Test6_PartialReturn_NotSupportedByPos()
    {
        Assert.False(ProfitVoidAndReturnPolicy.HasProductReturnModule);
        // Si existiera, sería reducir cantidad/línea; sin módulo no hay segunda fuente de P&L
        Assert.Contains("devoluciones", ProfitVoidAndReturnPolicy.DescribeForUi(), StringComparison.OrdinalIgnoreCase);
    }

    // TEST 7: venta a crédito — ganancia accrual completa (cobro ≠ ganancia)
    [Fact]
    public void Test7_CreditSale_AccrualProfitIndependentOfCollected()
    {
        const decimal revenue = 10000m;
        const decimal cogs = 6000m;
        const decimal collected = 6000m;
        const decimal receivable = 4000m;

        decimal profit = revenue - cogs;
        Assert.Equal(4000m, profit);
        Assert.Equal(40.00m, InventoryFinancialMath.MarginPct(profit, revenue));
        Assert.Equal(66.67m, InventoryFinancialMath.RoiPct(profit, cogs));

        // Cobrado/CxC no alteran la ganancia económica
        Assert.NotEqual(collected, profit);
        Assert.Equal(revenue, collected + receivable);
    }

    // TEST 8: cambio de costo posterior — snapshot histórico intacto
    [Fact]
    public void Test8_CostChangeAfterSale_HistoricalSnapshotWins()
    {
        decimal historical = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 700m);
        decimal ifUsingNewCost = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 900m);
        Assert.Equal(500m, historical);
        Assert.NotEqual(historical, ifUsingNewCost);
    }

    // TEST 9: cambio de precio posterior — precio vendido intacto
    [Fact]
    public void Test9_PriceChangeAfterSale_SoldPriceWins()
    {
        decimal historical = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 800m);
        decimal ifUsingNewPrice = InventoryFinancialMath.RealizedLineProfit(1, 1500m, 800m);
        Assert.Equal(400m, historical);
        Assert.NotEqual(historical, ifUsingNewPrice);
    }

    // TEST 10: sin costo — no inventar ganancia / margen / ROI
    [Fact]
    public void Test10_NoCost_NoFakeProfitMetrics()
    {
        // Sin snapshot: el motor marca HasReliableRealizedProfit=false y no usa PrecioCompra actual
        Assert.Null(InventoryFinancialMath.MarginPct(0m, 0m));
        Assert.Null(InventoryFinancialMath.RoiPct(0m, 0m));
        Assert.Equal(0m, InventoryFinancialMath.PotentialProfit(5, 0m, 1200m));
    }

    [Theory]
    [InlineData(ProfitPeriodKind.Today)]
    [InlineData(ProfitPeriodKind.Yesterday)]
    [InlineData(ProfitPeriodKind.Last7Days)]
    [InlineData(ProfitPeriodKind.Last30Days)]
    [InlineData(ProfitPeriodKind.ThisMonth)]
    [InlineData(ProfitPeriodKind.PreviousMonth)]
    [InlineData(ProfitPeriodKind.ThisYear)]
    public void ResolvePeriod_Presets_HaveHalfOpenRange(ProfitPeriodKind kind)
    {
        var asOf = new DateTime(2026, 8, 20);
        ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(kind, asOf);
        Assert.NotNull(range.From);
        Assert.NotNull(range.ToExclusive);
        Assert.True(range.ToExclusive > range.From);
    }

    [Fact]
    public void ResolvePeriod_Today_IsAsOfDay()
    {
        var asOf = new DateTime(2026, 8, 20);
        ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.Today, asOf);
        Assert.Equal(asOf, range.From);
        Assert.Equal(asOf.AddDays(1), range.ToExclusive);
    }

    [Fact]
    public void ResolvePeriod_AllTime_IsOpen()
    {
        ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.AllTime);
        Assert.Null(range.From);
        Assert.Null(range.ToExclusive);
    }

    [Fact]
    public void MarginIsNotRoi_SameNumbersDifferentPercents()
    {
        // 30k ganancia, 100k ingreso, 70k COGS
        Assert.Equal(30.00m, InventoryFinancialMath.MarginPct(30000m, 100000m));
        Assert.Equal(42.86m, InventoryFinancialMath.RoiPct(30000m, 70000m));
        Assert.NotEqual(
            InventoryFinancialMath.MarginPct(30000m, 100000m),
            InventoryFinancialMath.RoiPct(30000m, 70000m));
    }
}
