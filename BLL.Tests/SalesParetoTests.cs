using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.13 — Pareto calculado (no asumir 80/20).</summary>
public class SalesParetoTests
{
    private static SalesShareReport ShareFrom(params decimal[] amounts)
    {
        var items = amounts
            .Select((a, i) => ($"P{i + 1}", (int?)(i + 1), (int?)null, a))
            .ToArray();
        return SalesShareComposer.FromAmounts(
            items, SalesShareMetric.Revenue, ProfitPeriodKind.ThisMonth, topN: 5);
    }

    [Fact]
    public void Brief_Example_20pct_Items_Generate_75pct()
    {
        // 2 de 10 productos = 20% de ítems; montos: 40+35 + ocho de 3.125 = 75+25
        var amounts = new[] { 40m, 35m, 3.125m, 3.125m, 3.125m, 3.125m, 3.125m, 3.125m, 3.125m, 3.125m };
        var share = ShareFrom(amounts);
        var pareto = SalesParetoComposer.FromShareReport(share, targetCumulativePct: 75m);

        Assert.True(pareto.TargetReached);
        Assert.Equal(2, pareto.ItemsToReachTarget);
        Assert.Equal(20.00m, pareto.ItemPctToReachTarget);
        Assert.True(pareto.AchievedCumulativePct >= 75m);
        Assert.Contains("20%", pareto.Summary);
        Assert.Contains("75%", pareto.Summary);
    }

    [Fact]
    public void Does_Not_Assume_Exactly_80_20()
    {
        // Distribución plana: hace falta ~80% de ítems para 80% de ventas
        var amounts = Enumerable.Repeat(10m, 10).ToArray();
        var pareto = SalesParetoComposer.FromShareReport(ShareFrom(amounts), 80m);

        Assert.Equal(8, pareto.ItemsToReachTarget);
        Assert.Equal(80.00m, pareto.ItemPctToReachTarget);
        Assert.NotEqual(20.00m, pareto.ItemPctToReachTarget);
    }

    [Fact]
    public void Empty_Is_Insufficient()
    {
        var empty = SalesShareComposer.FromAmounts(
            Array.Empty<(string, int?, int?, decimal)>(),
            SalesShareMetric.Revenue,
            ProfitPeriodKind.Today);

        var pareto = SalesParetoComposer.FromShareReport(empty);
        Assert.False(pareto.TargetReached);
        Assert.Contains("insuficientes", pareto.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Policy_Forbids_Fixed_8020()
    {
        Assert.Contains("NO asumir", SalesParetoPolicy.Definition);
        Assert.Contains("datos reales", SalesParetoPolicy.Summary);
    }
}
