using System.Data;
using BLL.Models.Crm;
using DL;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ActionService (FASE 11.5–11.7).</summary>
    public static class BusinessActionServicePolicy
    {
        public const string Definition =
            "FASE 11.5: registro + estados. FASE 11.6: baseline. " +
            "FASE 11.7: ventana. FASE 11.8: CapturePostMetrics. " +
            "Pending→InProgress→Completed|Cancelled|NoResult. Sin mutar POS. " +
            "Cancelada no es Exitosa (TEST 8). Evaluación Outcome = BusinessActionEvaluationService (11.9).";

        public const string Transitions =
            "Pending→InProgress|Cancelled|NoResult|Completed · " +
            "InProgress→Completed|Cancelled|NoResult · " +
            "Completed|Cancelled|NoResult son terminales.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    public interface IBusinessActionStore
    {
        long Append(BusinessActionRecord record);
        BusinessActionRecord? FindByActionId(Guid actionId);
        BusinessActionRecord? Replace(BusinessActionRecord record);
        IReadOnlyList<BusinessActionRecord> Query(BusinessActionQuery query);
    }

    public sealed class InMemoryBusinessActionStore : IBusinessActionStore
    {
        private readonly object _gate = new();
        private readonly List<(long Id, BusinessActionRecord Record)> _rows = new();
        private long _nextId = 1;

        public long Append(BusinessActionRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            lock (_gate)
            {
                long id = _nextId++;
                _rows.Add((id, record));
                return id;
            }
        }

        public BusinessActionRecord? FindByActionId(Guid actionId)
        {
            lock (_gate)
                return _rows.LastOrDefault(r => r.Record.ActionId == actionId).Record;
        }

        public BusinessActionRecord? Replace(BusinessActionRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            lock (_gate)
            {
                int idx = _rows.FindLastIndex(r => r.Record.ActionId == record.ActionId);
                if (idx < 0)
                    return null;
                long id = _rows[idx].Id;
                _rows[idx] = (id, record);
                return record;
            }
        }

        public IReadOnlyList<BusinessActionRecord> Query(BusinessActionQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            int top = query.Top <= 0 ? 100 : Math.Min(query.Top, 500);
            lock (_gate)
            {
                IEnumerable<BusinessActionRecord> q = _rows.Select(r => r.Record);
                if (query.Status.HasValue)
                    q = q.Where(r => r.Status == query.Status);
                if (query.ActionType.HasValue)
                    q = q.Where(r => r.ActionType == query.ActionType);
                if (query.DecisionEventId.HasValue)
                    q = q.Where(r => r.DecisionEventId == query.DecisionEventId);
                if (query.EntityType.HasValue)
                    q = q.Where(r => r.EntityType == query.EntityType);
                if (!string.IsNullOrWhiteSpace(query.EntityId))
                    q = q.Where(r => string.Equals(r.EntityId, query.EntityId, StringComparison.Ordinal));
                if (query.FromUtc.HasValue)
                    q = q.Where(r => r.CreatedAt >= query.FromUtc);
                if (query.ToUtc.HasValue)
                    q = q.Where(r => r.CreatedAt <= query.ToUtc);

                return q.OrderByDescending(r => r.CreatedAt).Take(top).ToList();
            }
        }
    }

    public sealed class SqlBusinessActionStore : IBusinessActionStore
    {
        private readonly CrmBusinessActionDAL _dal = new();

        public long Append(BusinessActionRecord record)
            => _dal.Insert(BusinessActionPersistenceMapper.ToRow(record));

        public BusinessActionRecord? FindByActionId(Guid actionId)
        {
            DataRow? row = _dal.GetByActionId(actionId);
            return row == null ? null : BusinessActionPersistenceMapper.FromDataRow(row);
        }

        public BusinessActionRecord? Replace(BusinessActionRecord record)
        {
            int n = _dal.Update(BusinessActionPersistenceMapper.ToRow(record));
            return n > 0 ? record : null;
        }

        public IReadOnlyList<BusinessActionRecord> Query(BusinessActionQuery query)
        {
            DataTable table = _dal.Query(
                status: query.Status.HasValue ? (byte)query.Status.Value : null,
                actionType: query.ActionType.HasValue ? (int)query.ActionType.Value : null,
                decisionEventId: query.DecisionEventId,
                entityType: query.EntityType.HasValue ? (byte)query.EntityType.Value : null,
                entityId: query.EntityId,
                fromUtc: query.FromUtc,
                toUtc: query.ToUtc,
                top: query.Top);

            var list = new List<BusinessActionRecord>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
                list.Add(BusinessActionPersistenceMapper.FromDataRow(row));
            return list;
        }
    }

    /// <summary>Servicio de acciones de negocio (FASE 11.5+ / audit 11.12).</summary>
    public sealed class BusinessActionService
    {
        private readonly IBusinessActionStore _store;
        private readonly IBusinessActionAuditStore? _audit;

        public BusinessActionService(
            IBusinessActionStore? store = null,
            IBusinessActionAuditStore? audit = null)
        {
            _store = store ?? new SqlBusinessActionStore();
            _audit = audit;
        }

        public IBusinessActionStore Store => _store;

        /// <summary>TEST 1–2: registrar acción (usuario/fecha/tipo).</summary>
        public BusinessActionServiceResult Register(BusinessActionRegisterRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                BusinessActionStatus initial = request.StartImmediately
                    ? BusinessActionStatus.InProgress
                    : BusinessActionStatus.Pending;

                BusinessActionBaseline? baseline = request.Baseline;
                if (baseline == null && request.CaptureBaseline)
                {
                    baseline = BuildBaseline(
                        request.EntityType,
                        request.EntityId,
                        request.BaselinePeriodKind,
                        request.CreatedAt,
                        request.Analytics,
                        request.MetricValues,
                        metricKeys: null);
                }

                BusinessActionRecord record = BusinessActionRecordFactory.Create(
                    actionType: request.ActionType,
                    description: request.Description,
                    area: request.Area,
                    entityType: request.EntityType,
                    entityId: request.EntityId,
                    entityName: request.EntityName,
                    decisionEventId: request.DecisionEventId,
                    decisionHistoryId: request.DecisionHistoryId,
                    reason: request.Reason,
                    notes: request.Notes,
                    quantityInvolved: request.QuantityInvolved,
                    capitalInvolved: request.CapitalInvolved,
                    createdBy: request.CreatedBy,
                    expectedImpact: request.ExpectedImpact,
                    evaluationDays: request.EvaluationDays,
                    createdAt: request.CreatedAt,
                    status: initial,
                    baseline: baseline);

                long id = _store.Append(record);
                TryAudit(BusinessActionAuditService.FromRegister(
                    record,
                    actor: request.CreatedBy,
                    at: record.CreatedAt));
                return Ok($"Acción registrada ({BusinessActionCatalog.StatusLabel(record.Status)}).",
                    record, null, record.Status, id);
            }
            catch (ArgumentException ex)
            {
                return Fail(ex.Message);
            }
        }

        /// <summary>Adjunta baseline ya compuesto (FASE 11.6).</summary>
        public BusinessActionServiceResult AttachBaseline(Guid actionId, BusinessActionBaseline baseline)
            => CaptureBaseline(new BusinessActionBaselineRequest
            {
                ActionId = actionId,
                Baseline = baseline
            });

        /// <summary>Captura baseline desde Analytics / valores / objeto (FASE 11.6).</summary>
        public BusinessActionServiceResult CaptureBaseline(BusinessActionBaselineRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            BusinessActionRecord? current = _store.FindByActionId(request.ActionId);
            if (current == null)
                return Fail("No se encontró la acción.");

            if (current.Status is BusinessActionStatus.Completed
                or BusinessActionStatus.Cancelled
                or BusinessActionStatus.NoResult)
            {
                return Fail(
                    "No se puede capturar baseline en estado terminal.",
                    current,
                    current.Status);
            }

            BusinessActionBaseline baseline = request.Baseline
                ?? BuildBaseline(
                    current.EntityType,
                    current.EntityId,
                    request.PeriodKind,
                    request.CapturedAt,
                    request.Analytics,
                    request.MetricValues,
                    request.MetricKeys);

            BusinessActionRecord updated = BusinessActionRecordFactory.WithBaseline(current, baseline);
            if (_store.Replace(updated) == null)
                return Fail("No se pudo guardar el baseline.", current, current.Status);

            TryAudit(BusinessActionAuditService.FromBaseline(updated, at: request.CapturedAt));

            string msg = baseline.HasMetrics
                ? $"Baseline capturado ({baseline.Metrics.Count} métricas)."
                : "Baseline guardado sin métricas (datos insuficientes).";

            return Ok(msg, updated, current.Status, current.Status);
        }

        public BusinessActionServiceResult Start(Guid actionId, string? actor = null, string? notes = null)
            => ChangeStatus(new BusinessActionStatusRequest
            {
                ActionId = actionId,
                TargetStatus = BusinessActionStatus.InProgress,
                Actor = actor,
                Notes = notes
            });

        public BusinessActionServiceResult Complete(
            Guid actionId,
            string? actor = null,
            string? notes = null,
            BusinessActionActualImpact? actualImpact = null,
            int? evaluationDays = null,
            DateTime? atUtc = null)
            => ChangeStatus(new BusinessActionStatusRequest
            {
                ActionId = actionId,
                TargetStatus = BusinessActionStatus.Completed,
                Actor = actor,
                Notes = notes,
                ActualImpact = actualImpact,
                EvaluationDays = evaluationDays,
                AtUtc = atUtc
            });

        public BusinessActionServiceResult Cancel(Guid actionId, string? actor = null, string? notes = null)
            => ChangeStatus(new BusinessActionStatusRequest
            {
                ActionId = actionId,
                TargetStatus = BusinessActionStatus.Cancelled,
                Actor = actor,
                Notes = notes
            });

        public BusinessActionServiceResult MarkNoResult(
            Guid actionId,
            string? actor = null,
            string? notes = null)
            => ChangeStatus(new BusinessActionStatusRequest
            {
                ActionId = actionId,
                TargetStatus = BusinessActionStatus.NoResult,
                Actor = actor,
                Notes = notes
            });

        /// <summary>Ajusta EvaluationDays y DueAt (FASE 11.7). No clasifica Outcome.</summary>
        public BusinessActionServiceResult SetEvaluationWindow(BusinessActionEvaluationWindowRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.EvaluationDays <= 0)
                return Fail("EvaluationDays debe ser > 0.");

            BusinessActionRecord? current = _store.FindByActionId(request.ActionId);
            if (current == null)
                return Fail("No se encontró la acción.");

            if (current.Status is BusinessActionStatus.Cancelled or BusinessActionStatus.NoResult)
                return Fail("No se puede fijar ventana en Cancelada/Sin resultado.", current, current.Status);

            if (BusinessActionEvaluationWindowMath.HasEvaluatedOutcome(current))
                return Fail("La acción ya fue evaluada; no se cambia la ventana.", current, current.Status);

            DateTime anchor = current.Status == BusinessActionStatus.Completed
                ? (current.CompletedAt ?? request.AtUtc ?? DateTime.UtcNow)
                : current.CreatedAt;

            DateTime? due = BusinessActionEvaluationWindowMath.ComputeDueAt(anchor, request.EvaluationDays);
            BusinessActionRecord updated = CloneWithWindow(current, request.EvaluationDays, due);

            if (_store.Replace(updated) == null)
                return Fail("No se pudo actualizar la ventana.", current, current.Status);

            TryAudit(BusinessActionAuditService.FromSetWindow(updated, at: request.AtUtc));

            BusinessActionEvaluationWindow window = BusinessActionEvaluationWindowMath.Resolve(
                updated, request.AtUtc);
            return Ok(
                $"Ventana {request.EvaluationDays} d · vence {due:yyyy-MM-dd}.",
                updated,
                current.Status,
                current.Status,
                evaluationWindow: window);
        }

        public BusinessActionEvaluationWindow? GetEvaluationWindow(Guid actionId, DateTime? asOfUtc = null)
        {
            BusinessActionRecord? record = _store.FindByActionId(actionId);
            return record == null
                ? null
                : BusinessActionEvaluationWindowMath.Resolve(record, asOfUtc);
        }

        public IReadOnlyList<BusinessActionRecord> ListReadyForEvaluation(
            DateTime? asOfUtc = null,
            int top = 100)
            => List(new BusinessActionQuery
            {
                ReadyForEvaluationOnly = true,
                AsOfUtc = asOfUtc,
                Top = top
            });

        public IReadOnlyList<BusinessActionRecord> ListInEvaluationWindow(
            DateTime? asOfUtc = null,
            int top = 100)
            => List(new BusinessActionQuery
            {
                InEvaluationWindowOnly = true,
                AsOfUtc = asOfUtc,
                Top = top
            });

        /// <summary>
        /// Captura métricas post-acción vs Baseline y guarda deltas (FASE 11.8).
        /// No asigna Exitosa/Parcial (11.9). Lenguaje: se observó.
        /// </summary>
        public BusinessActionServiceResult CapturePostMetrics(BusinessActionPostMetricsRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            BusinessActionRecord? current = _store.FindByActionId(request.ActionId);
            if (current == null)
                return Fail("No se encontró la acción.");

            if (current.Status != BusinessActionStatus.Completed)
                return Fail("Solo se capturan post-métricas en acciones Completadas.", current, current.Status);

            if (current.Status is BusinessActionStatus.Cancelled)
                return Fail("Cancelada: no evaluar como resultado.", current, current.Status);

            if (BusinessActionEvaluationWindowMath.HasEvaluatedOutcome(current)
                && current.ActualImpact!.Outcome is BusinessActionOutcome.Successful
                    or BusinessActionOutcome.Partial
                    or BusinessActionOutcome.Ineffective)
            {
                return Fail(
                    "La acción ya tiene Outcome clasificado; no recalcular deltas aquí (usar 11.9).",
                    current,
                    current.Status);
            }

            DateTime asOf = request.CapturedAt ?? DateTime.UtcNow;
            BusinessActionEvaluationWindow window =
                BusinessActionEvaluationWindowMath.Resolve(current, asOf);

            if (window.Phase == BusinessActionEvaluationPhase.InWindow && !request.AllowBeforeWindowEnd)
            {
                return Fail(
                    $"Ventana aún abierta ({window.DaysRemaining} d restantes). Use AllowBeforeWindowEnd o espere.",
                    current,
                    current.Status);
            }

            if (current.Baseline == null || !current.Baseline.HasMetrics)
            {
                BusinessActionActualImpact insuf = BusinessActionRecordFactory.InsufficientData(
                    request.Notes ?? "Sin baseline para comparar.");
                BusinessActionRecord updatedInsuf =
                    BusinessActionRecordFactory.WithActualImpact(current, insuf);
                if (_store.Replace(updatedInsuf) == null)
                    return Fail("No se pudo guardar InsufficientData.", current, current.Status);
                return Ok(
                    "Sin baseline: ActualImpact = Sin datos.",
                    updatedInsuf,
                    current.Status,
                    current.Status,
                    evaluationWindow: window,
                    deltas: Array.Empty<BusinessActionMetricDelta>());
            }

            BusinessActionBaseline post = BuildBaseline(
                current.EntityType,
                current.EntityId,
                request.PeriodKind ?? current.Baseline.PeriodKind,
                asOf,
                request.Analytics,
                request.MetricValues,
                request.MetricKeys
                ?? current.ExpectedImpact?.TargetMetricKeys
                ?? current.Baseline.Metrics.Select(m => m.MetricKey).ToList());

            if (!post.HasMetrics)
            {
                BusinessActionActualImpact insuf = BusinessActionRecordFactory.InsufficientData(
                    request.Notes ?? "Sin métricas post-acción SSOT.");
                BusinessActionRecord updatedInsuf =
                    BusinessActionRecordFactory.WithActualImpact(current, insuf);
                if (_store.Replace(updatedInsuf) == null)
                    return Fail("No se pudo guardar InsufficientData.", current, current.Status);
                return Ok(
                    "Sin post-métricas: ActualImpact = Sin datos.",
                    updatedInsuf,
                    current.Status,
                    current.Status,
                    evaluationWindow: window,
                    deltas: Array.Empty<BusinessActionMetricDelta>());
            }

            IReadOnlyList<string>? preferred = request.MetricKeys
                ?? (current.ExpectedImpact?.TargetMetricKeys is { Count: > 0 } keys
                    ? keys
                    : null);

            IReadOnlyList<BusinessActionMetricDelta> deltas =
                BusinessActionMetricDeltaMath.Compare(current.Baseline, post, preferred);

            BusinessActionActualImpact actual = BusinessActionRecordFactory.ObservedDeltas(
                deltas, request.Notes);
            BusinessActionRecord updated = BusinessActionRecordFactory.WithActualImpact(current, actual);

            if (_store.Replace(updated) == null)
                return Fail("No se pudieron guardar los deltas.", current, current.Status);

            TryAudit(BusinessActionAuditService.FromPostMetrics(updated, at: asOf));

            return Ok(
                $"Post-métricas capturadas ({deltas.Count} deltas). Outcome pendiente (11.9).",
                updated,
                current.Status,
                current.Status,
                evaluationWindow: window,
                deltas: deltas);
        }

        public BusinessActionServiceResult ChangeStatus(BusinessActionStatusRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            BusinessActionRecord? current = _store.FindByActionId(request.ActionId);
            if (current == null)
                return Fail("No se encontró la acción.");

            if (!CanTransition(current.Status, request.TargetStatus, out string? err))
                return Fail(err ?? "Transición no permitida.", current, current.Status);

            if (request.ActualImpact != null
                && !BusinessActionCatalog.CanAssignOutcome(request.TargetStatus, request.ActualImpact.Outcome))
            {
                return Fail(
                    "Outcome no válido para el estado destino (p.ej. Cancelada ≠ Exitosa).",
                    current,
                    current.Status);
            }

            DateTime at = request.AtUtc ?? DateTime.UtcNow;
            string? actor = string.IsNullOrWhiteSpace(request.Actor) ? null : request.Actor.Trim();
            string? notes = MergeNotes(current.Notes, request.Notes);

            BusinessActionRecord updated = ApplyStatus(
                current,
                request.TargetStatus,
                at,
                actor,
                notes,
                request.ActualImpact,
                request.EvaluationDays);

            if (_store.Replace(updated) == null)
                return Fail("No se pudo actualizar la acción.", current, current.Status);

            BusinessActionAuditAction auditAction = request.TargetStatus switch
            {
                BusinessActionStatus.InProgress => BusinessActionAuditAction.Start,
                BusinessActionStatus.Completed => BusinessActionAuditAction.Complete,
                BusinessActionStatus.Cancelled => BusinessActionAuditAction.Cancel,
                BusinessActionStatus.NoResult => BusinessActionAuditAction.MarkNoResult,
                _ => BusinessActionAuditAction.Start
            };
            TryAudit(BusinessActionAuditService.FromStatusChange(
                updated, current.Status, auditAction, actor, request.Notes, at));

            BusinessActionEvaluationWindow? window = request.TargetStatus == BusinessActionStatus.Completed
                ? BusinessActionEvaluationWindowMath.Resolve(updated, at)
                : null;

            string msg =
                $"Estado {BusinessActionCatalog.StatusLabel(current.Status)} → {BusinessActionCatalog.StatusLabel(request.TargetStatus)}.";
            if (window != null && updated.EvaluationDueAt.HasValue)
                msg += $" Ventana hasta {updated.EvaluationDueAt:yyyy-MM-dd} ({window.Label}).";

            return Ok(msg, updated, current.Status, request.TargetStatus, evaluationWindow: window);
        }

        public BusinessActionRecord? Get(Guid actionId)
            => _store.FindByActionId(actionId);

        public IReadOnlyList<BusinessActionRecord> List(BusinessActionQuery? query = null)
        {
            query ??= new BusinessActionQuery();
            int top = query.Top <= 0 ? 100 : Math.Min(query.Top, 500);
            bool phaseFilter = query.ReadyForEvaluationOnly || query.InEvaluationWindowOnly;

            BusinessActionQuery storeQuery = phaseFilter
                ? new BusinessActionQuery
                {
                    Status = BusinessActionStatus.Completed,
                    ActionType = query.ActionType,
                    DecisionEventId = query.DecisionEventId,
                    EntityType = query.EntityType,
                    EntityId = query.EntityId,
                    FromUtc = query.FromUtc,
                    ToUtc = query.ToUtc,
                    Top = 500
                }
                : query;

            IReadOnlyList<BusinessActionRecord> list = _store.Query(storeQuery);
            DateTime asOf = query.AsOfUtc ?? DateTime.UtcNow;

            if (query.ReadyForEvaluationOnly)
            {
                list = list.Where(r =>
                        BusinessActionEvaluationWindowMath.Resolve(r, asOf).Phase
                        == BusinessActionEvaluationPhase.Ready)
                    .ToList();
            }
            else if (query.InEvaluationWindowOnly)
            {
                list = list.Where(r =>
                        BusinessActionEvaluationWindowMath.Resolve(r, asOf).Phase
                        == BusinessActionEvaluationPhase.InWindow)
                    .ToList();
            }

            return list.Take(top).ToList();
        }
        public static bool CanTransition(
            BusinessActionStatus from,
            BusinessActionStatus to,
            out string? error)
        {
            error = null;
            if (from == to)
            {
                error = "El estado ya está aplicado.";
                return false;
            }

            bool ok = from switch
            {
                BusinessActionStatus.Pending => to is BusinessActionStatus.InProgress
                    or BusinessActionStatus.Completed
                    or BusinessActionStatus.Cancelled
                    or BusinessActionStatus.NoResult,
                BusinessActionStatus.InProgress => to is BusinessActionStatus.Completed
                    or BusinessActionStatus.Cancelled
                    or BusinessActionStatus.NoResult,
                _ => false
            };

            if (!ok)
                error = $"No se permite {BusinessActionCatalog.StatusLabel(from)} → {BusinessActionCatalog.StatusLabel(to)}.";
            return ok;
        }

        private static BusinessActionRecord ApplyStatus(
            BusinessActionRecord current,
            BusinessActionStatus target,
            DateTime at,
            string? actor,
            string? notes,
            BusinessActionActualImpact? actualImpact,
            int? evaluationDaysOverride = null)
        {
            DateTime? started = current.StartedAt;
            if (started == null
                && target is BusinessActionStatus.InProgress or BusinessActionStatus.Completed)
                started = at;

            DateTime? completedAt = current.CompletedAt;
            string? completedBy = current.CompletedBy;
            if (target is BusinessActionStatus.Completed
                or BusinessActionStatus.Cancelled
                or BusinessActionStatus.NoResult)
            {
                completedAt = at;
                completedBy = actor ?? current.CompletedBy;
            }

            BusinessActionActualImpact? actual = actualImpact ?? current.ActualImpact;
            if (target == BusinessActionStatus.Cancelled && actual != null
                && actual.Outcome == BusinessActionOutcome.Successful)
            {
                actual = new BusinessActionActualImpact
                {
                    Outcome = BusinessActionOutcome.Unspecified,
                    Confidence = actual.Confidence,
                    Summary = actual.Summary,
                    Notes = actual.Notes,
                    Deltas = actual.Deltas
                };
            }

            int? evalDays = current.EvaluationDays;
            DateTime? evalDue = current.EvaluationDueAt;
            if (target == BusinessActionStatus.Completed)
            {
                int days = evaluationDaysOverride is > 0
                    ? evaluationDaysOverride.Value
                    : current.EvaluationDays is > 0
                        ? current.EvaluationDays.Value
                        : BusinessActionRecordFactory.DefaultEvaluationDays;
                evalDays = days;
                evalDue = BusinessActionEvaluationWindowMath.ComputeDueAt(at, days);
            }

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
                Notes = notes,
                QuantityInvolved = current.QuantityInvolved,
                CapitalInvolved = current.CapitalInvolved,
                CreatedAt = current.CreatedAt,
                CreatedBy = current.CreatedBy,
                Status = target,
                StartedAt = started,
                EvaluationDays = evalDays,
                EvaluationDueAt = evalDue,
                CompletedAt = completedAt,
                CompletedBy = completedBy,
                ExpectedImpact = current.ExpectedImpact,
                ActualImpact = actual,
                Baseline = current.Baseline
            };
        }

        private static BusinessActionRecord CloneWithWindow(
            BusinessActionRecord current,
            int evaluationDays,
            DateTime? evaluationDueAt)
            => new()
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
                EvaluationDays = evaluationDays,
                EvaluationDueAt = evaluationDueAt,
                CompletedAt = current.CompletedAt,
                CompletedBy = current.CompletedBy,
                ExpectedImpact = current.ExpectedImpact,
                ActualImpact = current.ActualImpact,
                Baseline = current.Baseline
            };

        private void TryAudit(BusinessActionAuditEntry entry)
        {
            if (_audit == null)
                return;
            try
            {
                _audit.Append(entry);
            }
            catch
            {
                // Auditoría no debe tumbar el flujo de negocio.
            }
        }

        private static BusinessActionBaseline BuildBaseline(
            DecisionEntityType entityType,
            string? entityId,
            ProfitPeriodKind? periodKind,
            DateTime? capturedAt,
            DecisionAnalyticsBundle? analytics,
            IReadOnlyDictionary<string, decimal?>? metricValues,
            IReadOnlyList<string>? metricKeys)
        {
            var capture = new BusinessActionBaselineCaptureRequest
            {
                EntityType = entityType,
                EntityId = entityId,
                PeriodKind = periodKind ?? analytics?.PeriodKind,
                CapturedAt = capturedAt,
                MetricKeys = metricKeys
            };

            if (metricValues != null)
                return BusinessActionBaselineComposer.FromMetricValues(capture, metricValues);

            return BusinessActionBaselineComposer.FromAnalytics(capture, analytics);
        }

        private static string? MergeNotes(string? existing, string? incoming)
        {
            if (string.IsNullOrWhiteSpace(incoming))
                return existing;
            if (string.IsNullOrWhiteSpace(existing))
                return incoming.Trim();
            return existing.Trim() + " | " + incoming.Trim();
        }

        private static BusinessActionServiceResult Ok(
            string message,
            BusinessActionRecord record,
            BusinessActionStatus? prev,
            BusinessActionStatus? next,
            long? id = null,
            BusinessActionEvaluationWindow? evaluationWindow = null,
            IReadOnlyList<BusinessActionMetricDelta>? deltas = null)
            => new()
            {
                Success = true,
                Message = message,
                Record = record,
                PreviousStatus = prev,
                NewStatus = next,
                PersistenceId = id,
                EvaluationWindow = evaluationWindow,
                Deltas = deltas
            };

        private static BusinessActionServiceResult Fail(
            string message,
            BusinessActionRecord? record = null,
            BusinessActionStatus? prev = null)
            => new()
            {
                Success = false,
                Message = message,
                Record = record,
                PreviousStatus = prev,
                NewStatus = record?.Status,
                EvaluationWindow = record == null
                    ? null
                    : BusinessActionEvaluationWindowMath.Resolve(record)
            };
    }
}
