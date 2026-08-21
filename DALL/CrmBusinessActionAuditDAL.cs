using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>Persistencia auditoría acciones (FASE 11.12). Sin dependencia BLL.</summary>
    public class CrmBusinessActionAuditDAL
    {
        private readonly DBHelper db = new();

        public long Insert(BusinessActionAuditRow row)
        {
            const string sql = @"
INSERT INTO CrmBusinessActionAudit
(
    ActionId, DecisionEventId, ActionType, AuditAction,
    PreviousStatus, NewStatus, Outcome, Actor, ActorUserId, Note, Detail, AtUtc
)
OUTPUT INSERTED.Id
VALUES
(
    @ActionId, @DecisionEventId, @ActionType, @AuditAction,
    @PreviousStatus, @NewStatus, @Outcome, @Actor, @ActorUserId, @Note, @Detail, @AtUtc
)";

            return Convert.ToInt64(db.ExecuteScalar(sql, new[]
            {
                new SqlParameter("@ActionId", row.ActionId),
                new SqlParameter("@DecisionEventId", (object?)row.DecisionEventId ?? DBNull.Value),
                new SqlParameter("@ActionType", (object?)row.ActionType ?? DBNull.Value),
                new SqlParameter("@AuditAction", row.AuditAction),
                new SqlParameter("@PreviousStatus", (object?)row.PreviousStatus ?? DBNull.Value),
                new SqlParameter("@NewStatus", (object?)row.NewStatus ?? DBNull.Value),
                new SqlParameter("@Outcome", (object?)row.Outcome ?? DBNull.Value),
                new SqlParameter("@Actor", (object?)row.Actor ?? DBNull.Value),
                new SqlParameter("@ActorUserId", (object?)row.ActorUserId ?? DBNull.Value),
                new SqlParameter("@Note", (object?)row.Note ?? DBNull.Value),
                new SqlParameter("@Detail", (object?)row.Detail ?? DBNull.Value),
                new SqlParameter("@AtUtc", row.AtUtc)
            }));
        }

        public DataTable Query(
            Guid? actionId,
            Guid? decisionEventId,
            byte? auditAction,
            string? actor,
            DateTime? fromUtc,
            DateTime? toUtc,
            int top)
        {
            if (top <= 0) top = 100;
            if (top > 500) top = 500;

            const string sql = @"
SELECT TOP (@Top)
    Id, ActionId, DecisionEventId, ActionType, AuditAction,
    PreviousStatus, NewStatus, Outcome, Actor, ActorUserId, Note, Detail, AtUtc
FROM CrmBusinessActionAudit
WHERE (@ActionId IS NULL OR ActionId = @ActionId)
  AND (@DecisionEventId IS NULL OR DecisionEventId = @DecisionEventId)
  AND (@AuditAction IS NULL OR AuditAction = @AuditAction)
  AND (@Actor IS NULL OR Actor = @Actor)
  AND (@FromUtc IS NULL OR AtUtc >= @FromUtc)
  AND (@ToUtc IS NULL OR AtUtc <= @ToUtc)
ORDER BY AtUtc DESC, Id DESC";

            return db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@Top", top),
                new SqlParameter("@ActionId", (object?)actionId ?? DBNull.Value),
                new SqlParameter("@DecisionEventId", (object?)decisionEventId ?? DBNull.Value),
                new SqlParameter("@AuditAction", (object?)auditAction ?? DBNull.Value),
                new SqlParameter("@Actor", (object?)actor ?? DBNull.Value),
                new SqlParameter("@FromUtc", (object?)fromUtc ?? DBNull.Value),
                new SqlParameter("@ToUtc", (object?)toUtc ?? DBNull.Value)
            });
        }
    }

    public sealed class BusinessActionAuditRow
    {
        public long Id { get; init; }
        public Guid ActionId { get; init; }
        public Guid? DecisionEventId { get; init; }
        public int? ActionType { get; init; }
        public byte AuditAction { get; init; }
        public byte? PreviousStatus { get; init; }
        public byte? NewStatus { get; init; }
        public byte? Outcome { get; init; }
        public string? Actor { get; init; }
        public int? ActorUserId { get; init; }
        public string? Note { get; init; }
        public string? Detail { get; init; }
        public DateTime AtUtc { get; init; }
    }
}
