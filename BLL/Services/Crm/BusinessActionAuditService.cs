using System.Globalization;
using BLL.Models.Crm;
using CORE;
using DL;

namespace BLL.Services.Crm
{
    /// <summary>Contrato auditoría de acciones (FASE 11.12).</summary>
    public static class BusinessActionAuditPolicy
    {
        public const string Definition =
            "FASE 11.12: auditoría append-only de acciones de negocio. " +
            "Qué · quién (Sesion / actor explícito) · cuándo · transición/outcome. Sin mutar POS.";

        public const string ActorRule =
            "Actor = parámetro explícito → si vacío, CORE.Sesion.Usuario (si Activa). " +
            "ActorUserId = Sesion.UsuarioId cuando aplique.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Resuelve actor desde parámetro o Sesion (FASE 11.12).</summary>
    public static class BusinessActionActorResolver
    {
        public static string? ResolveName(string? explicitActor = null)
        {
            if (!string.IsNullOrWhiteSpace(explicitActor))
                return explicitActor.Trim();
            if (Sesion.Activa && !string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario.Trim();
            return null;
        }

        public static int? ResolveUserId(string? explicitActor = null)
        {
            // Si el caller pasó actor explícito distinto de Sesion, no inventar UserId.
            if (!string.IsNullOrWhiteSpace(explicitActor)
                && Sesion.Activa
                && !string.Equals(explicitActor.Trim(), Sesion.Usuario, StringComparison.OrdinalIgnoreCase))
                return null;

            return Sesion.Activa && Sesion.UsuarioId > 0 ? Sesion.UsuarioId : null;
        }
    }

    public interface IBusinessActionAuditStore
    {
        long Append(BusinessActionAuditEntry entry);
        IReadOnlyList<BusinessActionAuditEntry> Query(BusinessActionAuditQuery query);
    }

    public sealed class InMemoryBusinessActionAuditStore : IBusinessActionAuditStore
    {
        private readonly object _gate = new();
        private readonly List<BusinessActionAuditEntry> _rows = new();
        private long _nextId = 1;

        public long Append(BusinessActionAuditEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            lock (_gate)
            {
                long id = _nextId++;
                _rows.Add(new BusinessActionAuditEntry
                {
                    Id = id,
                    ActionId = entry.ActionId,
                    DecisionEventId = entry.DecisionEventId,
                    ActionType = entry.ActionType,
                    AuditAction = entry.AuditAction,
                    PreviousStatus = entry.PreviousStatus,
                    NewStatus = entry.NewStatus,
                    Outcome = entry.Outcome,
                    Actor = entry.Actor,
                    ActorUserId = entry.ActorUserId,
                    Note = entry.Note,
                    Detail = entry.Detail,
                    AtUtc = entry.AtUtc
                });
                return id;
            }
        }

        public IReadOnlyList<BusinessActionAuditEntry> Query(BusinessActionAuditQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            int top = query.Top <= 0 ? 100 : Math.Min(query.Top, 500);
            lock (_gate)
            {
                IEnumerable<BusinessActionAuditEntry> q = _rows;
                if (query.ActionId.HasValue)
                    q = q.Where(e => e.ActionId == query.ActionId);
                if (query.DecisionEventId.HasValue)
                    q = q.Where(e => e.DecisionEventId == query.DecisionEventId);
                if (query.AuditAction.HasValue)
                    q = q.Where(e => e.AuditAction == query.AuditAction);
                if (!string.IsNullOrWhiteSpace(query.Actor))
                    q = q.Where(e => string.Equals(e.Actor, query.Actor, StringComparison.OrdinalIgnoreCase));
                if (query.FromUtc.HasValue)
                    q = q.Where(e => e.AtUtc >= query.FromUtc);
                if (query.ToUtc.HasValue)
                    q = q.Where(e => e.AtUtc <= query.ToUtc);

                return q.OrderByDescending(e => e.AtUtc).ThenByDescending(e => e.Id).Take(top).ToList();
            }
        }
    }

