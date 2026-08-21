using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ventana de evaluación (FASE 11.7).</summary>
    public static class BusinessActionEvaluationWindowPolicy
    {
        public const string Definition =
            "FASE 11.7: al Completar, anclar ventana = CompletedAt + EvaluationDays (default 14). " +
            "InWindow → Ready cuando asOf ≥ DueAt. Sin calcular métricas ni Outcome (11.8–11.9).";

        public const string Anchor =
            "Planned: DueAt provisional desde CreatedAt. " +
            "Completed: DueAt se recalcula desde CompletedAt (autoritativo). " +
            "Cancelled/NoResult: NotApplicable.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Cálculo puro de ventana (FASE 11.7).</summary>
    public static class BusinessActionEvaluationWindowMath
    {
        public static DateTime? ComputeDueAt(DateTime anchorUtc, int days)
        {
            if (days <= 0)
                return null;
            return anchorUtc.AddDays(days);
        }

        public static BusinessActionEvaluationWindow Resolve(
            BusinessActionRecord record,
            DateTime? asOfUtc = null)
        {
            ArgumentNullException.ThrowIfNull(record);
            DateTime asOf = asOfUtc ?? DateTime.UtcNow;

            if (record.Status is BusinessActionStatus.Cancelled
                or BusinessActionStatus.NoResult
                or BusinessActionStatus.Unspecified)
            {
                return new BusinessActionEvaluationWindow
                {
                    Phase = BusinessActionEvaluationPhase.NotApplicable,
                    EvaluationDays = record.EvaluationDays,
                    Label = "No aplica"
                };
            }

            if (HasEvaluatedOutcome(record))
            {
                return new BusinessActionEvaluationWindow
                {
                    Phase = BusinessActionEvaluationPhase.Evaluated,
                    WindowStart = record.CompletedAt,
                    WindowEnd = record.EvaluationDueAt,
                    EvaluationDays = record.EvaluationDays,
                    DaysRemaining = DaysRemaining(record.EvaluationDueAt, asOf),
                    Label = "Evaluada"
                };
            }

            if (record.Status != BusinessActionStatus.Completed)
            {
                DateTime? plannedEnd = record.EvaluationDueAt
                    ?? (record.EvaluationDays is > 0
                        ? ComputeDueAt(record.CreatedAt, record.EvaluationDays.Value)
                        : null);

                return new BusinessActionEvaluationWindow
                {
                    Phase = record.EvaluationDays is > 0
                        ? BusinessActionEvaluationPhase.Planned
                        : BusinessActionEvaluationPhase.NotApplicable,
                    WindowStart = record.CreatedAt,
                    WindowEnd = plannedEnd,
                    EvaluationDays = record.EvaluationDays,
                    DaysRemaining = DaysRemaining(plannedEnd, asOf),
                    Label = record.EvaluationDays is > 0
                        ? $"Planificada ({record.EvaluationDays} d)"
                        : "Sin ventana"
                };
            }

            // Completed — ventana anclada a CompletedAt
            DateTime start = record.CompletedAt ?? record.StartedAt ?? record.CreatedAt;
            int days = record.EvaluationDays is > 0
                ? record.EvaluationDays.Value
                : BusinessActionRecordFactory.DefaultEvaluationDays;
            DateTime? end = record.EvaluationDueAt ?? ComputeDueAt(start, days);
            int? remaining = DaysRemaining(end, asOf);

            bool ready = end.HasValue && asOf >= end.Value;
            return new BusinessActionEvaluationWindow
            {
                Phase = ready
                    ? BusinessActionEvaluationPhase.Ready
                    : BusinessActionEvaluationPhase.InWindow,
                WindowStart = start,
                WindowEnd = end,
                EvaluationDays = days,
                DaysRemaining = remaining,
                Label = ready
                    ? "Lista para evaluar"
                    : $"En ventana ({remaining} d restantes)"
            };
        }

        public static bool IsReadyForEvaluation(BusinessActionRecord record, DateTime? asOfUtc = null)
            => Resolve(record, asOfUtc).IsReady;

        public static bool HasEvaluatedOutcome(BusinessActionRecord record)
        {
            if (record.ActualImpact == null)
                return false;
            return record.ActualImpact.Outcome is not BusinessActionOutcome.Unspecified;
        }

        private static int? DaysRemaining(DateTime? dueAt, DateTime asOf)
        {
            if (!dueAt.HasValue)
                return null;
            return (int)Math.Ceiling((dueAt.Value - asOf).TotalDays);
        }
    }
}
