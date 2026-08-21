using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ActionRecord (FASE 11.3).</summary>
    public static class BusinessActionRecordPolicy
    {
        public const string Definition =
            "FASE 11.3: BusinessActionRecord = qué hizo el usuario (manual). " +
            "Vincula DecisionEvent opcional. ExpectedImpact cualitativo. " +
            "Sin mutar precios/stock/caja. Persistencia = 11.4.";

        public const string ExpectedImpact =
            "ExpectedImpact.Summary = objetivo (ej. 'Reducir capital congelado'). " +
            "No inventar 'ganaremos RD$X' sin base suficiente (brief §10).";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Factory pura — sin I/O (FASE 11.3).</summary>
    public static class BusinessActionRecordFactory
    {
        public const int DefaultEvaluationDays = 14;

        public static BusinessActionRecord Create(
            BusinessActionType actionType,
            string description,
            DecisionEventArea area = DecisionEventArea.Operations,
            DecisionEntityType entityType = DecisionEntityType.Portfolio,
            string? entityId = null,
            string? entityName = null,
            Guid? decisionEventId = null,
            long? decisionHistoryId = null,
            string? reason = null,
            string? notes = null,
            decimal? quantityInvolved = null,
            decimal? capitalInvolved = null,
            string? createdBy = null,
            BusinessActionExpectedImpact? expectedImpact = null,
            int? evaluationDays = DefaultEvaluationDays,
            DateTime? createdAt = null,
            BusinessActionStatus status = BusinessActionStatus.Pending,
            BusinessActionBaseline? baseline = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description es obligatoria.", nameof(description));

            DateTime at = createdAt ?? DateTime.UtcNow;
            int? days = evaluationDays is > 0 ? evaluationDays : null;

            return new BusinessActionRecord
            {
                ActionId = Guid.NewGuid(),
                DecisionEventId = decisionEventId,
                DecisionHistoryId = decisionHistoryId,
                ActionType = actionType,
                Area = area,
                EntityType = entityType,
                EntityId = string.IsNullOrWhiteSpace(entityId) ? null : entityId.Trim(),
                EntityName = entityName?.Trim() ?? string.Empty,
                Description = description.Trim(),
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                QuantityInvolved = quantityInvolved,
                CapitalInvolved = capitalInvolved,
                CreatedAt = at,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? null : createdBy.Trim(),
                Status = status,
                StartedAt = status is BusinessActionStatus.InProgress or BusinessActionStatus.Completed
                    ? at
                    : null,
                EvaluationDays = days,
                EvaluationDueAt = days.HasValue ? at.AddDays(days.Value) : null,
                ExpectedImpact = NormalizeExpected(expectedImpact),
                ActualImpact = null,
                Baseline = baseline
            };
        }

        /// <summary>Copia el registro aplicando un baseline (FASE 11.6).</summary>
        public static BusinessActionRecord WithBaseline(
            BusinessActionRecord current,
            BusinessActionBaseline baseline)
        {
            ArgumentNullException.ThrowIfNull(current);
            ArgumentNullException.ThrowIfNull(baseline);

            return new BusinessActionRecord
            {
                ActionId = current.ActionId,
                DecisionEventId = current.DecisionEventId,
                DecisionHistoryId = current.DecisionHistoryId,
                ActionType = current.ActionType,
                Area = current.Area,
                EntityType = current.EntityType,
                EntityId = current.EntityId,
                EntityName = current.EntityName,
                Description = current.Description,
                Reason = current.Reason,
                Notes = current.Notes,
                QuantityInvolved = current.QuantityInvolved,
                CapitalInvolved = current.CapitalInvolved,
                CreatedAt = current.CreatedAt,
                CreatedBy = current.CreatedBy,
                Status = current.Status,
                StartedAt = current.StartedAt,
                EvaluationDays = current.EvaluationDays,
                EvaluationDueAt = current.EvaluationDueAt,
                CompletedAt = current.CompletedAt,
                CompletedBy = current.CompletedBy,
                ExpectedImpact = current.ExpectedImpact,
                ActualImpact = current.ActualImpact,
                Baseline = baseline
            };
        }

        public static BusinessActionExpectedImpact Expected(
            string summary,
            IReadOnlyList<string>? targetMetricKeys = null,
            string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(summary))
                throw new ArgumentException("ExpectedImpact.Summary es obligatorio.", nameof(summary));

            return new BusinessActionExpectedImpact
            {
                Summary = summary.Trim(),
                TargetMetricKeys = targetMetricKeys ?? Array.Empty<string>(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            };
        }

        /// <summary>Shell de ActualImpact vacío / sin datos — no inventa deltas.</summary>
        public static BusinessActionActualImpact InsufficientData(string? notes = null)
            => new()
            {
                Outcome = BusinessActionOutcome.InsufficientData,
                Confidence = BusinessActionConfidence.Low,
                Summary = "Sin datos suficientes para evaluar el resultado.",
                Notes = notes,
                Deltas = Array.Empty<BusinessActionMetricDelta>()
            };

        /// <summary>ActualImpact con deltas observados (FASE 11.8) — Outcome aún Unspecified.</summary>
        public static BusinessActionActualImpact ObservedDeltas(
            IReadOnlyList<BusinessActionMetricDelta> deltas,
            string? notes = null)
        {
            ArgumentNullException.ThrowIfNull(deltas);
            return new BusinessActionActualImpact
            {
                Outcome = BusinessActionOutcome.Unspecified,
                Confidence = BusinessActionConfidence.Unspecified,
                Summary = BusinessActionMetricDeltaMath.BuildObservedSummary(deltas),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                Deltas = deltas
            };
        }

        public static BusinessActionRecord WithActualImpact(
            BusinessActionRecord current,
            BusinessActionActualImpact actualImpact)
        {
            ArgumentNullException.ThrowIfNull(current);
            ArgumentNullException.ThrowIfNull(actualImpact);

            return new BusinessActionRecord
            {
                ActionId = current.ActionId,
                DecisionEventId = current.DecisionEventId,
                DecisionHistoryId = current.DecisionHistoryId,
                ActionType = current.ActionType,
                Area = current.Area,
                EntityType = current.EntityType,
                EntityId = current.EntityId,
                EntityName = current.EntityName,
                Description = current.Description,
                Reason = current.Reason,
                Notes = current.Notes,
                QuantityInvolved = current.QuantityInvolved,
                CapitalInvolved = current.CapitalInvolved,
                CreatedAt = current.CreatedAt,
                CreatedBy = current.CreatedBy,
                Status = current.Status,
                StartedAt = current.StartedAt,
                EvaluationDays = current.EvaluationDays,
                EvaluationDueAt = current.EvaluationDueAt,
                CompletedAt = current.CompletedAt,
                CompletedBy = current.CompletedBy,
                ExpectedImpact = current.ExpectedImpact,
                ActualImpact = actualImpact,
                Baseline = current.Baseline
            };
        }

        private static BusinessActionExpectedImpact? NormalizeExpected(BusinessActionExpectedImpact? e)
        {
            if (e == null)
                return null;
            if (string.IsNullOrWhiteSpace(e.Summary))
                return null;
            return new BusinessActionExpectedImpact
            {
                Summary = e.Summary.Trim(),
                TargetMetricKeys = e.TargetMetricKeys ?? Array.Empty<string>(),
                Notes = string.IsNullOrWhiteSpace(e.Notes) ? null : e.Notes.Trim()
            };
        }
    }
}
