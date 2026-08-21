using BLL.Models.Crm;
using BLL.Services.Crm;
using DL;

namespace BLL.Tests;

/// <summary>FASE 11.4 — persistencia ActionRecord (mapper + contrato; sin DB en CI).</summary>
public class BusinessActionPersistenceTests
{
    [Fact]
    public void RoundTrip_Mapper_Preserves_Core_Fields()
    {
        var original = BusinessActionRecordFactory.Create(
            BusinessActionType.Promotion,
            "Promoción fin de mes",
            area: DecisionEventArea.Capital,
            entityType: DecisionEntityType.Product,
            entityId: "7",
            entityName: "SKU-7",
            decisionEventId: Guid.NewGuid(),
            reason: "Capital congelado",
            capitalInvolved: 25_000m,
            createdBy: "ana",
            expectedImpact: BusinessActionRecordFactory.Expected(
                "Reducir capital congelado.",
                ["capital.frozen", "sales.revenue"]),
            evaluationDays: 14,
            createdAt: new DateTime(2026, 8, 20, 15, 0, 0, DateTimeKind.Utc));

        BusinessActionRow row = BusinessActionPersistenceMapper.ToRow(original, id: 99);
        Assert.Equal(99, row.Id);
        Assert.Equal((int)BusinessActionType.Promotion, row.ActionType);
        Assert.Equal("capital.frozen|sales.revenue", row.ExpectedMetricKeys);
        Assert.Null(row.Outcome);

        BusinessActionRecord back = BusinessActionPersistenceMapper.FromRow(row);
        Assert.Equal(original.ActionId, back.ActionId);
        Assert.Equal(original.DecisionEventId, back.DecisionEventId);
        Assert.Equal(original.ActionType, back.ActionType);
        Assert.Equal(original.EntityId, back.EntityId);
        Assert.Equal(original.CapitalInvolved, back.CapitalInvolved);
        Assert.Equal(original.CreatedBy, back.CreatedBy);
        Assert.Equal(original.EvaluationDueAt, back.EvaluationDueAt);
        Assert.Equal("Reducir capital congelado.", back.ExpectedImpact!.Summary);
        Assert.Equal(2, back.ExpectedImpact.TargetMetricKeys.Count);
        Assert.Null(back.ActualImpact);
    }

    [Fact]
    public void RoundTrip_With_ActualImpact_InsufficientData()
    {
        var rec = BusinessActionRecordFactory.Create(
            BusinessActionType.PriceChange,
            "Bajar precio 10%",
            status: BusinessActionStatus.Completed);

        // Simula evaluación SinDatos (TEST 7/12)
        rec = new BusinessActionRecord
        {
            ActionId = rec.ActionId,
            ActionType = rec.ActionType,
            Area = rec.Area,
            EntityType = rec.EntityType,
            Description = rec.Description,
            CreatedAt = rec.CreatedAt,
            Status = BusinessActionStatus.Completed,
            CompletedAt = rec.CreatedAt,
            CompletedBy = "ana",
            ActualImpact = BusinessActionRecordFactory.InsufficientData("sin baseline")
        };

        var back = BusinessActionPersistenceMapper.FromRow(
            BusinessActionPersistenceMapper.ToRow(rec));

        Assert.Equal(BusinessActionOutcome.InsufficientData, back.ActualImpact!.Outcome);
        Assert.Empty(back.ActualImpact.Deltas);
        Assert.Contains("Sin datos", back.ActualImpact.Summary);
    }

    [Fact]
    public void MetricKeys_Join_Split()
    {
        Assert.Null(BusinessActionPersistenceMapper.JoinKeys(null));
        Assert.Equal("a|b", BusinessActionPersistenceMapper.JoinKeys([" a ", "b"]));
        Assert.Equal(["a", "b"], BusinessActionPersistenceMapper.SplitKeys("a|b"));
        Assert.Empty(BusinessActionPersistenceMapper.SplitKeys(" "));
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("SchemaVersion 14", BusinessActionPersistencePolicy.Definition);
        Assert.Contains("completa", BusinessActionPersistencePolicy.Deferred);
        Assert.Contains("completa", BusinessActionRecordPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("CrmBusinessActionDAL"));
    }
}
