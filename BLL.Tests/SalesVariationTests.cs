using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.5 — variaciones y señales cruzadas.</summary>
public class SalesVariationTests
{
    private static SalesMetricDelta D(decimal cur, decimal prev)
        => SalesComparisonComposer.Delta(cur, prev);

    [Fact]
    public void Brief_Test1_Display_Plus20()
    {
        var label = SalesVariationMath.Label(20m);
        Assert.Equal(SalesVariationDirection.Up, label.Direction);
        Assert.Equal(SalesVariationStrength.Strong, label.Strength);
        Assert.Equal("+20.00 %", label.Display);
    }

    [Fact]
    public void Brief_Test2_RevenueUp_ProfitDown_Signal()
    {
        var revenue = D(120_000m, 100_000m); // +20%
        var profit = D(19_000m, 20_000m);   // -5%

        var signals = SalesVariationMath.DetectCrossSignals(revenue, profit);
        Assert.Contains(signals, s => s.Kind == SalesCrossSignalKind.RevenueUpProfitDown);
        Assert.Contains("ganancia", signals[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Brief_Test3_RevenueUp_MarginDown_Signal()
    {
        var revenue = D(120_000m, 100_000m);
        var profit = D(25_000m, 22_000m);
        var margin = D(20m, 22m); // ~-9%

        var signals = SalesVariationMath.DetectCrossSignals(revenue, profit, margin);
        Assert.Contains(signals, s => s.Kind == SalesCrossSignalKind.RevenueUpMarginDown);
    }

    [Fact]
    public void NoBase_Is_ND()
    {
        var label = SalesVariationMath.Label(null);
        Assert.Equal(SalesVariationDirection.NoComparableBase, label.Direction);
        Assert.Equal("N/D", label.Display);
    }

    [Fact]
    public void Flat_Within_Band()
    {
        var label = SalesVariationMath.Label(1.5m);
        Assert.Equal(SalesVariationDirection.Flat, label.Direction);
        Assert.Equal(SalesVariationStrength.None, label.Strength);
    }

    [Fact]
    public void FromComparison_Builds_Full_Report()
    {
        var current = new SalesSummary
        {
            RevenueTotal = 300_000m,
            RealizedProfit = 19_000m,
            UnitsSold = 100,
            TransactionCount = 50,
            AverageTicket = 6_000m,
            MarginPct = 20m,
            HasReliableRealizedProfit = true
        };
        var previous = new SalesSummary
        {
            RevenueTotal = 250_000m,
            RealizedProfit = 20_000m,
            UnitsSold = 90,
            TransactionCount = 45,
            AverageTicket = 5_555.56m,
            MarginPct = 25m,
            HasReliableRealizedProfit = true
        };

        var cmp = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            new ProfitPeriodRange(null, null),
            new ProfitPeriodRange(null, null),
            current,
            previous);

        var report = SalesVariationMath.FromComparison(cmp);
        Assert.Equal("+20.00 %", report.Revenue.Display);
        Assert.Equal(SalesVariationDirection.Down, report.RealizedProfit.Direction);
        Assert.Contains(report.CrossSignals, s => s.Kind == SalesCrossSignalKind.RevenueUpProfitDown);
        Assert.Contains(report.CrossSignals, s => s.Kind == SalesCrossSignalKind.RevenueUpMarginDown);
    }

    [Fact]
    public void Policy_Documents_Formula_And_Cross()
    {
        Assert.Contains("Previous = 0", SalesVariationPolicy.Formula);
        Assert.Contains("§50", SalesVariationPolicy.CrossSignals);
        Assert.Contains("N/D", SalesVariationPolicy.Display);
    }
}
