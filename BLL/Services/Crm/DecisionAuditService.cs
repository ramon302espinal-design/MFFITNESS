using System.Globalization;
using BLL.Models.Crm;
using DL;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de auditoría (FASE 10.23 / brief §108).</summary>
    public static class DecisionAuditPolicy
    {
        public const string Definition =
            "FASE 10.23: auditoría append-only de detecciones y cambios de estado. " +
            "Qué ocurrió · quién · cuándo · transición. Sin auto-acciones.";

        public const string Rule =
            "No borra ni altera entradas previas. UI solo consulta.";

        public const string Deferred =
            "FASE 10 completa.";
    }

    public interface IDecisionAuditStore
    {
        long Append(DecisionAuditEntry entry);
        IReadOnlyList<DecisionAuditEntry> Query(DecisionAuditQuery query);
    }

    public sealed class InMemoryDecisionAuditStore : IDecisionAuditStore
    {
        private readonly object _gate = new();
        private readonly List<DecisionAuditEntry> _rows = new();
        private long _nextId = 1;

        public long Append(DecisionAuditEntry entry)
        {
            lock (_gate)
            {
                long id = _nextId++;
                _rows.Add(new DecisionAuditEntry
                {
                    Id = id,
                    HistoryId = entry.HistoryId,
                    EventId = entry.EventId,
                    Fingerprint = entry.Fingerprint,
                    EventType = entry.EventType,
                    Action = entry.Action,
                    PreviousStatus = entry.PreviousStatus,
                    NewStatus = entry.NewStatus,
                    Actor = entry.Actor,
                    Note = entry.Note,
                    Detail = entry.Detail,
                    AtUtc = entry.AtUtc
                });
                return id;
            }
        }

        public IReadOnlyList<DecisionAuditEntry> Query(DecisionAuditQuery query)
        {
            lock (_gate)
            {
                IEnumerable<DecisionAuditEntry> q = _rows;
                if (query.EventId.HasValue)
                    q = q.Where(e => e.EventId == query.EventId);
                if (query.HistoryId.HasValue)
                    q = q.Where(e => e.HistoryId == query.HistoryId);
                if (!string.IsNullOrWhiteSpace(query.Fingerprint))
                    q = q.Where(e => string.Equals(e.Fingerprint, query.Fingerprint, StringComparison.Ordinal));
                if (query.Action.HasValue)
                    q = q.Where(e => e.Action == query.Action);
                if (query.FromUtc.HasValue)
                    q = q.Where(e => e.AtUtc >= query.FromUtc);
                if (query.ToUtc.HasValue)
                    q = q.Where(e => e.AtUtc <= query.ToUtc);

                int top = query.Top <= 0 ? 100 : Math.Min(query.Top, 500);
                return q.OrderByDescending(e => e.AtUtc).ThenByDescending(e => e.Id).Take(top).ToList();
            }
        }
    }

    public sealed class SqlDecisionAuditStore : IDecisionAuditStore
    {
        private readonly CrmDecisionAuditDAL _dal = new();

        public long Append(DecisionAuditEntry entry)
            => _dal.Insert(new DecisionAuditRow
            {
                HistoryId = entry.HistoryId,
                EventId = entry.EventId,
                Fingerprint = entry.Fingerprint,
                EventType = entry.EventType,
                Action = (byte)entry.Action,
                PreviousStatus = entry.PreviousStatus.HasValue ? (byte)entry.PreviousStatus.Value : null,
                NewStatus = entry.NewStatus.HasValue ? (byte)entry.NewStatus.Value : null,
                Actor = entry.Actor,
                Note = entry.Note,
                Detail = entry.Detail,
                AtUtc = entry.AtUtc
            });

        public IReadOnlyList<DecisionAuditEntry> Query(DecisionAuditQuery query)
        {
            var table = _dal.Query(
                query.EventId,
                query.HistoryId,
                query.Fingerprint,
                query.Action.HasValue ? (byte)query.Action.Value : null,
                query.FromUtc,
                query.ToUtc,
                query.Top);

            var list = new List<DecisionAuditEntry>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                list.Add(FromRow(row));
            return list;
        }

        private static DecisionAuditEntry FromRow(System.Data.DataRow row) => new()
        {
            Id = Convert.ToInt64(row["Id"], CultureInfo.InvariantCulture),
            HistoryId = row["HistoryId"] == DBNull.Value
                ? null
                : Convert.ToInt64(row["HistoryId"], CultureInfo.InvariantCulture),
            EventId = row["EventId"] == DBNull.Value ? null : (Guid)row["EventId"],
            Fingerprint = row["Fingerprint"] == DBNull.Value ? null : Convert.ToString(row["Fingerprint"]),
            EventType = row["EventType"] == DBNull.Value ? null : Convert.ToString(row["EventType"]),
            Action = (DecisionAuditAction)Convert.ToByte(row["Action"], CultureInfo.InvariantCulture),
            PreviousStatus = row["PreviousStatus"] == DBNull.Value
                ? null
                : (DecisionEventStatus)Convert.ToByte(row["PreviousStatus"], CultureInfo.InvariantCulture),
            NewStatus = row["NewStatus"] == DBNull.Value
                ? null
                : (DecisionEventStatus)Convert.ToByte(row["NewStatus"], CultureInfo.InvariantCulture),
            Actor = row["Actor"] == DBNull.Value ? null : Convert.ToString(row["Actor"]),
            Note = row["Note"] == DBNull.Value ? null : Convert.ToString(row["Note"]),
            Detail = row["Detail"] == DBNull.Value ? null : Convert.ToString(row["Detail"]),
            AtUtc = Convert.ToDateTime(row["AtUtc"], CultureInfo.InvariantCulture)
        };
    }

    /// <summary>Servicio de auditoría (FASE 10.23).</summary>
    public sealed class DecisionAuditService
    {
        private readonly IDecisionAuditStore _store;

        public DecisionAuditService(IDecisionAuditStore? store = null)
        {
            _store = store ?? new SqlDecisionAuditStore();
        }

        public IDecisionAuditStore Store => _store;

        public long Record(DecisionAuditEntry entry)
            => _store.Append(entry);

        public IReadOnlyList<DecisionAuditEntry> GetAudit(DecisionAuditQuery? query = null)
            => _store.Query(query ?? new DecisionAuditQuery());

        public IReadOnlyList<DecisionAuditEntry> ForEvent(Guid eventId, int top = 50)
            => GetAudit(new DecisionAuditQuery { EventId = eventId, Top = top });

        public static DecisionAuditEntry FromDetection(DecisionHistoryRecord record, DateTime? at = null)
            => new()
            {
                HistoryId = record.Id,
                EventId = record.EventId,
                Fingerprint = record.Fingerprint,
                EventType = record.EventType,
                Action = DecisionAuditAction.Detected,
                PreviousStatus = null,
                NewStatus = record.Status,
                Actor = record.Source,
                Note = null,
                Detail = record.Title,
                AtUtc = at ?? DateTime.UtcNow
            };

        public static DecisionAuditEntry FromResolution(
            DecisionResolutionResult result,
            string? actor = null,
            DateTime? at = null)
        {
            DecisionAuditAction action = result.NewStatus switch
            {
                DecisionEventStatus.InReview => DecisionAuditAction.StartReview,
                DecisionEventStatus.Resolved => DecisionAuditAction.Resolve,
                DecisionEventStatus.Ignored => DecisionAuditAction.Ignore,
                DecisionEventStatus.Active => DecisionAuditAction.Reopen,
                _ => DecisionAuditAction.Resolve
            };

            DecisionHistoryRecord? r = result.Record;
            return new DecisionAuditEntry
            {
                HistoryId = r?.Id,
                EventId = r?.EventId,
                Fingerprint = r?.Fingerprint,
                EventType = r?.EventType,
                Action = action,
                PreviousStatus = result.PreviousStatus,
                NewStatus = result.NewStatus,
                Actor = actor ?? r?.ResolvedBy,
                Note = r?.ResolutionNote,
                Detail = result.Message,
                AtUtc = at ?? DateTime.UtcNow
            };
        }

        public static DecisionAuditEntry FromDuplicateSuppressed(
            DecisionEvent e,
            DateTime? at = null)
            => new()
            {
                EventId = e.EventId,
                Fingerprint = e.Fingerprint,
                EventType = e.EventType,
                Action = DecisionAuditAction.DuplicateSuppressed,
                PreviousStatus = DecisionEventStatus.Active,
                NewStatus = DecisionEventStatus.Active,
                Actor = e.Source,
                Detail = "Fingerprint abierto — no insertar (TEST 8).",
                AtUtc = at ?? DateTime.UtcNow
            };
    }
}
