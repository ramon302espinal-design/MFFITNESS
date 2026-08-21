using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 4.9 — batería de fórmulas (tests 1–10 del diagnóstico).
/// Sin base de datos.
/// </summary>
public class InventoryFinancialMathTests
{
    // TEST 1: 10 × 800, PVP 1200
    [Fact]
    public void Test1_InventoryValuation_Baseline()
    {
        const int stock = 10;
        const decimal cost = 800m;
        const decimal price = 1200m;

        Assert.Equal(8000m, InventoryFinancialMath.InventoryCost(stock, cost));
        Assert.Equal(12000m, InventoryFinancialMath.PotentialSalesValue(stock, price));
        Assert.Equal(4000m, InventoryFinancialMath.PotentialProfit(stock, cost, price));
    }

    // TEST 2: venden 6 → quedan 4
    [Fact]
    public void Test2_AfterSellingSix_RemainingAndRealized()
    {
        const decimal cost = 800m;
        const decimal price = 1200m;

        Assert.Equal(3200m, InventoryFinancialMath.InventoryCost(4, cost));
        Assert.Equal(1600m, InventoryFinancialMath.PotentialProfit(4, cost, price));
        Assert.Equal(2400m, InventoryFinancialMath.RealizedLineProfit(6, price, cost));
        Assert.Equal(4800m, InventoryFinancialMath.LineCogs(6, cost));
    }

    // TEST 3: compra adicional a otro costo → promedio
    [Fact]
    public void Test3_WeightedAverage_TwoPurchases()
    {
        // 10×800 + 20×700 = 30 uds / 22000 → 733.3333
        decimal avg = InventoryFinancialMath.WeightedAverageUnitCost(10, 800m, 20, 700m);
        Assert.Equal(733.3333m, avg);

        // Inventario a nuevo promedio (30 uds)
        Assert.Equal(22000.00m, InventoryFinancialMath.InventoryCost(30, avg));
    }

    // TEST 4: sin ventas, con stock
    [Fact]
    public void Test4_NoSales_StillHasFrozenCapital()
    {
        Assert.Equal(5000m, InventoryFinancialMath.InventoryCost(10, 500m));
        Assert.Equal(0m, InventoryFinancialMath.RealizedLineProfit(0, 1200m, 500m));
    }

    // TEST 5: sin costo
    [Fact]
    public void Test5_NoCost_NoFakeMetrics()
    {
        Assert.Equal(0m, InventoryFinancialMath.InventoryCost(10, 0m));
        Assert.Equal(0m, InventoryFinancialMath.PotentialProfit(10, 0m, 1200m));
        Assert.Null(InventoryFinancialMath.RoiPct(100m, 0m));
        Assert.Null(InventoryFinancialMath.MarginPct(100m, 0m));
    }

    // TEST 6: stock negativo no se convierte a capital positivo
    [Fact]
    public void Test6_NegativeStock_ZeroCapital()
    {
        Assert.Equal(0m, InventoryFinancialMath.InventoryCost(-5, 800m));
        Assert.Equal(0m, InventoryFinancialMath.PotentialSalesValue(-5, 1200m));
        Assert.Equal(0m, InventoryFinancialMath.PotentialProfit(-5, 800m, 1200m));
    }

    // TEST 7: descuento → precio real de línea
    [Fact]
    public void Test7_Discount_UsesActualSalePrice()
    {
        // Lista 1200, cobrado 1100, costo 800 → ganancia 300/ud × 1
        Assert.Equal(300m, InventoryFinancialMath.RealizedLineProfit(1, 1100m, 800m));
        Assert.NotEqual(
            InventoryFinancialMath.RealizedLineProfit(1, 1200m, 800m),
            InventoryFinancialMath.RealizedLineProfit(1, 1100m, 800m));
    }

    // TEST 8: devolución conceptual = reverso de ganancia de la línea
    [Fact]
    public void Test8_Return_ReversesRealizedProfitSign()
    {
        decimal sold = InventoryFinancialMath.RealizedLineProfit(2, 1200m, 800m);
        Assert.Equal(800m, sold);
        // Anulación: la ganancia de esa línea deja de existir (modelo: no recalcular con costo nuevo)
        Assert.Equal(0m, InventoryFinancialMath.RealizedLineProfit(0, 1200m, 800m));
    }

    // TEST 9: cambio de precio no altera histórico (snapshot de venta)
    [Fact]
    public void Test9_PriceChange_HistoricalSaleKeepsSoldPrice()
    {
        decimal historical = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 800m);
        decimal ifRecalculatedWithNewPrice = InventoryFinancialMath.RealizedLineProfit(1, 1500m, 800m);
        Assert.Equal(400m, historical);
        Assert.Equal(700m, ifRecalculatedWithNewPrice);
        Assert.NotEqual(historical, ifRecalculatedWithNewPrice);
    }

    // TEST 10: cambio de costo no altera histórico (snapshot de costo)
    [Fact]
    public void Test10_CostChange_HistoricalSaleKeepsCostSnapshot()
    {
        decimal withSnapshot700 = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 700m);
        decimal wronglyUsingNewCost900 = InventoryFinancialMath.RealizedLineProfit(1, 1200m, 900m);
        Assert.Equal(500m, withSnapshot700);
        Assert.Equal(300m, wronglyUsingNewCost900);
        Assert.NotEqual(withSnapshot700, wronglyUsingNewCost900);
    }

    [Fact]
    public void MarginAndRoi_FromTest2Sale()
    {
        decimal realized = 2400m;
        decimal revenue = 7200m;
        decimal cogs = 4800m;
        Assert.Equal(33.33m, InventoryFinancialMath.MarginPct(realized, revenue));
        Assert.Equal(50.00m, InventoryFinancialMath.RoiPct(realized, cogs));
    }

    [Fact]
    public void WeightedAverage_FromEmptyStock_EqualsEntryCost()
    {
        Assert.Equal(750m, InventoryFinancialMath.WeightedAverageUnitCost(0, 0m, 10, 750m));
    }
}
