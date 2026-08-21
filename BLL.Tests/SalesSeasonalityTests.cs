using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.16 — estacionalidad + YoY mismo mes.</summary>
public class SalesSeasonalityTests
{
    private static readonly DateTime AsOf = new(2026, 8, 20);

    private static SalesSummary Summary(decimal revenue, decimal profit = 0m, int units = 0, int txns = 1)
        => new()
        {
            RevenueTotal = revenue,
            RealizedProfit = profit,
            UnitsSold = units,
            TransactionCount = txns,
            AverageTicket = SalesAnalyticsMath.AverageTicket(revenue, txns),
            HasReliableRealizedProfit = true
        };

    [Fact]
    public void Brief_August_2026_Vs_August_2025()
    {
        var (cur, prior) = SalesSeasonalityMath.ResolveSameMonthYoY(AsOf);

        Assert.Equal(new DateTime(2026, 8, 1), cur.From);
        Assert.Equal(new DateTime(2026, 9, 1), cur.ToExclusive);
        Assert.Equal(new DateTime(2025, 8, 1), prior.From);
        Assert.Equal(new DateTime(2025, 9, 1), prior.ToExclusive);
    }

    [Fact]
    public void YoY_Not_Previous_Month()
    {
        var (cur, prior) = SalesSeasonalityMath.ResolveSameMonthYoY(AsOf);
        var mom = ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisMonth, AsOf)!.Value;

        Assert.Equal(cur.From, mom.Current.From);
        Assert.NotEqual(prior.From, mom.Previous.From);
        Assert.Equal(new DateTime(2026, 7, 1), mom.Previous.From);
    }

    [Fact]
    public void December_Is_High_Season()
    {
        Assert.Equal(SalesSeasonBand.High, SalesSeasonalityMath.ClassifyMonthBand(12));
        Assert.Contains("alta", SalesSeasonalityMath.SeasonLabel(12, SalesSeasonBand.High),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Distortion_When_Mom_And_Yoy_Disagree()
    {
        // YoY +20%, MoM -15% → posible distorsión estacional
        Assert.True(SalesSeasonalityMath.DetectSeasonalDistortion(20m, -15m));
        Assert.False(SalesSeasonalityMath.DetectSeasonalDistortion(20m, 10m));
        Assert.False(SalesSeasonalityMath.DetectSeasonalDistortion(1m, -15m));
    }

    [Fact]
    public void Compose_Flags_Distortion()
    {
        var (curR, priorR) = SalesSeasonalityMath.ResolveSameMonthYoY(AsOf);
        var yoyCurrent = Summary(300_000m);
        var yoyPrior = Summary(250_000m); // +20% YoY

        var sequential = SalesComparisonComposer.Build(
            ProfitPeriodKind.ThisMonth,
            curR,
            new ProfitPeriodRange(new DateTime(2026, 7, 1), new DateTime(2026, 8, 1)),
            yoyCurrent,
            Summary(350_000m)); // MoM negativo

        var report = SalesSeasonalityMath.Compose(
            SalesSeasonalityMode.SameMonthYoY,
            AsOf,
            curR,
            priorR,
            yoyCurrent,
            yoyPrior,
            sequential);

        Assert.True(report.PossibleSeasonalDistortion);
        Assert.Equal(20.00m, report.YoY.Revenue.VariationPct);
        Assert.Contains("crecimiento permanente", report.Caution);
    }

    [Fact]
    public void Week_And_Day_YoY_Ranges()
    {
        var (wCur, wPrior) = SalesSeasonalityMath.ResolveSameWeekYoY(AsOf);
        Assert.Equal(7, (wCur.ToExclusive!.Value - wCur.From!.Value).Days);
        Assert.Equal(wCur.From!.Value.AddYears(-1), wPrior.From);

        var (dCur, dPrior) = SalesSeasonalityMath.ResolveSameCalendarDayYoY(AsOf);
        Assert.Equal(AsOf, dCur.From);
        Assert.Equal(new DateTime(2025, 8, 20), dPrior.From);
    }

    [Fact]
    public void DayOfWeek_Profile_Orders_Monday_First()
    {
        var days = new[]
        {
            new ProfitDayRow
            {
                Date = new DateTime(2026, 8, 17), // lunes
                RevenueTotal = 10_000m,
                TransactionCount = 5
            },
            new ProfitDayRow
            {
                Date = new DateTime(2026, 8, 16), // domingo
                RevenueTotal = 3_000m,
                TransactionCount = 2
            }
        };

        var profile = SalesSeasonalityMath.BuildDayOfWeekProfile(days);
        Assert.Equal(DayOfWeek.Monday, profile[0].DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, profile[^1].DayOfWeek);
        Assert.Equal(10_000m, profile[0].RevenueTotal);
    }

    [Fact]
    public void Policy_Requires_Yoy_Not_Only_Mom()
    {
        Assert.Contains("ago-2026 vs ago-2025", SalesSeasonalityPolicy.Definition);
        Assert.Contains("crecimiento permanente", SalesSeasonalityPolicy.VsGrowth);
        Assert.Contains("PossibleSeasonalDistortion", SalesSeasonalityPolicy.Distortion);
    }
}
