using BLL.Models.Crm;
using System.Globalization;
using System.Text;

namespace BLL.Services.Crm
{
    /// <summary>Contrato aprendizaje por tipo (FASE 11.13).</summary>
    public static class BusinessActionLearningPolicy
    {
        public const string Definition =
            "FASE 11.13: agregados históricos por tipo de acción — tasas de éxito / parcial / fracaso. " +
            "Información histórica; no garantía futura. Sin ML. Cancelada/Sin resultado no entran a tasas.";

        public const string Caution =
            "Las tasas reflejan Outcomes ya clasificados (Exitosa/Parcial/No efectiva). " +
            "No predicen resultados futuros ni afirman causalidad. Pueden influir estacionalidad y otros factores.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Agrega tasas por tipo desde registros en memoria (FASE 11.13).</summary>
    public static class BusinessActionLearningComposer
    {
        public static BusinessActionLearningSummary Compose(
            IEnumerable<BusinessActionRecord> records,
            BusinessActionLearningQuery? query = null,
            DateTime? generatedAtUtc = null)
        {
            ArgumentNullException.ThrowIfNull(records);
            query ??= new BusinessActionLearningQuery();

            IEnumerable<BusinessActionRecord> filtered = records;
            if (query.FromUtc.HasValue)
                filtered = filtered.Where(r => r.CreatedAt >= query.FromUtc.Value);
            if (query.ToUtc.HasValue)
                filtered = filtered.Where(r => r.CreatedAt <= query.ToUtc.Value);
            if (query.ActionType.HasValue)
                filtered = filtered.Where(r => r.ActionType == query.ActionType.Value);

            var list = filtered.ToList();
            var byType = list
                .GroupBy(r => r.ActionType)
                .OrderBy(g => BusinessActionCatalog.DisplayName(g.Key))
                .Select(g => BuildTypeStats(g.Key, g.ToList()))
                .ToList();

            int classified = byType.Sum(t => t.ClassifiedCount);
            int successful = byType.Sum(t => t.SuccessfulCount);
            int partial = byType.Sum(t => t.PartialCount);
            int ineffective = byType.Sum(t => t.IneffectiveCount);

            return new BusinessActionLearningSummary
            {
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                GeneratedAtUtc = generatedAtUtc ?? DateTime.UtcNow,
                TotalActions = list.Count,
                ClassifiedActions = classified,
                OverallSuccessRatePct = Rate(successful, classified),
                OverallPartialRatePct = Rate(partial, classified),
                OverallFailureRatePct = Rate(ineffective, classified),
                ByType = byType,
                Caution = BusinessActionLearningPolicy.Caution,
                Narrative = BuildOverallNarrative(list.Count, classified, successful, partial, ineffective, byType)
            };
        }

        public static BusinessActionTypeLearningStats BuildTypeStats(
            BusinessActionType type,
            IReadOnlyList<BusinessActionRecord> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            int pending = 0, inProgress = 0, completed = 0, cancelled = 0, noResult = 0;
            int successful = 0, partial = 0, ineffective = 0, insuf = 0, unspecified = 0;

            foreach (BusinessActionRecord r in records)
            {
                switch (r.Status)
                {
                    case BusinessActionStatus.Pending: pending++; break;
                    case BusinessActionStatus.InProgress: inProgress++; break;
                    case BusinessActionStatus.Completed: completed++; break;
                    case BusinessActionStatus.Cancelled: cancelled++; break;
                    case BusinessActionStatus.NoResult: noResult++; break;
                }

                // Cancelada / Sin resultado no entran a tasas (Catalog.IsEvaluable).
                if (!BusinessActionCatalog.IsEvaluable(r.Status))
                    continue;

                BusinessActionOutcome outcome = r.ActualImpact?.Outcome ?? BusinessActionOutcome.Unspecified;
                switch (outcome)
                {
                    case BusinessActionOutcome.Successful: successful++; break;
                    case BusinessActionOutcome.Partial: partial++; break;
                    case BusinessActionOutcome.Ineffective: ineffective++; break;
                    case BusinessActionOutcome.InsufficientData: insuf++; break;
                    default: unspecified++; break;
                }
            }

            int classified = successful + partial + ineffective;
            return new BusinessActionTypeLearningStats
            {
                ActionType = type,
                DisplayName = BusinessActionCatalog.DisplayName(type),
                TotalCount = records.Count,
                PendingCount = pending,
                InProgressCount = inProgress,
                CompletedCount = completed,
                CancelledCount = cancelled,
                NoResultCount = noResult,
                ClassifiedCount = classified,
                SuccessfulCount = successful,
                PartialCount = partial,
                IneffectiveCount = ineffective,
                InsufficientDataCount = insuf,
                UnspecifiedOutcomeCount = unspecified,
                SuccessRatePct = Rate(successful, classified),
                PartialRatePct = Rate(partial, classified),
                FailureRatePct = Rate(ineffective, classified),
                Summary = BuildTypeSummary(type, records.Count, classified, successful, partial, ineffective, cancelled)
            };
        }

        public static decimal? Rate(int numerator, int denominator)
        {
            if (denominator <= 0)
                return null;
            return Math.Round(100m * numerator / denominator, 1, MidpointRounding.AwayFromZero);
        }

