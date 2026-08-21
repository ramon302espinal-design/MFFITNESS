using System.Globalization;
using BLL.Models.Crm;
using DL;

namespace BLL.Services.Crm
{
    /// <summary>Contrato historial (FASE 10.21 / brief §108–§110).</summary>
    public static class DecisionHistoryPolicy
    {
        public const string Definition =
            "FASE 10.21: historial append-only de DecisionEvents. " +
            "Qué ocurrió · cuándo · qué regla · estado. Sin auto-acciones.";

        public const string Dedup =
            "Misma Fingerprint + Status abierto (Active/InReview) ⇒ no insertar (TEST 8). " +
            "Resolved/Ignored permiten nueva detección.";

        public const string Reconcile =
            "TEST 9: tras un run completo, fingerprints abiertos ausentes ⇒ Resolved " +
            "(condición ya no aplica). Actor system. Sin mutar stock/caja.";

        public const string Deferred =
            "FASE 10 completa.";

        public const int DefaultRecurrenceMinOccurrences = 3;
        public const int DefaultRecurrenceLookbackDays = 90;
    }

    /// <summary>Contrato resolución / ignorado (FASE 10.22 / brief §107).</summary>
    public static class DecisionResolutionPolicy
    {
        public const string Definition =
            "FASE 10.22: el usuario marca Active→InReview→Resolved|Ignored. " +
            "No muta stock/caja/inversiones. Solo cambia estado del historial.";

        public const string Transitions =
            "Active→InReview|Resolved|Ignored · InReview→Resolved|Ignored|Active(reopen). " +
            "Resolved/Ignored son terminales (no re-resolver).";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>Abstracción de almacenamiento (SQL o memoria para tests).</summary>
    public interface IDecisionHistoryStore
    {
        /// <summary>True si hay registro abierto (Active o InReview) con esa huella.</summary>
        bool HasActiveFingerprint(string fingerprint);
        long Append(DecisionHistoryRecord record);
        IReadOnlyList<DecisionHistoryRecord> Query(DecisionHistoryQuery query);
        IReadOnlyList<DecisionRecurrenceSignal> GetRecurrence(DateTime fromUtc, int minOccurrences);
        DecisionHistoryMetrics GetMetrics(DateTime? fromUtc, DateTime? toUtc);

        DecisionHistoryRecord? FindByEventId(Guid eventId);
        DecisionHistoryRecord? FindById(long id);
        DecisionHistoryRecord? FindOpenByFingerprint(string fingerprint);

        /// <summary>Batch por Id (FASE 11.21 — timeline sin N+1).</summary>
        IReadOnlyList<DecisionHistoryRecord> FindManyByIds(IReadOnlyCollection<long> ids);

        /// <summary>Batch por EventId — última fila por EventId (FASE 11.21).</summary>
        IReadOnlyList<DecisionHistoryRecord> FindManyByEventIds(IReadOnlyCollection<Guid> eventIds);

        /// <summary>Aplica Status (+ campos de resolución). null si Id no existe.</summary>
        DecisionHistoryRecord? ApplyStatus(
            long id,
            DecisionEventStatus newStatus,
            DateTime? resolvedAt,
            string? resolvedBy,
            string? resolutionNote);
    }

    /// <summary>Store en memoria (tests / sin DB).</summary>
    public sealed class InMemoryDecisionHistoryStore : IDecisionHistoryStore
    {
        private readonly object _gate = new();
        private readonly List<DecisionHistoryRecord> _rows = new();
        private long _nextId = 1;

        public bool HasActiveFingerprint(string fingerprint)
        {
            lock (_gate)
            {
                return _rows.Any(r =>
                    IsOpenStatus(r.Status)
                    && string.Equals(r.Fingerprint, fingerprint, StringComparison.Ordinal));
            }
        }

        public DecisionHistoryRecord? FindByEventId(Guid eventId)
        {
            lock (_gate)
                return _rows.LastOrDefault(r => r.EventId == eventId);
        }

        public DecisionHistoryRecord? FindById(long id)
        {
            lock (_gate)
                return _rows.FirstOrDefault(r => r.Id == id);
        }

