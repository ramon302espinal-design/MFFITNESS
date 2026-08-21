using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.27 — performance: una carga SSOT, reglas en memoria (brief §88).</summary>
public class DecisionPerformanceTests
{
    private static SalesVariationReport DeclineVar()
        => new()
        {
            Revenue = SalesVariationMath.Label(-30m),
            RealizedProfit = SalesVariationMath.Label(-5m),
            Units = SalesVariationMath.Label(null),
            Transactions = SalesVariationMath.Label(null),
            Ticket = SalesVariationMath.Label(null),
            CrossSignals = Array.Empty<SalesCrossSignal>()
        };

    [Fact]
    public void Loader_Calls_Each_Source_Exactly_Once()
    {
        int salesVar = 0, share = 0, inv = 0, bridge = 0, cls = 0, stars = 0,
            stock = 0, accel = 0, series = 0, forecast = 0, trapped = 0, summaries = 0;

        var hooks = new DecisionAnalyticsBundleHooks
        {
            LoadSalesVariation = (_, _) => { salesVar++; return DeclineVar(); },
            LoadSalesShare = (_, _) => { share++; return null; },
            LoadInventoryAlerts = _ => { inv++; return null; },
            LoadCapitalBridge = (_, _) => { bridge++; return null; },
            LoadProductClassification = (_, _) => { cls++; return null; },
            LoadStarMix = (_, _) => { stars++; return null; },
            LoadStockRisk = (_, _) => { stock++; return null; },
            LoadAcceleration = (_, _) => { accel++; return null; },
            LoadSeriesTrend = (_, _) => { series++; return null; },
            LoadForecast = (_, _) => { forecast++; return null; },
            LoadTrappedCapital = () => { trapped++; return null; },
            LoadInvestmentSummaries = () => { summaries++; return Array.Empty<InvestmentSummary>(); }
        };

        DecisionAnalyticsBundle bundle = DecisionAnalyticsBundleLoader.Load(
            ProfitPeriodKind.ThisMonth, hooks: hooks);

        Assert.Equal(12, bundle.Stats.ServiceCalls);
        Assert.Equal(1, salesVar);
        Assert.Equal(1, share);
        Assert.Equal(1, inv);
        Assert.Equal(1, bridge);
        Assert.Equal(1, cls);
        Assert.Equal(1, stars);
        Assert.Equal(1, stock);
        Assert.Equal(1, accel);
        Assert.Equal(1, series);
        Assert.Equal(1, forecast);
        Assert.Equal(1, trapped);
        Assert.Equal(1, summaries);
        Assert.Equal(12, bundle.Stats.SourcesLoaded.Count);
        Assert.Contains("SalesVariation", bundle.Stats.SourcesLoaded);
    }

    [Fact]
    public void With_Analytics_Rules_Do_Not_Hit_Per_Rule_Loaders()
    {
        int salesLoads = 0, profitLoads = 0, invLoads = 0, capitalLoads = 0,
            roiLoads = 0, stockLoads = 0;

        var bundle = new DecisionAnalyticsBundle
        {
            SalesVariation = DeclineVar(),
            InventoryAlerts = new InventoryAlertReport
            {
                Alerts =
                [
                    new InventoryAlert
                    {
                        Kind = InventoryAlertKind.FrozenCapital,
                        ProductId = 1,
                        ProductName = "X",
                        CapitalAmount = 12_000m,
                        Priority = InventoryAlertPriority.High,
                        Message = "frozen"
                    }
                ],
                TotalAlerts = 1,
                ImmobilizedCapital = 12_000m
            },
            CapitalBridge = new SalesCapitalBridgeReport
            {
                Rows =
                [
                    new SalesCapitalBridgeRow
                    {
                        ProductId = 1,
                        ProductName = "X",
                        InventoryCapital = 12_000m,
                        RevenueChangePct = -20m,
                        RoiChangePct = -18m,
                        RoiPct = 10m,
                        Trend = ProductTrendDirection.Declining,
                        Signals =
                        [
                            new SalesCapitalSignal
                            {
                                Kind = SalesCapitalSignalKind.CapitalRisk,
                                Message = "risk"
                            }
                        ],
                        PrimarySignal = SalesCapitalSignalKind.CapitalRisk
                    }
                ]
            },
            StockRisk = new SalesStockRiskReport { Rows = Array.Empty<SalesStockSignalRow>() },
            Stats = new DecisionAnalyticsLoadStats { ServiceCalls = 0 }
        };

        var ctx = new DecisionRuleContext
        {
            PeriodKind = ProfitPeriodKind.ThisMonth,
            PeriodKey = "perf",
            Analytics = bundle
        };

        var rules = new IDecisionRule[]
        {
            new SalesAlertDecisionRule(
                (_, _) => { salesLoads++; return null; },
                (_, _) => null),
            new ProfitAlertDecisionRule(
                (_, _) => { profitLoads++; return null; }),
            new InventoryAlertDecisionRule(
                _ => { invLoads++; return null; },
                (_, _) => { stockLoads++; return null; }),
            new CapitalAlertDecisionRule(
                _ => { capitalLoads++; return null; },
                (_, _) => { capitalLoads++; return null; }),
            new RoiAlertDecisionRule(
                (_, _) => { roiLoads++; return null; })
        };

        var report = new DecisionEngine().Run(rules, ctx);

        Assert.Equal(0, salesLoads);
        Assert.Equal(0, profitLoads);
        Assert.Equal(0, invLoads);
        Assert.Equal(0, capitalLoads);
        Assert.Equal(0, roiLoads);
        Assert.Equal(0, stockLoads);
        Assert.True(report.EmittedCount >= 1);
        Assert.Contains(report.Events, e => e.EventType == "sales.strong_decline");
    }

