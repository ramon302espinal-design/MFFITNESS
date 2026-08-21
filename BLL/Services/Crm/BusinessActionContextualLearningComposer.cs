using BLL.Models.Crm;
using System.Globalization;
using System.Text;

namespace BLL.Services.Crm
{
    /// <summary>Contrato learning contextual (FASE 11.14).</summary>
    public static class BusinessActionContextualLearningPolicy
    {
        public const string Definition =
            "FASE 11.14: aprendizaje por producto/categoría, por problema (EventType) y señales de recurrencia. " +
            "Histórico ≠ garantía. Sin ML. Cancelada/Sin resultado fuera de tasas.";

        public const string Caution =
            "Señales y tasas son información histórica. " +
            "Nunca afirmar 'funcionará' ni causalidad automática. Revisar contexto antes de repetir una acción.";

        public const int DefaultMinOccurrences = 3;

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Composer contextual (FASE 11.14) — puro sobre registros en memoria.</summary>
    public static class BusinessActionContextualLearningComposer
    {
        public static BusinessActionContextualLearning Compose(
            IEnumerable<BusinessActionRecord> records,
            IEnumerable<DecisionHistoryRecord>? decisions = null,
            BusinessActionLearningQuery? query = null,
            int minOccurrences = BusinessActionContextualLearningPolicy.DefaultMinOccurrences,
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
            IReadOnlyDictionary<Guid, DecisionHistoryRecord> byEvent =
                (decisions ?? Array.Empty<DecisionHistoryRecord>())
                .GroupBy(d => d.EventId)
                .ToDictionary(g => g.Key, g => g.First());

            var byEntity = ComposeByEntity(list);
            var byProblem = ComposeByProblem(list, byEvent);
            var signals = DetectSignals(list, byEvent.Values, minOccurrences);

            return new BusinessActionContextualLearning
            {
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                GeneratedAtUtc = generatedAtUtc ?? DateTime.UtcNow,
                ByEntity = byEntity,
                ByProblem = byProblem,
                Signals = signals,
                Caution = BusinessActionContextualLearningPolicy.Caution,
                Narrative = BuildNarrative(byEntity, byProblem, signals)
            };
        }