        public IReadOnlyList<DecisionHistoryRecord> FindManyByIds(IReadOnlyCollection<long> ids)
        {
            if (ids == null || ids.Count == 0)
                return Array.Empty<DecisionHistoryRecord>();

            var set = ids.Where(id => id > 0).ToHashSet();
            if (set.Count == 0)
                return Array.Empty<DecisionHistoryRecord>();

            lock (_gate)
                return _rows.Where(r => set.Contains(r.Id)).ToList();
        }

        public IReadOnlyList<DecisionHistoryRecord> FindManyByEventIds(IReadOnlyCollection<Guid> eventIds)
        {
            if (eventIds == null || eventIds.Count == 0)
                return Array.Empty<DecisionHistoryRecord>();

            var set = eventIds.ToHashSet();
            lock (_gate)
            {
                return _rows
                    .Where(r => set.Contains(r.EventId))
                    .GroupBy(r => r.EventId)
                    .Select(g => g.OrderByDescending(x => x.Id).First())
                    .ToList();
            }
        }

        public DecisionHistoryRecord? FindOpenByFingerprint(string fingerprint)
        {
            lock (_gate)
            {
                return _rows
                    .Where(r => IsOpenStatus(r.Status)
                        && string.Equals(r.Fingerprint, fingerprint, StringComparison.Ordinal))
                    .OrderByDescending(r => r.DetectedAt)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();
            }
        }

        public DecisionHistoryRecord? ApplyStatus(
            long id,
            DecisionEventStatus newStatus,
            DateTime? resolvedAt,
            string? resolvedBy,
            string? resolutionNote)
        {
            lock (_gate)
            {
                int i = _rows.FindIndex(r => r.Id == id);
                if (i < 0) return null;
                DecisionHistoryRecord old = _rows[i];
                var updated = CloneWithStatus(old, newStatus, resolvedAt, resolvedBy, resolutionNote);
                _rows[i] = updated;
                return updated;
            }
        }

        private static bool IsOpenStatus(DecisionEventStatus s)
            => s is DecisionEventStatus.Active or DecisionEventStatus.InReview;

        private static DecisionHistoryRecord CloneWithStatus(
            DecisionHistoryRecord old,
            DecisionEventStatus status,
            DateTime? resolvedAt,
            string? resolvedBy,
            string? resolutionNote)
            => new()
            {
                Id = old.Id,
                EventId = old.EventId,
                Fingerprint = old.Fingerprint,
                EventType = old.EventType,
                Area = old.Area,
                EntityType = old.EntityType,
                EntityId = old.EntityId,
                EntityName = old.EntityName,
                PeriodKey = old.PeriodKey,
                Severity = old.Severity,
                Priority = old.Priority,
                Status = status,
                Title = old.Title,
                Description = old.Description,
                Reason = old.Reason,
                Impact = old.Impact,
                Recommendation = old.Recommendation,
                Source = old.Source,
                GroupKey = old.GroupKey,
                DetectedAt = old.DetectedAt,
                CreatedAt = old.CreatedAt,
                ResolvedAt = resolvedAt,
                ResolvedBy = resolvedBy,
                ResolutionNote = resolutionNote
            };

        public long Append(DecisionHistoryRecord record)
        {
            lock (_gate)
            {
                long id = _nextId++;
                _rows.Add(new DecisionHistoryRecord
                {
                    Id = id,
                    EventId = record.EventId,
                    Fingerprint = record.Fingerprint,
                    EventType = record.EventType,
                    Area = record.Area,
                    EntityType = record.EntityType,
                    EntityId = record.EntityId,
                    EntityName = record.EntityName,
                    PeriodKey = record.PeriodKey,
                    Severity = record.Severity,
                    Priority = record.Priority,
                    Status = record.Status,
                    Title = record.Title,
                    Description = record.Description,
                    Reason = record.Reason,
                    Impact = record.Impact,
                    Recommendation = record.Recommendation,
                    Source = record.Source,
                    GroupKey = record.GroupKey,
                    DetectedAt = record.DetectedAt,
                    CreatedAt = record.CreatedAt,
                    ResolvedAt = record.ResolvedAt,
                    ResolvedBy = record.ResolvedBy,
                    ResolutionNote = record.ResolutionNote
                });
                return id;
            }
        }

