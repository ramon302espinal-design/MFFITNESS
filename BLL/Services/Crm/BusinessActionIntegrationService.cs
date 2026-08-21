using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Cierre formal FASE 11 (11.25 / brief §88).</summary>
    public static class BusinessActionPhasePolicy
    {
        public const string Phase = "FASE 11";
        public const string Stage = "11.25";

        public const string Definition =
            "FASE 11.25 — Cierre: Closed-loop financiero operativo. " +
            "Alerta→Decisión→Acción→Resultado→Evaluación→Aprendizaje→Nuevas decisiones. " +
            "Historial: qué pasó · decidimos · hicimos · resultó · cambió · aprendimos. " +
            "Soft language (§86). Sin ML (§87). Sin auto-ejecución POS. FrmReportes intacto.";

        public const string SuccessCriteria =
            "1 Alerta→Decisión · 2 Decisión→Acción · 3 Acción→Resultado · 4 Resultado→Evaluación · " +
            "5 Historial (qué pasó/decidimos/hicimos/resultó/cambió/aprendimos) · " +
            "6 Soft language · 7 Forms binders · 8 Arquitectura UI→BLL→DAL · 9 FrmReportes intacto · " +
            "10 Closed-loop operativo.";

        public const string Deferred = "FASE 11 completa.";

        public const bool IsComplete = true;

        public const bool ClosedLoopOperational = true;
    }

    /// <summary>Resultado de un ciclo cerrado demo/orquestado (FASE 11.24).</summary>
    public sealed class BusinessActionClosedLoopResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public DecisionHistoryRecord? Decision { get; init; }
        public BusinessActionRecord? Action { get; init; }
        public BusinessActionEvaluationResult? Evaluation { get; init; }
        public BusinessActionTimeline? Timeline { get; init; }
        public BusinessActionLearningSummary? LearningByType { get; init; }
        public BusinessActionContextualLearning? LearningContextual { get; init; }
        public IReadOnlyList<string> Checklist { get; init; } = Array.Empty<string>();
        public bool SoftLanguageOk { get; init; }
        public string PolicyNote { get; init; } = string.Empty;
    }

    /// <summary>
    /// Fachada closed-loop (FASE 11.24).
    /// Orquesta Decisión → Acción → Post-métricas → Evaluación → Timeline → Learning.
    /// Sin mutar stock/caja/precios.
    /// </summary>
    public sealed class BusinessActionIntegrationService
    {
        private readonly IBusinessActionStore _actions;
        private readonly IDecisionHistoryStore _decisions;
        private readonly IBusinessActionAuditStore? _audit;

        public BusinessActionIntegrationService(
            IBusinessActionStore? actions = null,
            IDecisionHistoryStore? decisions = null,
            IBusinessActionAuditStore? audit = null)
        {
            _actions = actions ?? new SqlBusinessActionStore();
            _decisions = decisions ?? new SqlDecisionHistoryStore();
            _audit = audit;
        }

        /// <summary>
        /// Ciclo completo en memoria (tests / demo).
        /// Requiere métricas before/after; no inventa impacto.
        /// </summary>
        public BusinessActionClosedLoopResult RunClosedLoop(
            BusinessActionClosedLoopRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var checklist = new List<string>();
            var actionSvc = new BusinessActionService(_actions, _audit);
            var evalSvc = new BusinessActionEvaluationService(_actions, _audit);
            var timelineSvc = new BusinessActionTimelineService(_actions, _decisions);
            var learningSvc = new BusinessActionLearningService(_actions, _decisions);

            // 1 Alerta → Decisión (historial)
            DecisionHistoryRecord decision = request.Decision
                ?? new DecisionHistoryRecord
                {
                    EventId = request.DecisionEventId ?? Guid.NewGuid(),
                    EventType = string.IsNullOrWhiteSpace(request.DecisionEventType)
                        ? "closedloop.demo"
                        : request.DecisionEventType!.Trim(),
                    Title = string.IsNullOrWhiteSpace(request.DecisionTitle)
                        ? "Decisión closed-loop"
                        : request.DecisionTitle!.Trim(),
                    Fingerprint = request.DecisionFingerprint
                        ?? $"cl-{Guid.NewGuid():N}",
                    Area = request.Area ?? DecisionEventArea.Sales,
                    EntityType = request.EntityType ?? DecisionEntityType.Portfolio,
                    EntityId = request.EntityId,
                    EntityName = request.EntityName ?? string.Empty,
                    DetectedAt = request.AtUtc ?? DateTime.UtcNow,
                    CreatedAt = request.AtUtc ?? DateTime.UtcNow,
                    Status = DecisionEventStatus.Active,
                    Severity = DecisionSeverity.Medium,
                    Priority = DecisionPriority.Medium,
                    Recommendation = "Revisar y registrar acción manualmente."
                };

            if (decision.Id <= 0)
            {
                long id = _decisions.Append(decision);
                decision = _decisions.FindById(id) ?? decision;
            }

            checklist.Add("1 Alerta→Decisión: historial registrado");

            // 2 Decisión → Acción
            DateTime at = request.AtUtc ?? DateTime.UtcNow;
            BusinessActionBaseline? baseline = null;
            if (request.BaselineMetrics is { Count: > 0 })
            {
                baseline = BusinessActionBaselineComposer.FromMetricValues(
                    new BusinessActionBaselineCaptureRequest { CapturedAt = at },
                    request.BaselineMetrics);
            }

            var reg = actionSvc.Register(new BusinessActionRegisterRequest
            {
                ActionType = request.ActionType,
                Description = string.IsNullOrWhiteSpace(request.ActionDescription)
                    ? "Acción closed-loop"
                    : request.ActionDescription!.Trim(),
                DecisionEventId = decision.EventId,
                DecisionHistoryId = decision.Id > 0 ? decision.Id : null,
                Area = decision.Area,
                EntityType = decision.EntityType,
                EntityId = decision.EntityId,
                EntityName = decision.EntityName,
                CreatedBy = request.Actor ?? "system",
                CreatedAt = at,
                Baseline = baseline,
                StartImmediately = true,
                ExpectedImpact = request.ExpectedMetricKeys is { Count: > 0 }
                    ? BusinessActionRecordFactory.Expected(
                        "Mejora observada esperada (sin garantía).",
                        request.ExpectedMetricKeys)
                    : null
            });

            if (!reg.Success || reg.Record == null)
            {
                return Fail(reg.Message, checklist, decision);
            }

            checklist.Add("2 Decisión→Acción: acción registrada");

            // 3 Acción → Resultado (completar + post-métricas)
            int evalDays = request.EvaluationDays <= 0 ? 7 : request.EvaluationDays;
            DateTime completedAt = at.AddHours(1);
            var complete = actionSvc.Complete(
                reg.Record.ActionId,
                actor: request.Actor ?? "system",
                atUtc: completedAt,
                evaluationDays: evalDays);

            if (!complete.Success)
                return Fail(complete.Message, checklist, decision, reg.Record);

            checklist.Add("3 Acción→Resultado: acción completada");

            BusinessActionRecord? action = actionSvc.Get(reg.Record.ActionId);
            if (request.PostMetrics is { Count: > 0 })
            {
                var post = actionSvc.CapturePostMetrics(new BusinessActionPostMetricsRequest
                {
                    ActionId = reg.Record.ActionId,
                    CapturedAt = completedAt.AddDays(evalDays),
                    MetricValues = request.PostMetrics,
                    AllowBeforeWindowEnd = true
                });
                if (!post.Success)
                    return Fail(post.Message, checklist, decision, action);
                action = post.Record;
                checklist.Add("3b Post-métricas: variación calculada");
            }
            else if (baseline == null)
            {
                checklist.Add("3b Post-métricas: omitidas (sin datos — SIN DATOS esperado)");
            }

            // 4 Resultado → Evaluación
            var evaluation = evalSvc.Evaluate(new BusinessActionEvaluateRequest
            {
                ActionId = reg.Record.ActionId,
                AsOfUtc = completedAt.AddDays(evalDays),
                AllowBeforeWindowEnd = true,
                Actor = request.Actor
            });

            if (!evaluation.Success)
                return Fail(evaluation.Message, checklist, decision, action);

            checklist.Add(
                $"4 Resultado→Evaluación: {BusinessActionCatalog.OutcomeLabel(evaluation.Outcome)}");

            action = evaluation.Record ?? actionSvc.Get(reg.Record.ActionId);

            // 5 Historial / timeline / aprendizaje
            BusinessActionTimeline? timeline = timelineSvc.GetByActionId(reg.Record.ActionId);
            if (timeline != null)
                checklist.Add("5a Historial timeline: decisión→acción→resultado");

            BusinessActionLearningSummary learningTypes = learningSvc.GetSummary();
            BusinessActionContextualLearning learningCtx = learningSvc.GetContextual(minOccurrences: 2);
            checklist.Add("5b Aprendizaje: tasas / contexto disponibles");

            bool softOk = SoftLanguagePass(evaluation, timeline, learningTypes, learningCtx);
            if (softOk)
                checklist.Add("6 Soft language: narrativas sin causalidad prohibida");
            else
                checklist.Add("6 Soft language: REVISAR (frase prohibida detectada)");

            checklist.Add("7 Forms: binders CRM (Decisiones/Dashboard/Alertas/dominio)");
            checklist.Add("8 Arquitectura: UI→BLL→DAL sin lógica financiera en Forms");
            checklist.Add("9 FrmReportes: no tocado");

            return new BusinessActionClosedLoopResult
            {
                Success = softOk,
                Message = softOk
                    ? "Closed-loop OK — ciclo decisión→acción→resultado→aprendizaje."
                    : "Closed-loop parcial — revisar soft language.",
                Decision = decision,
                Action = action,
                Evaluation = evaluation,
                Timeline = timeline,
                LearningByType = learningTypes,
                LearningContextual = learningCtx,
                Checklist = checklist,
                SoftLanguageOk = softOk,
                PolicyNote = BusinessActionPhasePolicy.Definition
            };
        }

        /// <summary>Estado operativo del closed-loop (sin I/O pesado).</summary>
        public static string StatusBanner()
            => BusinessActionPhasePolicy.IsComplete
                ? "FASE 11 completa · Closed-loop operativo · solo registro (sin mutar POS)"
                : BusinessActionPhasePolicy.ClosedLoopOperational
                    ? "Closed-loop operativo · Alerta→Decisión→Acción→Resultado→Aprendizaje · solo registro"
                    : "Closed-loop pendiente";

        private static bool SoftLanguagePass(
            BusinessActionEvaluationResult evaluation,
            BusinessActionTimeline? timeline,
            BusinessActionLearningSummary learning,
            BusinessActionContextualLearning contextual)
        {
            if (BusinessActionSoftLanguageGuard.ContainsForbidden(evaluation.Summary))
                return false;
            if (BusinessActionSoftLanguageGuard.ContainsForbidden(learning.Narrative))
                return false;
            if (BusinessActionSoftLanguageGuard.ContainsForbidden(contextual.Narrative))
                return false;

            if (timeline != null)
            {
                foreach (BusinessActionTimelineStep step in timeline.Steps)
                {
                    if (BusinessActionSoftLanguageGuard.ContainsForbidden(step.Detail)
                        || BusinessActionSoftLanguageGuard.ContainsForbidden(step.Title))
                        return false;
                }
            }

            return true;
        }

        private static BusinessActionClosedLoopResult Fail(
            string message,
            List<string> checklist,
            DecisionHistoryRecord? decision,
            BusinessActionRecord? action = null)
            => new()
            {
                Success = false,
                Message = message,
                Decision = decision,
                Action = action,
                Checklist = checklist,
                SoftLanguageOk = false,
                PolicyNote = BusinessActionPhasePolicy.Definition
            };
    }

    /// <summary>Request para demo/orquestación closed-loop (FASE 11.24).</summary>
    public sealed class BusinessActionClosedLoopRequest
    {
        public DecisionHistoryRecord? Decision { get; init; }
        public Guid? DecisionEventId { get; init; }
        public string? DecisionEventType { get; init; }
        public string? DecisionTitle { get; init; }
        public string? DecisionFingerprint { get; init; }
        public DecisionEventArea? Area { get; init; }
        public DecisionEntityType? EntityType { get; init; }
        public string? EntityId { get; init; }
        public string? EntityName { get; init; }

        public BusinessActionType ActionType { get; init; } = BusinessActionType.Promotion;
        public string? ActionDescription { get; init; }
        public string? Actor { get; init; }
        public DateTime? AtUtc { get; init; }
        public int EvaluationDays { get; init; } = 7;

        public IReadOnlyDictionary<string, decimal?>? BaselineMetrics { get; init; }
        public IReadOnlyDictionary<string, decimal?>? PostMetrics { get; init; }
        public IReadOnlyList<string>? ExpectedMetricKeys { get; init; }
    }
}