    public sealed class SqlBusinessActionAuditStore : IBusinessActionAuditStore
    {
        private readonly CrmBusinessActionAuditDAL _dal = new();

        public long Append(BusinessActionAuditEntry entry)
            => _dal.Insert(new BusinessActionAuditRow
            {
                ActionId = entry.ActionId,
                DecisionEventId = entry.DecisionEventId,
                ActionType = entry.ActionType.HasValue ? (int)entry.ActionType.Value : null,
                AuditAction = (byte)entry.AuditAction,
                PreviousStatus = entry.PreviousStatus.HasValue ? (byte)entry.PreviousStatus.Value : null,
                NewStatus = entry.NewStatus.HasValue ? (byte)entry.NewStatus.Value : null,
                Outcome = entry.Outcome.HasValue ? (byte)entry.Outcome.Value : null,
                Actor = entry.Actor,
                ActorUserId = entry.ActorUserId,
                Note = entry.Note,
                Detail = entry.Detail,
                AtUtc = entry.AtUtc
            });

        public IReadOnlyList<BusinessActionAuditEntry> Query(BusinessActionAuditQuery query)
        {
            var table = _dal.Query(
                query.ActionId,
                query.DecisionEventId,
                query.AuditAction.HasValue ? (byte)query.AuditAction.Value : null,
                query.Actor,
                query.FromUtc,
                query.ToUtc,
                query.Top);

            var list = new List<BusinessActionAuditEntry>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                list.Add(FromRow(row));
            return list;
        }

        private static BusinessActionAuditEntry FromRow(System.Data.DataRow row) => new()
        {
            Id = Convert.ToInt64(row["Id"], CultureInfo.InvariantCulture),
            ActionId = (Guid)row["ActionId"],
            DecisionEventId = row["DecisionEventId"] == DBNull.Value ? null : (Guid)row["DecisionEventId"],
            ActionType = row["ActionType"] == DBNull.Value
                ? null
                : (BusinessActionType)Convert.ToInt32(row["ActionType"], CultureInfo.InvariantCulture),
            AuditAction = (BusinessActionAuditAction)Convert.ToByte(row["AuditAction"], CultureInfo.InvariantCulture),
            PreviousStatus = row["PreviousStatus"] == DBNull.Value
                ? null
                : (BusinessActionStatus)Convert.ToByte(row["PreviousStatus"], CultureInfo.InvariantCulture),
            NewStatus = row["NewStatus"] == DBNull.Value
                ? null
                : (BusinessActionStatus)Convert.ToByte(row["NewStatus"], CultureInfo.InvariantCulture),
            Outcome = row["Outcome"] == DBNull.Value
                ? null
                : (BusinessActionOutcome)Convert.ToByte(row["Outcome"], CultureInfo.InvariantCulture),
            Actor = row["Actor"] == DBNull.Value ? null : Convert.ToString(row["Actor"]),
            ActorUserId = row["ActorUserId"] == DBNull.Value
                ? null
                : Convert.ToInt32(row["ActorUserId"], CultureInfo.InvariantCulture),
            Note = row["Note"] == DBNull.Value ? null : Convert.ToString(row["Note"]),
            Detail = row["Detail"] == DBNull.Value ? null : Convert.ToString(row["Detail"]),
            AtUtc = Convert.ToDateTime(row["AtUtc"], CultureInfo.InvariantCulture)
        };
    }

    /// <summary>Servicio de auditoría de acciones (FASE 11.12).</summary>
    public sealed class BusinessActionAuditService
    {
        private readonly IBusinessActionAuditStore _store;

        public BusinessActionAuditService(IBusinessActionAuditStore? store = null)
        {
            _store = store ?? new SqlBusinessActionAuditStore();
        }

        public IBusinessActionAuditStore Store => _store;

        public long Record(BusinessActionAuditEntry entry)
            => _store.Append(entry);