        public IReadOnlyList<DecisionHistoryRecord> Query(DecisionHistoryQuery query)
        {
            lock (_gate)
            {
                IEnumerable<DecisionHistoryRecord> q = _rows;
                if (query.FromUtc.HasValue)
                    q = q.Where(r => r.DetectedAt >= query.FromUtc.Value);
                if (query.ToUtc.HasValue)
                    q = q.Where(r => r.DetectedAt <= query.ToUtc.Value);
                if (query.Status.HasValue)
                    q = q.Where(r => r.Status == query.Status.Value);
                if (!string.IsNullOrWhiteSpace(query.EventType))
                    q = q.Where(r => string.Equals(r.EventType, query.EventType, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(query.Fingerprint))
                    q = q.Where(r => string.Equals(r.Fingerprint, query.Fingerprint, StringComparison.Ordinal));
                if (query.EntityType.HasValue)
                    q = q.Where(r => r.EntityType == query.EntityType.Value);
                if (!string.IsNullOrWhiteSpace(query.EntityId))
                    q = q.Where(r => string.Equals(r.EntityId, query.EntityId, StringComparison.OrdinalIgnoreCase));

                int top = query.Top <= 0 ? 100 : Math.Min(query.Top, 500);
                return q.OrderByDescending(r => r.DetectedAt).ThenByDescending(r => r.Id).Take(top).ToList();
            }
        }

        public IReadOnlyList<DecisionRecurrenceSignal> GetRecurrence(DateTime fromUtc, int minOccurrences)
        {
            lock (_gate)
            {
                return _rows
                    .Where(r => r.DetectedAt >= fromUtc)
                    .GroupBy(r => new { r.EventType, r.EntityId })
                    .Select(g => new DecisionRecurrenceSignal
                    {
                        EventType = g.Key.EventType,
                        EntityId = g.Key.EntityId,
                        OccurrenceCount = g.Count(),
                        FirstDetectedAt = g.Min(x => x.DetectedAt),
                        LastDetectedAt = g.Max(x => x.DetectedAt),
                        IsRecurrent = g.Count() >= minOccurrences,
                        Message = g.Count() >= minOccurrences
                            ? $"PROBLEMA RECURRENTE: {g.Key.EventType} · {g.Count()} veces."
                            : string.Empty
                    })
                    .Where(s => s.IsRecurrent)
                    .OrderByDescending(s => s.OccurrenceCount)
                    .ToList();
            }
        }

        public DecisionHistoryMetrics GetMetrics(DateTime? fromUtc, DateTime? toUtc)
        {
            lock (_gate)
            {
                IEnumerable<DecisionHistoryRecord> q = _rows;
                if (fromUtc.HasValue) q = q.Where(r => r.DetectedAt >= fromUtc.Value);
                if (toUtc.HasValue) q = q.Where(r => r.DetectedAt <= toUtc.Value);
                var list = q.ToList();

                var resolvedHours = list
                    .Where(r => r.Status == DecisionEventStatus.Resolved && r.ResolvedAt.HasValue)
                    .Select(r => (r.ResolvedAt!.Value - r.DetectedAt).TotalHours)
                    .ToList();

                return new DecisionHistoryMetrics
                {
                    GeneratedCount = list.Count,
                    CriticalCount = list.Count(r => r.Severity == DecisionSeverity.Critical),
                    ActiveCount = list.Count(r => r.Status == DecisionEventStatus.Active),
                    ResolvedCount = list.Count(r => r.Status == DecisionEventStatus.Resolved),
                    IgnoredCount = list.Count(r => r.Status == DecisionEventStatus.Ignored),
                    AvgResolutionHours = resolvedHours.Count == 0
                        ? null
                        : resolvedHours.Average()
                };
            }
        }
    }

    /// <summary>Store SQL Server vía CrmDecisionHistoryDAL.</summary>
    public sealed class SqlDecisionHistoryStore : IDecisionHistoryStore
    {
        private readonly CrmDecisionHistoryDAL _dal = new();

        public bool HasActiveFingerprint(string fingerprint)
            => _dal.HasActiveFingerprint(fingerprint);

        public long Append(DecisionHistoryRecord record)
            => _dal.Insert(ToRow(record));

        public DecisionHistoryRecord? FindByEventId(Guid eventId)
        {
            System.Data.DataRow? row = _dal.GetByEventId(eventId);
            return row == null ? null : FromDataRow(row);
        }

