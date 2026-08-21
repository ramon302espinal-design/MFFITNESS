using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de capital (FASE 10.13). ≠ inventario operativo 10.12.</summary>
    public static class CapitalAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.13: capital crítico / congelado / lento / at-risk / % inmovilizado. " +
            "Consume InventoryAlertService (+ bridge CapitalRisk). " +
            "InventoryCapital ≠ FrozenCapital inversión FASE 6. TEST 11: aún vende → no forzar Critical.";

        public const string Mapping =
            "CriticalCapital/FrozenCapital/SlowCapital/AtRiskLoss/HighImmobilizedShare ← InventoryAlert. " +
            "capital.at_risk también desde SalesCapitalBridge CapitalRisk.";

        public const string AntiFatigue =
            "Máx. MaxProductAlerts por kind (default 10). Portfolio share = 1 evento.";
    }

    /// <summary>Composición pura alertas de capital (FASE 10.13).</summary>
    public static class CapitalAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public const int DefaultMaxProductAlerts = 10;

        public static IReadOnlyList<DecisionRuleCandidate> FromInventoryAlerts(
            InventoryAlertReport? report,
            string periodKey,
            int maxPerKind = DefaultMaxProductAlerts)
        {
            if (report == null || report.Alerts.Count == 0)
                return Array.Empty<DecisionRuleCandidate>();

            var list = new List<DecisionRuleCandidate>();

            // Portfolio: % inmovilizado
            foreach (InventoryAlert a in report.Alerts.Where(x =>
                         x.Kind == InventoryAlertKind.HighImmobilizedShare && !x.ProductId.HasValue))
            {
                list.Add(BuildShare(a, periodKey, report.FrozenSharePct));
            }

            list.AddRange(TakeKind(
                report.Alerts, InventoryAlertKind.CriticalCapital, maxPerKind,
                a => BuildProduct(
                    a, periodKey, "capital.critical", "Capital crítico",
                    "Salud Critical — capital material inmovilizado/agravado.",
                    "Revisar estrategia de salida antes de nueva compra.",
                    DecisionImpactLevel.Critical, DecisionUrgencyLevel.High,
                    stillSellingUnknown: true)));

            list.AddRange(TakeKind(
                report.Alerts, InventoryAlertKind.FrozenCapital, maxPerKind,
                a => BuildProduct(
                    a, periodKey, "capital.frozen", "Capital congelado",
                    "Capital de producto congelado (≠ FrozenCapital inversión FASE 6).",
                    "Revisar rotación, precio o liquidación simulada.",
                    DecisionImpactLevel.High, DecisionUrgencyLevel.Medium,
                    stillSellingUnknown: true)));

            list.AddRange(TakeKind(
                report.Alerts, InventoryAlertKind.AtRiskLoss, maxPerKind,
                a => BuildProduct(
                    a, periodKey, "capital.at_risk", "Capital en riesgo",
                    "Potencial de pérdida latente sobre capital inmovilizado.",
                    "Revisar descuentos simulados y prioridad de salida.",
                    DecisionImpactLevel.High, DecisionUrgencyLevel.High,
                    stillSellingUnknown: false)));

            list.AddRange(TakeKind(
                report.Alerts, InventoryAlertKind.SlowCapital, maxPerKind,
                a => BuildProduct(
                    a, periodKey, "capital.slow", "Capital lento",
                    "Capital material con rotación débil (aún no Frozen).",
                    "Monitorear idle y cobertura — no alarmar en exceso.",
                    DecisionImpactLevel.Medium, DecisionUrgencyLevel.Low,
                    stillSellingUnknown: true)));

            return list;
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromCapitalBridge(
            SalesCapitalBridgeReport? bridge,
            string periodKey,
            int maxProductAlerts = DefaultMaxProductAlerts)
        {
            if (bridge == null)
                return Array.Empty<DecisionRuleCandidate>();

            return bridge.Rows
                .Where(r => r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.CapitalRisk)
                            || r.PrimarySignal == SalesCapitalSignalKind.CapitalRisk)
                .OrderByDescending(r => r.InventoryCapital)
                .ThenBy(r => r.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, maxProductAlerts))
                .Select(r =>
                {
                    bool stillSelling = r.Trend is ProductTrendDirection.Growing
                        or ProductTrendDirection.Stable;

                    return new DecisionRuleCandidate
                    {
                        RuleId = "capital.alerts.bridge_risk",
                        EventType = "capital.at_risk",
                        Area = DecisionEventArea.Capital,
                        EntityType = DecisionEntityType.Product,
                        EntityId = r.ProductId.ToString(CultureInfo.InvariantCulture),
                        EntityName = r.ProductName,
                        PeriodKey = periodKey,
                        Title = "Capital en riesgo",
                        Description =
                            $"{r.ProductName}: ventas en declive con stock/capital alto " +
                            $"({r.InventoryCapital.ToString("N2", Cultura)}).",
                        Reason = "§48 Declining + overstock/inmovilizado — riesgo de capital atrapado.",
                        Recommendation = "Revisar estrategia de salida. No comprar más de este SKU.",
                        Source = "SalesCapitalBridgeService",
                        Materiality = new DecisionMaterialityInput
                        {
                            CapitalAmount = r.InventoryCapital,
                            VariationPct = r.RevenueChangePct
                        },
                        ImpactAssessment = new DecisionImpactAssessment
                        {
                            Capital = DecisionImpactLevel.Critical,
                            Sales = DecisionImpactLevel.High,
                            Inventory = DecisionImpactLevel.High,
                            ProductStillSelling = stillSelling
                        },
                        Urgency = stillSelling
                            ? DecisionUrgencyLevel.Medium
                            : DecisionUrgencyLevel.High,
                        ProductStillSelling = stillSelling,
                        RequiresImmediateReview = !stillSelling,
                        Evidence =
                        [
                            new DecisionEvidenceFact
                            {
                                Key = "inventory_capital",
                                Label = "Capital inventario",
                                ValueText = r.InventoryCapital.ToString("N2", Cultura),
                                MetricKey = "capital.inventory"
                            },
                            new DecisionEvidenceFact
                            {
                                Key = "revenue_var",
                                Label = "Var. ingresos",
                                ValueText = r.RevenueChangePct?.ToString("N2", Cultura) + " %" ?? "N/D",
                                MetricKey = "sales.revenue_var_pct"
                            }
                        ],
                        MetricKeys = ["capital.at_risk", "capital.signal_decline_overstock"]
                    };
                })
                .ToList();
        }

        private static IEnumerable<DecisionRuleCandidate> TakeKind(
            IReadOnlyList<InventoryAlert> alerts,
            InventoryAlertKind kind,
            int max,
            Func<InventoryAlert, DecisionRuleCandidate> map)
            => alerts
                .Where(a => a.Kind == kind && a.ProductId.HasValue)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CapitalAmount ?? 0m)
                .ThenBy(a => a.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(map);

        private static DecisionRuleCandidate BuildShare(
            InventoryAlert a,
            string periodKey,
            decimal? frozenSharePct)
        {
            decimal share = frozenSharePct ?? 0m;
            decimal capital = a.CapitalAmount ?? 0m;
            bool strong = share >= 40m;

            return new DecisionRuleCandidate
            {
                RuleId = "capital.alerts.immobilized_share",
                EventType = "capital.high_immobilized_share",
                Area = DecisionEventArea.Capital,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = periodKey,
                Title = "% capital inmovilizado alto",
                Description = string.IsNullOrWhiteSpace(a.Message)
                    ? $"Capital inmovilizado {share.ToString("N2", Cultura)}% del inventario."
                    : a.Message,
                Reason = "FrozenShare del portafolio — distinto de FrozenCapital por inversión.",
                Recommendation = "Revisar productos Frozen/Critical y plan de liberación de capital.",
                Source = "InventoryAlertService",
                Materiality = new DecisionMaterialityInput
                {
                    ImmobilizedSharePct = share > 0 ? share : InventoryAlertService.HighImmobilizedShareThresholdPct,
                    CapitalAmount = capital > 0 ? capital : null
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = strong ? DecisionImpactLevel.Critical : DecisionImpactLevel.High,
                    Liquidity = strong ? DecisionImpactLevel.Critical : DecisionImpactLevel.High
                },
                Urgency = strong ? DecisionUrgencyLevel.High : DecisionUrgencyLevel.Medium,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "frozen_share",
                        Label = "% inmovilizado",
                        ValueText = share.ToString("N2", Cultura) + " %",
                        MetricKey = "capital.frozen_share_pct"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "immobilized_capital",
                        Label = "Capital inmovilizado",
                        ValueText = capital.ToString("N2", Cultura),
                        MetricKey = "capital.immobilized"
                    }
                ],
                MetricKeys = ["capital.frozen_share_pct", "capital.immobilized"]
            };
        }

        private static DecisionRuleCandidate BuildProduct(
            InventoryAlert a,
            string periodKey,
            string eventType,
            string title,
            string reason,
            string recommendation,
            DecisionImpactLevel capitalImpact,
            DecisionUrgencyLevel urgency,
            bool stillSellingUnknown)
        {
            decimal capital = a.CapitalAmount ?? 0m;

            // TEST 11: si no sabemos que dejó de vender, amortiguar Critical→High vía flag
            bool dampen = stillSellingUnknown
                && eventType is "capital.frozen" or "capital.critical"
                && capitalImpact == DecisionImpactLevel.Critical;

            return new DecisionRuleCandidate
            {
                RuleId = "capital.alerts." + eventType.Replace('.', '_'),
                EventType = eventType,
                Area = DecisionEventArea.Capital,
                EntityType = DecisionEntityType.Product,
                EntityId = a.ProductId!.Value.ToString(CultureInfo.InvariantCulture),
                EntityName = a.ProductName,
                PeriodKey = periodKey,
                Title = title,
                Description = string.IsNullOrWhiteSpace(a.Message)
                    ? $"{a.ProductName}: {title} ({capital.ToString("N2", Cultura)})"
                    : a.Message,
                Reason = reason,
                Recommendation = recommendation,
                Source = "InventoryAlertService",
                Materiality = new DecisionMaterialityInput
                {
                    CapitalAmount = capital > 0 ? capital : null
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = capitalImpact,
                    Liquidity = capitalImpact >= DecisionImpactLevel.High
                        ? DecisionImpactLevel.High
                        : DecisionImpactLevel.Medium,
                    // Sin señal de ventas↓, no asumir muerte comercial (TEST 11)
                    ProductStillSelling = dampen || stillSellingUnknown
                },
                Urgency = urgency,
                ProductStillSelling = dampen || stillSellingUnknown,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "capital",
                        Label = "Capital",
                        ValueText = capital.ToString("N2", Cultura),
                        MetricKey = "capital.inventory"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "idle",
                        Label = "Idle días",
                        ValueText = a.IdleDays?.ToString(Cultura) ?? "N/D",
                        MetricKey = "inv.idle_days"
                    }
                ],
                MetricKeys = ["capital.inventory", "capital.immobilized"]
            };
        }
    }

    /// <summary>Regla de dominio capital (FASE 10.13).</summary>
    public sealed class CapitalAlertDecisionRule : IDecisionRule
    {
        private readonly Func<DateTime?, InventoryAlertReport?> _loadAlerts;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesCapitalBridgeReport?> _loadBridge;

        public string RuleId => "capital.alerts.v1";

        public CapitalAlertDecisionRule()
            : this(null, null)
        {
        }

        public CapitalAlertDecisionRule(
            Func<DateTime?, InventoryAlertReport?>? loadAlerts,
            Func<ProfitPeriodKind, DateTime?, SalesCapitalBridgeReport?>? loadBridge)
        {
            if (loadAlerts != null)
                _loadAlerts = loadAlerts;
            else
            {
                var svc = new InventoryAlertService();
                _loadAlerts = a => svc.GetAlerts(a);
            }

            if (loadBridge != null)
                _loadBridge = loadBridge;
            else
            {
                var svc = new SalesCapitalBridgeService();
                _loadBridge = (k, a) => svc.GetReport(k, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            InventoryAlertReport? alerts = context.Analytics != null
                ? context.Analytics.InventoryAlerts
                : _loadAlerts(context.AsOf);
            SalesCapitalBridgeReport? bridge = context.Analytics != null
                ? context.Analytics.CapitalBridge
                : _loadBridge(context.PeriodKind, context.AsOf);

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (DecisionRuleCandidate c in CapitalAlertRuleComposer.FromInventoryAlerts(
                         alerts, periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }

            foreach (DecisionRuleCandidate c in CapitalAlertRuleComposer.FromCapitalBridge(
                         bridge, periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }
        }
    }
}
