using System.Diagnostics;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato timeline decisión→acción→resultado (FASE 11.11 + perf 11.21).</summary>
    public static class BusinessActionTimelinePolicy
    {
        public const string Definition =
            "FASE 11.11/11.21: timeline ordenada Decisión detectada → Acción → Baseline → Completada → " +
            "Post-métricas → Outcome. Prefetch decisiones en lote (≤2 consultas). Sin N+1. Sin mutar POS.";

        public const string SoftLanguage =
            "Títulos descriptivos ('se registró', 'se observó'). Sin 'causó' ni garantías (FASE 11.23).";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Compone timeline pura desde Action + DecisionHistory opcional (FASE 11.11).</summary>
    public static class BusinessActionTimelineComposer
    {
        public static BusinessActionTimeline Build(
            BusinessActionRecord action,
            DecisionHistoryRecord? decision = null)
        {
            ArgumentNullException.ThrowIfNull(action);

            var steps = new List<BusinessActionTimelineStep>();

            if (decision != null)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.DecisionDetected,
                    decision.DetectedAt,
                    "Decisión detectada",
                    Truncate($"{decision.Title} ({decision.EventType})"),
                    null));

                if (decision.ResolvedAt.HasValue)
                {
                    steps.Add(Step(
                        BusinessActionTimelineStepKind.DecisionResolved,
                        decision.ResolvedAt.Value,
                        "Decisión cerrada en historial",
                        Truncate(decision.ResolutionNote ?? decision.Status.ToString()),
                        decision.ResolvedBy));
                }
            }
            else if (action.DecisionEventId.HasValue)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.DecisionDetected,
                    action.CreatedAt,
                    "Decisión vinculada",
                    $"EventId {action.DecisionEventId:N}",
                    null));
            }

            steps.Add(Step(
                BusinessActionTimelineStepKind.ActionRegistered,
                action.CreatedAt,
                "Acción registrada",
                Truncate($"{BusinessActionCatalog.DisplayName(action.ActionType)}: {action.Description}"),
                action.CreatedBy));

            if (action.StartedAt.HasValue
                && action.Status is BusinessActionStatus.InProgress
                    or BusinessActionStatus.Completed
                    or BusinessActionStatus.Cancelled
                    or BusinessActionStatus.NoResult)
            {
                if (!NearlySame(action.StartedAt.Value, action.CreatedAt))
                {
                    steps.Add(Step(
                        BusinessActionTimelineStepKind.ActionStarted,
                        action.StartedAt.Value,
                        "Acción en proceso",
                        BusinessActionCatalog.StatusLabel(BusinessActionStatus.InProgress),
                        action.CreatedBy));
                }
            }

            if (action.Baseline?.HasMetrics == true)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.BaselineCaptured,
                    action.Baseline.CapturedAt,
                    "Baseline capturado",
                    $"{action.Baseline.Metrics.Count} métrica(s) · {action.Baseline.SourceNote}",
                    null));
            }

            if (action.Status == BusinessActionStatus.Completed && action.CompletedAt.HasValue)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.ActionCompleted,
                    action.CompletedAt.Value,
                    "Acción completada",
                    action.EvaluationDueAt.HasValue
                        ? $"Ventana de evaluación hasta {action.EvaluationDueAt:yyyy-MM-dd}"
                        : "Completada",
                    action.CompletedBy));

                if (action.EvaluationDueAt.HasValue)
                {
                    steps.Add(Step(
                        BusinessActionTimelineStepKind.EvaluationWindowReady,
                        action.EvaluationDueAt.Value,
                        "Ventana de evaluación vencida",
                        $"EvaluationDays={action.EvaluationDays?.ToString() ?? "—"}",
                        null));
                }
            }

            if (action.Status == BusinessActionStatus.Cancelled && action.CompletedAt.HasValue)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.ActionCancelled,
                    action.CompletedAt.Value,
                    "Acción cancelada",
                    "No clasificar como Exitosa (TEST 8).",
                    action.CompletedBy));
            }

            if (action.Status == BusinessActionStatus.NoResult && action.CompletedAt.HasValue)
            {
                steps.Add(Step(
                    BusinessActionTimelineStepKind.ActionNoResult,
                    action.CompletedAt.Value,
                    "Sin resultado",
                    "Cerrada sin evaluación de Outcome.",
                    action.CompletedBy));
            }

            if (action.ActualImpact?.Deltas is { Count: > 0 })
            {
                DateTime at = action.CompletedAt ?? action.EvaluationDueAt ?? action.CreatedAt;
                steps.Add(Step(
                    BusinessActionTimelineStepKind.PostMetricsCaptured,
                    at,
                    "Post-métricas capturadas",
                    $"{action.ActualImpact.Deltas.Count} delta(s) observados",
                    null));
            }

            if (action.ActualImpact != null
                && action.ActualImpact.Outcome is not BusinessActionOutcome.Unspecified)
            {
                DateTime at = action.CompletedAt ?? action.CreatedAt;
                steps.Add(Step(
                    BusinessActionTimelineStepKind.OutcomeEvaluated,
                    at.AddSeconds(1),
                    "Resultado evaluado",
                    $"{BusinessActionCatalog.OutcomeGlyph(action.ActualImpact.Outcome)} " +
                    $"{BusinessActionCatalog.OutcomeLabel(action.ActualImpact.Outcome)} · " +
                    $"confianza {BusinessActionCatalog.ConfidenceLabel(action.ActualImpact.Confidence)}",
                    action.CompletedBy));

                BusinessActionObservedCapitalImpact capital =
                    BusinessActionCapitalImpactComposer.FromDeltas(action.ActualImpact.Deltas);
                if (capital.HasAnySignal)
                {
                    steps.Add(Step(
                        BusinessActionTimelineStepKind.CapitalImpactNoted,
                        at.AddSeconds(2),
                        "Capital / incremento observado",
                        Truncate(capital.Narrative),
                        null));
                }
            }

            IReadOnlyList<BusinessActionTimelineStep> ordered = steps
                .OrderBy(s => s.AtUtc)
                .ThenBy(s => (int)s.Kind)
                .ToList();

            DateTime? from = ordered.Count > 0 ? ordered[0].AtUtc : null;
            DateTime? to = ordered.Count > 0 ? ordered[^1].AtUtc : null;

            return new BusinessActionTimeline
            {
                ActionId = action.ActionId,
                DecisionEventId = action.DecisionEventId ?? decision?.EventId,
                DecisionHistoryId = action.DecisionHistoryId ?? (decision?.Id > 0 ? decision.Id : null),
                ActionDescription = action.Description,
                ActionType = action.ActionType,
                ActionStatus = action.Status,
                Outcome = action.ActualImpact == null
                    || action.ActualImpact.Outcome == BusinessActionOutcome.Unspecified
                    ? null
                    : action.ActualImpact.Outcome,
                SpanFrom = from,
                SpanTo = to,
                Steps = ordered,
                SummaryLabel = BuildSummaryLabel(action, ordered.Count)
            };
        }

        private static string BuildSummaryLabel(BusinessActionRecord action, int stepCount)
        {
            string outcome = action.ActualImpact?.Outcome is BusinessActionOutcome o
                && o != BusinessActionOutcome.Unspecified
                ? BusinessActionCatalog.OutcomeLabel(o)
                : BusinessActionCatalog.StatusLabel(action.Status);
            return $"{BusinessActionCatalog.DisplayName(action.ActionType)} · {outcome} · {stepCount} hitos";
        }

        private static BusinessActionTimelineStep Step(
            BusinessActionTimelineStepKind kind,
            DateTime at,
            string title,
            string detail,
            string? actor)
            => new()
            {
                Kind = kind,
                AtUtc = at,
                Title = title,
                Detail = detail,
                Actor = string.IsNullOrWhiteSpace(actor) ? null : actor.Trim()
            };

        private static bool NearlySame(DateTime a, DateTime b)
            => Math.Abs((a - b).TotalSeconds) < 2;

        private static string Truncate(string? text, int max = 180)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "—";
            string t = text.Trim();
            return t.Length <= max ? t : t[..(max - 1)] + "…";
        }
    }

    /// <summary>
    /// Servicio de timeline (FASE 11.11 + 11.21).
    /// Listados: 1 query acciones + ≤2 batch decisiones (nunca N+1).
    /// </summary>
    public sealed class BusinessActionTimelineService
    {
        private readonly IBusinessActionStore _actions;
        private readonly IDecisionHistoryStore _decisions;

        public BusinessActionTimelineService(
            IBusinessActionStore? actions = null,
            IDecisionHistoryStore? decisions = null)
        {
            _actions = actions ?? new SqlBusinessActionStore();
            _decisions = decisions ?? new SqlDecisionHistoryStore();
        }

        public BusinessActionTimeline? GetByActionId(Guid actionId)
        {
            BusinessActionRecord? action = _actions.FindByActionId(actionId);
            if (action == null)
                return null;

            DecisionHistoryRecord? decision = ResolveDecision(action);
            return BusinessActionTimelineComposer.Build(action, decision);
        }

        public IReadOnlyList<BusinessActionTimeline> GetByDecisionEventId(Guid decisionEventId)
            => GetByDecisionEventIdBatch(decisionEventId).Items;

        public BusinessActionTimelineBatch GetByDecisionEventIdBatch(Guid decisionEventId)
        {
            var sw = Stopwatch.StartNew();

            DecisionHistoryRecord? decision = _decisions.FindByEventId(decisionEventId);
            IReadOnlyList<BusinessActionRecord> actions = _actions.Query(new BusinessActionQuery
            {
                DecisionEventId = decisionEventId,
                Top = 100
            });

            var items = actions
                .Select(a => BusinessActionTimelineComposer.Build(a, decision))
                .OrderByDescending(t => t.SpanFrom ?? DateTime.MinValue)
                .ToList();

            sw.Stop();
            return new BusinessActionTimelineBatch
            {
                Items = items,
                Stats = new BusinessActionTimelineLoadStats
                {
                    ActionStoreCalls = 1,
                    DecisionStoreCalls = 1,
                    ActionsLoaded = actions.Count,
                    DecisionsPrefetched = decision == null ? 0 : 1,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    PolicyNote = BusinessActionTimelinePolicy.Definition
                }
            };
        }

        public IReadOnlyList<BusinessActionTimeline> ListRecent(int top = 50)
            => ListRecentBatch(top).Items;

        /// <summary>Listado reciente con stats (FASE 11.21 — prefetch batch).</summary>
        public BusinessActionTimelineBatch ListRecentBatch(int top = 50)
        {
            var sw = Stopwatch.StartNew();
            int take = top <= 0 ? 50 : Math.Min(top, 200);

            IReadOnlyList<BusinessActionRecord> actions =
                _actions.Query(new BusinessActionQuery { Top = take });

            var (byId, byEvent, decisionCalls) = PrefetchDecisions(actions);

            var items = new List<BusinessActionTimeline>(actions.Count);
            foreach (BusinessActionRecord action in actions)
            {
                DecisionHistoryRecord? decision = ResolveFromPrefetch(action, byId, byEvent);
                items.Add(BusinessActionTimelineComposer.Build(action, decision));
            }

            sw.Stop();
            return new BusinessActionTimelineBatch
            {
                Items = items,
                Stats = new BusinessActionTimelineLoadStats
                {
                    ActionStoreCalls = 1,
                    DecisionStoreCalls = decisionCalls,
                    ActionsLoaded = actions.Count,
                    DecisionsPrefetched = byId.Count + byEvent.Count,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    PolicyNote = BusinessActionTimelinePolicy.Definition
                }
            };
        }

        private (Dictionary<long, DecisionHistoryRecord> byId,
            Dictionary<Guid, DecisionHistoryRecord> byEvent,
            int decisionCalls) PrefetchDecisions(IReadOnlyList<BusinessActionRecord> actions)
        {
            var byId = new Dictionary<long, DecisionHistoryRecord>();
            var byEvent = new Dictionary<Guid, DecisionHistoryRecord>();
            int decisionCalls = 0;

            var historyIds = actions
                .Where(a => a.DecisionHistoryId is > 0)
                .Select(a => a.DecisionHistoryId!.Value)
                .Distinct()
                .ToList();

            if (historyIds.Count > 0)
            {
                decisionCalls++;
                foreach (DecisionHistoryRecord d in _decisions.FindManyByIds(historyIds))
                    byId[d.Id] = d;
            }

            var missingEventIds = actions
                .Where(a =>
                    a.DecisionEventId.HasValue
                    && !(a.DecisionHistoryId is > 0 && byId.ContainsKey(a.DecisionHistoryId.Value)))
                .Select(a => a.DecisionEventId!.Value)
                .Distinct()
                .ToList();

            if (missingEventIds.Count > 0)
            {
                decisionCalls++;
                foreach (DecisionHistoryRecord d in _decisions.FindManyByEventIds(missingEventIds))
                    byEvent[d.EventId] = d;
            }

            return (byId, byEvent, decisionCalls);
        }

        private static DecisionHistoryRecord? ResolveFromPrefetch(
            BusinessActionRecord action,
            IReadOnlyDictionary<long, DecisionHistoryRecord> byId,
            IReadOnlyDictionary<Guid, DecisionHistoryRecord> byEvent)
        {
            if (action.DecisionHistoryId is > 0
                && byId.TryGetValue(action.DecisionHistoryId.Value, out DecisionHistoryRecord? byHistory))
                return byHistory;

            if (action.DecisionEventId.HasValue
                && byEvent.TryGetValue(action.DecisionEventId.Value, out DecisionHistoryRecord? byEv))
                return byEv;

            return null;
        }

        private DecisionHistoryRecord? ResolveDecision(BusinessActionRecord action)
        {
            if (action.DecisionHistoryId is > 0)
            {
                DecisionHistoryRecord? byId = _decisions.FindById(action.DecisionHistoryId.Value);
                if (byId != null)
                    return byId;
            }

            if (action.DecisionEventId.HasValue)
                return _decisions.FindByEventId(action.DecisionEventId.Value);

            return null;
        }
    }
}