        public DecisionHistoryRecord? FindById(long id)
        {
            System.Data.DataRow? row = _dal.GetById(id);
            return row == null ? null : FromDataRow(row);
        }

        public IReadOnlyList<DecisionHistoryRecord> FindManyByIds(IReadOnlyCollection<long> ids)
        {
            if (ids == null || ids.Count == 0)
                return Array.Empty<DecisionHistoryRecord>();

            var table = _dal.GetByIds(ids is IReadOnlyList<long> list ? list : ids.ToList());
            var result = new List<DecisionHistoryRecord>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                result.Add(FromDataRow(row));
            return result;
        }

        public IReadOnlyList<DecisionHistoryRecord> FindManyByEventIds(IReadOnlyCollection<Guid> eventIds)
        {
            if (eventIds == null || eventIds.Count == 0)
                return Array.Empty<DecisionHistoryRecord>();

            var table = _dal.GetByEventIds(
                eventIds is IReadOnlyList<Guid> list ? list : eventIds.ToList());
            var result = new List<DecisionHistoryRecord>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                result.Add(FromDataRow(row));
            return result;
        }

        public DecisionHistoryRecord? FindOpenByFingerprint(string fingerprint)
        {
            System.Data.DataRow? row = _dal.GetOpenByFingerprint(fingerprint);
            return row == null ? null : FromDataRow(row);
        }

        public DecisionHistoryRecord? ApplyStatus(
            long id,
            DecisionEventStatus newStatus,
            DateTime? resolvedAt,
            string? resolvedBy,
            string? resolutionNote)
        {
            int n = _dal.UpdateStatus(
                id, (byte)newStatus, resolvedAt, resolvedBy, resolutionNote);
            return n <= 0 ? null : FindById(id);
        }

        public IReadOnlyList<DecisionHistoryRecord> Query(DecisionHistoryQuery query)
        {
            var table = _dal.Query(
                query.FromUtc,
                query.ToUtc,
                query.Status.HasValue ? (byte)query.Status.Value : null,
                query.EventType,
                query.Fingerprint,
                query.EntityType.HasValue ? (byte)query.EntityType.Value : null,
                query.EntityId,
                query.Top);

            var list = new List<DecisionHistoryRecord>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                list.Add(FromDataRow(row));
            return list;
        }

        public IReadOnlyList<DecisionRecurrenceSignal> GetRecurrence(DateTime fromUtc, int minOccurrences)
        {
            var table = _dal.Recurrence(fromUtc, minOccurrences);
            var list = new List<DecisionRecurrenceSignal>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
            {
                int count = Convert.ToInt32(row["OccurrenceCount"], CultureInfo.InvariantCulture);
                string eventType = Convert.ToString(row["EventType"]) ?? string.Empty;
                string? entityId = row["EntityId"] == DBNull.Value
                    ? null
                    : Convert.ToString(row["EntityId"]);
                list.Add(new DecisionRecurrenceSignal
                {
                    EventType = eventType,
                    EntityId = entityId,
                    OccurrenceCount = count,
                    FirstDetectedAt = Convert.ToDateTime(row["FirstDetectedAt"], CultureInfo.InvariantCulture),
                    LastDetectedAt = Convert.ToDateTime(row["LastDetectedAt"], CultureInfo.InvariantCulture),
                    IsRecurrent = true,
                    Message = $"PROBLEMA RECURRENTE: {eventType} · {count} veces."
                });
            }

            return list;
        }

        public DecisionHistoryMetrics GetMetrics(DateTime? fromUtc, DateTime? toUtc)
        {
            System.Data.DataRow? row = _dal.Metrics(fromUtc, toUtc);
            if (row == null)
                return new DecisionHistoryMetrics();

            return new DecisionHistoryMetrics
            {
                GeneratedCount = Convert.ToInt32(row["GeneratedCount"], CultureInfo.InvariantCulture),
                CriticalCount = row["CriticalCount"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["CriticalCount"], CultureInfo.InvariantCulture),
                ActiveCount = row["ActiveCount"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["ActiveCount"], CultureInfo.InvariantCulture),
                ResolvedCount = row["ResolvedCount"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["ResolvedCount"], CultureInfo.InvariantCulture),
                IgnoredCount = row["IgnoredCount"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["IgnoredCount"], CultureInfo.InvariantCulture),
                AvgResolutionHours = row["AvgResolutionHours"] == DBNull.Value
                    ? null
                    : Convert.ToDouble(row["AvgResolutionHours"], CultureInfo.InvariantCulture)
            };
        }

