using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de prioridad (FASE 10.6).</summary>
    public static class DecisionPriorityPolicy
    {
        public const string Definition =
            "FASE 10.6: Prioridad = qué revisar primero (cola de atención). " +
            "INFORMATIVA < BAJA < MEDIA < ALTA < CRÍTICA.";

        public const string Separation =
            "PRIORIDAD ≠ SEVERIDAD. " +
            "Severidad ALTA puede ser Prioridad MEDIA si no hay acción inmediata (brief §12).";

        public const string Urgency =
            "PRIORIDAD se nutre de URGENCIA temporal. " +
            "IMPACTO/SEVERIDAD miden magnitud; URGENCIA mide cuándo revisar.";

        public const string Deferred =
            "FASE 10 completa.";
    }

    public static class DecisionPriorityCatalog
    {
        public static string DisplayName(DecisionPriority p) => p switch
        {
            DecisionPriority.Info => "INFORMATIVA",
            DecisionPriority.Low => "BAJA",
            DecisionPriority.Medium => "MEDIA",
            DecisionPriority.High => "ALTA",
            DecisionPriority.Critical => "CRÍTICA",
            _ => "N/D"
        };

        public static int Rank(DecisionPriority p) => (int)p;
    }

    /// <summary>
    /// Resuelve prioridad de revisión. Determinista · sin umbrales monetarios.
    /// </summary>
    public static class DecisionPriorityResolver
    {
        public static DecisionPriority Resolve(DecisionPriorityAssessment input)
        {
            if (input.InsufficientData)
                return DecisionPriority.Info;

            DecisionPriority fromSeverity = MapSeverity(input.Severity);
            DecisionPriority fromUrgency = MapUrgency(input.Urgency);

            // Base: urgencia manda la cola; si no hay urgencia, usar severidad
            DecisionPriority result = input.Urgency != DecisionUrgencyLevel.None
                ? fromUrgency
                : fromSeverity;

            // Brief §12: Severidad ALTA sin acción inmediata → Prioridad MEDIA
            if (input.Severity == DecisionSeverity.High
                && !input.RequiresImmediateReview
                && !input.TimeSensitiveStockout
                && input.Urgency is DecisionUrgencyLevel.None or DecisionUrgencyLevel.Medium or DecisionUrgencyLevel.Low)
            {
                result = Min(result, DecisionPriority.Medium);
                if (input.Urgency == DecisionUrgencyLevel.None)
                    result = DecisionPriority.Medium;
            }

            // TEST 11 / §15: capital alto + aún vende → urgencia no Immediate
            if (input.ProductStillSelling
                && !input.TimeSensitiveStockout
                && !input.RequiresImmediateReview)
            {
                result = Min(result, DecisionPriority.Medium);
            }

            if (input.TimeSensitiveStockout)
            {
                result = Max(result, DecisionPriority.High);
                if (input.Severity == DecisionSeverity.Critical)
                    result = DecisionPriority.Critical;
            }

            if (input.RequiresImmediateReview)
            {
                result = Max(result, DecisionPriority.High);
                if (input.Severity >= DecisionSeverity.High
                    && input.Urgency >= DecisionUrgencyLevel.High)
                {
                    result = DecisionPriority.Critical;
                }
            }

            // Oportunidad: no enterrar bajo Info/Low si hay ventana
            if (input.OpportunityWindow && input.Severity >= DecisionSeverity.Medium)
                result = Max(result, DecisionPriority.Medium);

            // Floor: urgencia Immediate nunca baja de High
            if (input.Urgency == DecisionUrgencyLevel.Immediate)
            {
                result = Max(result, DecisionPriority.High);
                if (input.Severity == DecisionSeverity.Critical)
                    result = DecisionPriority.Critical;
            }

            if (result == DecisionPriority.Unspecified)
                result = fromSeverity != DecisionPriority.Unspecified
                    ? fromSeverity
                    : DecisionPriority.Info;

            return result;
        }

        public static DecisionEvent Apply(DecisionEvent e, DecisionPriorityAssessment input)
        {
            DecisionPriority priority = Resolve(input);
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
                Severity = input.Severity != DecisionSeverity.Unspecified
                    ? input.Severity
                    : e.Severity,
                Priority = priority,
                Title = e.Title,
                Description = e.Description,
                Reason = e.Reason,
                Impact = e.Impact,
                Recommendation = e.Recommendation,
                Status = input.InsufficientData
                    ? DecisionEventStatus.InsufficientData
                    : e.Status,
                Source = e.Source,
                Fingerprint = e.Fingerprint,
                Evidence = e.Evidence,
                MetricKeys = e.MetricKeys
            };
        }

        private static DecisionPriority MapSeverity(DecisionSeverity s) => s switch
        {
            DecisionSeverity.Info => DecisionPriority.Info,
            DecisionSeverity.Low => DecisionPriority.Low,
            DecisionSeverity.Medium => DecisionPriority.Medium,
            DecisionSeverity.High => DecisionPriority.High,
            DecisionSeverity.Critical => DecisionPriority.Critical,
            _ => DecisionPriority.Unspecified
        };

        private static DecisionPriority MapUrgency(DecisionUrgencyLevel u) => u switch
        {
            DecisionUrgencyLevel.Low => DecisionPriority.Low,
            DecisionUrgencyLevel.Medium => DecisionPriority.Medium,
            DecisionUrgencyLevel.High => DecisionPriority.High,
            DecisionUrgencyLevel.Immediate => DecisionPriority.Critical,
            _ => DecisionPriority.Unspecified
        };

        private static DecisionPriority Max(DecisionPriority a, DecisionPriority b)
            => Rank(a) >= Rank(b) ? a : b;

        private static DecisionPriority Min(DecisionPriority a, DecisionPriority b)
            => Rank(a) <= Rank(b) ? a : b;

        private static int Rank(DecisionPriority p) => (int)p;
    }
}
