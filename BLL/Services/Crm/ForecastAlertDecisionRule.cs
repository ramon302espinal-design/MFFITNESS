using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de forecast (FASE 10.16).</summary>
    public static class ForecastAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.16: alerta solo si confianza BAJA. " +
            "Toda proyección es ESTIMACIÓN — nunca certeza ni probabilidad. " +
            "Consume SalesForecastService (SSOT). InsufficientData → silencio (TEST 7).";

        public const string Language =
            "Mensajes: estimación / escenario. Prohibido: 'va a vender', 'probabilidad 80%'.";
    }

    /// <summary>Composición pura alertas de forecast (FASE 10.16).</summary>
    public static class ForecastAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static IReadOnlyList<DecisionRuleCandidate> FromForecast(
            SalesForecastReport? forecast,
            string periodKey)
        {
            if (forecast == null)
                return Array.Empty<DecisionRuleCandidate>();

            if (forecast.Confidence == SalesForecastConfidence.InsufficientData)
                return Array.Empty<DecisionRuleCandidate>();

            if (forecast.Confidence != SalesForecastConfidence.Low)
                return Array.Empty<DecisionRuleCandidate>();

            string baseText = forecast.Base?.EstimatedRevenue.ToString("N2", Cultura) ?? "N/D";
            string lowText = forecast.Low?.EstimatedRevenue.ToString("N2", Cultura) ?? "N/D";
            string highText = forecast.High?.EstimatedRevenue.ToString("N2", Cultura) ?? "N/D";

            return
            [
                new DecisionRuleCandidate
                {
                    RuleId = "forecast.alerts.low_confidence",
                    EventType = "forecast.low_confidence",
                    Area = DecisionEventArea.Forecast,
                    EntityType = DecisionEntityType.Portfolio,
                    PeriodKey = periodKey,
                    Title = "Forecast con baja confianza",
                    Description =
                        "La estimación de ingresos tiene confianza BAJA. "
                        + (string.IsNullOrWhiteSpace(forecast.ConfidenceReason)
                            ? "Usar solo como escenario orientativo."
                            : forecast.ConfidenceReason),
                    Reason =
                        "ESTIMACIÓN / ESCENARIO — no es certeza ni probabilidad numérica. "
                        + $"Escenarios: bajo {lowText} · base {baseText} · alto {highText}.",
                    Recommendation =
                        "Revisar el forecast como escenario; no decidir compras solo con esta estimación.",
                    Source = "SalesForecastService",
                    Materiality = new DecisionMaterialityInput
                    {
                        CrossSignal = true
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Sales = DecisionImpactLevel.Low,
                        Operational = DecisionImpactLevel.Medium
                    },
                    Urgency = DecisionUrgencyLevel.Low,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "confidence",
                            Label = "Confianza",
                            ValueText = "BAJA",
                            MetricKey = "forecast.confidence"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "base_est",
                            Label = "Escenario base (est.)",
                            ValueText = baseText,
                            MetricKey = "forecast.base_revenue"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "trend_used",
                            Label = "Tendencia usada",
                            ValueText = forecast.TrendUsed.ToString()
                        }
                    ],
                    MetricKeys =
                    [
                        "forecast.confidence",
                        "forecast.base_revenue",
                        "forecast.low_revenue",
                        "forecast.high_revenue"
                    ]
                }
            ];
        }
    }

    /// <summary>Regla de dominio forecast (FASE 10.16).</summary>
    public sealed class ForecastAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, SalesForecastReport?> _loadForecast;

        public string RuleId => "forecast.alerts.v1";

        public ForecastAlertDecisionRule()
            : this(null)
        {
        }

        public ForecastAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, SalesForecastReport?>? loadForecast)
        {
            if (loadForecast != null)
            {
                _loadForecast = loadForecast;
            }
            else
            {
                var svc = new SalesForecastService();
                _loadForecast = (k, a) => svc.GetEstimate(k, horizonDays: 30, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            return ForecastAlertRuleComposer.FromForecast(
                context.Analytics != null
                ? context.Analytics.Forecast
                : _loadForecast(context.PeriodKind, context.AsOf),
                periodKey);
        }
    }
}
