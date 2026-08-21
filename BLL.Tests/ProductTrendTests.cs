using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.11 — tendencia MoM (Growing/Stable/Declining/Insufficient).</summary>
public class ProductTrendTests
{
    [Fact]
    public void Growing_When_Units_Double()
    {
        // Brief §31: 20 → 40
        var dir = ProductTrendMath.Classify(40, 20);
        Assert.Equal(ProductTrendDirection.Growing, dir);
        Assert.Equal(100m, ProductTrendMath.ChangePct(40, 20));
    }

    [Fact]
    public void Declining_When_Units_Halve()
    {
        // Brief §32: 100 → 50
        Assert.Equal(ProductTrendDirection.Declining, ProductTrendMath.Classify(50, 100));
        Assert.Equal(-50m, ProductTrendMath.ChangePct(50, 100));
    }

    [Fact]
    public void Stable_Within_Band()
    {
        // Brief §33: ~100 ±2%
        Assert.Equal(ProductTrendDirection.Stable, ProductTrendMath.Classify(102, 100));
        Assert.Equal(ProductTrendDirection.Stable, ProductTrendMath.Classify(98, 100));
    }

    [Fact]
    public void Insufficient_When_Both_Zero()
    {
        Assert.Equal(
            ProductTrendDirection.InsufficientData,
            ProductTrendMath.Classify(0, 0));
        Assert.Null(ProductTrendMath.ChangePct(0, 0));
    }

    [Fact]
    public void New_Activity_From_Zero_Is_Growing()
    {
        Assert.Equal(ProductTrendDirection.Growing, ProductTrendMath.Classify(10, 0));
    }

    [Fact]
    public void ComposeAll_Merges_Periods_Primary_Is_Units()
    {
        var current = new[]
        {
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "A",
                GroupName = "A",
                UnitsSold = 40,
                RevenueTotal = 4000m
            }
        };
        var previous = new[]
        {
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "A",
                GroupName = "A",
                UnitsSold = 20,
                RevenueTotal = 5000m
            }
        };

        var row = ProductTrendMath.ComposeAll(current, previous).Single();
        Assert.Equal(ProductTrendDirection.Growing, row.PrimaryTrend);
        Assert.Equal(ProductTrendDirection.Growing, row.UnitsTrend);
        Assert.Equal(ProductTrendDirection.Declining, row.RevenueTrend);
        Assert.Equal(ProductAccelerationKind.Unknown, row.Acceleration);
    }

    [Fact]
    public void PeriodPair_ThisMonth_Uses_PreviousMonth()
    {
        var asOf = new DateTime(2026, 8, 20);
        var pair = ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisMonth, asOf);
        Assert.NotNull(pair);
        Assert.Equal(new DateTime(2026, 8, 1), pair.Value.Current.From);
        Assert.Equal(new DateTime(2026, 7, 1), pair.Value.Previous.From);
        Assert.Equal(new DateTime(2026, 8, 1), pair.Value.Previous.ToExclusive);
    }

    [Fact]
    public void Policy_Reserves_Acceleration()
    {
        Assert.Contains("Unknown", ProductTrendPolicy.AccelerationNote);
        Assert.Contains("±10%", ProductTrendPolicy.StableBandDefinition);
        Assert.Contains("PrimaryTrend = unidades", ProductTrendPolicy.TrendDefinition);
    }
}