        public static IReadOnlyList<BusinessActionEntityLearningStats> ComposeByEntity(
            IReadOnlyList<BusinessActionRecord> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            return records
                .Where(r => !string.IsNullOrWhiteSpace(r.EntityId)
                    && r.EntityType is DecisionEntityType.Product
                        or DecisionEntityType.Category
                        or DecisionEntityType.Investment)
                .GroupBy(r => new { r.EntityType, EntityId = r.EntityId!.Trim() })
                .Select(g =>
                {
                    var items = g.ToList();
                    var typeStats = items
                        .GroupBy(x => x.ActionType)
                        .Select(tg => BusinessActionLearningComposer.BuildTypeStats(tg.Key, tg.ToList()))
                        .OrderBy(t => t.DisplayName)
                        .ToList();

                    int classified = typeStats.Sum(t => t.ClassifiedCount);
                    int successful = typeStats.Sum(t => t.SuccessfulCount);
                    int partial = typeStats.Sum(t => t.PartialCount);
                    int ineffective = typeStats.Sum(t => t.IneffectiveCount);
                    string name = items
                        .Select(x => x.EntityName)
                        .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                        ?? g.Key.EntityId;

                    return new BusinessActionEntityLearningStats
                    {
                        EntityType = g.Key.EntityType,
                        EntityId = g.Key.EntityId,
                        EntityName = name,
                        TotalCount = items.Count,
                        ClassifiedCount = classified,
                        SuccessfulCount = successful,
                        PartialCount = partial,
                        IneffectiveCount = ineffective,
                        SuccessRatePct = BusinessActionLearningComposer.Rate(successful, classified),
                        PartialRatePct = BusinessActionLearningComposer.Rate(partial, classified),
                        FailureRatePct = BusinessActionLearningComposer.Rate(ineffective, classified),
                        ByActionType = typeStats,
                        Summary = BuildEntitySummary(g.Key.EntityType, name, items.Count, classified, successful, ineffective)
                    };
                })
                .OrderByDescending(e => e.TotalCount)
                .ThenBy(e => e.EntityName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<BusinessActionProblemLearningStats> ComposeByProblem(
            IReadOnlyList<BusinessActionRecord> records,
            IReadOnlyDictionary<Guid, DecisionHistoryRecord> decisionsByEventId)
        {
            ArgumentNullException.ThrowIfNull(records);
            ArgumentNullException.ThrowIfNull(decisionsByEventId);

            return records
                .Select(r => (Record: r, Key: ResolveProblemKey(r, decisionsByEventId)))
                .Where(x => x.Key != null)
                .GroupBy(x => x.Key!.Value.Key)
                .Select(g =>
                {
                    var items = g.Select(x => x.Record).ToList();
                    var sample = g.First().Key!.Value;
                    var typeStats = items
                        .GroupBy(x => x.ActionType)
                        .Select(tg => BusinessActionLearningComposer.BuildTypeStats(tg.Key, tg.ToList()))
                        .OrderByDescending(t => t.SuccessRatePct ?? -1m)
                        .ThenByDescending(t => t.ClassifiedCount)
                        .ToList();

                    int classified = typeStats.Sum(t => t.ClassifiedCount);
                    int successful = typeStats.Sum(t => t.SuccessfulCount);
                    int partial = typeStats.Sum(t => t.PartialCount);
                    int ineffective = typeStats.Sum(t => t.IneffectiveCount);

                    int distinctDecisions = items
                        .Where(r => r.DecisionEventId.HasValue)
                        .Select(r => r.DecisionEventId!.Value)
                        .Distinct()
                        .Count();

                    BusinessActionTypeLearningStats? best = typeStats
                        .FirstOrDefault(t => t.ClassifiedCount >= 2 && t.SuccessRatePct.HasValue);

                    string? hint = best == null
                        ? null
                        : BusinessActionSoftLanguageGuard.EnsureHistoricalHint(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Las acciones de tipo {0} han tenido mejores resultados históricos para este tipo de problema ({1:0.#}% éxito en {2} clasificadas).",
                                best.DisplayName,
                                best.SuccessRatePct,
                                best.ClassifiedCount));

                    return new BusinessActionProblemLearningStats
                    {
                        ProblemKey = sample.Key,
                        DisplayName = sample.DisplayName,
                        Area = sample.Area,
                        LinkedActionCount = items.Count,
                        DistinctDecisionCount = distinctDecisions,
                        ClassifiedCount = classified,
                        SuccessfulCount = successful,
                        PartialCount = partial,
                        IneffectiveCount = ineffective,
                        SuccessRatePct = BusinessActionLearningComposer.Rate(successful, classified),
                        ByActionType = typeStats,
                        BestHistoricalActionType = best?.ActionType,
                        BestHistoricalHint = hint,
                        Summary = BuildProblemSummary(sample.DisplayName, items.Count, classified, successful, best)
                    };
                })
                .OrderByDescending(p => p.LinkedActionCount)
                .ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<BusinessActionLearningSignal> DetectSignals(
            IReadOnlyList<BusinessActionRecord> records,
            IEnumerable<DecisionHistoryRecord> decisions,
            int minOccurrences = BusinessActionContextualLearningPolicy.DefaultMinOccurrences)
        {
            ArgumentNullException.ThrowIfNull(records);
            ArgumentNullException.ThrowIfNull(decisions);
            if (minOccurrences < 2)
                minOccurrences = 2;

            var signals = new List<BusinessActionLearningSignal>();

            // TEST 10 — problema recurrente (decisiones: EventType + EntityId).
            foreach (var g in decisions.GroupBy(d => new
                     {
                         d.EventType,
                         EntityId = string.IsNullOrWhiteSpace(d.EntityId) ? "" : d.EntityId.Trim()
                     }))
            {
                int n = g.Count();
                if (n < minOccurrences)
                    continue;

                string key = $"{g.Key.EventType}|{g.Key.EntityId}";
                string label = string.IsNullOrEmpty(g.Key.EntityId)
                    ? g.Key.EventType
                    : $"{g.Key.EventType} · {g.Key.EntityId}";

                signals.Add(new BusinessActionLearningSignal
                {
                    Kind = BusinessActionLearningSignalKind.RecurrentProblem,
                    Key = key,
                    Label = label,
                    OccurrenceCount = n,
                    Message =
                        $"⚠️ PROBLEMA RECURRENTE: se observó {label} {n} veces. " +
                        "Revisar historial; no implica que se repita necesariamente."
                });
            }

            // Acciones inefectivas / históricamente efectivas por tipo (solo clasificadas).
            foreach (var g in records.GroupBy(r => r.ActionType))
            {
                BusinessActionTypeLearningStats stats =
                    BusinessActionLearningComposer.BuildTypeStats(g.Key, g.ToList());
                if (stats.ClassifiedCount < minOccurrences)
                    continue;

                string name = stats.DisplayName;

                if (stats.FailureRatePct is >= 60m)
                {
                    signals.Add(new BusinessActionLearningSignal
                    {
                        Kind = BusinessActionLearningSignalKind.IneffectiveActionPattern,
                        Key = g.Key.ToString(),
                        Label = name,
                        OccurrenceCount = stats.ClassifiedCount,
                        ClassifiedCount = stats.ClassifiedCount,
                        RatePct = stats.FailureRatePct,
                        Message =
                            $"⚠️ ACCIÓN POCO EFECTIVA: {name} muestra {stats.FailureRatePct:0.#}% no efectiva " +
                            $"en {stats.ClassifiedCount} clasificadas. Revisar estrategia; no garantiza fracaso futuro."
                    });
                }

                if (stats.SuccessRatePct is >= 60m)
                {
                    signals.Add(new BusinessActionLearningSignal
                    {
                        Kind = BusinessActionLearningSignalKind.HistoricallyEffectiveAction,
                        Key = g.Key.ToString(),
                        Label = name,
                        OccurrenceCount = stats.ClassifiedCount,
                        ClassifiedCount = stats.ClassifiedCount,
                        RatePct = stats.SuccessRatePct,
                        Message = BusinessActionSoftLanguageGuard.EnsureHistoricalHint(
                            $"Información histórica: {name} ha mostrado resultados positivos " +
                            $"({stats.SuccessRatePct:0.#}% éxito en {stats.ClassifiedCount} clasificadas).")
                    });
                }
            }

            return signals
                .OrderBy(s => (int)s.Kind)
                .ThenByDescending(s => s.OccurrenceCount)
                .ToList();
        }

        private static (string Key, string DisplayName, DecisionEventArea? Area)? ResolveProblemKey(
            BusinessActionRecord record,
            IReadOnlyDictionary<Guid, DecisionHistoryRecord> decisionsByEventId)
        {
            if (record.DecisionEventId.HasValue
                && decisionsByEventId.TryGetValue(record.DecisionEventId.Value, out DecisionHistoryRecord? d)
                && !string.IsNullOrWhiteSpace(d.EventType))
            {
                return (d.EventType.Trim(), d.EventType.Trim(), d.Area);
            }

            if (record.Area != default)
            {
                string key = $"area:{record.Area}";
                return (key, $"Área {record.Area}", record.Area);
            }

            return null;
        }

        private static string BuildEntitySummary(
            DecisionEntityType entityType,
            string name,
            int total,
            int classified,
            int successful,
            int ineffective)
        {
            if (classified == 0)
                return $"Se observaron {total} acción(es) sobre {entityType} '{name}' sin Outcomes clasificados aún.";

            return string.Format(
                CultureInfo.InvariantCulture,
                "Histórico {0} '{1}': {2} exitosas, {3} no efectivas de {4} clasificadas. No garantiza resultados futuros.",
                entityType, name, successful, ineffective, classified);
        }

        private static string BuildProblemSummary(
            string problem,
            int linked,
            int classified,
            int successful,
            BusinessActionTypeLearningStats? best)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "Problema '{0}': {1} acción(es) vinculadas",
                problem, linked);
            if (classified > 0)
            {
                sb.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "; {0}/{1} exitosas históricamente",
                    successful, classified);
            }
            if (best != null)
                sb.AppendFormat(CultureInfo.InvariantCulture, ". Mejor tipo histórico: {0}", best.DisplayName);
            sb.Append(". Información histórica; no es una garantía futura.");
            return BusinessActionSoftLanguageGuard.EnsureHistoricalHint(sb.ToString());
        }

        private static string BuildNarrative(
            IReadOnlyList<BusinessActionEntityLearningStats> byEntity,
            IReadOnlyList<BusinessActionProblemLearningStats> byProblem,
            IReadOnlyList<BusinessActionLearningSignal> signals)
        {
            if (byEntity.Count == 0 && byProblem.Count == 0 && signals.Count == 0)
                return "Sin datos contextuales suficientes para aprendizaje por producto/problema.";

            var sb = new StringBuilder();
            if (byEntity.Count > 0)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} entidad(es) con historial. ", byEntity.Count);
            if (byProblem.Count > 0)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} problema(s) con acciones vinculadas. ", byProblem.Count);
            if (signals.Count > 0)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} señal(es) de recurrencia/efectividad. ", signals.Count);
            sb.Append("Todo es histórico; no garantiza resultados futuros.");
            return sb.ToString();
        }
    }
}
