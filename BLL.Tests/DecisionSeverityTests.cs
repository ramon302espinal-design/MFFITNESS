using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.5 — severidad por impacto real.</summary>
public class DecisionSeverityTests
{
    [Fact]
    public void Levels_Match_Brief_Order()
    {
        Assert.True(DecisionSeverity.Info < DecisionSeverity.Low);
        Assert.True(DecisionSeverity.Low < DecisionSeverity.Medium);
        Assert.True(DecisionSeverity.Medium < DecisionSeverity.High);
        Assert.True(DecisionSeverity.High < DecisionSeverity.Critical);
    }

    [Fact]
    public void Catalog_Display_Names_Are_Spanish_Labels()
    {
        Assert.Equal("INFO", DecisionSeverityCatalog.DisplayName(DecisionSeverity.Info));
        Assert.Equal("BAJA", DecisionSeverityCatalog.DisplayName(DecisionSeverity.Low));
        Assert.Equal("MEDIA", DecisionSeverityCatalog.DisplayName(DecisionSeverity.Medium));
        Assert.Equal("ALTA", DecisionSeverityCatalog.DisplayName(DecisionSeverity.High));
        Assert.Equal("CRÍTICA", DecisionSeverityCatalog.DisplayName(DecisionSeverity.Critical));
    }

    [Fact]
    public void Resolve_Maps_Max_Impact_To_Severity()
    {
        var impact = new DecisionImpactAssessment
        {
            Sales = DecisionImpactLevel.Medium,
            Capital = DecisionImpactLevel.High,
            Financial = DecisionImpactLevel.Low
        };
        Assert.Equal(DecisionSeverity.High, DecisionSeverityResolver.Resolve(impact));
    }

    [Fact]
    public void Insufficient_Data_Is_Info_Not_Advanced_Alert()
    {
        var impact = new DecisionImpactAssessment
        {
            InsufficientData = true,
            Capital = DecisionImpactLevel.Critical
        };
        Assert.Equal(DecisionSeverity.Info, DecisionSeverityResolver.Resolve(impact));
    }

    [Fact]
    public void High_Frozen_Capital_But_Still_Selling_Is_Not_Auto_Critical()
    {
        // TEST 11 / brief §15
        var impact = new DecisionImpactAssessment
        {
            Capital = DecisionImpactLevel.Critical,
            Liquidity = DecisionImpactLevel.High,
            Sales = DecisionImpactLevel.Low,
            ProductStillSelling = true
        };
        Assert.Equal(DecisionSeverity.High, DecisionSeverityResolver.Resolve(impact));
    }

    [Fact]
    public void Critical_Capital_With_Sales_Collapse_Stays_Critical()
    {
        var impact = new DecisionImpactAssessment
        {
            Capital = DecisionImpactLevel.Critical,
            Sales = DecisionImpactLevel.High,
            Financial = DecisionImpactLevel.Critical,
            ProductStillSelling = false
        };
        Assert.Equal(DecisionSeverity.Critical, DecisionSeverityResolver.Resolve(impact));
    }

    [Fact]
    public void Seasonal_Sales_Shock_Does_Not_Force_Critical_Alone()
    {
        // TEST 12 — contexto estacional amortigua
        var impact = new DecisionImpactAssessment
        {
            Sales = DecisionImpactLevel.Critical,
            SeasonalContext = true,
            Capital = DecisionImpactLevel.Low,
            Financial = DecisionImpactLevel.High
        };
        Assert.Equal(DecisionSeverity.High, DecisionSeverityResolver.Resolve(impact));
    }

    [Fact]
    public void Apply_Sets_Severity_Without_Changing_Priority()
    {
        var e = DecisionEventFactory.Create(
            "capital.frozen", DecisionEventArea.Capital,
            DecisionEntityType.Product, "1", "X",
            "asOf", "UnitTest", "Capital congelado", "Frozen");

        // Priority remains Unspecified (10.6)
        Assert.Equal(DecisionPriority.Unspecified, e.Priority);

        var applied = DecisionSeverityResolver.Apply(e, new DecisionImpactAssessment
        {
            Capital = DecisionImpactLevel.Medium
        });

        Assert.Equal(DecisionSeverity.Medium, applied.Severity);
        Assert.Equal(DecisionPriority.Unspecified, applied.Priority);
        Assert.Equal(e.Fingerprint, applied.Fingerprint);
        Assert.Equal(e.EventId, applied.EventId);
    }

    [Fact]
    public void Policy_Separates_Severity_From_Priority()
    {
        Assert.Contains("SEVERIDAD ≠ PRIORIDAD", DecisionSeverityPolicy.Separation);
        Assert.Contains("IMPACTO ≠ URGENCIA", DecisionSeverityPolicy.ImpactVsUrgency);
        Assert.Contains("10.6", DecisionSeverityPolicy.Deferred);
        Assert.Contains("impacto real", DecisionSeverityPolicy.Definition);
    }

    [Fact]
    public void Severity_Is_Independent_Of_Priority_Conceptually()
    {
        // High severity can coexist with unspecified/low priority until 10.6
        var e = DecisionEventFactory.Create(
            "sales.strong_decline", DecisionEventArea.Sales,
            DecisionEntityType.Portfolio, null, "",
            "ThisMonth", "UnitTest", "Caída", "↓");

        var applied = DecisionSeverityResolver.Apply(e, new DecisionImpactAssessment
        {
            Sales = DecisionImpactLevel.High,
            Financial = DecisionImpactLevel.High
        });

        Assert.Equal(DecisionSeverity.High, applied.Severity);
        Assert.True(DecisionSeverityCatalog.Rank(applied.Severity) >= DecisionSeverityCatalog.Rank(DecisionSeverity.High));
        Assert.Equal(DecisionPriority.Unspecified, applied.Priority);
    }
}
