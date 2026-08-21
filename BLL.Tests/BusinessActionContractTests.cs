using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.2 — contratos ActionType / Status / Outcome / Confidence.</summary>
public class BusinessActionContractTests
{
    [Fact]
    public void Catalog_Covers_Brief_Types_With_Unique_Codes()
    {
        var codes = BusinessActionCatalog.All.Select(d => d.Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.Promotion));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.PriceChange));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.Replenishment));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.StockReduction));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.MixChange));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.Campaign));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.PurchasePause));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.CostReview));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.MarginReview));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.StrategyChange));
        Assert.NotNull(BusinessActionCatalog.Find(BusinessActionType.Other));

        Assert.Equal(11, BusinessActionCatalog.All.Count);
    }

    [Fact]
    public void FindByCode_Is_Case_Insensitive()
    {
        Assert.Equal(BusinessActionType.Promotion,
            BusinessActionCatalog.FindByCode("PROMOTION")!.Type);
        Assert.Equal("Promoción", BusinessActionCatalog.DisplayName(BusinessActionType.Promotion));
    }

    [Fact]
    public void Status_And_Outcome_Labels_Match_Brief()
    {
        Assert.Equal("PENDIENTE", BusinessActionCatalog.StatusLabel(BusinessActionStatus.Pending));
        Assert.Equal("EN PROCESO", BusinessActionCatalog.StatusLabel(BusinessActionStatus.InProgress));
        Assert.Equal("COMPLETADA", BusinessActionCatalog.StatusLabel(BusinessActionStatus.Completed));
        Assert.Equal("CANCELADA", BusinessActionCatalog.StatusLabel(BusinessActionStatus.Cancelled));
        Assert.Equal("SIN RESULTADO", BusinessActionCatalog.StatusLabel(BusinessActionStatus.NoResult));

        Assert.Equal("EXITOSA", BusinessActionCatalog.OutcomeLabel(BusinessActionOutcome.Successful));
        Assert.Equal("PARCIAL", BusinessActionCatalog.OutcomeLabel(BusinessActionOutcome.Partial));
        Assert.Equal("NO EFECTIVA", BusinessActionCatalog.OutcomeLabel(BusinessActionOutcome.Ineffective));
        Assert.Equal("SIN DATOS", BusinessActionCatalog.OutcomeLabel(BusinessActionOutcome.InsufficientData));

        Assert.Equal("MEDIA", BusinessActionCatalog.ConfidenceLabel(BusinessActionConfidence.Medium));
        Assert.Contains("🟢", BusinessActionCatalog.OutcomeGlyph(BusinessActionOutcome.Successful));
    }

    [Fact]
    public void Cancelled_Is_Not_Evaluable_As_Successful()
    {
        // TEST 8 precursor
        Assert.False(BusinessActionCatalog.IsEvaluable(BusinessActionStatus.Cancelled));
        Assert.False(BusinessActionCatalog.IsEvaluable(BusinessActionStatus.Pending));
        Assert.False(BusinessActionCatalog.IsEvaluable(BusinessActionStatus.InProgress));
        Assert.True(BusinessActionCatalog.IsEvaluable(BusinessActionStatus.Completed));

        Assert.False(BusinessActionCatalog.CanAssignOutcome(
            BusinessActionStatus.Cancelled, BusinessActionOutcome.Successful));
        Assert.True(BusinessActionCatalog.CanAssignOutcome(
            BusinessActionStatus.Completed, BusinessActionOutcome.Successful));
        Assert.True(BusinessActionCatalog.CanAssignOutcome(
            BusinessActionStatus.Completed, BusinessActionOutcome.InsufficientData));
    }

    [Fact]
    public void Types_That_Imply_Pos_Change_Are_Flagged_Manual_Only()
    {
        Assert.True(BusinessActionCatalog.Find(BusinessActionType.Promotion)!.ImpliesManualPosChange);
        Assert.True(BusinessActionCatalog.Find(BusinessActionType.PriceChange)!.ImpliesManualPosChange);
        Assert.True(BusinessActionCatalog.Find(BusinessActionType.Replenishment)!.ImpliesManualPosChange);
        Assert.False(BusinessActionCatalog.Find(BusinessActionType.CostReview)!.ImpliesManualPosChange);
        Assert.False(BusinessActionCatalog.Find(BusinessActionType.PurchasePause)!.ImpliesManualPosChange);
    }

    [Fact]
    public void Policy_Separates_From_Decision_Resolution_And_Defers_Record()
    {
        Assert.Contains("≠ DecisionResolutionAction", BusinessActionPolicy.Definition);
        Assert.Contains("PROHIBIDO", BusinessActionPolicy.NoAutomation);
        Assert.Contains("Se observó", BusinessActionPolicy.Causality);
        Assert.Contains("completa", BusinessActionPolicy.Deferred);
        Assert.Contains("11.6", BusinessActionBaselinePolicy.Definition);
        Assert.DoesNotContain("Machine Learning", BusinessActionPolicy.Definition);
    }

    [Fact]
    public void SourceMap_Includes_Catalog()
    {
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionCatalog"));
    }
}
