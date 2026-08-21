using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de severidad (FASE 10.5).</summary>
    public static class DecisionSeverityPolicy
    {
        public const string Definition =
            "FASE 10.5: Severidad = magnitud del impacto real. " +
            "INFO < BAJA < MEDIA < ALTA < CRÍTICA. " +
            "Colores son etiquetas semánticas, no decoración.";

        public const string Separation =
            "SEVERIDAD ≠ PRIORIDAD. " +
            "Ej.: Severidad ALTA + Prioridad MEDIA si no requiere acción inmediata (brief §12).";

        public const string ImpactVsUrgency =
            "IMPACTO ≠ URGENCIA. " +
            "Capital RD$50k puede ser impacto ALTO con urgencia MEDIA si aún vende (brief §15 / TEST 11).";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>Metadatos de presentación de severidad (sin UI).</summary>
    public static class DecisionSeverityCatalog
    {
        public static string DisplayName(DecisionSeverity s) => s switch
        {
            DecisionSeverity.Info => "INFO",
            DecisionSeverity.Low => "BAJA",
            DecisionSeverity.Medium => "MEDIA",
            DecisionSeverity.High => "ALTA",
            DecisionSeverity.Critical => "CRÍTICA",
            _ => "N/D"
        };

        /// <summary>Token semántico (no hex arbitrario).</summary>
        public static string SemanticToken(DecisionSeverity s) => s switch
        {
            DecisionSeverity.Info => "info",
            DecisionSeverity.Low => "low",
            DecisionSeverity.Medium => "medium",
            DecisionSeverity.High => "high",
            DecisionSeverity.Critical => "critical",
            _ => "unspecified"
        };

        public static int Rank(DecisionSeverity s) => (int)s;
    }

    /// <summary>
    /// Resuelve severidad desde impacto multi-dimensión.
    /// No inventa umbrales monetarios — el caller aporta niveles cualitativos.
    /// </summary>
    public static class DecisionSeverityResolver
    {
        public static DecisionSeverity Resolve(DecisionImpactAssessment impact)
        {
            if (impact.InsufficientData)
                return DecisionSeverity.Info;

            DecisionImpactLevel max = MaxImpact(impact);
            DecisionSeverity severity = MapImpactToSeverity(max);

            // TEST 11 / brief §15: capital crítico solo + aún vende → no CRÍTICA automática
            if (severity == DecisionSeverity.Critical
                && impact.ProductStillSelling
                && IsPrimarilyCapitalCritical(impact))
            {
                severity = DecisionSeverity.High;
            }

            // TEST 12: distorsión estacional → no tratar caída como Critical por defecto
            if (severity == DecisionSeverity.Critical
                && impact.SeasonalContext
                && impact.Sales >= DecisionImpactLevel.High
                && impact.Capital < DecisionImpactLevel.High
                && impact.Financial < DecisionImpactLevel.Critical)
            {
                severity = DecisionSeverity.High;
            }

            return severity;
        }

        /// <summary>Aplica severidad a un evento (inmutable → nuevo).</summary>
        public static DecisionEvent Apply(DecisionEvent e, DecisionImpactAssessment impact)
        {
            DecisionSeverity severity = Resolve(impact);
            return new DecisionEvent
            {
                EventId = e.EventId,
                EventType = e.EventType,
                Area = e.Area,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                EntityName = e.EntityName,
                PeriodKey = e.PeriodKey,
                DetectedAt = e.DetectedAt,
                CreatedAt = e.CreatedAt,
                Severity = severity,
                Priority = e.Priority,
                Title = e.Title,
                Description = e.Description,
                Reason = e.Reason,
                Impact = string.IsNullOrWhiteSpace(e.Impact)
                    ? DecisionSeverityCatalog.DisplayName(severity)
                    : e.Impact,
                Recommendation = e.Recommendation,
                Status = e.Status == DecisionEventStatus.InsufficientData || impact.InsufficientData
                    ? DecisionEventStatus.InsufficientData
                    : e.Status,
                Source = e.Source,
                Fingerprint = e.Fingerprint,
                Evidence = e.Evidence,
                MetricKeys = e.MetricKeys
            };
        }

        public static DecisionImpactLevel MaxImpact(DecisionImpactAssessment a)
        {
            DecisionImpactLevel m = a.Financial;
            if (a.Sales > m) m = a.Sales;
            if (a.Inventory > m) m = a.Inventory;
            if (a.Liquidity > m) m = a.Liquidity;
            if (a.Capital > m) m = a.Capital;
            if (a.Operational > m) m = a.Operational;
            return m;
        }

        private static DecisionSeverity MapImpactToSeverity(DecisionImpactLevel level) => level switch
        {
            DecisionImpactLevel.None => DecisionSeverity.Info,
            DecisionImpactLevel.Low => DecisionSeverity.Low,
            DecisionImpactLevel.Medium => DecisionSeverity.Medium,
            DecisionImpactLevel.High => DecisionSeverity.High,
            DecisionImpactLevel.Critical => DecisionSeverity.Critical,
            _ => DecisionSeverity.Unspecified
        };

        /// <summary>
        /// Capital (y opcionalmente liquidity) en Critical; resto &lt; High.
        /// </summary>
        private static bool IsPrimarilyCapitalCritical(DecisionImpactAssessment a)
        {
            if (a.Capital < DecisionImpactLevel.Critical)
                return false;

            return a.Sales < DecisionImpactLevel.High
                   && a.Financial < DecisionImpactLevel.Critical
                   && a.Inventory < DecisionImpactLevel.High
                   && a.Operational < DecisionImpactLevel.High;
        }
    }
}
