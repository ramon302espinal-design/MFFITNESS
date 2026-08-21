using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.7 — materialidad anti-fatiga (SSOT thresholds).</summary>
public class DecisionMaterialityTests
{
    [Fact]
    public void Defaults_Match_Existing_Ssot()
    {
        var t = DecisionMaterialityThresholds.FromSsot();
        Assert.Equal(SalesVariationThresholds.Default.FlatBandPct, t.FlatVariationBandPct);
        Assert.Equal(SalesVariationThresholds.Default.StrongBandPct, t.StrongVariationBandPct);
        Assert.Equal(InventoryHealthThresholds.Default.MinMaterialCapital, t.MinMaterialCapital);
        Assert.Equal(InventoryHealthThresholds.Default.CriticalCapitalMin, t.CriticalCapitalMin);
        Assert.Equal(InventoryAlertService.HighImmobilizedShareThresholdPct, t.HighImmobilizedSharePct);
    }

    [Fact]
    public void Flat_Variation_Is_Noise_Not_Emitted()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = 1.5m
        });
        Assert.False(r.IsMaterial);
        Assert.False(r.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.NotMaterial, r.Kind);
    }

    [Fact]
    public void Strong_Decline_Is_Strong_Material()
    {
        // TEST 1 conceptual: ventas ↓ 30%
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = -30m
        });
        Assert.True(r.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.Strong, r.Kind);
        Assert.Equal(DecisionImpactLevel.High, r.SuggestedImpact);
    }

    [Fact]
    public void Mild_Variation_Between_Bands_Is_Material()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = 8m
        });
        Assert.True(r.IsMaterial);
        Assert.Equal(DecisionMaterialityKind.Material, r.Kind);
    }

    [Fact]
    public void Capital_Below_MinMaterial_Is_Not_Emitted()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            CapitalAmount = 800m
        });
        Assert.False(r.ShouldEmitAlert);
    }

    [Fact]
    public void Capital_At_MinMaterial_Is_Material()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            CapitalAmount = 1_000m
        });
        Assert.True(r.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.Material, r.Kind);
    }

    [Fact]
    public void Critical_Capital_Is_Strong()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            CapitalAmount = 12_000m
        });
        Assert.Equal(DecisionMaterialityKind.Strong, r.Kind);
        Assert.Equal(DecisionImpactLevel.Critical, r.SuggestedImpact);
    }

    [Fact]
    public void Cross_Signal_Is_Material_Even_If_Legs_Mild()
    {
        // TEST 2 — contradicción
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = 1m, // flat alone
            CrossSignal = true
        });
        Assert.True(r.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.Material, r.Kind);
    }

    [Fact]
    public void Stockout_Bypasses_Low_Capital_Filter()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            CapitalAmount = 100m,
            TimeSensitiveStockout = true
        });
        Assert.True(r.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.Strong, r.Kind);
    }

    [Fact]
    public void Insufficient_Data_Blocks_Advanced_Alert()
    {
        var r = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            InsufficientData = true,
            VariationPct = -40m,
            CapitalAmount = 50_000m
        });
        Assert.False(r.ShouldEmitAlert);
        Assert.Contains("insuficientes", r.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Immobilized_Share_Uses_Alert_Service_Bands()
    {
        var low = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            ImmobilizedSharePct = 20m
        });
        Assert.False(low.ShouldEmitAlert);

        var mid = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            ImmobilizedSharePct = 30m
        });
        Assert.True(mid.ShouldEmitAlert);
        Assert.Equal(DecisionMaterialityKind.Material, mid.Kind);

        var hi = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            ImmobilizedSharePct = 45m
        });
        Assert.Equal(DecisionMaterialityKind.Strong, hi.Kind);
    }

    [Fact]
    public void GateEmit_Returns_Null_When_Not_Material()
    {
        var draft = DecisionEventFactory.Create(
            "sales.strong_decline", DecisionEventArea.Sales,
            DecisionEntityType.Portfolio, null, "",
            "ThisMonth", "UnitTest", "Caída", "↓1%");

        var mat = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = 1m
        });

        Assert.Null(DecisionMaterialityEvaluator.GateEmit(draft, mat));
    }

    [Fact]
    public void GateEmit_Passes_When_Material()
    {
        var draft = DecisionEventFactory.Create(
            "sales.strong_decline", DecisionEventArea.Sales,
            DecisionEntityType.Portfolio, null, "",
            "ThisMonth", "UnitTest", "Caída", "↓30%");

        var mat = DecisionMaterialityEvaluator.Evaluate(new DecisionMaterialityInput
        {
            VariationPct = -30m
        });

        Assert.NotNull(DecisionMaterialityEvaluator.GateEmit(draft, mat));
    }

    [Fact]
    public void Policy_Reuses_Ssot_Not_New_Magic_Numbers()
    {
        Assert.Contains("NO inventa umbrales", DecisionMaterialityPolicy.Definition);
        Assert.Contains("SalesVariationThresholds", DecisionMaterialityPolicy.Sources);
        Assert.Contains("InventoryHealthThresholds", DecisionMaterialityPolicy.Sources);
        Assert.Contains("10.8+", DecisionMaterialityPolicy.Deferred);
    }
}
