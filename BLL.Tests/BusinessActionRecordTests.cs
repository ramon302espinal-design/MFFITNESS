using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.3 — ActionRecord + ExpectedImpact (sin persistencia).</summary>
public class BusinessActionRecordTests
{
    [Fact]
    public void Create_Requires_Description_And_Sets_Ids()
    {
        Assert.Throws<ArgumentException>(() =>
            BusinessActionRecordFactory.Create(
                BusinessActionType.Promotion, "  "));

        var rec = BusinessActionRecordFactory.Create(
            BusinessActionType.Promotion,
            "Realizar promoción 2x1",
            area: DecisionEventArea.Capital,
            entityType: DecisionEntityType.Product,
            entityId: "42",
            entityName: "SKU-X",
            decisionEventId: Guid.NewGuid(),
            reason: "Ventas ↓ 32%",
            capitalInvolved: 25_000m,
            createdBy: "ana",
            expectedImpact: BusinessActionRecordFactory.Expected(
                "Reducir capital congelado.",
                ["capital.frozen", "sales.revenue"]),
            evaluationDays: 14,
            createdAt: new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc));

        Assert.NotEqual(Guid.Empty, rec.ActionId);
        Assert.Equal(BusinessActionType.Promotion, rec.ActionType);
        Assert.Equal(BusinessActionStatus.Pending, rec.Status);
        Assert.Equal("42", rec.EntityId);
        Assert.Equal("SKU-X", rec.EntityName);
        Assert.Equal("ana", rec.CreatedBy);
        Assert.Equal(25_000m, rec.CapitalInvolved);
        Assert.Equal(14, rec.EvaluationDays);
        Assert.Equal(new DateTime(2026, 9, 3, 12, 0, 0, DateTimeKind.Utc), rec.EvaluationDueAt);
        Assert.Null(rec.ActualImpact);
        Assert.Equal("Reducir capital congelado.", rec.ExpectedImpact!.Summary);
        Assert.Contains("capital.frozen", rec.ExpectedImpact.TargetMetricKeys);
    }

    [Fact]
    public void Expected_Rejects_Empty_Summary()
    {
        Assert.Throws<ArgumentException>(() =>
            BusinessActionRecordFactory.Expected(" "));
    }

    [Fact]
    public void Links_Optional_DecisionEvent_Without_Requiring_It()
    {
        var standalone = BusinessActionRecordFactory.Create(
            BusinessActionType.CostReview,
            "Revisar costos proveedor");

        Assert.Null(standalone.DecisionEventId);
        Assert.Equal(DecisionEventArea.Operations, standalone.Area);

        Guid decisionId = Guid.NewGuid();
        var linked = BusinessActionRecordFactory.Create(
            BusinessActionType.StockReduction,
            "Liquidar exceso",
            decisionEventId: decisionId,
            area: DecisionEventArea.Inventory);

        Assert.Equal(decisionId, linked.DecisionEventId);
    }

    [Fact]
    public void InsufficientData_ActualImpact_Has_No_Invented_Deltas()
    {
        // TEST 7/12 precursor
        var actual = BusinessActionRecordFactory.InsufficientData("baseline ausente");
        Assert.Equal(BusinessActionOutcome.InsufficientData, actual.Outcome);
        Assert.Empty(actual.Deltas);
        Assert.Contains("Sin datos", actual.Summary);
    }

    [Fact]
    public void InProgress_Sets_StartedAt_Pending_Does_Not()
    {
        var pending = BusinessActionRecordFactory.Create(
            BusinessActionType.Campaign, "Campaña redes",
            status: BusinessActionStatus.Pending);
        Assert.Null(pending.StartedAt);

        var running = BusinessActionRecordFactory.Create(
            BusinessActionType.Campaign, "Campaña redes",
            status: BusinessActionStatus.InProgress);
        Assert.NotNull(running.StartedAt);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("completa", BusinessActionRecordPolicy.Deferred);
        Assert.Contains("ExpectedImpact", BusinessActionRecordPolicy.ExpectedImpact);
        Assert.Contains("completa", BusinessActionPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionRecordFactory"));
    }
}