        private static DecisionHistoryRow ToRow(DecisionHistoryRecord r) => new()
        {
            EventId = r.EventId,
            Fingerprint = r.Fingerprint,
            EventType = r.EventType,
            Area = (byte)r.Area,
            EntityType = (byte)r.EntityType,
            EntityId = r.EntityId,
            EntityName = r.EntityName,
            PeriodKey = r.PeriodKey,
            Severity = (byte)r.Severity,
            Priority = (byte)r.Priority,
            Status = (byte)r.Status,
            Title = r.Title,
            Description = r.Description,
            Reason = r.Reason,
            Impact = r.Impact,
            Recommendation = r.Recommendation,
            Source = r.Source,
            GroupKey = r.GroupKey,
            DetectedAt = r.DetectedAt,
            CreatedAt = r.CreatedAt
        };

        private static DecisionHistoryRecord FromDataRow(System.Data.DataRow row) => new()
        {
            Id = Convert.ToInt64(row["Id"], CultureInfo.InvariantCulture),
            EventId = (Guid)row["EventId"],
            Fingerprint = Convert.ToString(row["Fingerprint"]) ?? string.Empty,
            EventType = Convert.ToString(row["EventType"]) ?? string.Empty,
            Area = (DecisionEventArea)Convert.ToByte(row["Area"], CultureInfo.InvariantCulture),
            EntityType = (DecisionEntityType)Convert.ToByte(row["EntityType"], CultureInfo.InvariantCulture),
            EntityId = row["EntityId"] == DBNull.Value ? null : Convert.ToString(row["EntityId"]),
            EntityName = Convert.ToString(row["EntityName"]) ?? string.Empty,
            PeriodKey = row["PeriodKey"] == DBNull.Value ? null : Convert.ToString(row["PeriodKey"]),
            Severity = (DecisionSeverity)Convert.ToByte(row["Severity"], CultureInfo.InvariantCulture),
            Priority = (DecisionPriority)Convert.ToByte(row["Priority"], CultureInfo.InvariantCulture),
            Status = (DecisionEventStatus)Convert.ToByte(row["Status"], CultureInfo.InvariantCulture),
            Title = Convert.ToString(row["Title"]) ?? string.Empty,
            Description = Convert.ToString(row["Description"]) ?? string.Empty,
            Reason = Convert.ToString(row["Reason"]) ?? string.Empty,
            Impact = Convert.ToString(row["Impact"]) ?? string.Empty,
            Recommendation = Convert.ToString(row["Recommendation"]) ?? string.Empty,
            Source = Convert.ToString(row["Source"]) ?? string.Empty,
            GroupKey = row["GroupKey"] == DBNull.Value ? null : Convert.ToString(row["GroupKey"]),
            DetectedAt = Convert.ToDateTime(row["DetectedAt"], CultureInfo.InvariantCulture),
            CreatedAt = Convert.ToDateTime(row["CreatedAt"], CultureInfo.InvariantCulture),
            ResolvedAt = row["ResolvedAt"] == DBNull.Value
                ? null
                : Convert.ToDateTime(row["ResolvedAt"], CultureInfo.InvariantCulture),
            ResolvedBy = row["ResolvedBy"] == DBNull.Value ? null : Convert.ToString(row["ResolvedBy"]),
            ResolutionNote = row["ResolutionNote"] == DBNull.Value
                ? null
                : Convert.ToString(row["ResolutionNote"])
        };
    }

    /// <summary>Servicio de historial (FASE 10.21).</summary>
    public sealed class DecisionHistoryService
    {
        private readonly IDecisionHistoryStore _store;
        private readonly IDecisionAuditStore? _audit;

        public DecisionHistoryService(
            IDecisionHistoryStore? store = null,
            IDecisionAuditStore? audit = null)
        {
            _store = store ?? new SqlDecisionHistoryStore();
            _audit = audit;
        }

        /// <summary>Store subyacente (compartir con DecisionResolutionService).</summary>
        public IDecisionHistoryStore Store => _store;

