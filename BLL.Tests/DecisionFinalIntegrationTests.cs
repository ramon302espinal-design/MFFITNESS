using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 10.28 — integración final / criterios de éxito brief §129.
/// Pipeline completo en memoria: sin auto-compra.
/// </summary>
public class DecisionFinalIntegrationTests
{
    private static SalesVariationReport DeclineVar()
        => new()
        {
            Revenue = SalesVariationMath.Label(-30m),
            RealizedProfit = SalesVariationMath.Label(-12m),
            Units = SalesVariationMath.Label(null),
            Transactions = SalesVariationMath.Label(null),
            Ticket = SalesVariationMath.Label(null),
            CrossSignals =
            [
                new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpProfitDown,
                    Message = "contradicción test"
                }
            ]
        };

    private static DecisionAnalyticsBundleHooks Hooks()
        => new()
        {
            LoadSalesVariation = (_, _) => DeclineVar(),
            LoadSalesShare = (_, _) => null,
            LoadInventoryAlerts = _ => new InventoryAlertReport
            {
                Alerts =
                [
                    new InventoryAlert
                    {
                        Kind = InventoryAlertKind.FrozenCapital,
                        ProductId = 7,
                        ProductName = "SKU-7",
                        CapitalAmount = 25_000m,
                        Priority = InventoryAlertPriority.High,
                        Message = "frozen"
                    }
                ],
                TotalAlerts = 1,
                ImmobilizedCapital = 25_000m
            },
            LoadCapitalBridge = (_, _) => new SalesCapitalBridgeReport
            {
                Rows =
                [
                    new SalesCapitalBridgeRow
                    {
                        ProductId = 7,
                        ProductName = "SKU-7",
                        InventoryCapital = 25_000m,
                        RevenueChangePct = -32m,
                        RoiChangePct = -18m,
                        RoiPct = 8m,
                        Trend = ProductTrendDirection.Declining,
                        Signals =
                        [
                            new SalesCapitalSignal
                            {
                                Kind = SalesCapitalSignalKind.CapitalRisk,
                                Message = "capital riesgo"
                            }
                        ],
                        PrimarySignal = SalesCapitalSignalKind.CapitalRisk
                    }
                ]
            },
            LoadProductClassification = (_, _) => new ProductClassificationReport
            {
                Rows =
                [
                    new ProductClassificationRow
                    {
                        ProductId = 3,
                        ProductName = "Rising",
                        Class = ProductPerformanceClass.Opportunity,
                        Trend = ProductTrendDirection.Growing,
                        Reasons = ["opp"],
                        Performance = new ProductPerformanceRow
                        {
                            ProductId = 3,
                            ProductName = "Rising",
                            InventoryCapital = 2_000m,
                            RevenueTotal = 15_000m
                        }
                    }
                ]
            },
            LoadStarMix = (_, _) => new SalesStarMixReport
            {
                StarsWithStockoutRisk =
                [
                    new SalesStarContributionRow
                    {
                        ProductId = 1,
                        ProductName = "Estrella",
                        RevenueTotal = 40_000m,
                        FlagStockoutRisk = true
                    }
                ]
            },
            LoadStockRisk = (_, _) => null,
            LoadAcceleration = (_, _) => null,
            LoadSeriesTrend = (_, _) => null,
            LoadForecast = (_, _) => null,
            LoadTrappedCapital = () => null,
            LoadInvestmentSummaries = () => Array.Empty<InvestmentSummary>()
        };

    [Fact]
    public void Success_01_to_04_Explain_Prioritize_Group_Recommend()
    {
        var result = new DecisionIntegrationService().RunInMemory(
            new DecisionRuleContext { PeriodKey = "final", PeriodKind = ProfitPeriodKind.ThisMonth },
            snapshot: new DecisionCenterSnapshot
            {
                SalesVariationPct = -30m,
                FrozenCapitalAmount = 25_000m
            },
            hooks: Hooks());

        DecisionCenterReport center = result.Center;
        Assert.NotNull(center.Engine);
        Assert.True(center.Summary.TotalEvents >= 1);

        // 1 Explicar — eventos con título/descripcion/recomendación
        Assert.All(center.Engine!.Events, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Title));
            Assert.False(string.IsNullOrWhiteSpace(e.Recommendation));
            Assert.False(DecisionSoftLanguageGuard.ContainsForbidden(e.Recommendation));
        });

        // 2 Priorizar
        Assert.NotEmpty(center.PrioritiesToday);
        Assert.NotNull(center.TopPriority);
        Assert.Equal(1, center.PrioritiesToday[0].Rank);

        // 3 Agrupar
        Assert.NotEmpty(center.Groups);

        // 4 Recomendar
        Assert.NotEmpty(center.Recommendations);
        Assert.All(center.Recommendations, r =>
            Assert.False(DecisionSoftLanguageGuard.ContainsForbidden(r.Body)));
    }

    [Fact]
    public void Success_05_to_08_Register_History_Dedup_Audit()
    {
        var store = new InMemoryDecisionHistoryStore();
        var audit = new InMemoryDecisionAuditStore();
        var history = new DecisionHistoryService(store, audit);
        var res = new DecisionResolutionService(store, audit);

        var integration = new DecisionIntegrationService(history: history);
        var ctx = new DecisionRuleContext { PeriodKey = "final", PeriodKind = ProfitPeriodKind.ThisMonth };

        DecisionIntegrationResult first = integration.RunAndPersist(history, ctx, hooks: Hooks());
        Assert.True(first.Persisted);
        Assert.True(first.Capture!.Inserted >= 1);

        // 7 Dedup — segundo capture no duplica abiertos
        DecisionIntegrationResult second = integration.RunAndPersist(history, ctx, hooks: Hooks());
        Assert.Equal(0, second.Capture!.Inserted);
        Assert.True(second.Capture.SkippedActiveDuplicate >= 1);

        // 5 Registrar estado — Ignore
        Guid id = first.Center.Engine!.Events[0].EventId;
        Assert.True(res.Ignore(id, "ana", "ruido").Success);

        // 6 Historial conserva Ignored
        Assert.Contains(history.GetHistory(), r => r.Status == DecisionEventStatus.Ignored);

        // 8 Trazabilidad — auditoría
        Assert.Contains(audit.Query(new DecisionAuditQuery { Top = 50 }),
            a => a.Action is DecisionAuditAction.Detected or DecisionAuditAction.Ignore
                 or DecisionAuditAction.DuplicateSuppressed);
    }

    [Fact]
    public void Success_09_AreaHint_For_Forms()
    {
        var center = new DecisionIntegrationService().RunInMemory(
            new DecisionRuleContext { PeriodKey = "p", PeriodKind = ProfitPeriodKind.ThisMonth },
            hooks: Hooks()).Center;

        string salesHint = DecisionIntegrationService.AreaPriorityHint(
            center, DecisionEventArea.Sales, DecisionEventArea.Profit);
        Assert.False(string.IsNullOrWhiteSpace(salesHint));

        string empty = DecisionIntegrationService.AreaPriorityHint(
            center, DecisionEventArea.Liquidity);
        // puede ser vacío si no hay señales liquidity — OK
        Assert.NotNull(empty);
    }

    [Fact]
    public void Success_10_Architecture_No_Forbidden_Language_And_Phase_Complete()
    {
        Assert.True(DecisionPhasePolicy.IsComplete);
        Assert.Contains("10.28", DecisionPhasePolicy.Stage);
        Assert.Contains("completa", DecisionPhasePolicy.Deferred);
        Assert.Contains("completa", DecisionCenterPolicy.Deferred);
        Assert.Contains("completa", DecisionPerformancePolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("DecisionIntegrationService"));
        Assert.NotNull(DecisionSourceMap.Find("CrmDecisionUiBinder"));
        Assert.NotNull(DecisionSourceMap.Find("FrmAnaDecisiones"));
        Assert.NotNull(DecisionSourceMap.Find("FrmAnaDashboard"));
        Assert.NotNull(DecisionSourceMap.Find("FrmReportes"));
        Assert.Contains("NO TOCAR", DecisionSourceMap.Find("FrmReportes")!.MustNot);
    }

    [Fact]
    public void Reconcile_Closes_When_Condition_Gone()
    {
        var history = new DecisionHistoryService(new InMemoryDecisionHistoryStore());
        var svc = new DecisionIntegrationService();
        var ctx = new DecisionRuleContext { PeriodKey = "p", PeriodKind = ProfitPeriodKind.ThisMonth };

        svc.RunAndPersist(history, ctx, hooks: Hooks());
        Assert.True(history.GetMetrics().ActiveCount >= 1);

        // Run vacío → ReconcileAbsent
        var emptyHooks = new DecisionAnalyticsBundleHooks
        {
            LoadSalesVariation = (_, _) => null,
            LoadSalesShare = (_, _) => null,
            LoadInventoryAlerts = _ => null,
            LoadCapitalBridge = (_, _) => null,
            LoadProductClassification = (_, _) => null,
            LoadStarMix = (_, _) => null,
            LoadStockRisk = (_, _) => null,
            LoadAcceleration = (_, _) => null,
            LoadSeriesTrend = (_, _) => null,
            LoadForecast = (_, _) => null,
            LoadTrappedCapital = () => null,
            LoadInvestmentSummaries = () => Array.Empty<InvestmentSummary>()
        };

        DecisionIntegrationResult gone = svc.RunAndPersist(history, ctx, hooks: emptyHooks);
        Assert.True(gone.Reconcile!.ResolvedAbsent >= 1);
        Assert.Equal(0, history.GetMetrics().ActiveCount);
    }
}
