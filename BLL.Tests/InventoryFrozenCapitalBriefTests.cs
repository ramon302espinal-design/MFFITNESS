using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 7.15 — batería del brief de capital congelado (tests 1–8 + contratos clave).
/// Sin BD: fórmulas + clasificador + simulación (no muta PrecioVenta).
/// </summary>
public class InventoryFrozenCapitalBriefTests
{
    private static readonly DateTime AsOf = new(2026, 8, 20);

    // TEST 1: Stock 100 × costo 500 → capital RD$50,000
    [Fact]
    public void Test1_InventoryCapital_StockTimesCost()
    {
        Assert.Equal(50_000m, InventoryFinancialMath.InventoryCapital(100, 500m));
        Assert.NotEqual(
            InventoryFinancialMath.InventoryCapital(100, 500m),
            InventoryFinancialMath.PotentialSalesValue(100, 800m));
    }

    // TEST 2: valor potencial 80k, ganancia potencial 30k
    [Fact]
    public void Test2_PotentialSales_And_PotentialProfit()
    {
        Assert.Equal(80_000m, InventoryFinancialMath.PotentialSalesValue(100, 800m));
        Assert.Equal(30_000m, InventoryFinancialMath.PotentialProfit(100, 500m, 800m));
        Assert.Equal(
            50_000m,
            InventoryFinancialMath.PotentialSalesValue(100, 800m)
                - InventoryFinancialMath.PotentialProfit(100, 500m, 800m));
    }

    // TEST 3: última venta 40 días atrás → días sin venta
    [Fact]
    public void Test3_DaysWithoutSale_From_LastSale()
    {
        DateTime lastSale = AsOf.AddDays(-40);
        var idle = InventoryFinancialMath.ResolveIdle(lastSale, AsOf.AddDays(-100), AsOf);

        Assert.Equal(InventoryIdleKind.HasSales, idle.Kind);
        Assert.Equal(40, idle.DaysWithoutSale);
        Assert.Equal(40, idle.IdleDays);
    }

    // TEST 4: producto nunca vendido
    [Fact]
    public void Test4_NeverSold_Uses_FirstEntry()
    {
        var idle = InventoryFinancialMath.ResolveIdle(
            lastSaleDate: null,
            firstEntryDate: AsOf.AddDays(-45),
            asOf: AsOf);

        Assert.Equal(InventoryIdleKind.NeverSold, idle.Kind);
        Assert.Equal(45, idle.IdleDays);
        Assert.Null(idle.DaysWithoutSale);
    }

    // TEST 5: producto nuevo — no clasificar como congelado
    [Fact]
    public void Test5_NewProduct_Not_Frozen()
    {
        var status = InventoryHealthClassifier.Classify(
            stock: 100,
            inventoryCapital: 50_000m,
            potentialProfit: 30_000m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 5,
            daysSinceFirstEntry: 5,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.Equal(InventoryHealthStatus.New, status);
        Assert.NotEqual(InventoryHealthStatus.Frozen, status);
        Assert.NotEqual(InventoryHealthStatus.Critical, status);
    }

    // TEST 6: alta rotación + alto capital → saludable
    [Fact]
    public void Test6_HighRotation_HighCapital_Is_Healthy()
    {
        var status = InventoryHealthClassifier.Classify(
            stock: 60,
            inventoryCapital: 50_000m,
            potentialProfit: 20_000m,
            idleKind: InventoryIdleKind.HasSales,
            idleDays: 2,
            daysSinceFirstEntry: 90,
            daysOfCover: 20m,
            unitsPerDay: 3m);

        Assert.Equal(InventoryHealthStatus.Healthy, status);
    }

    // TEST 7: bajo capital sin ventas ≠ crítico de alto capital
    [Fact]
    public void Test7_LowCapital_NoSales_Not_Critical_Vs_HighCapital()
    {
        var low = InventoryHealthClassifier.Classify(
            stock: 2,
            inventoryCapital: 400m,
            potentialProfit: 100m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 45,
            daysSinceFirstEntry: 50,
            daysOfCover: null,
            unitsPerDay: 0m);

        var high = InventoryHealthClassifier.Classify(
            stock: 100,
            inventoryCapital: 50_000m,
            potentialProfit: 5_000m,
            idleKind: InventoryIdleKind.NeverSold,
            idleDays: 70,
            daysSinceFirstEntry: 80,
            daysOfCover: null,
            unitsPerDay: 0m);

        Assert.NotEqual(InventoryHealthStatus.Critical, low);
        Assert.Equal(InventoryHealthStatus.Slow, low);
        Assert.Equal(InventoryHealthStatus.Critical, high);
    }

    // TEST 8: estacional / sin demanda ni idle — InsufficientData (no inventar Frozen)
    [Fact]
    public void Test8_Seasonal_InsufficientData_No_Invention()
    {
        var status = InventoryHealthClassifier.Classify(
            stock: 80,
            inventoryCapital: 20_000m,
            potentialProfit: 5_000m,
            idleKind: InventoryIdleKind.Unknown,
            idleDays: null,
            daysSinceFirstEntry: 120,
            daysOfCover: null,
            unitsPerDay: null);

        Assert.Equal(InventoryHealthStatus.InsufficientData, status);
        Assert.NotEqual(InventoryHealthStatus.Frozen, status);
        Assert.NotEqual(InventoryHealthStatus.Critical, status);
    }

    // Brief §21: 20,000 / 150,000 = 13.33% — inmovilizado ≠ inventario
    [Fact]
    public void Contract_FrozenShare_And_Inventory_NotEqual()
    {
        Assert.Equal(13.33m, InventoryFinancialMath.FrozenShareOfInventoryPct(20_000m, 150_000m));
        Assert.Contains("≠", InventoryCapitalPolicy.FrozenVsInventoryNote);
        Assert.Contains("Frozen o Critical", InventoryCapitalPolicy.FrozenVsInventoryNote);
    }

    // Brief §24–25: liquidación simula; liberable = capital a costo; no muta PVP
    [Fact]
    public void Contract_Liquidation_Is_Simulation_Liberable()
    {
        var s = InventoryFinancialMath.SimulateLiquidation(100, 500m, 800m, 10m);
        Assert.Equal(50_000m, s.CapitalLiberable);
        Assert.Equal(72_000m, s.SimulatedRevenue);
        Assert.Contains("NUNCA modifica PrecioVenta", InventoryCapitalPolicy.RiskAndLiberableDefinition);
        Assert.Contains("AtRisk", InventoryCapitalPolicy.RiskAndLiberableDefinition);
    }

    // Brief §19 / 7.13: Frozen inversión FIFO ≠ inmovilizado global
    [Fact]
    public void Contract_Investment_Trapped_Distinct_From_Global()
    {
        Assert.Contains("≠", InventoryCapitalPolicy.InvestmentBridgeDefinition);
        Assert.Contains("FIFO", InventoryCapitalPolicy.InvestmentBridgeDefinition);
    }
}