    [Fact]
    public void Shared_Sources_Not_Multiplied_By_Rule_Count()
    {
        // Sin bundle: Inventory+Capital cargarían InventoryAlerts 2×; con bundle = 1×
        int invAlertCalls = 0;
        int bridgeCalls = 0;

        var hooks = new DecisionAnalyticsBundleHooks
        {
            LoadSalesVariation = (_, _) => DeclineVar(),
            LoadSalesShare = (_, _) => null,
            LoadInventoryAlerts = _ =>
            {
                invAlertCalls++;
                return new InventoryAlertReport { Alerts = Array.Empty<InventoryAlert>(), TotalAlerts = 0 };
            },
            LoadCapitalBridge = (_, _) =>
            {
                bridgeCalls++;
                return new SalesCapitalBridgeReport { Rows = Array.Empty<SalesCapitalBridgeRow>() };
            },
            LoadProductClassification = (_, _) => null,
            LoadStarMix = (_, _) => null,
            LoadStockRisk = (_, _) => null,
            LoadAcceleration = (_, _) => null,
            LoadSeriesTrend = (_, _) => null,
            LoadForecast = (_, _) => null,
            LoadTrappedCapital = () => null,
            LoadInvestmentSummaries = () => Array.Empty<InvestmentSummary>()
        };

        DecisionAnalyticsBundle bundle = DecisionAnalyticsBundleLoader.Load(hooks: hooks);
        var ctx = DecisionAnalyticsBundleLoader.WithAnalytics(
            new DecisionRuleContext { PeriodKey = "p", PeriodKind = ProfitPeriodKind.ThisMonth },
            bundle);

        // 9 built-in rules; shared loaders must stay at 1
        _ = new DecisionEngine().Run(DecisionRuleRegistry.BuiltIn, ctx);

        Assert.Equal(1, invAlertCalls);
        Assert.Equal(1, bridgeCalls);
    }

    [Fact]
    public void RunBuiltIn_Preloads_Via_Hooks_Once()
    {
        int calls = 0;
        var hooks = new DecisionAnalyticsBundleHooks
        {
            LoadSalesVariation = (_, _) => { calls++; return DeclineVar(); },
            LoadSalesShare = (_, _) => { calls++; return null; },
            LoadInventoryAlerts = _ => { calls++; return null; },
            LoadCapitalBridge = (_, _) => { calls++; return null; },
            LoadProductClassification = (_, _) => { calls++; return null; },
            LoadStarMix = (_, _) => { calls++; return null; },
            LoadStockRisk = (_, _) => { calls++; return null; },
            LoadAcceleration = (_, _) => { calls++; return null; },
            LoadSeriesTrend = (_, _) => { calls++; return null; },
            LoadForecast = (_, _) => { calls++; return null; },
            LoadTrappedCapital = () => { calls++; return null; },
            LoadInvestmentSummaries = () => { calls++; return Array.Empty<InvestmentSummary>(); }
        };

        var center = new DecisionCenterService().RunBuiltIn(
            new DecisionRuleContext { PeriodKey = "p", PeriodKind = ProfitPeriodKind.ThisMonth },
            analyticsHooks: hooks);

        Assert.Equal(12, calls);
        Assert.NotNull(center);
        Assert.True(center.Summary.TotalEvents >= 1);
    }

    [Fact]
    public void EnsureAnalytics_Is_Idempotent()
    {
        int loads = 0;
        var hooks = new DecisionAnalyticsBundleHooks
        {
            LoadSalesVariation = (_, _) => { loads++; return DeclineVar(); },
            LoadSalesShare = (_, _) => { loads++; return null; },
            LoadInventoryAlerts = _ => { loads++; return null; },
            LoadCapitalBridge = (_, _) => { loads++; return null; },
            LoadProductClassification = (_, _) => { loads++; return null; },
            LoadStarMix = (_, _) => { loads++; return null; },
            LoadStockRisk = (_, _) => { loads++; return null; },
            LoadAcceleration = (_, _) => { loads++; return null; },
            LoadSeriesTrend = (_, _) => { loads++; return null; },
            LoadForecast = (_, _) => { loads++; return null; },
            LoadTrappedCapital = () => { loads++; return null; },
            LoadInvestmentSummaries = () => { loads++; return Array.Empty<InvestmentSummary>(); }
        };

        var ctx0 = new DecisionRuleContext { PeriodKind = ProfitPeriodKind.ThisMonth };
        var ctx1 = DecisionAnalyticsBundleLoader.EnsureAnalytics(ctx0, hooks);
        var ctx2 = DecisionAnalyticsBundleLoader.EnsureAnalytics(ctx1, hooks);

        Assert.Equal(12, loads);
        Assert.Same(ctx1.Analytics, ctx2.Analytics);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("UNA vez", DecisionPerformancePolicy.Definition);
        Assert.Contains("completa", DecisionPerformancePolicy.Deferred);
        Assert.Contains("completa", DecisionCenterPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("DecisionAnalyticsBundleLoader"));
    }
}
