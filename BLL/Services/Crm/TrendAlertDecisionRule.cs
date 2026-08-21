using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de tendencia (FASE 10.15).</summary>
    public static class TrendAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.15: desaceleración ≠ caída; volatilidad ≠ pendiente. " +
            "Consume SalesAcceleration + SalesSeriesTrend (SSOT). " +
            "≠ MoM 2 puntos FASE 8. InsufficientData → silencio.";

        public const string Deceleration =
            "trend.deceleration = SalesAccelerationKind.Decelerating (puede seguir Growing).";

        public const string Volatile =
            "trend.volatile = SalesSeriesTrendKind.Volatile (CV ≥ umbral).";
    }

    /// <summary>Composición pura alertas de tendencia (FASE 10.15).</summary>
    public static class TrendAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static IReadOnlyList<DecisionRuleCandidate> FromAcceleration(
            SalesAccelerationReport? report,
            string periodKey)
        {
            if (report?.Revenue == null)
                return Array.Empty<DecisionRuleCandidate>();

            SalesAccelerationResult rev = report.Revenue;
            if (rev.Kind != SalesAccelerationKind.Decelerating)
                return Array.Empty<DecisionRuleCandidate>();

            decimal? delta = rev.AccelerationDeltaPp;

            return
            [
                new DecisionRuleCandidate
                {
                    RuleId = "trend.alerts.deceleration",
                    EventType = "trend.deceleration",
                    Area = DecisionEventArea.Trend,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Desaceleración de ventas",
                    Description = string.IsNullOrWhiteSpace(rev.Reason)
                        ? "Las tasas de variación de ingresos están desacelerando."
                        : rev.Reason,
                    Reason =
                        "Desaceleración ≠ caída: puede seguir creciendo con tasas menores " +
                        "(ej. +40 → +20 → +5).",
                    Recommendation = "Revisar momentum y no confundir con declive absoluto.",
                    Source = "SalesAccelerationService",
                    Materiality = new DecisionMaterialityInput
                    {
                        // Señal estructural SSOT — no filtrar como ruido flat
                        CrossSignal = true,
                        VariationPct = delta.HasValue ? Math.Abs(delta.Value) : 15m
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Sales = DecisionImpactLevel.Medium,
                        Financial = DecisionImpactLevel.Medium,
                        // No forzar Critical por desaceleración sola
                        SeasonalContext = false
                    },
                    Urgency = DecisionUrgencyLevel.Medium,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "accel_delta_pp",
                            Label = "Δ tasas (pp)",
                            ValueText = delta.HasValue
                                ? delta.Value.ToString("N1", Cultura) + " pp"
                                : "N/D",
                            MetricKey = "trend.accel_delta_pp"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "first_last",
                            Label = "Primera→Última",
                            ValueText =
                                $"{FormatPct(rev.FirstChangePct)} → {FormatPct(rev.LastChangePct)}"
                        }
                    ],
                    MetricKeys = ["trend.accel_kind", "trend.accel_delta_pp"]
                }
            ];
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromSeriesTrend(
            SalesSeriesTrendReport? report,
            string periodKey)
        {
            if (report?.Revenue == null)
                return Array.Empty<DecisionRuleCandidate>();

            SalesSeriesTrendResult rev = report.Revenue;
            if (rev.Kind != SalesSeriesTrendKind.Volatile)
                return Array.Empty<DecisionRuleCandidate>();

            decimal? cv = rev.CoefficientOfVariationPct;

            return
            [
                new DecisionRuleCandidate
                {
                    RuleId = "trend.alerts.volatile",
                    EventType = "trend.volatile",
                    Area = DecisionEventArea.Trend,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Serie de ingresos volátil",
                    Description = string.IsNullOrWhiteSpace(rev.Reason)
                        ? $"CV {cv?.ToString("N0", Cultura)}% — no clasificar solo por pendiente."
                        : rev.Reason,
                    Reason =
                        "Volátil (FASE 9.14): alta dispersión. Pendiente Growing/Declining " +
                        "puede ser engañosa.",
                    Recommendation = "Revisar con cautela; evitar decisiones solo por pendiente.",
                    Source = "SalesSeriesTrendService",
                    Materiality = new DecisionMaterialityInput
                    {
                        CrossSignal = true,
                        VariationPct = cv ?? 40m
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Sales = DecisionImpactLevel.Medium,
                        Operational = DecisionImpactLevel.Medium
                    },
                    Urgency = DecisionUrgencyLevel.Low,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "cv_pct",
                            Label = "CV %",
                            ValueText = cv.HasValue
                                ? cv.Value.ToString("N1", Cultura) + " %"
                                : "N/D",
                            MetricKey = "trend.series_cv_pct"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "points",
                            Label = "Puntos",
                            ValueText = rev.PointCount.ToString(Cultura),
                            MetricKey = "trend.series_kind"
                        }
                    ],
                    MetricKeys = ["trend.series_kind", "trend.series_cv_pct"]
                }
            ];
        }

        private static string FormatPct(decimal? v)
        {
            if (!v.HasValue)
                return "N/D";
            string sign = v.Value > 0 ? "+" : string.Empty;
            return sign + v.Value.ToString("N0", Cultura) + "%";
        }
    }

    /// <summary>Regla de dominio tendencias (FASE 10.15).</summary>
    public sealed class TrendAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, SalesAccelerationReport?> _loadAccel;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesSeriesTrendReport?> _loadTrend;

        public string RuleId => "trend.alerts.v1";

        public TrendAlertDecisionRule()
            : this(null, null)
        {
        }

        public TrendAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, SalesAccelerationReport?>? loadAccel,
            Func<ProfitPeriodKind, DateTime?, SalesSeriesTrendReport?>? loadTrend)
        {
            if (loadAccel != null)
                _loadAccel = loadAccel;
            else
            {
                var svc = new SalesAccelerationService();
                _loadAccel = (k, a) => svc.GetReport(k, a);
            }

            if (loadTrend != null)
                _loadTrend = loadTrend;
            else
            {
                var svc = new SalesSeriesTrendService();
                _loadTrend = (k, a) => svc.GetReport(k, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            foreach (DecisionRuleCandidate c in TrendAlertRuleComposer.FromAcceleration(
                         context.Analytics != null
                         ? context.Analytics.Acceleration
                         : _loadAccel(context.PeriodKind, context.AsOf),
                         periodKey))
                yield return c;

            foreach (DecisionRuleCandidate c in TrendAlertRuleComposer.FromSeriesTrend(
                         context.Analytics != null
                         ? context.Analytics.SeriesTrend
                         : _loadTrend(context.PeriodKind, context.AsOf),
                         periodKey))
                yield return c;
        }
    }
}
