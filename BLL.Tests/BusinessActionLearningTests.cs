using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.13 — Learning: tasas por tipo (agregados).</summary>
public class BusinessActionLearningTests
{
    private static BusinessActionRecord Completed(
        BusinessActionType type,
        BusinessActionOutcome outcome,
        DateTime? created = null)
    {
        var impact = new BusinessActionActualImpact
        {
            Outcome = outcome,
            Confidence = BusinessActionConfidence.Medium,
            Summary = "test"
        };
        return new BusinessActionRecord
        {
            ActionId = Guid.NewGuid(),
            ActionType = type,
            Status = BusinessActionStatus.Completed,
            Description = "t",
            CreatedAt = created ?? DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ActualImpact = impact
        };
    }

    [Fact]
    public void Rates_Match_Brief_Example_60_25_15()
    {
        var records = new List<BusinessActionRecord>();
        for (int i = 0; i < 60; i++)
            records.Add(Completed(BusinessActionType.Campaign, BusinessActionOutcome.Successful));
        for (int i = 0; i < 25; i++)
            records.Add(Completed(BusinessActionType.Campaign, BusinessActionOutcome.Partial));
        for (int i = 0; i < 15; i++)
            records.Add(Completed(BusinessActionType.Campaign, BusinessActionOutcome.Ineffective));

        BusinessActionTypeLearningStats stats =
            BusinessActionLearningComposer.BuildTypeStats(BusinessActionType.Campaign, records);

        Assert.Equal(100, stats.ClassifiedCount);
        Assert.Equal(60m, stats.SuccessRatePct);
        Assert.Equal(25m, stats.PartialRatePct);
        Assert.Equal(15m, stats.FailureRatePct);
        Assert.Contains("Histórico", stats.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("causó", stats.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancelled_And_NoResult_Excluded_From_Rates()
    {
        var records = new List<BusinessActionRecord>
        {
            Completed(BusinessActionType.Promotion, BusinessActionOutcome.Successful),
            Completed(BusinessActionType.Promotion, BusinessActionOutcome.Successful),
            new BusinessActionRecord
            {
                ActionId = Guid.NewGuid(),
                ActionType = BusinessActionType.Promotion,
                Status = BusinessActionStatus.Cancelled,
                Description = "x",
                CreatedAt = DateTime.UtcNow
            },
            new BusinessActionRecord
            {
                ActionId = Guid.NewGuid(),
                ActionType = BusinessActionType.Promotion,
                Status = BusinessActionStatus.NoResult,
                Description = "y",
                CreatedAt = DateTime.UtcNow
            }
        };

        BusinessActionTypeLearningStats stats =
            BusinessActionLearningComposer.BuildTypeStats(BusinessActionType.Promotion, records);

        Assert.Equal(4, stats.TotalCount);
        Assert.Equal(1, stats.CancelledCount);
        Assert.Equal(1, stats.NoResultCount);
        Assert.Equal(2, stats.ClassifiedCount);
        Assert.Equal(100m, stats.SuccessRatePct);
    }

    [Fact]
    public void InsufficientData_Not_In_Classified_Denominator()
    {
        var records = new[]
        {
            Completed(BusinessActionType.PriceChange, BusinessActionOutcome.Successful),
            Completed(BusinessActionType.PriceChange, BusinessActionOutcome.InsufficientData)
        };

        BusinessActionTypeLearningStats stats =
            BusinessActionLearningComposer.BuildTypeStats(BusinessActionType.PriceChange, records);

        Assert.Equal(1, stats.ClassifiedCount);
        Assert.Equal(1, stats.InsufficientDataCount);
        Assert.Equal(100m, stats.SuccessRatePct);
    }

    [Fact]
    public void Compose_Groups_By_Type_With_Soft_Narrative()
    {
        var records = new[]
        {
            Completed(BusinessActionType.Promotion, BusinessActionOutcome.Successful),
            Completed(BusinessActionType.Promotion, BusinessActionOutcome.Successful),
            Completed(BusinessActionType.Replenishment, BusinessActionOutcome.Ineffective),
            Completed(BusinessActionType.Replenishment, BusinessActionOutcome.Ineffective)
        };

        BusinessActionLearningSummary summary = BusinessActionLearningComposer.Compose(records);

        Assert.Equal(4, summary.TotalActions);
        Assert.Equal(4, summary.ClassifiedActions);
        Assert.Equal(50m, summary.OverallSuccessRatePct);
        Assert.Equal(2, summary.ByType.Count);
        Assert.Contains("histórica", summary.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no es una garantía", summary.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(BusinessActionLearningPolicy.Caution, summary.Caution);
    }

    [Fact]
    public void Service_Reads_From_Store()
    {
        var store = new InMemoryBusinessActionStore();
        var svc = new BusinessActionService(store);
        var learning = new BusinessActionLearningService(store);

        for (int i = 0; i < 3; i++)
        {
            var reg = svc.Register(new BusinessActionRegisterRequest
            {
                ActionType = BusinessActionType.Campaign,
                Description = $"c{i}",
                StartImmediately = true,
                CreatedBy = "ana"
            });
            svc.Complete(reg.Record!.ActionId, "ana");
            // Outcome via factory replace
            BusinessActionRecord? cur = store.FindByActionId(reg.Record.ActionId)!;
            store.Replace(BusinessActionRecordFactory.WithActualImpact(cur, new BusinessActionActualImpact
            {
                Outcome = i < 2 ? BusinessActionOutcome.Successful : BusinessActionOutcome.Partial,
                Confidence = BusinessActionConfidence.Low,
                Summary = "obs"
            }));
        }

        BusinessActionTypeLearningStats? stats = learning.GetByType(BusinessActionType.Campaign);
        Assert.NotNull(stats);
        Assert.Equal(3, stats!.ClassifiedCount);
        Assert.Equal(66.7m, stats.SuccessRatePct);
        Assert.Equal(33.3m, stats.PartialRatePct);
    }

    [Fact]
    public void Empty_Store_Soft_Message()
    {
        var summary = new BusinessActionLearningService(new InMemoryBusinessActionStore()).GetSummary();
        Assert.Equal(0, summary.TotalActions);
        Assert.Contains("Sin acciones", summary.Narrative);
        Assert.Null(summary.OverallSuccessRatePct);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.13", BusinessActionLearningPolicy.Definition);
        Assert.Contains("completa", BusinessActionLearningPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionLearningComposer"));
    }
}
