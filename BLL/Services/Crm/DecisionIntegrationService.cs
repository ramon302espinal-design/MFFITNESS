using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Cierre formal FASE 10 (10.28 / brief §129).</summary>
    public static class DecisionPhasePolicy
    {
        public const string Phase = "FASE 10";
        public const string Stage = "10.28";

        public const string Definition =
            "FASE 10.28 — Integración final: Centro de decisiones operativo. " +
            "Detecta · explica · prioriza · agrupa · recomienda · registra · " +
            "historial · dedup · auditoría · Forms · arquitectura intacta. " +
            "El usuario DECIDE — sin auto-compra ni mutación de stock/caja.";

        public const string SuccessCriteria =
            "1 Explicar · 2 Priorizar · 3 Agrupar · 4 Recomendar · 5 Registrar estado · " +
            "6 Historial · 7 Sin duplicados · 8 Trazabilidad · 9 Forms · 10 Arquitectura.";

        public const string Deferred = "FASE 10 completa.";

        public const bool IsComplete = true;
    }

    /// <summary>Resultado de un run integrado (motor + historial opcional).</summary>
    public sealed class DecisionIntegrationResult
    {
        public DecisionCenterReport Center { get; init; } = null!;
        public DecisionHistoryCaptureResult? Capture { get; init; }
        public DecisionHistoryReconcileResult? Reconcile { get; init; }
        public bool Persisted { get; init; }
        public string PolicyNote { get; init; } = string.Empty;
    }

    /// <summary>
    /// Fachada de integración final (FASE 10.28).
    /// Orquesta Centro + Capture + ReconcileAbsent. Sin auto-acciones de negocio.
    /// </summary>
    public sealed class DecisionIntegrationService
    {
        private readonly DecisionCenterService _center;
        private readonly DecisionHistoryService? _history;

        public DecisionIntegrationService(
            DecisionCenterService? center = null,
            DecisionHistoryService? history = null)
        {
            _center = center ?? new DecisionCenterService();
            _history = history;
        }

        /// <summary>Run completo en memoria (tests / UI sin persistir).</summary>
        public DecisionIntegrationResult RunInMemory(
            DecisionRuleContext? context = null,
            DecisionCenterSnapshot? snapshot = null,
            DecisionAnalyticsBundleHooks? hooks = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities)
        {
            DecisionCenterReport center = _center.RunBuiltIn(
                context, snapshot, maxPriorities, hooks);

            return new DecisionIntegrationResult
            {
                Center = center,
                Persisted = false,
                PolicyNote = DecisionPhasePolicy.Definition
            };
        }

        /// <summary>
        /// Run + Capture + ReconcileAbsent (TEST 8/9).
        /// Requiere historial inyectado (memoria o SQL).
        /// </summary>
        public DecisionIntegrationResult RunAndPersist(
            DecisionHistoryService history,
            DecisionRuleContext? context = null,
            DecisionCenterSnapshot? snapshot = null,
            DecisionAnalyticsBundleHooks? hooks = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities)
        {
            ArgumentNullException.ThrowIfNull(history);

            DecisionCenterReport center = _center.RunBuiltIn(
                context, snapshot, maxPriorities, hooks);

            DecisionHistoryCaptureResult? capture = null;
            DecisionHistoryReconcileResult? reconcile = null;

            if (center.Engine != null)
            {
                capture = history.Capture(center.Engine);
                reconcile = history.ReconcileAbsent(center.Engine);
            }

            return new DecisionIntegrationResult
            {
                Center = center,
                Capture = capture,
                Reconcile = reconcile,
                Persisted = true,
                PolicyNote = DecisionPhasePolicy.Definition + " " + DecisionHistoryPolicy.Reconcile
            };
        }

        /// <summary>
        /// Hint de prioridad por área (Forms de dominio — una línea, sin saturar).
        /// </summary>
        public static string AreaPriorityHint(
            DecisionCenterReport? center,
            params DecisionEventArea[] areas)
        {
            if (center == null || areas.Length == 0)
                return string.Empty;

            var set = areas.ToHashSet();
            DecisionCenterPriorityItem? hit = center.PrioritiesToday
                .FirstOrDefault(p =>
                {
                    if (p.PrimaryEventId == null || center.Engine == null)
                        return false;
                    DecisionEvent? e = center.Engine.Events
                        .FirstOrDefault(x => x.EventId == p.PrimaryEventId);
                    return e != null && set.Contains(e.Area);
                });

            if (hit == null)
            {
                int n = DecisionCenterDisplay.CountEventsInAreas(center, areas);
                return n <= 0
                    ? string.Empty
                    : $"Centro · {n} señal(es) en área — ver Decisiones.";
            }

            return DecisionCenterDisplay.PriorityLine(hit, maxLen: 100);
        }
    }
}