        public IReadOnlyList<BusinessActionAuditEntry> GetAudit(BusinessActionAuditQuery? query = null)
            => _store.Query(query ?? new BusinessActionAuditQuery());

        public IReadOnlyList<BusinessActionAuditEntry> ForAction(Guid actionId, int top = 50)
            => GetAudit(new BusinessActionAuditQuery { ActionId = actionId, Top = top });

        public static BusinessActionAuditEntry FromRegister(
            BusinessActionRecord record,
            string? actor = null,
            DateTime? at = null)
            => Base(record, BusinessActionAuditAction.Register, null, record.Status, actor, at,
                detail: BusinessActionCatalog.DisplayName(record.ActionType));

        public static BusinessActionAuditEntry FromStatusChange(
            BusinessActionRecord record,
            BusinessActionStatus? previous,
            BusinessActionAuditAction auditAction,
            string? actor = null,
            string? note = null,
            DateTime? at = null)
            => Base(record, auditAction, previous, record.Status, actor, at, note,
                detail: $"{BusinessActionCatalog.StatusLabel(previous ?? BusinessActionStatus.Unspecified)} → {BusinessActionCatalog.StatusLabel(record.Status)}");

        public static BusinessActionAuditEntry FromBaseline(
            BusinessActionRecord record,
            string? actor = null,
            DateTime? at = null)
            => Base(record, BusinessActionAuditAction.CaptureBaseline, record.Status, record.Status, actor, at,
                detail: record.Baseline?.HasMetrics == true
                    ? $"{record.Baseline.Metrics.Count} métricas"
                    : "sin métricas");

        public static BusinessActionAuditEntry FromPostMetrics(
            BusinessActionRecord record,
            string? actor = null,
            DateTime? at = null)
            => Base(record, BusinessActionAuditAction.CapturePostMetrics, record.Status, record.Status, actor, at,
                detail: $"{record.ActualImpact?.Deltas?.Count ?? 0} deltas");

        public static BusinessActionAuditEntry FromEvaluate(
            BusinessActionRecord record,
            string? actor = null,
            string? note = null,
            DateTime? at = null)
            => Base(record, BusinessActionAuditAction.Evaluate, record.Status, record.Status, actor, at, note,
                detail: record.ActualImpact == null
                    ? null
                    : $"{BusinessActionCatalog.OutcomeLabel(record.ActualImpact.Outcome)} / {BusinessActionCatalog.ConfidenceLabel(record.ActualImpact.Confidence)}",
                outcome: record.ActualImpact?.Outcome);

        public static BusinessActionAuditEntry FromSetWindow(
            BusinessActionRecord record,
            string? actor = null,
            DateTime? at = null)
            => Base(record, BusinessActionAuditAction.SetEvaluationWindow, record.Status, record.Status, actor, at,
                detail: $"Days={record.EvaluationDays}; Due={record.EvaluationDueAt:yyyy-MM-dd}");

        private static BusinessActionAuditEntry Base(
            BusinessActionRecord record,
            BusinessActionAuditAction auditAction,
            BusinessActionStatus? previous,
            BusinessActionStatus? next,
            string? actor,
            DateTime? at,
            string? note = null,
            string? detail = null,
            BusinessActionOutcome? outcome = null)
        {
            string? resolvedActor = BusinessActionActorResolver.ResolveName(actor ?? record.CompletedBy ?? record.CreatedBy);
            return new BusinessActionAuditEntry
            {
                ActionId = record.ActionId,
                DecisionEventId = record.DecisionEventId,
                ActionType = record.ActionType,
                AuditAction = auditAction,
                PreviousStatus = previous,
                NewStatus = next,
                Outcome = outcome is BusinessActionOutcome.Unspecified ? null : outcome,
                Actor = resolvedActor,
                ActorUserId = BusinessActionActorResolver.ResolveUserId(resolvedActor),
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim(),
                AtUtc = at ?? DateTime.UtcNow
            };
        }
    }
}
