using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.10 — Capital liberado / incremento observado (lenguaje cauteloso).</summary>
public class BusinessActionCapitalImpactTests
{
    [Fact]
    public void LiberatedAmount_Only_When_Decrease()
    {
        var down = BusinessActionMetricDeltaMath.Compute(
            "capital.immobilized", null, 10_000m, 7_500m)!;
        Assert.Equal(2_500m, BusinessActionCapitalImpactComposer.LiberatedAmount(down));

        var up = BusinessActionMetricDeltaMath.Compute(
            "capital.immobilized", null, 5_000m, 6_000m)!;
        Assert.Null(BusinessActionCapitalImpactComposer.LiberatedAmount(up));
    }

    [Fact]
    public void IncreaseAmount_Only_When_Increase()
    {
        var up = BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 1_000m, 1_380m)!;
        Assert.Equal(380m, BusinessActionCapitalImpactComposer.IncreaseAmount(up));

        var down = BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 1_000m, 900m)!;
        Assert.Null(BusinessActionCapitalImpactComposer.IncreaseAmount(down));
    }

    [Fact]
    public void FromDeltas_Builds_Soft_Narrative_Without_Causality()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("capital.immobilized", null, 8_000m, 5_000m)!,
            BusinessActionMetricDeltaMath.Compute("capital.at_risk", null, 2_000m, 1_500m)!,
            BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 10_000m, 12_000m)!,
            BusinessActionMetricDeltaMath.Compute("profit.margin_pct", null, 20m, 23m)!
        };

        BusinessActionObservedCapitalImpact impact =
            BusinessActionCapitalImpactComposer.FromDeltas(deltas);

        Assert.True(impact.HasAnySignal);
        Assert.Equal(3_000m, impact.LiberatedImmobilized);
        Assert.Equal(500m, impact.LiberatedAtRisk);
        Assert.Equal(3_500m, impact.TotalLiberatedCapital);
        Assert.Equal(2_000m, impact.ObservedRevenueIncrease);
        Assert.Equal(3m, impact.ObservedMarginChangePp);

        Assert.Contains("se observó", impact.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("incremento observado", impact.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No se atribuye causalidad", impact.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("causó", impact.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("la acción liberó", impact.Narrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No atribuir causalidad", impact.Caution, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Inventory_Only_Fills_Total_When_No_Immobilized_Or_Risk()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("capital.inventory", null, 4_000m, 3_000m)!
        };

        var impact = BusinessActionCapitalImpactComposer.FromDeltas(deltas);
        Assert.Equal(1_000m, impact.LiberatedInventoryCapital);
        Assert.Equal(1_000m, impact.TotalLiberatedCapital);
    }

    [Fact]
    public void Evaluate_Includes_CapitalImpact_In_Result()
    {
        var store = new InMemoryBusinessActionStore();
        var actions = new BusinessActionService(store);
        var eval = new BusinessActionEvaluationService(store);

        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest(),
            new Dictionary<string, decimal?>
            {
                ["capital.immobilized"] = 9_000m,
                ["sales.revenue"] = 5_000m
            });

        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.StockReduction,
            Description = "Liquidar",
            Baseline = baseline,
            StartImmediately = true
        });
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 5);
        actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(5),
            MetricValues = new Dictionary<string, decimal?>
            {
                ["capital.immobilized"] = 6_000m,
                ["sales.revenue"] = 5_500m
            }
        });

        var r = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            AsOfUtc = completed.AddDays(5)
        });

        Assert.True(r.Success);
        Assert.NotNull(r.CapitalImpact);
        Assert.Equal(3_000m, r.CapitalImpact!.LiberatedImmobilized);
        Assert.Equal(500m, r.CapitalImpact.ObservedRevenueIncrease);
        Assert.Contains("liberación aparente", r.Summary, StringComparison.OrdinalIgnoreCase);

        var viaGet = eval.GetCapitalImpact(reg.Record.ActionId);
        Assert.Equal(3_000m, viaGet!.LiberatedImmobilized);
    }

    [Fact]
    public void Empty_Deltas_Has_No_Signal()
    {
        var impact = BusinessActionCapitalImpactComposer.FromDeltas(Array.Empty<BusinessActionMetricDelta>());
        Assert.False(impact.HasAnySignal);
        Assert.Contains("Sin deltas", impact.Narrative);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.10", BusinessActionCapitalImpactPolicy.Definition);
        Assert.Contains("causalidad", BusinessActionCapitalImpactPolicy.Caution, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("completa", BusinessActionCapitalImpactPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionCapitalImpactComposer"));
    }
}
