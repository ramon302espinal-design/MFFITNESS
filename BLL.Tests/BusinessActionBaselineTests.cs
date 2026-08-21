using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.6 — Baseline snapshot (métricas mínimas SSOT).</summary>
public class BusinessActionBaselineTests
{
    [Fact]
    public void FromMetricValues_Captures_Minimal_Set()
    {
        var values = new Dictionary<string, decimal?>
        {
            ["sales.revenue"] = 50_000m,
            ["profit.realized"] = 12_000m,
            ["profit.margin_pct"] = 24m,
            ["inv.stock"] = 80m,
            ["capital.inventory"] = 40_000m,
            ["capital.immobilized"] = 15_000m
        };

        BusinessActionBaseline baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest
            {
                EntityType = DecisionEntityType.Portfolio,
                CapturedAt = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc),
                PeriodKind = ProfitPeriodKind.ThisMonth
            },
            values,
            "TestHook");

        Assert.True(baseline.HasMetrics);
        Assert.Equal(6, baseline.Metrics.Count);
        Assert.Equal(50_000m, Find(baseline, "sales.revenue"));
        Assert.Equal(24m, Find(baseline, "profit.margin_pct"));
        Assert.Equal("TestHook", baseline.SourceNote);
        Assert.All(baseline.Metrics, m => Assert.False(string.IsNullOrWhiteSpace(m.Label)));
    }

    [Fact]
    public void FromCapitalBridge_Product_Uses_Row_Not_Totals()
    {
        var bridge = new SalesCapitalBridgeReport
        {
            PeriodKind = ProfitPeriodKind.ThisMonth,
            TotalRevenue = 999_999m,
            TotalRealizedProfit = 1m,
            TotalInventoryCapital = 1m,
            TotalImmobilizedCapital = 1m,
            Rows =
            [
                new SalesCapitalBridgeRow
                {
                    ProductId = 7,
                    ProductName = "SKU-7",
                    RevenueTotal = 8_000m,
                    RealizedProfit = 2_000m,
                    MarginPct = 25m,
                    Stock = 40,
                    InventoryCapital = 5_000m,
                    ImmobilizedCapital = 1_200m
                }
            ]
        };

        BusinessActionBaseline baseline = BusinessActionBaselineComposer.FromCapitalBridge(
            new BusinessActionBaselineCaptureRequest
            {
                EntityType = DecisionEntityType.Product,
                EntityId = "7",
                CapturedAt = DateTime.UtcNow
            },
            bridge);

        Assert.Equal(8_000m, Find(baseline, "sales.revenue"));
        Assert.Equal(40m, Find(baseline, "inv.stock"));
        Assert.Equal(5_000m, Find(baseline, "capital.inventory"));
        Assert.DoesNotContain(baseline.Metrics, m => m.MetricKey == "capital.at_risk");
        Assert.Equal(BusinessActionBaselineComposer.CapitalBridgeSource, baseline.SourceNote);
    }

    [Fact]
    public void FromAnalytics_Without_Bridge_Yields_Empty_Metrics()
    {
        BusinessActionBaseline baseline = BusinessActionBaselineComposer.FromAnalytics(
            new BusinessActionBaselineCaptureRequest { EntityType = DecisionEntityType.Portfolio },
            new DecisionAnalyticsBundle { PeriodKind = ProfitPeriodKind.Last7Days });

        Assert.False(baseline.HasMetrics);
        Assert.Contains("insuficientes", baseline.SourceNote!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Codec_RoundTrip_Preserves_Metrics()
    {
        var original = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest
            {
                EntityType = DecisionEntityType.Product,
                EntityId = "42",
                PeriodKind = ProfitPeriodKind.ThisMonth,
                CapturedAt = new DateTime(2026, 8, 21, 9, 30, 0, DateTimeKind.Utc)
            },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 1_234.56m,
                ["capital.immobilized"] = 900m
            },
            "note with pipes");

        string? encoded = BusinessActionBaselineCodec.Encode(original);
        Assert.False(string.IsNullOrWhiteSpace(encoded));

        BusinessActionBaseline? back = BusinessActionBaselineCodec.Decode(encoded);
        Assert.NotNull(back);
        Assert.Equal(original.EntityId, back!.EntityId);
        Assert.Equal(original.PeriodKind, back.PeriodKind);
        Assert.Equal(original.CapturedAt, back.CapturedAt);
        Assert.Equal(original.SourceNote, back.SourceNote);
        Assert.Equal(2, back.Metrics.Count);
        Assert.Equal(1_234.56m, Find(back, "sales.revenue"));
        Assert.Equal(900m, Find(back, "capital.immobilized"));
    }

    [Fact]
    public void Mapper_Persists_BaselinePayload()
    {
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest
            {
                EntityType = DecisionEntityType.Portfolio,
                CapturedAt = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Dictionary<string, decimal?> { ["sales.revenue"] = 10m });

        var rec = BusinessActionRecordFactory.Create(
            BusinessActionType.Promotion,
            "Promo",
            baseline: baseline);

        BusinessActionRecord back = BusinessActionPersistenceMapper.FromRow(
            BusinessActionPersistenceMapper.ToRow(rec));

        Assert.NotNull(back.Baseline);
        Assert.Equal(10m, Find(back.Baseline!, "sales.revenue"));
    }

    [Fact]
    public void Service_Register_With_CaptureBaseline_Attaches_Snapshot()
    {
        var svc = new BusinessActionService(new InMemoryBusinessActionStore());
        var r = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "Ajuste precio",
            CaptureBaseline = true,
            MetricValues = new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 3_000m,
                ["capital.inventory"] = 7_500m
            }
        });

        Assert.True(r.Success);
        Assert.NotNull(r.Record!.Baseline);
        Assert.Equal(3_000m, Find(r.Record.Baseline!, "sales.revenue"));
    }

    [Fact]
    public void Service_CaptureBaseline_After_Register_And_Preserves_On_Start()
    {
        var svc = new BusinessActionService(new InMemoryBusinessActionStore());
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Campaña",
            EntityType = DecisionEntityType.Portfolio
        });

        Assert.Null(reg.Record!.Baseline);

        var cap = svc.CaptureBaseline(new BusinessActionBaselineRequest
        {
            ActionId = reg.Record.ActionId,
            MetricValues = new Dictionary<string, decimal?>
            {
                ["profit.margin_pct"] = 18.5m,
                ["capital.at_risk"] = 2_000m
            }
        });

        Assert.True(cap.Success);
        Assert.Equal(2, cap.Record!.Baseline!.Metrics.Count);

        Assert.True(svc.Start(reg.Record.ActionId, "ana").Success);
        BusinessActionRecord mid = svc.Get(reg.Record.ActionId)!;
        Assert.Equal(BusinessActionStatus.InProgress, mid.Status);
        Assert.Equal(18.5m, Find(mid.Baseline!, "profit.margin_pct"));
    }

    [Fact]
    public void Service_Rejects_Baseline_On_Terminal_Status()
    {
        var svc = new BusinessActionService(new InMemoryBusinessActionStore());
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "X"
        });
        Assert.True(svc.Cancel(reg.Record!.ActionId).Success);

        var cap = svc.AttachBaseline(
            reg.Record.ActionId,
            BusinessActionBaselineComposer.FromMetricValues(
                new BusinessActionBaselineCaptureRequest(),
                new Dictionary<string, decimal?> { ["sales.revenue"] = 1m }));

        Assert.False(cap.Success);
        Assert.Contains("terminal", cap.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static decimal? Find(BusinessActionBaseline baseline, string key)
        => baseline.Metrics.FirstOrDefault(m =>
            string.Equals(m.MetricKey, key, StringComparison.OrdinalIgnoreCase))?.Value;
}
