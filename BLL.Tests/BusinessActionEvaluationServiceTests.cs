using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.9 — ActionEvaluationService (Outcome + Confidence).</summary>
public class BusinessActionEvaluationServiceTests
{
    private static (BusinessActionService Actions, BusinessActionEvaluationService Eval)
        Svc()
    {
        var store = new InMemoryBusinessActionStore();
        return (new BusinessActionService(store), new BusinessActionEvaluationService(store));
    }

    private static BusinessActionRecord ReadyActionWithDeltas(
        BusinessActionService actions,
        BusinessActionType type,
        Dictionary<string, decimal?> before,
        Dictionary<string, decimal?> after,
        IReadOnlyList<string>? targetKeys = null)
    {
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest(), before);

        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = type,
            Description = "Acción test",
            Baseline = baseline,
            StartImmediately = true,
            ExpectedImpact = targetKeys == null
                ? null
                : BusinessActionRecordFactory.Expected("Mejorar métricas.", targetKeys)
        });

        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 7);
        actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(7),
            MetricValues = after
        });

        return actions.Get(reg.Record.ActionId)!;
    }

    [Fact]
    public void DesiredSign_Capital_Down_Is_Favorable()
    {
        Assert.Equal(-1, BusinessActionEvaluationMath.DesiredSign("capital.immobilized", BusinessActionType.Promotion));
        Assert.Equal(+1, BusinessActionEvaluationMath.DesiredSign("sales.revenue", BusinessActionType.Promotion));
        Assert.Equal(-1, BusinessActionEvaluationMath.DesiredSign("inv.stock", BusinessActionType.StockReduction));
        Assert.Equal(+1, BusinessActionEvaluationMath.DesiredSign("inv.stock", BusinessActionType.Replenishment));
    }

    [Fact]
    public void Classify_All_Favorable_Is_Successful()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 100m, 120m)!,
            BusinessActionMetricDeltaMath.Compute("profit.margin_pct", null, 20m, 23m)!
        };

        var (outcome, fav, unfav, _) =
            BusinessActionEvaluationMath.Classify(deltas, BusinessActionType.Campaign);

        Assert.Equal(BusinessActionOutcome.Successful, outcome);
        Assert.Equal(2, fav);
        Assert.Equal(0, unfav);
    }

    [Fact]
    public void Classify_Mixed_Is_Partial()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 100m, 120m)!,
            BusinessActionMetricDeltaMath.Compute("capital.immobilized", null, 1000m, 1500m)! // ↑ malo
        };

        var (outcome, fav, unfav, _) =
            BusinessActionEvaluationMath.Classify(deltas, BusinessActionType.Promotion);

        Assert.Equal(BusinessActionOutcome.Partial, outcome);
        Assert.Equal(1, fav);
        Assert.Equal(1, unfav);
    }

    [Fact]
    public void Classify_All_Unfavorable_Is_Ineffective()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 100m, 80m)!,
            BusinessActionMetricDeltaMath.Compute("profit.realized", null, 50m, 40m)!
        };

        var (outcome, _, unfav, _) =
            BusinessActionEvaluationMath.Classify(deltas, BusinessActionType.Campaign);

        Assert.Equal(BusinessActionOutcome.Ineffective, outcome);
        Assert.True(unfav >= 1);
    }

    [Fact]
    public void Classify_No_Signals_Is_InsufficientData()
    {
        var (outcome, _, _, _) =
            BusinessActionEvaluationMath.Classify(Array.Empty<BusinessActionMetricDelta>(), BusinessActionType.Other);
        Assert.Equal(BusinessActionOutcome.InsufficientData, outcome);
    }

    [Fact]
    public void Evaluate_Persists_Successful_Outcome()
    {
        var (actions, eval) = Svc();
        var rec = ReadyActionWithDeltas(
            actions,
            BusinessActionType.Campaign,
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 10_000m,
                ["profit.margin_pct"] = 18m
            },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 12_000m,
                ["profit.margin_pct"] = 21m
            });

        BusinessActionEvaluationResult r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = rec.ActionId,
            AsOfUtc = rec.EvaluationDueAt,
            Actor = "ana"
        });

        Assert.True(r.Success);
        Assert.Equal(BusinessActionOutcome.Successful, r.Outcome);
        Assert.Equal(BusinessActionOutcome.Successful, r.Record!.ActualImpact!.Outcome);
        Assert.NotEqual(BusinessActionConfidence.Unspecified, r.Confidence);
        Assert.Contains("se observó", r.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("causó", r.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ana", r.Record.ActualImpact.Notes!);
    }

    [Fact]
    public void Evaluate_Partial_When_Mixed()
    {
        var (actions, eval) = Svc();
        var rec = ReadyActionWithDeltas(
            actions,
            BusinessActionType.Promotion,
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 5_000m,
                ["capital.immobilized"] = 2_000m
            },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 6_000m,
                ["capital.immobilized"] = 2_500m
            });

        var r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = rec.ActionId,
            AsOfUtc = rec.EvaluationDueAt
        });

        Assert.True(r.Success);
        Assert.Equal(BusinessActionOutcome.Partial, r.Outcome);
        Assert.Equal(1, r.FavorableCount);
        Assert.Equal(1, r.UnfavorableCount);
    }

    [Fact]
    public void Test08_Cancelled_Cannot_Evaluate_Successful()
    {
        var (actions, eval) = Svc();
        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "X"
        });
        actions.Cancel(reg.Record!.ActionId);

        var r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            OverrideOutcome = BusinessActionOutcome.Successful
        });

        Assert.False(r.Success);
        Assert.Contains("Cancelada", r.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Test07_Without_Deltas_Is_InsufficientData()
    {
        var (actions, eval) = Svc();
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "Sin baseline",
            StartImmediately = true
        });
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 1);

        // CapturePostMetrics sin baseline → InsufficientData ya; Evaluate confirma
        actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(1),
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 1m }
        });

        var r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            AsOfUtc = completed.AddDays(1)
        });

        Assert.True(r.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, r.Outcome);
        Assert.Equal(BusinessActionConfidence.Low, r.Confidence);
    }

    [Fact]
    public void Manual_Override_Outcome_Is_Respected()
    {
        var (actions, eval) = Svc();
        var rec = ReadyActionWithDeltas(
            actions,
            BusinessActionType.Campaign,
            new Dictionary<string, decimal?> { ["sales.revenue"] = 100m },
            new Dictionary<string, decimal?> { ["sales.revenue"] = 130m });

        var r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = rec.ActionId,
            AsOfUtc = rec.EvaluationDueAt,
            OverrideOutcome = BusinessActionOutcome.Partial,
            OverrideConfidence = BusinessActionConfidence.Low,
            Notes = "revisión manual"
        });

        Assert.True(r.Success);
        Assert.True(r.UsedOverride);
        Assert.Equal(BusinessActionOutcome.Partial, r.Outcome);
        Assert.Equal(BusinessActionConfidence.Low, r.Confidence);
        Assert.Contains("manual", r.Record!.ActualImpact!.Notes!);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.9", BusinessActionEvaluationPolicy.Definition);
        Assert.Contains("TEST 8", BusinessActionEvaluationPolicy.Definition);
        Assert.Contains("completa", BusinessActionEvaluationPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionEvaluationService"));
    }
}
