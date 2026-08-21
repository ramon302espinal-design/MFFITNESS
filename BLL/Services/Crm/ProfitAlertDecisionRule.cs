using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de ganancia/margen → DecisionEngine (FASE 10.10).</summary>
    public static class ProfitAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.10: reglas de ganancia/margen consumen SalesVariation (SSOT ProfitAnalytics). " +
            "Ganancia ≠ ingresos ≠ margen. No auto-acciones.";

        public const string StrongOnly =
            "Deterioro fuerte = Strength.Strong (≥15% StrongBand). Mild no emite alerta de dominio.";

        public const string Separation =
            "profit.decline = ganancia realizada ↓. " +
            "margin.deterioration = margen % ↓. " +
            "Cruce ingresos↑+margen↓ sigue en sales.rev_up_* (10.9).";
    }

    /// <summary>Composición pura de candidatos de alerta de ganancia.</summary>
    public static class ProfitAlertRuleComposer
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

            SalesVariationLabel profit = variations.RealizedProfit;
            if (profit.Direction == SalesVariationDirection.Down
                && profit.Strength == SalesVariationStrength.Strong
                && profit.VariationPct.HasValue)
            {
                list.Add(new DecisionRuleCandidate
                {
                    RuleId = "profit.alerts.decline",
                    EventType = "profit.decline",
                    Area = DecisionEventArea.Profit,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Deterioro de ganancia",
                    Description =
                        $"La ganancia realizada cayó {profit.Display} durante {periodLabel}.",
                    Reason = "Ganancia materialmente peor vs período comparable (COGS confiable).",
                    Recommendation = "Revisar costos, mezcla de productos y descuentos.",
                    Source = "SalesVariationService",
                    Materiality = new DecisionMaterialityInput
                    {
                        VariationPct = profit.VariationPct.Value
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Financial = DecisionImpactLevel.High,
                        Sales = variations.Revenue.Direction == SalesVariationDirection.Down
                            ? DecisionImpactLevel.High
                            : DecisionImpactLevel.Medium
                    },
                    Urgency = DecisionUrgencyLevel.High,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "profit_var_pct",
                            Label = "Var. ganancia",
                            ValueText = profit.VariationPct.Value.ToString("N2", Cultura) + " %",
                            MetricKey = "profit.var_pct"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "revenue_var_pct",
                            Label = "Var. ingresos",
                            ValueText = variations.Revenue.Display,
                            MetricKey = "sales.revenue_var_pct"
                        }
                    ],
                    MetricKeys = ["profit.var_pct", "profit.realized"]
                });
            }

            SalesVariationLabel? margin = variations.Margin;
            if (margin != null
                && margin.Direction == SalesVariationDirection.Down
                && margin.Strength == SalesVariationStrength.Strong
                && margin.VariationPct.HasValue)
            {
                bool revenueUpOrFlat = variations.Revenue.Direction
                    is SalesVariationDirection.Up
                    or SalesVariationDirection.Flat;

                list.Add(new DecisionRuleCandidate
                {
                    RuleId = "profit.alerts.margin",
                    EventType = "margin.deterioration",
                    Area = DecisionEventArea.Margin,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Deterioro de margen",
                    Description =
                        $"El margen cayó {margin.Display} durante {periodLabel}"
                        + (revenueUpOrFlat
                            ? " mientras los ingresos no caen con la misma fuerza."
                            : "."),
                    Reason = "Margen % materialmente peor — distinto de caída de ingresos.",
                    Recommendation = "Revisar mezcla de productos, costos y descuentos.",
                    Source = "SalesVariationService",
                    Materiality = new DecisionMaterialityInput
                    {
                        VariationPct = margin.VariationPct.Value
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Financial = DecisionImpactLevel.High,
                        Sales = revenueUpOrFlat
                            ? DecisionImpactLevel.Medium
                            : DecisionImpactLevel.High
                    },
                    Urgency = DecisionUrgencyLevel.Medium,
                    RequiresImmediateReview = revenueUpOrFlat,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "margin_var_pct",
                            Label = "Var. margen",
                            ValueText = margin.VariationPct.Value.ToString("N2", Cultura) + " %",
                            MetricKey = "profit.margin_var_pct"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "revenue_var_pct",
                            Label = "Var. ingresos",
                            ValueText = variations.Revenue.Display,
                            MetricKey = "sales.revenue_var_pct"
                        }
                    ],
                    MetricKeys = ["profit.margin_var_pct", "profit.margin_pct"]
                });
            }

            return list;
        }
    }

    /// <summary>Regla de dominio ganancia/margen (FASE 10.10).</summary>
    public sealed class ProfitAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, SalesVariationReport?> _loadVariations;

        public string RuleId => "profit.alerts.v1";

        public ProfitAlertDecisionRule()
            : this(null)
        {
        }

        public ProfitAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, SalesVariationReport?>? loadVariations)
        {
            if (loadVariations != null)
            {
                _loadVariations = loadVariations;
            }
            else
            {
                var svc = new SalesVariationService();
                _loadVariations = (k, a) => svc.GetVariations(k, a);
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

            return ProfitAlertRuleComposer.FromVariation(variations, periodKey, periodLabel);
        }
    }
}