        /// <summary>
        /// Persiste eventos del motor. Respeta fingerprint Active (TEST 8).
        /// </summary>
        public DecisionHistoryCaptureResult Capture(
            DecisionEngineReport engine,
            IReadOnlyDictionary<Guid, string>? groupKeyByEventId = null)
        {
            ArgumentNullException.ThrowIfNull(engine);

            int considered = 0;
            int inserted = 0;
            int skipped = 0;
            var ids = new List<long>();

            // Map event → group key from report groups if not provided
            Dictionary<Guid, string> map = groupKeyByEventId != null
                ? new Dictionary<Guid, string>(groupKeyByEventId)
                : BuildGroupMap(engine);

            foreach (DecisionEvent e in engine.Events)
            {
                considered++;
                if (e.Status == DecisionEventStatus.Active
                    && _store.HasActiveFingerprint(e.Fingerprint))
                {
                    skipped++;
                    _audit?.Append(DecisionAuditService.FromDuplicateSuppressed(e));
                    continue;
                }

                map.TryGetValue(e.EventId, out string? groupKey);
                DecisionHistoryRecord record = FromEvent(e, groupKey);
                long id = _store.Append(record);
                ids.Add(id);
                inserted++;

                if (_audit != null)
                {
                    DecisionHistoryRecord withId = new()
                    {
                        Id = id,
                        EventId = record.EventId,
                        Fingerprint = record.Fingerprint,
                        EventType = record.EventType,
                        Area = record.Area,
                        EntityType = record.EntityType,
                        EntityId = record.EntityId,
                        EntityName = record.EntityName,
                        PeriodKey = record.PeriodKey,
                        Severity = record.Severity,
                        Priority = record.Priority,
                        Status = record.Status,
                        Title = record.Title,
                        Description = record.Description,
                        Reason = record.Reason,
                        Impact = record.Impact,
                        Recommendation = record.Recommendation,
                        Source = record.Source,
                        GroupKey = record.GroupKey,
                        DetectedAt = record.DetectedAt,
                        CreatedAt = record.CreatedAt
                    };
                    _audit.Append(DecisionAuditService.FromDetection(withId));
                }
            }

            return new DecisionHistoryCaptureResult
            {
                Considered = considered,
                Inserted = inserted,
                SkippedActiveDuplicate = skipped,
                InsertedIds = ids,
                PolicyNote = DecisionHistoryPolicy.Definition + " " + DecisionHistoryPolicy.Dedup
            };
        }

        public IReadOnlyList<DecisionHistoryRecord> GetHistory(DecisionHistoryQuery? query = null)
            => _store.Query(query ?? new DecisionHistoryQuery());

        public IReadOnlyList<DecisionRecurrenceSignal> GetRecurrentProblems(
            int lookbackDays = DecisionHistoryPolicy.DefaultRecurrenceLookbackDays,
            int minOccurrences = DecisionHistoryPolicy.DefaultRecurrenceMinOccurrences)
        {
            DateTime from = DateTime.UtcNow.AddDays(-Math.Abs(lookbackDays));
            return _store.GetRecurrence(from, Math.Max(2, minOccurrences));
        }

        public DecisionHistoryMetrics GetMetrics(DateTime? fromUtc = null, DateTime? toUtc = null)
            => _store.GetMetrics(fromUtc, toUtc);

