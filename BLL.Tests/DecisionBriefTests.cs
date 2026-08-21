using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 10.26 — batería del brief §119 (TEST 1–15).
/// Sin BD: motor + reglas + historial en memoria. Sin auto-compra.
/// </summary>
public class DecisionBriefTests
{
    private static SalesVariationReport SalesVar(
        decimal? revenueVar,
        decimal? profitVar = null,
        decimal? marginVar = null,
        IReadOnlyList<SalesCrossSignal>? crosses = null)
    {
        return new SalesVariationReport
        {
            Revenue = SalesVariationMath.Label(revenueVar),
            RealizedProfit = SalesVariationMath.Label(profitVar),
            Units = SalesVariationMath.Label(null),
            Transactions = SalesVariationMath.Label(null),
            Ticket = SalesVariationMath.Label(null),
            Margin = marginVar.HasValue ? SalesVariationMath.Label(marginVar) : null,
            CrossSignals = crosses ?? Array.Empty<SalesCrossSignal>()
        };
    }

    private static DecisionRuleCandidate DeclineCand(decimal varPct, string period = "p")
        => new()
        {
            RuleId = "brief.sales",
            EventType = "sales.strong_decline",
            Area = DecisionEventArea.Sales,
            EntityType = DecisionEntityType.Portfolio,
            PeriodKey = period,
            Title = "Caída de ventas",
            Description = $"Ingresos {varPct:N0}%",
            Recommendation = "Revisar mezcla y demanda.",
            Materiality = new DecisionMaterialityInput { VariationPct = varPct },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Sales = DecisionImpactLevel.High,
                Financial = DecisionImpactLevel.High
            },
            Urgency = DecisionUrgencyLevel.Medium
        };

    // TEST 1: Ventas ↓ 30% → alerta
    [Fact]
    public void Test01_SalesDown30_GeneratesAlert()
    {
        var rule = new SalesAlertDecisionRule(
            (_, _) => SalesVar(-30m),
            (_, _) => null);

        var report = new DecisionEngine().Run(
            [rule],
            new DecisionRuleContext { PeriodKey = "ThisMonth|brief" });

        Assert.Equal(1, report.EmittedCount);
        Assert.Equal("sales.strong_decline", report.Events[0].EventType);
        Assert.True(report.Events[0].Severity >= DecisionSeverity.High);
        Assert.Contains("Revisar", report.Events[0].Recommendation);
        Assert.False(DecisionSoftLanguageGuard.ContainsForbidden(report.Events[0].Recommendation));
    }

    // TEST 2: Ventas ↑ 20% / Ganancia ↓ 10% → contradicción
    [Fact]
    public void Test02_SalesUp_ProfitDown_DetectsContradiction()
    {
        var variations = SalesVar(
            20m,
            -10m,
            crosses:
            [
                new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpProfitDown,
                    Message = "Crecimiento de ventas sin crecimiento de ganancia"
                }
            ]);

        var candidates = SalesAlertRuleComposer.FromVariation(variations, "p");
        Assert.Contains(candidates, c => c.EventType == "sales.rev_up_profit_down");
        Assert.Contains(candidates, c => c.Materiality.CrossSignal);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Contains(report.Events, e => e.EventType == "sales.rev_up_profit_down");
    }

    // TEST 3: Stock alto + Ventas ↓ → capital en riesgo
    [Fact]
    public void Test03_HighStock_SalesDown_CapitalAtRisk()
    {
        var bridge = new SalesCapitalBridgeReport
        {
            Rows =
            [
                new SalesCapitalBridgeRow
                {
                    ProductId = 10,
                    ProductName = "OverstockSKU",
                    InventoryCapital = 25_000m,
                    RevenueChangePct = -32m,
                    Trend = ProductTrendDirection.Declining,
                    Signals =
                    [
                        new SalesCapitalSignal
                        {
                            Kind = SalesCapitalSignalKind.CapitalRisk,
                            Message = "Capital en riesgo"
                        }
                    ],
                    PrimarySignal = SalesCapitalSignalKind.CapitalRisk
                }
            ]
        };

        var candidates = CapitalAlertRuleComposer.FromCapitalBridge(bridge, "p");
        Assert.Single(candidates);
        Assert.Equal("capital.at_risk", candidates[0].EventType);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.Equal(DecisionEventArea.Capital, report.Events[0].Area);
    }

    // TEST 4: Estrella + stock crítico → quiebre
    [Fact]
    public void Test04_Star_CriticalStock_StockoutRisk()
    {
        var mix = new SalesStarMixReport
        {
            StarsWithStockoutRisk =
            [
                new SalesStarContributionRow
                {
                    ProductId = 1,
                    ProductName = "Estrella",
                    RevenueTotal = 50_000m,
                    FlagStockoutRisk = true
                }
            ]
        };

        var candidates = ProductAlertRuleComposer.FromStarMix(mix, "p");
        Assert.Equal("product.star_stockout", candidates[0].EventType);
        Assert.True(candidates[0].TimeSensitiveStockout);

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Priority >= DecisionPriority.High);
    }

    // TEST 5: Creciente + stock saludable → oportunidad
    [Fact]
    public void Test05_Growing_HealthyStock_Opportunity()
    {
        var report = new ProductClassificationReport
        {
            Rows =
            [
                new ProductClassificationRow
                {
                    ProductId = 3,
                    ProductName = "Rising",
                    Class = ProductPerformanceClass.Opportunity,
                    Trend = ProductTrendDirection.Growing,
                    Reasons = ["crecimiento"],
                    Performance = new ProductPerformanceRow
                    {
                        ProductId = 3,
                        ProductName = "Rising",
                        InventoryCapital = 2_000m,
                        RevenueTotal = 12_000m
                    }
                }
            ]
        };

        var candidates = ProductAlertRuleComposer.FromClassification(report, "p");
        Assert.Single(candidates);
        Assert.Equal("product.growth_opportunity", candidates[0].EventType);
        Assert.True(candidates[0].OpportunityWindow);

        var engine = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, engine.EmittedCount);
    }

    // TEST 6: ROI ↓ → deterioro
    [Fact]
    public void Test06_RoiDecline_Deterioration()
    {
        var bridge = new SalesCapitalBridgeReport
        {
            Rows =
            [
                new SalesCapitalBridgeRow
                {
                    ProductId = 2,
                    ProductName = "SKU-B",
                    RevenueChangePct = -5m,
                    RoiChangePct = -18m,
                    RoiPct = 22m,
                    InventoryCapital = 12_000m,
                    Signals = Array.Empty<SalesCapitalSignal>(),
                    PrimarySignal = SalesCapitalSignalKind.None
                }
            ]
        };

        var candidates = RoiAlertRuleComposer.FromBridge(bridge, "p");
        Assert.Single(candidates);
        Assert.Equal("roi.deterioration", candidates[0].EventType);

        var engine = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, engine.EmittedCount);
        Assert.Equal(DecisionEventArea.Roi, engine.Events[0].Area);
    }

    // TEST 7: Datos insuficientes → NO alerta avanzada
    [Fact]
    public void Test07_InsufficientData_NoAdvancedAlert()
    {
        var c = new DecisionRuleCandidate
        {
            RuleId = "brief.insuf",
            EventType = "product.insufficient_data",
            Area = DecisionEventArea.Product,
            EntityType = DecisionEntityType.Product,
            EntityId = "9",
            PeriodKey = "asOf",
            Title = "Nuevo",
            Description = "Sin historial",
            Materiality = new DecisionMaterialityInput
            {
                InsufficientData = true,
                VariationPct = -40m,
                CapitalAmount = 50_000m
            }
        };

        var report = new DecisionEngine().Evaluate([c]);
        Assert.Equal(0, report.EmittedCount);
        Assert.Equal(1, report.SuppressedByMateriality);
    }

    // TEST 8: Misma alerta repetida → NO duplicarse
    [Fact]
    public void Test08_SameAlert_DoesNotDuplicate()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var engine = new DecisionEngine().Evaluate([DeclineCand(-30m)]);

        Assert.Equal(1, history.Capture(engine).Inserted);
        Assert.Equal(0, history.Capture(engine).Inserted);
        Assert.Equal(1, history.Capture(engine).SkippedActiveDuplicate);
        Assert.Single(history.GetHistory());
    }

    // TEST 9: Condición deja de existir → resolver
    [Fact]
    public void Test09_ConditionGone_ReconcilesToResolved()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);

        var withAlert = new DecisionEngine().Evaluate([DeclineCand(-30m)]);
        history.Capture(withAlert);
        Assert.Equal(1, history.GetMetrics().ActiveCount);

        // Misma huella ausente (run sin candidatos) → Resolved
        var empty = new DecisionEngine().Evaluate([]);
        DecisionHistoryReconcileResult rec = history.ReconcileAbsent(empty);

        Assert.Equal(1, rec.ResolvedAbsent);
        Assert.Equal(0, history.GetMetrics().ActiveCount);
        Assert.Equal(1, history.GetMetrics().ResolvedCount);

        DecisionHistoryRecord row = history.GetHistory().Single();
        Assert.Equal(DecisionEventStatus.Resolved, row.Status);
        Assert.Equal("system", row.ResolvedBy);
        Assert.Contains("Condición", row.ResolutionNote);
    }

    // TEST 10: Varias alertas relacionadas → agruparse
    [Fact]
    public void Test10_RelatedAlerts_GroupTogether()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            new DecisionRuleCandidate
            {
                RuleId = "b",
                EventType = "inv.stockout_risk",
                Area = DecisionEventArea.Inventory,
                EntityType = DecisionEntityType.Product,
                EntityId = "42",
                EntityName = "SKU-X",
                PeriodKey = "p",
                Title = "Quiebre",
                Description = "d",
                Recommendation = "Evaluar reposición — no comprar automáticamente.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate,
                RequiresImmediateReview = true
            },
            new DecisionRuleCandidate
            {
                RuleId = "b",
                EventType = "capital.at_risk",
                Area = DecisionEventArea.Capital,
                EntityType = DecisionEntityType.Product,
                EntityId = "42",
                EntityName = "SKU-X",
                PeriodKey = "p",
                Title = "Capital",
                Description = "d",
                Recommendation = "Revisar estrategia de salida.",
                Materiality = new DecisionMaterialityInput { CapitalAmount = 8_000m },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.High
            }
        ]);

        Assert.Equal(2, engine.EmittedCount);
        Assert.True(engine.Groups.Count >= 1);
        Assert.Contains(engine.Groups, g => g.EventCount >= 2 && g.Title.Contains("SKU-X"));
    }

    // TEST 11: Capital congelado alto pero vende bien → NO crítico automático
    [Fact]
    public void Test11_HighFrozen_StillSelling_NotAutoCritical()
    {
        var candidates = CapitalAlertRuleComposer.FromInventoryAlerts(
            new InventoryAlertReport
            {
                Alerts =
                [
                    new InventoryAlert
                    {
                        Kind = InventoryAlertKind.FrozenCapital,
                        ProductId = 1,
                        ProductName = "X",
                        CapitalAmount = 50_000m,
                        Priority = InventoryAlertPriority.Critical,
                        Message = "Frozen"
                    }
                ],
                TotalAlerts = 1,
                ImmobilizedCapital = 50_000m
            },
            "p");

        Assert.True(candidates[0].ProductStillSelling);
        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(
            report.Events[0].Severity < DecisionSeverity.Critical
            || report.Events[0].Priority <= DecisionPriority.Medium);

        Assert.Equal(
            DecisionSeverity.High,
            DecisionSeverityResolver.Resolve(new DecisionImpactAssessment
            {
                Capital = DecisionImpactLevel.Critical,
                ProductStillSelling = true
            }));
    }

    // TEST 12: Stock alto + estacional → considerar contexto
    [Fact]
    public void Test12_SeasonalContext_DampensSeverity()
    {
        Assert.Equal(
            DecisionSeverity.High,
            DecisionSeverityResolver.Resolve(new DecisionImpactAssessment
            {
                Sales = DecisionImpactLevel.Critical,
                SeasonalContext = true,
                Capital = DecisionImpactLevel.Low,
                Financial = DecisionImpactLevel.High
            }));

        // Sin contexto estacional, el mismo impacto de ventas puede ser Critical
        Assert.Equal(
            DecisionSeverity.Critical,
            DecisionSeverityResolver.Resolve(new DecisionImpactAssessment
            {
                Sales = DecisionImpactLevel.Critical,
                SeasonalContext = false,
                Financial = DecisionImpactLevel.Critical
            }));
    }

    // TEST 13: Producto nuevo → datos insuficientes (sin alerta avanzada)
    [Fact]
    public void Test13_NewProduct_ShowsInsufficient_NoAdvanced()
    {
        Assert.True(ProductAlertRuleComposer.ShouldSuppressAdvancedAlert(
            ProductPerformanceClass.New));
        Assert.True(ProductAlertRuleComposer.ShouldSuppressAdvancedAlert(
            ProductPerformanceClass.InsufficientData));

        var report = new ProductClassificationReport
        {
            Rows =
            [
                new ProductClassificationRow
                {
                    ProductId = 4,
                    ProductName = "Nuevo",
                    Class = ProductPerformanceClass.New,
                    Reasons = ["nuevo"],
                    Performance = new ProductPerformanceRow
                    {
                        ProductId = 4,
                        ProductName = "Nuevo",
                        InventoryCapital = 1_000m
                    }
                }
            ]
        };

        Assert.Empty(ProductAlertRuleComposer.FromClassification(report, "p"));
    }

    // TEST 14: Alerta resuelta permanece en historial
    [Fact]
    public void Test14_ResolvedAlert_RemainsInHistory()
    {
        var store = new InMemoryDecisionHistoryStore();
        var history = new DecisionHistoryService(store);
        var res = new DecisionResolutionService(store);

        var engine = new DecisionEngine().Evaluate([DeclineCand(-30m)]);
        history.Capture(engine);
        Assert.True(res.Resolve(engine.Events[0].EventId, "ana", "ok").Success);

        var rows = history.GetHistory();
        Assert.Single(rows);
        Assert.Equal(DecisionEventStatus.Resolved, rows[0].Status);
        Assert.Equal("ana", rows[0].ResolvedBy);
        Assert.Equal(1, history.GetMetrics().ResolvedCount);
        Assert.Equal(0, history.GetMetrics().ActiveCount);
    }

    // TEST 15: Usuario ignora → registrar estado
    [Fact]
    public void Test15_UserIgnores_RegistersStatus()
    {
        var store = new InMemoryDecisionHistoryStore();
        var auditStore = new InMemoryDecisionAuditStore();
        var history = new DecisionHistoryService(store, auditStore);
        var res = new DecisionResolutionService(store, auditStore);

        var engine = new DecisionEngine().Evaluate([DeclineCand(-25m)]);
        history.Capture(engine);

        DecisionResolutionResult r = res.Ignore(
            engine.Events[0].EventId, "ana", "ruido estacional");

        Assert.True(r.Success);
        Assert.Equal(DecisionEventStatus.Ignored, r.NewStatus);
        Assert.Equal(DecisionEventStatus.Ignored, history.GetHistory().Single().Status);
        Assert.Equal(1, history.GetMetrics().IgnoredCount);

        Assert.Contains(
            auditStore.Query(new DecisionAuditQuery { Top = 20 }),
            a => a.Action == DecisionAuditAction.Ignore);
    }

    [Fact]
    public void Policy_Points_To_Performance_Next()
    {
        Assert.Contains("TEST 9", DecisionHistoryPolicy.Reconcile);
        Assert.Contains("completa", DecisionHistoryPolicy.Deferred);
        Assert.Contains("completa", DecisionCenterPolicy.Deferred);
    }
}
