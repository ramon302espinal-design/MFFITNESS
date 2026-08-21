using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;
using CORE;

namespace UI.Helpers
{
    /// <summary>
    /// Binder UI de acciones de negocio (FASE 11.19 — API completa para Forms).
    /// Forms solo muestran/capturan — sin lógica financiera ni mutación POS.
    /// </summary>
    public static class CrmBusinessActionUiBinder
    {
        public const string Policy =
            "FASE 11.19: binder SSOT para acciones CRM. " +
            "Register/Start/Complete/Cancel · Resultado/Impacto · Dashboard · Alertas · Timeline/Learning. " +
            "PROHIBIDO en Forms: recalcular métricas, mutar stock/caja/precios, auto-ejecutar.";

        public const string NoPosMutation =
            "El CRM solo registra. Usted ejecuta cambios en el POS.";

        public const string Deferred = "FASE 11 completa.";

        public static string ClosedLoopStatusLine()
            => BusinessActionIntegrationService.StatusBanner();

        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static string StatusLabel(BusinessActionStatus status)
            => BusinessActionCatalog.StatusLabel(status);

        public static string TypeLabel(BusinessActionType type)
            => BusinessActionCatalog.DisplayName(type);

        public static string FormatActionLine(BusinessActionRecord? record, int maxLen = 120)
        {
            if (record == null)
                return string.Empty;

            string line =
                $"{record.CreatedAt:dd/MM} · {TypeLabel(record.ActionType)} · " +
                $"{StatusLabel(record.Status)} · {Truncate(record.Description, 48)}";
            return Truncate(line, maxLen);
        }

        /// <summary>Opciones de tipo para ComboBox (sin Unspecified).</summary>
        public static IReadOnlyList<BusinessActionTypeChoice> TypeChoices()
            => BusinessActionCatalog.All
                .Where(d => d.Type != BusinessActionType.Unspecified)
                .Select(d => new BusinessActionTypeChoice(d.Type, d.DisplayName))
                .ToList();

