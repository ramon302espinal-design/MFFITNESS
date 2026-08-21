using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.14 — tendencia multi-punto + volátil.</summary>
public class SalesSeriesTrendTests
{
    [Fact]
    public void Brief_Growing_Four_Weeks()
    {
        var series = new[] { 50_000m, 55_000m, 62_000m, 70_000m };
        var r = SalesSeriesTrendMath.Classify(series);
        Assert.Equal(SalesSeriesTrendKind.Growing, r.Kind);
        Assert.True(r.SlopePerStepPct > 0);
    }

    [Fact]
    public void Brief_Declining()
    {
        var series = new[] { 50_000m, 45_000m, 38_000m, 30_000m };
        Assert.Equal(SalesSeriesTrendKind.Declining, SalesSeriesTrendMath.Classify(series).Kind);
    }

    [Fact]
    public void Brief_Stable()
    {
        var series = new[] { 50_000m, 51_000m, 49_000m, 50_500m };
        Assert.Equal(SalesSeriesTrendKind.Stable, SalesSeriesTrendMath.Classify(series).Kind);
    }

    [Fact]
    public void Brief_Volatile_Not_Growing()
    {
        var series = new[] { 20_000m, 80_000m, 15_000m, 90_000m };
        var r = SalesSeriesTrendMath.Classify(series);
        Assert.Equal(SalesSeriesTrendKind.Volatile, r.Kind);
        Assert.True(r.CoefficientOfVariationPct >= 40m);
    }

    [Fact]
    public void Two_Points_Insufficient()
    {
        var r = SalesSeriesTrendMath.Classify(new[] { 10_000m, 20_000m });
        Assert.Equal(SalesSeriesTrendKind.InsufficientData, r.Kind);
        Assert.Contains("≥ 4", r.Reason);
    }

    [Fact]
    public void Policy_Requires_MultiPoint()
    {
        Assert.Contains("≥ MinPoints", SalesSeriesTrendPolicy.Definition);
        Assert.Contains("VOLÁTIL", SalesSeriesTrendPolicy.Volatile);
        Assert.Contains("≠ ProductTrend", SalesSeriesTrendPolicy.Definition);
    }
}
