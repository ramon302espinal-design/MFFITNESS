using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 6.5–6.8 — capital y ganancia de inversión (sin BD).</summary>
public class InvestmentMathTests
{
    [Fact]
    public void CapitalInvested_SumsLineCapitals()
    {
        Assert.Equal(50000m, InvestmentMath.CapitalInvested(new[] { 20000m, 15000m, 15000m }));
    }

    [Fact]
    public void LineCapital_PrefersCostTotal()
    {
        Assert.Equal(1000m, InvestmentMath.LineCapital(10, 80m, 1000m));
        Assert.Equal(800m, InvestmentMath.LineCapital(10, 80m, null));
        Assert.Equal(0m, InvestmentMath.LineCapital(10, null, null));
    }

    [Fact]
    public void RecoveredFifo_ConsumesEntriesInOrder_NotRevenue()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 6, 7200m),
        };

        Assert.Equal(4800m, InvestmentMath.CapitalRecoveredFifo(entries, sales));
        Assert.Equal(5200m, InvestmentMath.CapitalPending(10000m, 4800m));
        Assert.Equal(48.00m, InvestmentMath.RecoveryPct(4800m, 10000m));
    }

    [Fact]
    public void RecoveredFifo_IgnoresSalesBeforeEntry()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 10), 10, 500m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 1), 5, 6000m),
            new InvestmentFifoSale(2, 10, new DateTime(2026, 8, 15), 3, 3600m),
        };

        Assert.Equal(1500m, InvestmentMath.CapitalRecoveredFifo(entries, sales));
    }

    [Fact]
    public void RecoveredFifo_DoesNotExceedEntryQuantity()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 4, 1000m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 10, 15000m),
        };

        Assert.Equal(4000m, InvestmentMath.CapitalRecoveredFifo(entries, sales));
    }

    [Fact]
    public void Frozen_IsRemainingPoolAtEntryCost()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 6, 7200m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(4800m, r.Recovered);
        Assert.Equal(3200m, r.Frozen);
        Assert.Equal(4, r.UnitsRemaining);
        Assert.Equal(3200m, InvestmentMath.CapitalPending(8000m, 4800m));
    }

    [Fact]
    public void Frozen_WithoutSales_EqualsInvested()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 5, 200m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, Array.Empty<InvestmentFifoSale>());
        Assert.Equal(0m, r.Recovered);
        Assert.Equal(1000m, r.Frozen);
    }

    [Fact]
    public void RealizedProfit_RevenueMinusEntryCogs()
    {
        // 6 × 1200 ingreso, 6 × 800 costo → ganancia 2400
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 6, 7200m),
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales, new Dictionary<int, decimal> { [10] = 1200m });
        Assert.Equal(7200m, r.AttributedRevenue);
        Assert.Equal(4800m, r.AttributedCogs);
        Assert.Equal(2400m, r.RealizedProfit);
        Assert.Equal(1600m, r.PotentialProfit); // 4 × (1200-800)
        Assert.False(r.RealizedProfit < 0);
    }

    [Fact]
    public void RealizedProfit_LossWhenSoldBelowCost()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 5, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 5, 3500m), // 700/u
        };

        InvestmentFifoResult r = InvestmentMath.RunFifo(entries, sales);
        Assert.Equal(-500m, r.RealizedProfit);
    }

    [Fact]
    public void Roi_UsesCapitalInvested_NotCogs()
    {
        Assert.Equal(30.00m, InvestmentMath.RoiPct(30000m, 100000m));
        Assert.NotEqual(30.00m, InvestmentMath.RoiPct(30000m, 70000m));
    }

    [Fact]
    public void Roi_RealizedPotentialProjected_Separated()
    {
        const decimal capital = 100000m;
        const decimal realized = 20000m;
        const decimal potential = 10000m;

        Assert.Equal(20.00m, InvestmentMath.RoiPct(realized, capital));
        Assert.Equal(10.00m, InvestmentMath.RoiPct(potential, capital));
        Assert.Equal(30.00m, InvestmentMath.RoiProjectedPct(realized, potential, capital));
    }

    [Fact]
    public void PaybackDays_WhenFullyRecovered()
    {
        // Capital 8000 (10×800). Venta día 1: 5 uds → 4000. Día 20: 5 uds → alcanza 8000.
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 5, 6000m),
            new InvestmentFifoSale(2, 10, new DateTime(2026, 8, 20), 5, 6000m),
        };

        Assert.Equal(19, InvestmentMath.PaybackDays(new DateTime(2026, 8, 1), 8000m, entries, sales));
    }

    [Fact]
    public void PaybackDays_NullWhenNotYetRecovered()
    {
        var entries = new[]
        {
            new InvestmentFifoEntry(1, 10, new DateTime(2026, 8, 1), 10, 800m),
        };
        var sales = new[]
        {
            new InvestmentFifoSale(1, 10, new DateTime(2026, 8, 5), 6, 7200m),
        };

        Assert.Null(InvestmentMath.PaybackDays(new DateTime(2026, 8, 1), 8000m, entries, sales));
    }

    [Fact]
    public void SuggestStatus_Planificada_WhenNoCapital()
    {
        var s = new InvestmentSummary { CapitalInvested = 0m };
        Assert.Equal(InvestmentStatus.Planificada, InvestmentStatusPolicy.SuggestStatus(s));
    }

    [Fact]
    public void SuggestStatus_Activa_WhenPartialRecovery()
    {
        var s = new InvestmentSummary
        {
            CapitalInvested = 10000m,
            CapitalRecovered = 4000m,
            CapitalPending = 6000m,
            FrozenCapital = 6000m,
            RealizedProfit = 1000m
        };
        Assert.Equal(InvestmentStatus.Activa, InvestmentStatusPolicy.SuggestStatus(s));
    }

    [Fact]
    public void SuggestStatus_Recuperada_WhenRecoveredButStockRemains()
    {
        var s = new InvestmentSummary
        {
            CapitalInvested = 10000m,
            CapitalRecovered = 10000m,
            CapitalPending = 0m,
            FrozenCapital = 2000m,
            RealizedProfit = 3000m
        };
        Assert.Equal(InvestmentStatus.Recuperada, InvestmentStatusPolicy.SuggestStatus(s));
    }

    [Fact]
    public void SuggestStatus_ConPerdida_WhenDepletedAtLoss()
    {
        var s = new InvestmentSummary
        {
            CapitalInvested = 10000m,
            CapitalRecovered = 10000m,
            CapitalPending = 0m,
            FrozenCapital = 0m,
            RealizedProfit = -500m,
            IsLoss = true
        };
        Assert.Equal(InvestmentStatus.ConPerdida, InvestmentStatusPolicy.SuggestStatus(s));
    }

    [Fact]
    public void CanTransition_DoesNotReopenCerrada()
    {
        Assert.False(InvestmentStatusPolicy.CanTransition(InvestmentStatus.Cerrada, InvestmentStatus.Activa));
    }

    [Fact]
    public void RankingLabels_RoiVsProfit_AreDifferentDimensions()
    {
        // A: alto ROI bajo capital; B: más ganancia absoluta, menor ROI
        var a = new InvestmentSummary
        {
            Name = "A",
            CapitalInvested = 10000m,
            RealizedProfit = 5000m,
            RoiRealizedPct = 50m,
            HasReliableCost = true
        };
        var b = new InvestmentSummary
        {
            Name = "B",
            CapitalInvested = 100000m,
            RealizedProfit = 30000m,
            RoiRealizedPct = 30m,
            HasReliableCost = true
        };

        var byProfit = new[] { a, b }.OrderByDescending(s => s.RealizedProfit).Select(s => s.Name).ToList();
        var byRoi = new[] { a, b }.OrderByDescending(s => s.RoiRealizedPct).Select(s => s.Name).ToList();

        Assert.Equal(new[] { "B", "A" }, byProfit);
        Assert.Equal(new[] { "A", "B" }, byRoi);
    }
}
