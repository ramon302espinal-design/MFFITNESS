using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ActionEvaluationService (FASE 11.9).</summary>
    public static class BusinessActionEvaluationPolicy
    {
        public const string Definition =
            "FASE 11.9: clasifica Outcome (Exitosa/Parcial/No efectiva/Sin datos) + Confidence. " +
            "Sin ML. Sin causalidad. Cancelada ≠ Exitosa (TEST 8). " +
            "Sin baseline/deltas ⇒ InsufficientData (TEST 7/12).";

        public const string Classification =
            "Por dirección deseada de métricas (no umbrales de negocio inventados): " +
            "favorable / desfavorable / neutro-N/D. " +
            "Exitosa = solo favorables; Parcial = mixtas; No efectiva = solo desfavorables; " +
            "Sin datos = sin señales comparables.";

        public const string DesiredDirection =
            "↑ bueno: ventas/ganancia/margen/ROI. " +
            "↓ bueno: capital inmovilizado/en riesgo/congelado; stock si StockReduction; " +
            "↑ stock si Replenishment.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Clasificación pura Outcome/Confidence (FASE 11.9).</summary>
    public static class BusinessActionEvaluationMath
    {
        /// <summary>+1 = subir es favorable; -1 = bajar es favorable; 0 = sin señal.</summary>
        public static int DesiredSign(string metricKey, BusinessActionType actionType)
        {
            if (string.IsNullOrWhiteSpace(metricKey))
                return 0;

            string key = metricKey.Trim().ToLowerInvariant();

            if (key.Contains("immobilized")
                || key.Contains("at_risk")
                || key.Contains("frozen")
                || key.Contains("idle")
                || key.Contains("trapped"))
                return -1;

            if (key.Contains("inv.stock") || key.EndsWith(".stock"))
            {
                if (actionType == BusinessActionType.StockReduction)
                    return -1;
                if (actionType == BusinessActionType.Replenishment)
                    return +1;
                return 0;
            }

            if (key.Contains("revenue")
                || key.Contains("profit")
                || key.Contains("margin")
                || key.Contains("units")
                || key.Contains("roi")
                || key.Contains("ticket")
                || key.Contains("transactions"))
                return +1;

            if (key.Contains("capital.inventory") && actionType == BusinessActionType.StockReduction)
                return -1;

            return 0;
        }

        public static int Favorability(BusinessActionMetricDelta delta, BusinessActionType actionType)
        {
            ArgumentNullException.ThrowIfNull(delta);
            int desired = DesiredSign(delta.MetricKey, actionType);
            if (desired == 0 || !delta.Change.HasValue || delta.Change.Value == 0m)
                return 0;

            int observed = delta.Change.Value > 0m ? 1 : -1;
            return observed == desired ? 1 : -1;
        }

        public static (BusinessActionOutcome Outcome, int Fav, int Unfav, int Other)
            Classify(IReadOnlyList<BusinessActionMetricDelta>? deltas, BusinessActionType actionType)
        {
            int fav = 0, unfav = 0, other = 0;
            if (deltas != null)
            {
                foreach (BusinessActionMetricDelta d in deltas)
                {
                    int f = Favorability(d, actionType);
                    if (f > 0) fav++;
                    else if (f < 0) unfav++;
                    else other++;
                }
            }

            BusinessActionOutcome outcome;
            if (fav == 0 && unfav == 0)
                outcome = BusinessActionOutcome.InsufficientData;
            else if (fav > 0 && unfav == 0)
                outcome = BusinessActionOutcome.Successful;
            else if (unfav > 0 && fav == 0)
                outcome = BusinessActionOutcome.Ineffective;
            else
                outcome = BusinessActionOutcome.Partial;

            return (outcome, fav, unfav, other);
        }

        public static BusinessActionConfidence ConfidenceFor(
            BusinessActionOutcome outcome,
            int comparableSignals,
            BusinessActionEvaluationPhase windowPhase,
            bool hasBaseline)
        {
            if (outcome == BusinessActionOutcome.InsufficientData || !hasBaseline)
                return BusinessActionConfidence.Low;

            if (windowPhase == BusinessActionEvaluationPhase.InWindow)
                return BusinessActionConfidence.Low;

            if (comparableSignals >= 3 && windowPhase is BusinessActionEvaluationPhase.Ready
                    or BusinessActionEvaluationPhase.Evaluated)
                return BusinessActionConfidence.High;

            if (comparableSignals >= 1)
                return BusinessActionConfidence.Medium;

            return BusinessActionConfidence.Low;
        }

        public static string BuildEvaluationSummary(
            BusinessActionOutcome outcome,
            IReadOnlyList<BusinessActionMetricDelta>? deltas,
            int fav,
            int unfav)
        {
            string observed = deltas is { Count: > 0 }
                ? BusinessActionMetricDeltaMath.BuildObservedSummary(deltas)
                : "Sin deltas comparables.";

            string verdict = outcome switch
            {
                BusinessActionOutcome.Successful =>
                    $"Clasificación sugerida: {BusinessActionCatalog.OutcomeGlyph(outcome)} EXITOSA " +
                    $"({fav} métrica(s) en dirección favorable). Sin afirmar causalidad.",
                BusinessActionOutcome.Partial =>
                    $"Clasificación sugerida: {BusinessActionCatalog.OutcomeGlyph(outcome)} PARCIAL " +
                    $"({fav} favorable(s), {unfav} desfavorable(s)). Revisar contexto.",
                BusinessActionOutcome.Ineffective =>
                    $"Clasificación sugerida: {BusinessActionCatalog.OutcomeGlyph(outcome)} NO EFECTIVA " +
                    $"({unfav} métrica(s) en dirección desfavorable). Sin afirmar causalidad.",
                BusinessActionOutcome.InsufficientData =>
                    $"Clasificación: {BusinessActionCatalog.OutcomeGlyph(outcome)} SIN DATOS — no inventar resultado.",
                _ => "Clasificación pendiente."
            };

            return BusinessActionSoftLanguageGuard.EnsureObserved(observed + " " + verdict);
        }
    }

    /// <summary>Evalúa y persiste Outcome de acción (FASE 11.9 / audit 11.12).</summary>
    public sealed class BusinessActionEvaluationService
    {
        private readonly IBusinessActionStore _store;
        private readonly IBusinessActionAuditStore? _audit;

        public BusinessActionEvaluationService(
            IBusinessActionStore? store = null,
            IBusinessActionAuditStore? audit = null)
        {
            _store = store ?? new SqlBusinessActionStore();
            _audit = audit;
        }

        /// <summary>Sugerencia pura — no persiste.</summary>
        public BusinessActionEvaluationResult Suggest(
            BusinessActionRecord record,
            DateTime? asOfUtc = null)
        {
            ArgumentNullException.ThrowIfNull(record);
            return BuildSuggestion(record, asOfUtc, usedOverride: false, overrideOutcome: null, overrideConfidence: null);
        }

        public BusinessActionEvaluationResult Evaluate(BusinessActionEvaluateRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            BusinessActionRecord? current = _store.FindByActionId(request.ActionId);
            if (current == null)
                return Fail("No se encontró la acción.");

            if (current.Status == BusinessActionStatus.Cancelled)
                return Fail("Cancelada: no clasificar como Exitosa (TEST 8).", current);

            if (current.Status == BusinessActionStatus.NoResult)
                return Fail("Sin resultado: no hay evaluación de Outcome.", current);

            if (current.Status != BusinessActionStatus.Completed)
                return Fail("Solo se evalúan acciones Completadas.", current);

            DateTime asOf = request.AsOfUtc ?? DateTime.UtcNow;
            BusinessActionEvaluationWindow window =
                BusinessActionEvaluationWindowMath.Resolve(current, asOf);

            if (window.Phase == BusinessActionEvaluationPhase.InWindow && !request.AllowBeforeWindowEnd)
            {
                return Fail(
                    $"Ventana aún abierta ({window.DaysRemaining} d). Use AllowBeforeWindowEnd o espere.",
                    current,
                    window);
            }

            BusinessActionEvaluationResult suggestion = BuildSuggestion(
                current,
                asOf,
                usedOverride: request.OverrideOutcome.HasValue,
                overrideOutcome: request.OverrideOutcome,
                overrideConfidence: request.OverrideConfidence);

            if (!suggestion.Success)
                return suggestion;

            if (!BusinessActionCatalog.CanAssignOutcome(current.Status, suggestion.Outcome))
                return Fail("Outcome no permitido para el estado.", current, window);

            IReadOnlyList<BusinessActionMetricDelta> deltas =
                current.ActualImpact?.Deltas ?? Array.Empty<BusinessActionMetricDelta>();

            string? notes = MergeNotes(current.ActualImpact?.Notes, request.Notes);
            if (!string.IsNullOrWhiteSpace(request.Actor))
                notes = MergeNotes(notes, $"Evaluado por {request.Actor.Trim()}");

            var actual = new BusinessActionActualImpact
            {
                Outcome = suggestion.Outcome,
                Confidence = suggestion.Confidence,
                Summary = suggestion.Summary,
                Notes = notes,
                Deltas = deltas
            };

            BusinessActionRecord updated = BusinessActionRecordFactory.WithActualImpact(current, actual);
            if (_store.Replace(updated) == null)
                return Fail("No se pudo persistir la evaluación.", current, window);

            if (_audit != null)
            {
                try
                {
                    _audit.Append(BusinessActionAuditService.FromEvaluate(
                        updated, request.Actor, request.Notes, asOf));
                }
                catch
                {
                    // Auditoría no tumba evaluación.
                }
            }

            return new BusinessActionEvaluationResult
            {
                Success = true,
                Message =
                    $"{BusinessActionCatalog.OutcomeGlyph(suggestion.Outcome)} " +
                    $"{BusinessActionCatalog.OutcomeLabel(suggestion.Outcome)} · " +
                    $"confianza {BusinessActionCatalog.ConfidenceLabel(suggestion.Confidence)}.",
                Record = updated,
                Outcome = suggestion.Outcome,
                Confidence = suggestion.Confidence,
                Summary = suggestion.Summary,
                FavorableCount = suggestion.FavorableCount,
                UnfavorableCount = suggestion.UnfavorableCount,
                NeutralOrUnknownCount = suggestion.NeutralOrUnknownCount,
                UsedOverride = suggestion.UsedOverride,
                Window = window,
                CapitalImpact = suggestion.CapitalImpact
            };
        }

        /// <summary>Lectura FASE 11.10 — capital liberado / incrementos (sin I/O extra).</summary>
        public BusinessActionObservedCapitalImpact? GetCapitalImpact(Guid actionId)
        {
            BusinessActionRecord? record = _store.FindByActionId(actionId);
            return record == null
                ? null
                : BusinessActionCapitalImpactComposer.FromRecord(record);
        }

        private static BusinessActionEvaluationResult BuildSuggestion(
            BusinessActionRecord record,
            DateTime? asOfUtc,
            bool usedOverride,
            BusinessActionOutcome? overrideOutcome,
            BusinessActionConfidence? overrideConfidence)
        {
            BusinessActionEvaluationWindow window =
                BusinessActionEvaluationWindowMath.Resolve(record, asOfUtc);

            bool hasBaseline = record.Baseline?.HasMetrics == true;
            IReadOnlyList<BusinessActionMetricDelta> deltas =
                record.ActualImpact?.Deltas ?? Array.Empty<BusinessActionMetricDelta>();

            // Preferir claves esperadas si hay deltas filtrables
            if (record.ExpectedImpact?.TargetMetricKeys is { Count: > 0 } keys
                && deltas.Count > 0)
            {
                var set = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
                var filtered = deltas.Where(d => set.Contains(d.MetricKey)).ToList();
                if (filtered.Count > 0)
                    deltas = filtered;
            }

            var (autoOutcome, fav, unfav, other) =
                BusinessActionEvaluationMath.Classify(deltas, record.ActionType);

            if (!hasBaseline && deltas.Count == 0)
                autoOutcome = BusinessActionOutcome.InsufficientData;

            BusinessActionOutcome outcome = overrideOutcome ?? autoOutcome;
            int comparable = fav + unfav;
            BusinessActionConfidence confidence = overrideConfidence
                ?? BusinessActionEvaluationMath.ConfidenceFor(
                    outcome, comparable, window.Phase, hasBaseline);

            if (usedOverride && overrideOutcome == BusinessActionOutcome.Successful
                && record.Status == BusinessActionStatus.Cancelled)
            {
                return Fail("Cancelada ≠ Exitosa (TEST 8).", record, window);
            }

            string summary = BusinessActionEvaluationMath.BuildEvaluationSummary(
                outcome, deltas, fav, unfav);

            BusinessActionObservedCapitalImpact capital =
                BusinessActionCapitalImpactComposer.FromDeltas(deltas);
            if (capital.HasAnySignal)
                summary = summary + " " + capital.Narrative;

            return new BusinessActionEvaluationResult
            {
                Success = true,
                Message = "Sugerencia de evaluación lista.",
                Record = record,
                Outcome = outcome,
                Confidence = confidence,
                Summary = summary,
                FavorableCount = fav,
                UnfavorableCount = unfav,
                NeutralOrUnknownCount = other,
                UsedOverride = usedOverride,
                Window = window,
                CapitalImpact = capital
            };
        }

        private static string? MergeNotes(string? existing, string? incoming)
        {
            if (string.IsNullOrWhiteSpace(incoming))
                return existing;
            if (string.IsNullOrWhiteSpace(existing))
                return incoming.Trim();
            return existing.Trim() + " | " + incoming.Trim();
        }

        private static BusinessActionEvaluationResult Fail(
            string message,
            BusinessActionRecord? record = null,
            BusinessActionEvaluationWindow? window = null)
            => new()
            {
                Success = false,
                Message = message,
                Record = record,
                Window = window ?? (record == null
                    ? null
                    : BusinessActionEvaluationWindowMath.Resolve(record)),
                Outcome = BusinessActionOutcome.Unspecified,
                Confidence = BusinessActionConfidence.Unspecified
            };
    }
}
