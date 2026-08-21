using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.15 — aceleración / desaceleración.</summary>
public class SalesAccelerationTests
{
    [Fact]
    public void Brief_Accelerating_20_25_35()
    {
        var r = SalesAccelerationMath.ClassifyFromChangePcts(new[] { 20m, 25m, 35m });
        Assert.Equal(SalesAccelerationKind.Accelerating, r.Kind);
        Assert.Equal(20m, r.FirstChangePct);
        Assert.Equal(35m, r.LastChangePct);
        Assert.True(r.AccelerationDeltaPp > 0);
    }

    [Fact]
    public void Brief_Decelerating_Still_Growing()
    {
        var r = SalesAccelerationMath.ClassifyFromChangePcts(new[] { 40m, 20m, 5m });
        Assert.Equal(SalesAccelerationKind.Decelerating, r.Kind);
        Assert.True(r.LastChangePct > 0);
        Assert.Contains("desacelera", r.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromValues_Produces_Accelerating()
    {
        // 100 → 120 (+20%), → 150 (+25%), → 202.5 (+35%)
        var r = SalesAccelerationMath.ClassifyFromValues(new[] { 100m, 120m, 150m, 202.5m });
        Assert.Equal(SalesAccelerationKind.Accelerating, r.Kind);
        Assert.Equal(3, r.ChangeCount);
    }

    [Fact]
    public void Steady_Within_Band()
    {
        var r = SalesAccelerationMath.ClassifyFromChangePcts(new[] { 10m, 12m, 11m });
        Assert.Equal(SalesAccelerationKind.Steady, r.Kind);
    }

    [Fact]
    public void Two_Changes_Insufficient()
    {
        var r = SalesAccelerationMath.ClassifyFromChangePcts(new[] { 20m, 30m });
        Assert.Equal(SalesAccelerationKind.InsufficientData, r.Kind);
        Assert.Contains("≥ 3", r.Reason);
    }

    [Fact]
    public void Previous_Zero_Skips_Step()
    {
        // 0 → 100 (omitido), 100 → 120 (+20), 120 → 150 (+25), 150 → 202.5 (+35)
        var r = SalesAccelerationMath.ClassifyFromValues(new[] { 0m, 100m, 120m, 150m, 202.5m });
        Assert.Equal(SalesAccelerationKind.Accelerating, r.Kind);
        Assert.Equal(3, r.ChangeCount);
    }

    [Fact]
    public void Policy_Separates_Growth_From_Acceleration()
    {
        Assert.Contains("aceleración ≠ crecimiento", SalesAccelerationPolicy.Definition);
        Assert.Contains("DESACELERACIÓN", SalesAccelerationPolicy.GrowthVsAcceleration);
        Assert.Contains("Unknown", SalesAccelerationPolicy.ProductBridge);
    }
}
