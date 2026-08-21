using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de ventas → DecisionEngine (FASE 10.9).</summary>
    public static class SalesAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.9: reglas de ventas consumen SalesVariation / SalesShare (SSOT). " +
            "No recalculan ingresos. No auto-acciones.";

        public const string StrongOnly =
            "Crecimiento/caída fuerte = Strength.Strong (StrongBand 15%), alineado a SalesDashboard §62.";

        public const string Cross =
            "Ingresos↑+Ganancia↓ / Ingresos↑+Margen↓ = material aunque piernas mild (TEST 2).";

        public const string Concentration =
            "Concentración Top N ≥ 50% = señal estratégica (no automáticamente mala).";
    }

    /// <summary>Composición pura de candidatos de alerta de ventas.</summary>
    public static class SalesAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static IReadOnlyList<DecisionRuleCandidate> FromVariation(
            SalesVariationReport? variations,
            string periodKey,
            string periodLabel = "el período")
        {
            if (variations == null)
                return Array.Empty<DecisionRuleCandidate>();

            var list = new List<DecisionRuleCandidate>();

            SalesVariationLabel rev = variations.Revenue;
            if (rev.Direction == SalesVariationDirection.Down
                && rev.Strength == SalesVariationStrength.Strong
                && rev.VariationPct.HasValue)
            {
                list.Add(StrongRevenue(
                    "sales.strong_decline",
                    "Caída fuerte de ventas",
                    $"Los ingresos cayeron {rev.Display} durante {periodLabel}.",
                    "Demanda o ticket materialmente peor vs período comparable.",
                    "Revisar productos, precios y demanda del período.",
                    rev.VariationPct.Value,
                    periodKey,
                    DecisionImpactLevel.High,
                    DecisionUrgencyLevel.High));
            }
            else if (rev.Direction == SalesVariationDirection.Up
                     && rev.Strength == SalesVariationStrength.Strong
                     && rev.VariationPct.HasValue)
            {
                list.Add(StrongRevenue(
                    "sales.strong_growth",
                    "Crecimiento fuerte de ventas",
                    $"Los ingresos aumentaron {rev.Display} durante {periodLabel}.",
                    "Demanda materialmente mayor vs período comparable.",
                    "Evaluar cobertura de stock en productos que impulsan el crecimiento.",
                    rev.VariationPct.Value,
                    periodKey,
                    DecisionImpactLevel.Medium,
                    DecisionUrgencyLevel.Medium,
                    opportunity: true));
            }

            foreach (SalesCrossSignal cross in variations.CrossSignals)
            {
                if (cross.Kind == SalesCrossSignalKind.RevenueUpProfitDown)
                {
                    list.Add(Cross(
                        "sales.rev_up_profit_down",
                        "Ingresos↑ Ganancia↓",
                        cross.Message,
                        "Las ventas crecen pero la ganancia no acompaña.",
                        "Revisar costos, mezcla de productos y descuentos.",
                        periodKey,
                        variations.Revenue.VariationPct,
                        variations.RealizedProfit.VariationPct));
                }
                else if (cross.Kind == SalesCrossSignalKind.RevenueUpMarginDown)
                {
                    list.Add(Cross(
                        "sales.rev_up_margin_down",
                        "Ingresos↑ Margen↓",
                        cross.Message,
                        "Las ventas crecen con deterioro de margen.",
                        "Revisar mezcla, costos y descuentos.",
                        periodKey,
                        variations.Revenue.VariationPct,
                        variations.Margin?.VariationPct));
                }
            }

            return list;
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromConcentration(
            SalesShareReport? share,
            string periodKey,
            decimal thresholdPct = 50m)
        {
            if (share == null || !share.TopNSharePct.HasValue || share.TopN <= 0)
                return Array.Empty<DecisionRuleCandidate>();
            if (share.TopNSharePct.Value < thresholdPct)
                return Array.Empty<DecisionRuleCandidate>();

            decimal pct = share.TopNSharePct.Value;
            string msg = share.TopN <= 3
                ? $"El volumen está concentrado en {share.TopN} productos ({pct.ToString("N0", Cultura)}% de ingresos)."
                : $"Alta concentración: Top {share.TopN} = {pct.ToString("N0", Cultura)}% de ingresos.";

            return
            [
                new DecisionRuleCandidate
                {
                    RuleId = "sales.alerts.concentration",
                    EventType = "sales.concentration",
                    Area = DecisionEventArea.Sales,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Concentración de ingresos",
                    Description = msg,
                    Reason = "Dependencia de pocos productos — estratégico, no automáticamente malo.",
                    Recommendation = "Revisar diversificación y riesgo de dependencia.",
                    Source = "SalesShareService",
                    Materiality = new DecisionMaterialityInput
                    {
                        // % share no es variación; forzar material vía cross-like flag
                        CrossSignal = true,
                        VariationPct = null
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Sales = pct >= 70m ? DecisionImpactLevel.High : DecisionImpactLevel.Medium,
                        Operational = DecisionImpactLevel.Medium
                    },
                    Urgency = DecisionUrgencyLevel.Low,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "topn_share",
                            Label = $"Top {share.TopN} share",
                            ValueText = pct.ToString("N2", Cultura) + " %",
                            MetricKey = "conc.topn_share_pct"
                        }
                    ],
                    MetricKeys = ["conc.topn_share_pct"]
                }
            ];
        }

        public static string PeriodKey(ProfitPeriodKind kind, DateTime? asOf)
            => asOf.HasValue
                ? $"{kind}|{asOf.Value:yyyy-MM-dd}"
                : kind.ToString();

        private static DecisionRuleCandidate StrongRevenue(
            string eventType,
            string title,
            string description,
            string reason,
            string recommendation,
            decimal variationPct,
            string periodKey,
            DecisionImpactLevel salesImpact,
            DecisionUrgencyLevel urgency,
            bool opportunity = false)
            => new()
            {
                RuleId = "sales.alerts.variation",
                EventType = eventType,
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = periodKey,
                Title = title,
                Description = description,
                Reason = reason,
                Recommendation = recommendation,
                Source = "SalesVariationService",
                Materiality = new DecisionMaterialityInput { VariationPct = variationPct },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = salesImpact,
                    Financial = salesImpact
                },
                Urgency = urgency,
                OpportunityWindow = opportunity,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "revenue_var_pct",
                        Label = "Var. ingresos",
                        ValueText = variationPct.ToString("N2", Cultura) + " %",
                        MetricKey = "sales.revenue_var_pct"
                    }
                ],
                MetricKeys = ["sales.revenue_var_pct"]
            };

        private static DecisionRuleCandidate Cross(
            string eventType,
            string title,
            string description,
            string reason,
            string recommendation,
            string periodKey,
            decimal? revenueVar,
            decimal? otherVar)
            => new()
            {
                RuleId = "sales.alerts.cross",
                EventType = eventType,
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = periodKey,
                Title = title,
                Description = description,
                Reason = reason,
                Recommendation = recommendation,
                Source = "SalesVariationService",
                Materiality = new DecisionMaterialityInput
                {
                    VariationPct = revenueVar,
                    CrossSignal = true
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = DecisionImpactLevel.Medium,
                    Financial = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.Medium,
                RequiresImmediateReview = false,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "revenue_var",
                        Label = "Var. ingresos",
                        ValueText = revenueVar?.ToString("N2", Cultura) + " %" ?? "N/D",
                        MetricKey = "sales.revenue_var_pct"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "other_var",
                        Label = "Var. acompañante",
                        ValueText = otherVar?.ToString("N2", Cultura) + " %" ?? "N/D"
                    }
                ],
                MetricKeys = ["sales.revenue_var_pct", "sales.cross_rev_up_profit_down"]
            };
    }

    /// <summary>
    /// Regla de dominio ventas (FASE 10.9).
    /// Por defecto carga SSOT; tests pueden inyectar reportes.
    /// </summary>
    public sealed class SalesAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, SalesVariationReport?> _loadVariations;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesShareReport?> _loadShare;

        public string RuleId => "sales.alerts.v1";

        public SalesAlertDecisionRule()
            : this(null, null)
        {
        }

        public SalesAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, SalesVariationReport?>? loadVariations,
            Func<ProfitPeriodKind, DateTime?, SalesShareReport?>? loadShare)
        {
            if (loadVariations != null)
                _loadVariations = loadVariations;
            else
            {
                var svc = new SalesVariationService();
                _loadVariations = (k, a) => svc.GetVariations(k, a);
            }

            if (loadShare != null)
                _loadShare = loadShare;
            else
            {
                var svc = new SalesShareService();
                _loadShare = (k, a) => svc.GetProductShare(k, SalesShareMetric.Revenue, topN: 3, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            string periodLabel = SalesDecisionMath.PeriodLabel(context.PeriodKind);

            SalesVariationReport? variations = context.Analytics != null
                ? context.Analytics.SalesVariation
                : _loadVariations(context.PeriodKind, context.AsOf);
            SalesShareReport? share = context.Analytics != null
                ? context.Analytics.SalesShare
                : _loadShare(context.PeriodKind, context.AsOf);

            foreach (DecisionRuleCandidate c in SalesAlertRuleComposer.FromVariation(
                         variations, periodKey, periodLabel))
                yield return c;

            foreach (DecisionRuleCandidate c in SalesAlertRuleComposer.FromConcentration(
                         share, periodKey))
                yield return c;
        }
    }
}