        public static IReadOnlyList<BusinessActionRecord>? TryListRecent(
            out string? error,
            int top = 20)
        {
            error = null;
            try
            {
                return CreateService().List(new BusinessActionQuery { Top = top });
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static (int Pending, int InProgress, int Completed) TryCountOpen(out string? error)
        {
            error = null;
            try
            {
                var svc = CreateService();
                int pending = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Pending,
                    Top = 500
                }).Count;
                int inProgress = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.InProgress,
                    Top = 500
                }).Count;
                int completed = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Completed,
                    Top = 500
                }).Count;
                return (pending, inProgress, completed);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return (0, 0, 0);
            }
        }

        /// <summary>Decisiones activas/en revisión para vincular (opcional).</summary>
        public static IReadOnlyList<DecisionLinkChoice> TryLoadDecisionLinks(
            out string? error,
            int top = 30)
        {
            error = null;
            var list = new List<DecisionLinkChoice>
            {
                new(null, null, "(Sin vincular a decisión)")
            };

            try
            {
                var history = new DecisionHistoryService();
                IReadOnlyList<DecisionHistoryRecord> open = history.GetHistory(new DecisionHistoryQuery
                {
                    Status = DecisionEventStatus.Active,
                    Top = top
                });

                IReadOnlyList<DecisionHistoryRecord> review = history.GetHistory(new DecisionHistoryQuery
                {
                    Status = DecisionEventStatus.InReview,
                    Top = top
                });

                foreach (DecisionHistoryRecord r in open.Concat(review)
                             .OrderByDescending(x => x.DetectedAt)
                             .Take(top))
                {
                    string title = string.IsNullOrWhiteSpace(r.Title) ? r.EventType : r.Title;
                    list.Add(new DecisionLinkChoice(
                        r.EventId,
                        r.Id,
                        Truncate($"{r.DetectedAt:dd/MM} · {title}", 90)));
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return list;
        }

        /// <summary>REGISTRAR ACCIÓN — solo registro; no muta POS.</summary>
        public static BusinessActionServiceResult? TryRegister(
            out string? error,
            BusinessActionType actionType,
            string description,
            Guid? decisionEventId = null,
            long? decisionHistoryId = null,
            DecisionEventArea area = DecisionEventArea.Operations,
            bool startImmediately = false,
            string? reason = null,
            string? notes = null)
        {
            error = null;
            try
            {
                if (actionType == BusinessActionType.Unspecified)
                {
                    error = "Seleccione un tipo de acción.";
                    return null;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    error = "Indique una descripción de la acción.";
                    return null;
                }

                string? actor = CurrentActor();

                var request = new BusinessActionRegisterRequest
                {
                    ActionType = actionType,
                    Description = description.Trim(),
                    Area = area,
                    DecisionEventId = decisionEventId,
                    DecisionHistoryId = decisionHistoryId,
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                    CreatedBy = actor,
                    StartImmediately = startImmediately,
                    CaptureBaseline = false
                };

                return CreateService().Register(request);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string OpenSummaryLine(int pending, int inProgress, int completed)
            => $"{Count(pending)} pendientes · {Count(inProgress)} en proceso · {Count(completed)} completadas · solo registro";

        /// <summary>Contadores para Dashboard (FASE 11.17 / brief §76).</summary>
        public static BusinessActionDashboardCounters? TryLoadDashboardCounters(out string? error)
        {
            error = null;
            try
            {
                var svc = CreateService();
                int pending = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Pending,
                    Top = 500
                }).Count;
                int inProgress = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.InProgress,
                    Top = 500
                }).Count;
                int completed = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Completed,
                    Top = 500
                }).Count;

                IReadOnlyList<BusinessActionRecord> classifiedPool = svc.List(new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Completed,
                    Top = 500
                });

                int successful = classifiedPool.Count(r =>
                    r.ActualImpact?.Outcome == BusinessActionOutcome.Successful);
                int partial = classifiedPool.Count(r =>
                    r.ActualImpact?.Outcome == BusinessActionOutcome.Partial);
                int ineffective = classifiedPool.Count(r =>
                    r.ActualImpact?.Outcome == BusinessActionOutcome.Ineffective);

                string impactHint;
                if (successful + partial + ineffective == 0)
                {
                    impactHint = "Sin Outcomes clasificados aún · impacto observado pendiente";
                }
                else
                {
                    impactHint =
                        $"{Count(successful)} exitosas · {Count(partial)} parciales · " +
                        $"{Count(ineffective)} no efectivas (histórico; ≠ garantía)";
                }

                return new BusinessActionDashboardCounters
                {
                    Pending = pending,
                    InProgress = inProgress,
                    Completed = completed,
                    Successful = successful,
                    Partial = partial,
                    Ineffective = ineffective,
                    ImpactHint = impactHint
                };
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string FormatDashboardTitle(BusinessActionDashboardCounters? c)
            => c == null
                ? "Acciones de negocio"
                : $"Acciones · {Count(c.Pending)} pend. · {Count(c.InProgress)} proceso · {Count(c.Successful)} exitosas";

        public static BusinessActionRecord? TryGet(Guid actionId, out string? error)
        {
            error = null;
            try
            {
                return CreateService().Get(actionId);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>MARCAR COMPLETADA — no muta POS; abre ventana de evaluación.</summary>
        public static BusinessActionServiceResult? TryComplete(
            out string? error,
            Guid actionId,
            string? notes = null)
        {
            error = null;
            try
            {
                return CreateService().Complete(actionId, CurrentActor(), notes);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static BusinessActionServiceResult? TryStart(
            out string? error,
            Guid actionId,
            string? notes = null)
        {
            error = null;
            try
            {
                return CreateService().Start(actionId, CurrentActor(), notes);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static BusinessActionServiceResult? TryCancel(
            out string? error,
            Guid actionId,
            string? notes = null)
        {
            error = null;
            try
            {
                return CreateService().Cancel(actionId, CurrentActor(), notes);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static BusinessActionServiceResult? TryMarkNoResult(
            out string? error,
            Guid actionId,
            string? notes = null)
        {
            error = null;
            try
            {
                return CreateService().MarkNoResult(actionId, CurrentActor(), notes);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<BusinessActionRecord>? TryList(
            out string? error,
            BusinessActionQuery? query = null)
        {
            error = null;
            try
            {
                return CreateService().List(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<BusinessActionRecord>? TryListByDecision(
            out string? error,
            Guid decisionEventId,
            int top = 20)
            => TryList(out error, new BusinessActionQuery
            {
                DecisionEventId = decisionEventId,
                Top = top
            });

        public static string FormatTimeline(Guid actionId)
        {
            try
            {
                BusinessActionTimeline? timeline =
                    new BusinessActionTimelineService().GetByActionId(actionId);
                if (timeline == null || timeline.Steps.Count == 0)
                    return "Sin timeline para esta acción.";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("— TIMELINE (decisión → acción → resultado) —");
                foreach (BusinessActionTimelineStep step in timeline.Steps)
                {
                    sb.AppendLine($"{step.AtUtc:dd/MM HH:mm} · {step.Kind} · {step.Title}");
                    if (!string.IsNullOrWhiteSpace(step.Detail))
                        sb.AppendLine($"    {step.Detail}");
                }

                sb.AppendLine(NoPosMutation);
                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"Timeline no disponible: {ex.Message}";
            }
        }

        public static BusinessActionLearningSummary? TryLoadLearningByType(
            out string? error,
            BusinessActionLearningQuery? query = null)
        {
            error = null;
            try
            {
                return new BusinessActionLearningService(CreateService().Store)
                    .GetSummary(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static BusinessActionContextualLearning? TryLoadContextualLearning(
            out string? error,
            BusinessActionLearningQuery? query = null)
        {
            error = null;
            try
            {
                return new BusinessActionLearningService(
                        CreateService().Store,
                        new DecisionHistoryService().Store)
                    .GetContextual(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string FormatLearningLine(BusinessActionLearningSummary? summary)
        {
            if (summary == null)
                return "Learning no disponible.";
            if (summary.ClassifiedActions == 0)
                return summary.Narrative;
            return Truncate($"{summary.Narrative} · {summary.Caution}", 220);
        }

        private static string? CurrentActor()
            => BusinessActionActorResolver.ResolveName(
                Sesion.Activa ? Sesion.Usuario : null);

        /// <summary>Evalúa Outcome si hay deltas (FASE 11.9). Soft — no garantía.</summary>
        public static BusinessActionEvaluationResult? TryEvaluate(
            out string? error,
            Guid actionId,
            bool allowBeforeWindowEnd = true)
        {
            error = null;
            try
            {
                return CreateEvaluationService().Evaluate(new BusinessActionEvaluateRequest
                {
                    ActionId = actionId,
                    Actor = CurrentActor(),
                    AllowBeforeWindowEnd = allowBeforeWindowEnd
                });
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>VER RESULTADO / IMPACTO — texto listo para UI (FASE 11.16).</summary>
        public static string FormatImpactReport(BusinessActionRecord? record)
        {
            if (record == null)
                return "Seleccione una acción de la lista.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Acción: {TypeLabel(record.ActionType)}");
            sb.AppendLine($"Estado: {StatusLabel(record.Status)}");
            sb.AppendLine($"Descripción: {record.Description}");
            if (!string.IsNullOrWhiteSpace(record.EntityName))
                sb.AppendLine($"Entidad: {record.EntityName}");

            BusinessActionEvaluationWindow window =
                BusinessActionEvaluationWindowMath.Resolve(record);
            sb.AppendLine($"Ventana: {window.Label}");

            BusinessActionActualImpact? actual = record.ActualImpact;
            if (actual == null
                || (actual.Outcome == BusinessActionOutcome.Unspecified && actual.Deltas.Count == 0))
            {
                sb.AppendLine();
                sb.AppendLine("Resultado: aún sin Outcome / deltas observados.");
                sb.AppendLine("Complete la acción y capture post-métricas cuando venza la ventana.");
                sb.AppendLine();
                sb.AppendLine("Recordatorio: el CRM no muta precios ni stock — solo observa.");
                return sb.ToString().TrimEnd();
            }

            sb.AppendLine();
            sb.AppendLine(
                $"Resultado: {BusinessActionCatalog.OutcomeGlyph(actual.Outcome)} " +
                $"{BusinessActionCatalog.OutcomeLabel(actual.Outcome)} · " +
                $"confianza {BusinessActionCatalog.ConfidenceLabel(actual.Confidence)}");

            if (!string.IsNullOrWhiteSpace(actual.Summary))
                sb.AppendLine($"Resumen: {actual.Summary}");

            if (actual.Deltas.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Deltas observados (Before→After):");
                foreach (BusinessActionMetricDelta d in actual.Deltas.Take(8))
                    sb.AppendLine("  · " + FormatDeltaLine(d));
            }

            BusinessActionObservedCapitalImpact capital =
                BusinessActionCapitalImpactComposer.FromRecord(record);
            if (capital.HasAnySignal || !string.IsNullOrWhiteSpace(capital.Narrative))
            {
                sb.AppendLine();
                sb.AppendLine("Impacto de capital / ventas (observado):");
                sb.AppendLine(capital.Narrative);
                if (!string.IsNullOrWhiteSpace(capital.Caution))
                    sb.AppendLine(capital.Caution);
            }

            sb.AppendLine();
            sb.AppendLine("Información histórica/observada — no afirma causalidad ni garantiza resultados futuros.");
            return sb.ToString().TrimEnd();
        }

        public static string FormatDeltaLine(BusinessActionMetricDelta d)
        {
            string label = string.IsNullOrWhiteSpace(d.Label) ? d.MetricKey : d.Label;
            string before = d.Before?.ToString("N2", Cultura) ?? "—";
            string after = d.After?.ToString("N2", Cultura) ?? "—";
            string change = d.Change.HasValue
                ? (d.IsPercentagePoints
                    ? $"{d.Change.Value:+0.##;-0.##;0} pp"
                    : $"{d.Change.Value:+0.##;-0.##;0} %")
                : "—";
            return $"{label}: {before} → {after} ({change})";
        }

        public static IReadOnlyList<BusinessActionListItem> ToListItems(
            IEnumerable<BusinessActionRecord>? records)
        {
            if (records == null)
                return Array.Empty<BusinessActionListItem>();

            return records
                .Select(r => new BusinessActionListItem(r, FormatActionLine(r)))
                .ToList();
        }

        private static BusinessActionService CreateService()
        {
            // Audit best-effort: si SQL no está, el store SQL fallará al append;
            // ActionService traga fallos de audit. Alternativa in-memory no persiste.
            try
            {
                return new BusinessActionService(
                    audit: new SqlBusinessActionAuditStore());
            }
            catch
            {
                return new BusinessActionService();
            }
        }

        private static BusinessActionEvaluationService CreateEvaluationService()
        {
            try
            {
                return new BusinessActionEvaluationService(
                    audit: new SqlBusinessActionAuditStore());
            }
            catch
            {
                return new BusinessActionEvaluationService();
            }
        }

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max)
                return text;
            return text[..(max - 1)] + "…";
        }
    }

    public sealed class BusinessActionTypeChoice
    {
        public BusinessActionTypeChoice(BusinessActionType type, string display)
        {
            Type = type;
            Display = display;
        }

        public BusinessActionType Type { get; }
        public string Display { get; }
        public override string ToString() => Display;
    }

    public sealed class DecisionLinkChoice
    {
        public DecisionLinkChoice(Guid? eventId, long? historyId, string display)
        {
            EventId = eventId;
            HistoryId = historyId;
            Display = display;
        }

        public Guid? EventId { get; }
        public long? HistoryId { get; }
        public string Display { get; }
        public override string ToString() => Display;
    }

    /// <summary>Ítem de lista con ActionId (FASE 11.16).</summary>
    public sealed class BusinessActionListItem
    {
        public BusinessActionListItem(BusinessActionRecord record, string display)
        {
            Record = record;
            Display = display;
        }

        public BusinessActionRecord Record { get; }
        public Guid ActionId => Record.ActionId;
        public string Display { get; }
        public override string ToString() => Display;
    }

    /// <summary>Contadores Dashboard (FASE 11.17).</summary>
    public sealed class BusinessActionDashboardCounters
    {
        public int Pending { get; init; }
        public int InProgress { get; init; }
        public int Completed { get; init; }
        public int Successful { get; init; }
        public int Partial { get; init; }
        public int Ineffective { get; init; }
        public string ImpactHint { get; init; } = string.Empty;
    }

    /// <summary>Prioridad del Centro enlazable a decisión/acción (FASE 11.18).</summary>
    public sealed class AlertDecisionLinkItem
    {
        public AlertDecisionLinkItem(DecisionCenterPriorityItem priority, string display)
        {
            Priority = priority;
            Display = display;
        }

        public DecisionCenterPriorityItem Priority { get; }
        public Guid? EventId => Priority.PrimaryEventId;
        public string Display { get; }
        public override string ToString() => Display;
    }

    /// <summary>Enlaces alerta → decisión → acción → resultado (FASE 11.18).</summary>
    public static class CrmAlertLinkUiBinder
    {
        public static IReadOnlyList<AlertDecisionLinkItem> ToLinkItems(
            DecisionCenterReport? center,
            int max = 20)
        {
            if (center == null || center.PrioritiesToday.Count == 0)
                return Array.Empty<AlertDecisionLinkItem>();

            return center.PrioritiesToday
                .Take(max)
                .Select(p => new AlertDecisionLinkItem(
                    p,
                    Truncate(
                        $"{CrmDecisionUiBinder.BucketLabel(p.Bucket)} · {p.Title}",
                        100)))
                .ToList();
        }

        public static string FormatDecisionView(AlertDecisionLinkItem? item)
        {
            if (item == null)
                return "Seleccione una alerta/prioridad de la lista.";

            DecisionCenterPriorityItem p = item.Priority;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("— DECISIÓN ASOCIADA —");
            sb.AppendLine($"Título: {p.Title}");
            sb.AppendLine($"Bucket: {CrmDecisionUiBinder.BucketLabel(p.Bucket)}");
            sb.AppendLine($"Por qué importa: {p.WhyItMatters}");
            sb.AppendLine($"Recomendación: {p.Recommendation} · sin auto-acción");

            if (item.EventId.HasValue)
            {
                sb.AppendLine($"EventId: {item.EventId:N}");
                try
                {
                    DecisionHistoryRecord? hist =
                        new DecisionHistoryService().Store.FindByEventId(item.EventId.Value);
                    if (hist != null)
                    {
                        sb.AppendLine($"Historial: {hist.EventType} · {hist.Status} · {hist.DetectedAt:dd/MM/yyyy}");
                        if (!string.IsNullOrWhiteSpace(hist.Description))
                            sb.AppendLine(hist.Description);
                    }
                    else
                        sb.AppendLine("Historial persistido: no encontrado (puede ser solo del Centro en memoria).");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Historial no disponible: {ex.Message}");
                }
            }
            else
                sb.AppendLine("Sin EventId primario — revise el grupo en Centro de decisiones.");

            return sb.ToString().TrimEnd();
        }

        public static string FormatActionsView(AlertDecisionLinkItem? item)
        {
            if (item == null)
                return "Seleccione una alerta/prioridad de la lista.";

            if (!item.EventId.HasValue)
                return FormatDecisionView(item) +
                       "\n\n— ACCIONES —\nSin EventId: no hay vínculo a acciones registradas.";

            IReadOnlyList<BusinessActionRecord>? actions =
                CrmBusinessActionUiBinder.TryListByDecision(out string? error, item.EventId.Value);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("— ACCIONES ASOCIADAS —");
            sb.AppendLine($"Decisión: {item.Priority.Title}");

            if (actions == null)
            {
                sb.AppendLine(error ?? "No se pudieron cargar acciones.");
                return sb.ToString().TrimEnd();
            }

            if (actions.Count == 0)
            {
                sb.AppendLine("Sin acciones registradas para esta decisión.");
                sb.AppendLine("Use REGISTRAR ACCIÓN en Centro de decisiones.");
                return sb.ToString().TrimEnd();
            }

            foreach (BusinessActionRecord a in actions)
                sb.AppendLine("· " + CrmBusinessActionUiBinder.FormatActionLine(a));

            return sb.ToString().TrimEnd();
        }

        public static string FormatResultView(AlertDecisionLinkItem? item)
        {
            if (item == null)
                return "Seleccione una alerta/prioridad de la lista.";

            if (!item.EventId.HasValue)
                return "Sin EventId — no hay resultado vinculado.";

            IReadOnlyList<BusinessActionRecord>? actions =
                CrmBusinessActionUiBinder.TryListByDecision(out string? error, item.EventId.Value);
            if (actions == null)
                return $"No se pudieron cargar resultados: {error}";

            var withResult = actions
                .Where(a => a.Status == BusinessActionStatus.Completed
                    || a.ActualImpact != null)
                .ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("— RESULTADO / IMPACTO —");
            sb.AppendLine($"Decisión: {item.Priority.Title}");

            if (withResult.Count == 0)
            {
                sb.AppendLine("Sin resultados observados aún para acciones de esta alerta.");
                sb.AppendLine("Complete la acción y capture post-métricas / evaluación.");
                return sb.ToString().TrimEnd();
            }

            foreach (BusinessActionRecord a in withResult.Take(5))
            {
                sb.AppendLine();
                sb.AppendLine(CrmBusinessActionUiBinder.FormatImpactReport(a));
                sb.AppendLine("---");
            }

            return sb.ToString().TrimEnd();
        }

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max)
                return text;
            return text[..(max - 1)] + "…";
        }
    }
}
