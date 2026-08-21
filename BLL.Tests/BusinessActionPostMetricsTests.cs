using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.8 — Post-métricas + variación (% / pp margen).</summary>
public class BusinessActionPostMetricsTests
{
    private static BusinessActionService Svc()
        => new(new InMemoryBusinessActionStore());

    [Fact]
    public void Margin_Uses_Percentage_Points_Not_Relative()
    {
        // 22 → 25 = +3 pp (no +13.636%)
        BusinessActionMetricDelta? d = BusinessActionMetricDeltaMath.Compute(
            "profit.margin_pct", "Margen %", 22m, 25m);

        Assert.NotNull(d);
        Assert.True(d!.IsPercentagePoints);
        Assert.Equal(3m, d.Change);
    }

    [Fact]
    public void Revenue_Uses_Relative_Percent()
    {
        BusinessActionMetricDelta? d = BusinessActionMetricDeltaMath.Compute(
            "sales.revenue", "Ingresos", 1000m, 1380m);

        Assert.NotNull(d);
        Assert.False(d!.IsPercentagePoints);
        Assert.Equal(38m, d.Change);
    }

    [Fact]
    public void RelativePct_When_Before_Zero_Is_Null()
    {
        Assert.Null(BusinessActionMetricDeltaMath.RelativePct(0m, 50m));
        Assert.Null(BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 0m, 50m)!.Change);
    }

    [Fact]
    public void Compare_Baseline_Vs_Post_Builds_Deltas()
    {
        var before = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest { EntityType = DecisionEntityType.Portfolio },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 10_000m,
                ["profit.margin_pct"] = 20m,
                ["capital.immobilized"] = 5_000m
            });

        var after = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest { EntityType = DecisionEntityType.Portfolio },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 12_000m,
                ["profit.margin_pct"] = 23m,
                ["capital.immobilized"] = 4_000m
            });

        IReadOnlyList<BusinessActionMetricDelta> deltas =
            BusinessActionMetricDeltaMath.Compare(before, after);

        Assert.Equal(3, deltas.Count);
        Assert.Equal(20m, Find(deltas, "sales.revenue")!.Change);
        Assert.Equal(3m, Find(deltas, "profit.margin_pct")!.Change);
        Assert.True(Find(deltas, "profit.margin_pct")!.IsPercentagePoints);
        Assert.Equal(-20m, Find(deltas, "capital.immobilized")!.Change);

        string summary = BusinessActionMetricDeltaMath.BuildObservedSummary(deltas);
        Assert.StartsWith("Durante el período posterior se observó:", summary);
        Assert.DoesNotContain("causó", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Codec_RoundTrip_Deltas()
    {
        var deltas = new List<BusinessActionMetricDelta>
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", "Ingresos", 100m, 110m)!,
            BusinessActionMetricDeltaMath.Compute("profit.margin_pct", "Margen %", 22m, 25m)!
        };

        string? encoded = BusinessActionDeltaCodec.Encode(deltas);
        IReadOnlyList<BusinessActionMetricDelta> back = BusinessActionDeltaCodec.Decode(encoded);

        Assert.Equal(2, back.Count);
        Assert.Equal(10m, Find(back, "sales.revenue")!.Change);
        Assert.True(Find(back, "profit.margin_pct")!.IsPercentagePoints);
        Assert.Equal(3m, Find(back, "profit.margin_pct")!.Change);
    }

    [Fact]
    public void Mapper_Persists_DeltasPayload()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", "Ingresos", 1m, 2m)!
        };
        var rec = BusinessActionRecordFactory.Create(
            BusinessActionType.Promotion,
            "Promo",
            status: BusinessActionStatus.Completed);
        rec = BusinessActionRecordFactory.WithActualImpact(
            rec, BusinessActionRecordFactory.ObservedDeltas(deltas));

        BusinessActionRecord back = BusinessActionPersistenceMapper.FromRow(
            BusinessActionPersistenceMapper.ToRow(rec));

        Assert.Single(back.ActualImpact!.Deltas);
        Assert.Equal(100m, back.ActualImpact.Deltas[0].Change);
        Assert.Equal(BusinessActionOutcome.Unspecified, back.ActualImpact.Outcome);
    }

    [Fact]
    public void Service_CapturePostMetrics_Computes_And_Stores()
    {
        var svc = Svc();
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest(),
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 8_000m,
                ["profit.margin_pct"] = 18m
            });

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Campaña",
            Baseline = baseline,
            StartImmediately = true
        });
        svc.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 7);

        var cap = svc.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(7),
            MetricValues = new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 10_400m,
                ["profit.margin_pct"] = 21m
            }
        });

        Assert.True(cap.Success);
        Assert.Equal(2, cap.Deltas!.Count);
        Assert.Equal(30m, Find(cap.Deltas, "sales.revenue")!.Change);
        Assert.Equal(3m, Find(cap.Deltas, "profit.margin_pct")!.Change);
        Assert.Equal(BusinessActionOutcome.Unspecified, cap.Record!.ActualImpact!.Outcome);
        Assert.Contains("se observó", cap.Record.ActualImpact.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Service_Blocks_Capture_While_InWindow_Unless_Allowed()
    {
        var svc = Svc();
        var completed = new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest(),
            new Dictionary<string, decimal?> { ["sales.revenue"] = 1m });

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "X",
            Baseline = baseline,
            StartImmediately = true
        });
        svc.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 14);

        var blocked = svc.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(3),
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 2m }
        });
        Assert.False(blocked.Success);
        Assert.Contains("Ventana", blocked.Message);

        var early = svc.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(3),
            AllowBeforeWindowEnd = true,
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 2m }
        });
        Assert.True(early.Success);
    }

    [Fact]
    public void Service_Without_Baseline_Sets_InsufficientData()
    {
        var svc = Svc();
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "Precio",
            StartImmediately = true
        });
        svc.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 1);

        var cap = svc.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(1),
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 9m }
        });

        Assert.True(cap.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, cap.Record!.ActualImpact!.Outcome);
        Assert.Empty(cap.Record.ActualImpact.Deltas);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.8", BusinessActionPostMetricsPolicy.Definition);
        Assert.Contains("pp", BusinessActionPostMetricsPolicy.PpRule);
        Assert.Contains("completa", BusinessActionPostMetricsPolicy.Deferred);
        Assert.Contains("11.8", BusinessActionServicePolicy.Definition);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionMetricDeltaMath"));
    }

    private static BusinessActionMetricDelta? Find(
        IReadOnlyList<BusinessActionMetricDelta> deltas,
        string key)
        => deltas.FirstOrDefault(d =>
            string.Equals(d.MetricKey, key, StringComparison.OrdinalIgnoreCase));
}