        /// <summary>
        /// TEST 9 — si la condición deja de existir en el run actual, cierra el historial abierto.
        /// Llamar solo tras evaluación completa (todas las reglas), no tras un subconjunto.
        /// </summary>
        public DecisionHistoryReconcileResult ReconcileAbsent(
            DecisionEngineReport current,
            string actor = "system",
            string? note = null,
            DateTime? atUtc = null)
        {
            ArgumentNullException.ThrowIfNull(current);

            string resolvedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor.Trim();
            string resolutionNote = string.IsNullOrWhiteSpace(note)
                ? "Condición ya no aplica"
                : note.Trim();
            DateTime at = atUtc ?? DateTime.UtcNow;

            var present = new HashSet<string>(
                current.Events.Select(e => e.Fingerprint),
                StringComparer.Ordinal);

            var open = _store.Query(new DecisionHistoryQuery
                {
                    Status = DecisionEventStatus.Active,
                    Top = 5000
                })
                .Concat(_store.Query(new DecisionHistoryQuery
                {
                    Status = DecisionEventStatus.InReview,
                    Top = 5000
                }))
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();

            int still = 0;
            int resolved = 0;
            var ids = new List<long>();

            foreach (DecisionHistoryRecord row in open)
            {
                if (present.Contains(row.Fingerprint))
                {
                    still++;
                    continue;
                }

                DecisionHistoryRecord? updated = _store.ApplyStatus(
                    row.Id,
                    DecisionEventStatus.Resolved,
                    at,
                    resolvedBy,
                    resolutionNote);

                if (updated == null)
                    continue;

                resolved++;
                ids.Add(updated.Id);

                if (_audit != null)
                {
                    var result = new DecisionResolutionResult
                    {
                        Success = true,
                        Message = "ReconcileAbsent → Resolved.",
                        Record = updated,
                        PreviousStatus = row.Status,
                        NewStatus = DecisionEventStatus.Resolved
                    };
                    _audit.Append(DecisionAuditService.FromResolution(result, resolvedBy, at));
                }
            }

            return new DecisionHistoryReconcileResult
            {
                OpenConsidered = open.Count,
                ResolvedAbsent = resolved,
                StillPresent = still,
                ResolvedIds = ids,
                PolicyNote = DecisionHistoryPolicy.Reconcile
            };
        }

        public static DecisionHistoryRecord FromEvent(DecisionEvent e, string? groupKey = null)
            => new()
            {
                EventId = e.EventId,
                Fingerprint = e.Fingerprint,
                EventType = e.EventType,
                Area = e.Area,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                EntityName = e.EntityName,
                PeriodKey = e.PeriodKey,
                Severity = e.Severity,
                Priority = e.Priority,
                Status = e.Status,
                Title = e.Title,
                Description = e.Description,
                Reason = e.Reason,
                Impact = e.Impact,
                Recommendation = e.Recommendation,
                Source = e.Source,
                GroupKey = groupKey,
                DetectedAt = e.DetectedAt,
                CreatedAt = e.CreatedAt
            };

        private static Dictionary<Guid, string> BuildGroupMap(DecisionEngineReport engine)
        {
            var map = new Dictionary<Guid, string>();
            foreach (DecisionGroup g in engine.Groups)
            {
                foreach (DecisionEvent e in g.Events)
                    map[e.EventId] = g.GroupKey;
            }

            return map;
        }
    }

    /// <summary>Resolución / ignorado de eventos de historial (FASE 10.22).</summary>
    public sealed class DecisionResolutionService
    {
        private readonly IDecisionHistoryStore _store;
        private readonly IDecisionAuditStore? _audit;

        public DecisionResolutionService(
            IDecisionHistoryStore? store = null,
            IDecisionAuditStore? audit = null)
        {
            _store = store ?? new SqlDecisionHistoryStore();
            _audit = audit;
        }

        public DecisionResolutionService(DecisionHistoryService history)
            : this(history.Store, null)
        {
        }

        public DecisionResolutionService(DecisionHistoryService history, IDecisionAuditStore audit)
            : this(history.Store, audit)
        {
        }

        public DecisionResolutionResult Apply(DecisionResolutionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            DecisionHistoryRecord? current = Locate(request);
            if (current == null)
            {
                return Fail("No se encontró el evento de historial.");
            }

            if (!TryMap(request.Action, current.Status, out DecisionEventStatus target, out string? err))
            {
                return Fail(err ?? "Transición no permitida.", current, current.Status);
            }

            DateTime at = request.AtUtc ?? DateTime.UtcNow;
            bool closing = target is DecisionEventStatus.Resolved or DecisionEventStatus.Ignored;
            DateTime? resolvedAt = closing ? at : null;
            string? actor = string.IsNullOrWhiteSpace(request.Actor) ? null : request.Actor.Trim();
            string? note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            string? auditActor = actor;

            // Reopen limpia campos de resolución en el registro
            if (target == DecisionEventStatus.Active || target == DecisionEventStatus.InReview)
            {
                resolvedAt = null;
                if (target == DecisionEventStatus.Active)
                {
                    actor = null;
                    note = null;
                }
            }

            DecisionHistoryRecord? updated = _store.ApplyStatus(
                current.Id, target, resolvedAt, actor, note);

            if (updated == null)
                return Fail("No se pudo actualizar el estado.", current, current.Status);

            var result = new DecisionResolutionResult
            {
                Success = true,
                Message = $"Estado {Display(current.Status)} → {Display(target)}.",
                Record = updated,
                PreviousStatus = current.Status,
                NewStatus = target
            };

            _audit?.Append(DecisionAuditService.FromResolution(result, auditActor, at));
            return result;
        }

