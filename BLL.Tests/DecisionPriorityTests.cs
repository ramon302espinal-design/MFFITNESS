using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.6 — prioridad de revisión (≠ severidad).</summary>
public class DecisionPriorityTests
{
    [Fact]
    public void Levels_Match_Brief_Order()
    {
        Assert.True(DecisionPriority.Info < DecisionPriority.Low);
        Assert.True(DecisionPriority.Low < DecisionPriority.Medium);
        Assert.True(DecisionPriority.Medium < DecisionPriority.High);
        Assert.True(DecisionPriority.High < DecisionPriority.Critical);
    }

    [Fact]
    public void Catalog_Uses_Informativa_Label()
    {
        Assert.Equal("INFORMATIVA", DecisionPriorityCatalog.DisplayName(DecisionPriority.Info));
        Assert.Equal("CRÍTICA", DecisionPriorityCatalog.DisplayName(DecisionPriority.Critical));
    }

    [Fact]
    public void High_Severity_Without_Immediate_Action_Is_Medium_Priority()
    {
        // brief §12
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.High,
            Urgency = DecisionUrgencyLevel.None,
            RequiresImmediateReview = false
        });
        Assert.Equal(DecisionPriority.Medium, p);
    }

    [Fact]
    public void High_Severity_With_Medium_Urgency_Is_Medium_Priority()
    {
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.High,
            Urgency = DecisionUrgencyLevel.Medium
        });
        Assert.Equal(DecisionPriority.Medium, p);
    }

    [Fact]
    public void Frozen_Capital_Still_Selling_Caps_At_Medium()
    {
        // TEST 11 / §15 — impacto alto, urgencia media
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.High,
            Urgency = DecisionUrgencyLevel.High,
            ProductStillSelling = true
        });
        Assert.Equal(DecisionPriority.Medium, p);
    }

    [Fact]
    public void Stockout_Time_Sensitive_Is_At_Least_High()
    {
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.Medium,
            Urgency = DecisionUrgencyLevel.Low,
            TimeSensitiveStockout = true
        });
        Assert.Equal(DecisionPriority.High, p);
    }

    [Fact]
    public void Critical_Severity_Plus_Stockout_Is_Critical_Priority()
    {
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.Critical,
            TimeSensitiveStockout = true
        });
        Assert.Equal(DecisionPriority.Critical, p);
    }

    [Fact]
    public void Opportunity_Window_Not_Buried_As_Info()
    {
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.Medium,
            Urgency = DecisionUrgencyLevel.Low,
            OpportunityWindow = true
        });
        Assert.Equal(DecisionPriority.Medium, p);
    }

    [Fact]
    public void Insufficient_Data_Is_Informativa()
    {
        var p = DecisionPriorityResolver.Resolve(new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.Critical,
            InsufficientData = true
        });
        Assert.Equal(DecisionPriority.Info, p);
    }

    [Fact]
    public void Apply_Sets_Priority_Keeping_Fingerprint()
    {
        var e = DecisionEventFactory.Create(
            "inv.stockout_risk", DecisionEventArea.Inventory,
            DecisionEntityType.Product, "3", "Star",
            "asOf", "UnitTest", "Quiebre", "Stock bajo");

        var withSev = DecisionSeverityResolver.Apply(e, new DecisionImpactAssessment
        {
            Inventory = DecisionImpactLevel.Critical,
            Sales = DecisionImpactLevel.High
        });

        var withPri = DecisionPriorityResolver.Apply(withSev, new DecisionPriorityAssessment
        {
            Severity = withSev.Severity,
            Urgency = DecisionUrgencyLevel.Immediate,
            TimeSensitiveStockout = true
        });

        Assert.Equal(DecisionSeverity.Critical, withPri.Severity);
        Assert.Equal(DecisionPriority.Critical, withPri.Priority);
        Assert.Equal(e.Fingerprint, withPri.Fingerprint);
    }

    [Fact]
    public void Severity_And_Priority_Can_Diverge()
    {
        var e = DecisionEventFactory.Create(
            "capital.frozen", DecisionEventArea.Capital,
            DecisionEntityType.Product, "1", "X",
            "asOf", "UnitTest", "Congelado", "Frozen");

        var applied = DecisionPriorityResolver.Apply(e, new DecisionPriorityAssessment
        {
            Severity = DecisionSeverity.High,
            Urgency = DecisionUrgencyLevel.Medium,
            ProductStillSelling = true
        });

        Assert.Equal(DecisionSeverity.High, applied.Severity);
        Assert.Equal(DecisionPriority.Medium, applied.Priority);
        Assert.True(DecisionSeverityCatalog.Rank(applied.Severity)
                    > DecisionPriorityCatalog.Rank(applied.Priority));
    }

    [Fact]
    public void Policy_Separates_Priority_From_Severity()
    {
        Assert.Contains("PRIORIDAD ≠ SEVERIDAD", DecisionPriorityPolicy.Separation);
        Assert.Contains("URGENCIA", DecisionPriorityPolicy.Urgency);
        Assert.Contains("10.7", DecisionPriorityPolicy.Deferred);
    }
}