        private static string BuildTypeSummary(
            BusinessActionType type,
            int total,
            int classified,
            int successful,
            int partial,
            int ineffective,
            int cancelled)
        {
            string name = BusinessActionCatalog.DisplayName(type);
            if (classified == 0)
            {
                string extra = cancelled > 0
                    ? $" ({cancelled} cancelada(s) excluidas de tasas)."
                    : ".";
                return $"Se observaron {total} acción(es) de tipo {name} sin Outcomes clasificados aún{extra}";
            }

            decimal? s = Rate(successful, classified);
            return string.Format(
                CultureInfo.InvariantCulture,
                "Histórico {0}: {1}/{2} exitosas ({3:0.#}%), {4} parciales, {5} no efectivas. No garantiza resultados futuros.",
                name, successful, classified, s, partial, ineffective);
        }

        private static string BuildOverallNarrative(
            int total,
            int classified,
            int successful,
            int partial,
            int ineffective,
            IReadOnlyList<BusinessActionTypeLearningStats> byType)
        {
            if (total == 0)
                return "Sin acciones registradas en el período. Nada que aprender aún.";

            if (classified == 0)
                return $"Se observaron {total} acción(es); ninguna con Outcome clasificado (Exitosa/Parcial/No efectiva).";

            var sb = new StringBuilder();
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "En {0} acción(es) clasificadas se observó {1:0.#}% exitosas, {2:0.#}% parciales, {3:0.#}% no efectivas. ",
                classified,
                Rate(successful, classified),
                Rate(partial, classified),
                Rate(ineffective, classified));

            BusinessActionTypeLearningStats? best = byType
                .Where(t => t.ClassifiedCount >= 2 && t.SuccessRatePct.HasValue)
                .OrderByDescending(t => t.SuccessRatePct)
                .ThenByDescending(t => t.ClassifiedCount)
                .FirstOrDefault();

            if (best != null)
            {
                sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Entre tipos con ≥2 clasificadas, {0} muestra la mayor tasa histórica de éxito ({1:0.#}%). ",
                    best.DisplayName,
                    best.SuccessRatePct);
            }

            sb.Append("Información histórica; no es una garantía futura.");
            return BusinessActionSoftLanguageGuard.EnsureHistoricalHint(sb.ToString());
        }
    }

    /// <summary>Servicio de aprendizaje (FASE 11.13 tipos · 11.14 contextual). Solo lectura.</summary>
    public sealed class BusinessActionLearningService
    {
        private readonly IBusinessActionStore _store;
        private readonly IDecisionHistoryStore? _decisions;

        public BusinessActionLearningService(
            IBusinessActionStore? store = null,
            IDecisionHistoryStore? decisions = null)
        {
            _store = store ?? new SqlBusinessActionStore();
            _decisions = decisions;
        }

        public BusinessActionLearningSummary GetSummary(BusinessActionLearningQuery? query = null)
        {
            query ??= new BusinessActionLearningQuery();
            int top = query.Top <= 0 ? 500 : Math.Min(query.Top, 2000);

            IReadOnlyList<BusinessActionRecord> records = _store.Query(new BusinessActionQuery
            {
                ActionType = query.ActionType,
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                Top = top
            });

            return BusinessActionLearningComposer.Compose(records, query);
        }

        public BusinessActionTypeLearningStats? GetByType(
            BusinessActionType actionType,
            BusinessActionLearningQuery? query = null)
        {
            var q = query ?? new BusinessActionLearningQuery();
            var scoped = new BusinessActionLearningQuery
            {
                FromUtc = q.FromUtc,
                ToUtc = q.ToUtc,
                ActionType = actionType,
                Top = q.Top
            };
            return GetSummary(scoped).ByType.FirstOrDefault(t => t.ActionType == actionType);
        }

        /// <summary>FASE 11.14 — producto/categoría + problema + señales.</summary>
        public BusinessActionContextualLearning GetContextual(
            BusinessActionLearningQuery? query = null,
            int minOccurrences = BusinessActionContextualLearningPolicy.DefaultMinOccurrences)
        {
            query ??= new BusinessActionLearningQuery();
            int top = query.Top <= 0 ? 500 : Math.Min(query.Top, 2000);

            IReadOnlyList<BusinessActionRecord> records = _store.Query(new BusinessActionQuery
            {
                ActionType = query.ActionType,
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                Top = top
            });

            IReadOnlyList<DecisionHistoryRecord> decisions = _decisions == null
                ? Array.Empty<DecisionHistoryRecord>()
                : _decisions.Query(new DecisionHistoryQuery
                {
                    FromUtc = query.FromUtc,
                    ToUtc = query.ToUtc,
                    Top = top
                });

            return BusinessActionContextualLearningComposer.Compose(
                records, decisions, query, minOccurrences);
        }

        public BusinessActionEntityLearningStats? GetByEntity(
            DecisionEntityType entityType,
            string entityId,
            BusinessActionLearningQuery? query = null)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                return null;

            return GetContextual(query).ByEntity
                .FirstOrDefault(e =>
                    e.EntityType == entityType
                    && string.Equals(e.EntityId, entityId.Trim(), StringComparison.Ordinal));
        }

        public BusinessActionProblemLearningStats? GetByProblem(
            string problemKey,
            BusinessActionLearningQuery? query = null)
        {
            if (string.IsNullOrWhiteSpace(problemKey))
                return null;

            return GetContextual(query).ByProblem
                .FirstOrDefault(p =>
                    string.Equals(p.ProblemKey, problemKey.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
