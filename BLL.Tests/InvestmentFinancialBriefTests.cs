using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 6.13 — batería del brief de inversiones (tests 1–10).
/// Sin BD: fórmulas + política (devolución/crédito proveedor = N/A en POS).
/// </summary>
public class InvestmentFinancialBriefTests
{
    // TEST 1: capital 100k, ventas 130k, COGS 100k → ganancia 30k, ROI 30%
    [Fact]
    public void Test1_FullRecovery_Profit30k_Roi30()
    {
        // 100 uds × 1000 costo = 100000; vender 100 a 1300 = 130000 ingreso
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 1, 1), 100, 1000m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 1, new DateTime(2026, 2, 1), 100, 130000m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(100000m, r.AttributedCogs);
        Assert.Equal(130000m, r.AttributedRevenue);
        Assert.Equal(30000m, r.RealizedProfit);
        Assert.Equal(30.00m, InvestmentMath.RoiPct(r.RealizedProfit, 100000m));
        Assert.Equal(100000m, r.Recovered);
        Assert.Equal(0m, r.Frozen);
    }

    // TEST 2: ganancia 50k, ROI 50%
    [Fact]
    public void Test2_HigherMargin_Roi50()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 1, 1), 100, 1000m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 1, new DateTime(2026, 2, 1), 100, 150000m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(50000m, r.RealizedProfit);
        Assert.Equal(50.00m, InvestmentMath.RoiPct(r.RealizedProfit, 100000m));
    }

    // TEST 3: capital 10k, ganancia 5k → ROI 50%
    [Fact]
    public void Test3_SmallCapital_HighRoi()
    {
        Assert.Equal(50.00m, InvestmentMath.RoiPct(5000m, 10000m));
    }

    // TEST 4: capital 100k, ganancia 30k → ROI 30% (menor ROI que TEST 3, más ganancia absoluta)
    [Fact]
    public void Test4_LargerCapital_LowerRoi_MoreAbsoluteProfit()
    {
        Assert.Equal(30.00m, InvestmentMath.RoiPct(30000m, 100000m));
        Assert.True(30000m > 5000m);
        Assert.True(InvestmentMath.RoiPct(5000m, 10000m) > InvestmentMath.RoiPct(30000m, 100000m));
    }

    // TEST 5: inventario restante → congelado + potencial + ROI realizado
    [Fact]
    public void Test5_RemainingInventory_FrozenAndPotential()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 1, new DateTime(2026, 8, 5), 6, 7200m),
        };
        var prices = new Dictionary<int, decimal> { [1] = 1200m };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales, prices);
        Assert.Equal(4800m, r.Recovered);
        Assert.Equal(3200m, r.Frozen);
        Assert.Equal(2400m, r.RealizedProfit);
        Assert.Equal(1600m, r.PotentialProfit);
        Assert.Equal(30.00m, InvestmentMath.RoiPct(r.RealizedProfit, 8000m));
        Assert.Equal(20.00m, InvestmentMath.RoiPct(r.PotentialProfit, 8000m));
    }

    // TEST 6: inversión con pérdida
    [Fact]
    public void Test6_LossInvestment()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 1, 1), 10, 1000m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 1, new DateTime(2026, 2, 1), 10, 8000m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(-2000m, r.RealizedProfit);
        Assert.Equal(-20.00m, InvestmentMath.RoiPct(r.RealizedProfit, 10000m));

        var summary = new InvestmentSummary
        {
            CapitalInvested = 10000m,
            CapitalRecovered = 10000m,
            CapitalPending = 0m,
            FrozenCapital = 0m,
            RealizedProfit = r.RealizedProfit,
            IsLoss = true
        };
        Assert.Equal(InvestmentStatus.ConPerdida, InvestmentStatusPolicy.SuggestStatus(summary));
    }

    // TEST 7: compra a crédito — POS no tiene CxP proveedor; capital ≠ efectivo
    [Fact]
    public void Test7_CreditPurchase_NotModeled_CapitalIsInventoryNotCash()
    {
        // Documentado: capital invertido = costo de ENTRADAS, no egreso de caja.
        decimal capitalInventario = InvestmentMath.LineCapital(50, 200m, null);
        decimal efectivoPagado = 6000m; // parcial hipotético
        Assert.Equal(10000m, capitalInventario);
        Assert.NotEqual(capitalInventario, efectivoPagado);
    }

    // TEST 8: devolución — POS no tiene módulo; anulación DELETE no deja rastro
    [Fact]
    public void Test8_Return_NotSupported_VoidDeletesHistory()
    {
        Assert.False(ProfitVoidAndReturnPolicy.HasProductReturnModule);
        Assert.True(ProfitVoidAndReturnPolicy.AnnulmentDeletesHistory);
        // Tras DELETE no hay venta que consuma el pool → recuperado 0
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 1, 1), 5, 100m),
        };
        Assert.Equal(0m, InvestmentMath.CapitalRecoveredFifo(entries, Array.Empty<InvestmentFifoSale>()));
    }

    // TEST 9: cambio de costo posterior — histórico de ENTRADA no cambia
    [Fact]
    public void Test9_CostChangeLater_EntrySnapshotWins()
    {
        decimal historical = InvestmentMath.LineCapital(10, 700m, 7000m);
        decimal ifRecalculated = InvestmentMath.LineCapital(10, 900m, null);
        Assert.Equal(7000m, historical);
        Assert.NotEqual(historical, ifRecalculated);

        var entries = new[]
        {
            new InvestmentFifoEntry(1, 1, new DateTime(2026, 1, 1), 10, 700m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 1, new DateTime(2026, 2, 1), 10, 12000m),
        };
        // Recuperación usa costo de ENTRADA 700, no 900
        Assert.Equal(7000m, InvestmentMath.CapitalRecoveredFifo(entries, sales));
    }

    // TEST 10: múltiples productos
    [Fact]
    public void Test10_MultipleProducts()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 1, 1), 10, 500m), // 5000
            new InvestmentFifoEntry(2, 20, new DateTime(2026, 1, 2), 5, 1000m), // 5000
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 2, 1), 10, 8000m),
            new InvestmentFifoSale(2, 20, new DateTime(2026, 2, 2), 5, 7000m),
        };

        decimal invested = InvestmentMath.CapitalInvested(new[] { 5000m, 5000m });
        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(10000m, invested);
        Assert.Equal(10000m, r.AttributedCogs);
        Assert.Equal(15000m, r.AttributedRevenue);
        Assert.Equal(5000m, r.RealizedProfit);
        Assert.Equal(50.00m, InvestmentMath.RoiPct(r.RealizedProfit, invested));
        Assert.Equal(0m, r.Frozen);
    }
}
