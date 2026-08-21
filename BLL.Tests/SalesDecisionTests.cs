using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.22 — señales Centro de decisiones.</summary>
public class SalesDecisionTests
{
    [Fact]
    public void Brief_Sales_Up_18_Percent()
    {
        var label = new SalesVariationLabel
        {
            VariationPct = 18m,
            Direction = SalesVariationDirection.Up,
            Strength = SalesVariationStrength.Strong,
            Display = "+18.00 %"
        };

        var signal = SalesDecisionMath.FromRevenueVariation(label, "los últimos 30 días");
        Assert.NotNull(signal);
        Assert.Equal("SalesGrowth", signal!.Code);
        Assert.Contains("aumentaron +18%", signal.Message);
        Assert.Contains("30 días", signal.Message);
    }

    [Fact]
    public void Brief_Concentration_Top3()
    {
        var signal = SalesDecisionMath.FromConcentration(70m, topN: 3);
        Assert.NotNull(signal);
        Assert.Contains("3 productos", signal!.Message);
        Assert.Contains("70%", signal.Message);
    }

    [Fact]
    public void Brief_Growth_With_Low_Cover()
    {
        var signal = SalesDecisionMath.FromGrowthWithLowCover("Whey Iso", 35m, 7m);
        Assert.NotNull(signal);
        Assert.Equal(SalesDecisionSeverity.Action, signal!.Severity);
        Assert.Contains("Whey Iso", signal.Message);
        Assert.Contains("+35%", signal.Message);
        Assert.Contains("7 días", signal.Message);
    }

    [Fact]
    public void Brief_Revenue_Up_Margin_Down()
    {
        var signal = SalesDecisionMath.FromRevenueUpMarginDown(20m, -6m);
        Assert.NotNull(signal);
        Assert.Contains("aumentaron +20%", signal!.Message);
        Assert.Contains("margen cayó -6%", signal.Message);
    }

    [Fact]
    public void Build_Orders_By_Priority_Primary()
    {
        var report = SalesDecisionMath.Build(
            ProfitPeriodKind.Last30Days,
            new SalesDecisionSignal?[]
            {
                SalesDecisionMath.FromRevenueVariation(new SalesVariationLabel
                {
                    VariationPct = 10m,
                    Direction = SalesVariationDirection.Up,
                    Strength = SalesVariationStrength.Mild,
                    Display = "+10 %"
                }),
                SalesDecisionMath.FromGrowthWithLowCover("X", 35m, 7m),
                SalesDecisionMath.FromStockout(2)
            });

        Assert.Equal(3, report.SignalCount);
        Assert.Equal("GrowthLowCover", report.Primary!.Code);
        Assert.Contains("NO ejecutar acciones", report.PolicyNote);
    }

    [Fact]
    public void Flat_Variation_No_Signal()
    {
        var signal = SalesDecisionMath.FromRevenueVariation(new SalesVariationLabel
        {
            VariationPct = 1m,
            Direction = SalesVariationDirection.Flat,
            Strength = SalesVariationStrength.None,
            Display = "+1.00 %"
        });
        Assert.Null(signal);
    }

    [Fact]
    public void Policy_No_Auto_Actions()
    {
        Assert.Contains("FrmAnaDecisiones", SalesDecisionPolicy.Definition);
        Assert.Contains("NO ejecutar acciones", SalesDecisionPolicy.Definition);
    }
}