        public DecisionResolutionResult Resolve(Guid eventId, string? actor = null, string? note = null)
            => Apply(new DecisionResolutionRequest
            {
                EventId = eventId,
                Action = DecisionResolutionAction.Resolve,
                Actor = actor,
                Note = note
            });

        public DecisionResolutionResult Ignore(Guid eventId, string? actor = null, string? note = null)
            => Apply(new DecisionResolutionRequest
            {
                EventId = eventId,
                Action = DecisionResolutionAction.Ignore,
                Actor = actor,
                Note = note
            });

        public DecisionResolutionResult StartReview(Guid eventId, string? actor = null, string? note = null)
            => Apply(new DecisionResolutionRequest
            {
                EventId = eventId,
                Action = DecisionResolutionAction.StartReview,
                Actor = actor,
                Note = note
            });

        public DecisionResolutionResult Reopen(Guid eventId, string? actor = null, string? note = null)
            => Apply(new DecisionResolutionRequest
            {
                EventId = eventId,
                Action = DecisionResolutionAction.Reopen,
                Actor = actor,
                Note = note
            });

        private DecisionHistoryRecord? Locate(DecisionResolutionRequest request)
        {
            if (request.HistoryId.HasValue)
                return _store.FindById(request.HistoryId.Value);
            if (request.EventId.HasValue)
                return _store.FindByEventId(request.EventId.Value);
            if (!string.IsNullOrWhiteSpace(request.Fingerprint))
                return _store.FindOpenByFingerprint(request.Fingerprint.Trim());
            return null;
        }

        private static bool TryMap(
            DecisionResolutionAction action,
            DecisionEventStatus current,
            out DecisionEventStatus target,
            out string? error)
        {
            target = current;
            error = null;

            if (current is DecisionEventStatus.Resolved or DecisionEventStatus.Ignored)
            {
                error = "El evento ya está cerrado (Resolved/Ignored).";
                return false;
            }

            if (current == DecisionEventStatus.InsufficientData)
            {
                error = "InsufficientData no se resuelve por este flujo.";
                return false;
            }

            switch (action)
            {
                case DecisionResolutionAction.StartReview:
                    if (current != DecisionEventStatus.Active)
                    {
                        error = "StartReview solo desde Active.";
                        return false;
                    }
                    target = DecisionEventStatus.InReview;
                    return true;

                case DecisionResolutionAction.Resolve:
                    if (current is not (DecisionEventStatus.Active or DecisionEventStatus.InReview))
                    {
                        error = "Resolve solo desde Active o InReview.";
                        return false;
                    }
                    target = DecisionEventStatus.Resolved;
                    return true;

                case DecisionResolutionAction.Ignore:
                    if (current is not (DecisionEventStatus.Active or DecisionEventStatus.InReview))
                    {
                        error = "Ignore solo desde Active o InReview.";
                        return false;
                    }
                    target = DecisionEventStatus.Ignored;
                    return true;

                case DecisionResolutionAction.Reopen:
                    if (current != DecisionEventStatus.InReview)
                    {
                        error = "Reopen solo desde InReview.";
                        return false;
                    }
                    target = DecisionEventStatus.Active;
                    return true;

                default:
                    error = "Acción desconocida.";
                    return false;
            }
        }

        private static DecisionResolutionResult Fail(
            string message,
            DecisionHistoryRecord? record = null,
            DecisionEventStatus? previous = null)
            => new()
            {
                Success = false,
                Message = message,
                Record = record,
                PreviousStatus = previous,
                NewStatus = record?.Status
            };

        private static string Display(DecisionEventStatus s) => s switch
        {
            DecisionEventStatus.Active => "NUEVA",
            DecisionEventStatus.InReview => "EN REVISIÓN",
            DecisionEventStatus.Resolved => "RESUELTA",
            DecisionEventStatus.Ignored => "IGNORADA",
            DecisionEventStatus.InsufficientData => "DATOS INSUFICIENTES",
            _ => s.ToString()
        };
    }
}
