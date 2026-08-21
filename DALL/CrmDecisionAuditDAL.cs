using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>Persistencia auditoría decisiones (FASE 10.23).</summary>
    public class CrmDecisionAuditDAL
    {
        private readonly DBHelper db = new();

        public long Insert(DecisionAuditRow row)
        {
            const string sql = @"
INSERT INTO CrmDecisionAudit
(
    HistoryId, EventId, Fingerprint, EventType, Action,
    PreviousStatus, NewStatus, Actor, Note, Detail, AtUtc
)
OUTPUT INSERTED.Id
VALUES
(
    @HistoryId, @EventId, @Fingerprint, @EventType, @Action,
    @PreviousStatus, @NewStatus, @Actor, @Note, @Detail, @AtUtc
)";

            return Convert.ToInt64(db.ExecuteScalar(sql, new[]
            {
                new SqlParameter("@HistoryId", (object?)row.HistoryId ?? DBNull.Value),
                new SqlParameter("@EventId", (object?)row.EventId ?? DBNull.Value),
                new SqlParameter("@Fingerprint", (object?)row.Fingerprint ?? DBNull.Value),
                new SqlParameter("@EventType", (object?)row.EventType ?? DBNull.Value),
                new SqlParameter("@Action", row.Action),
                new SqlParameter("@PreviousStatus", (object?)row.PreviousStatus ?? DBNull.Value),
                new SqlParameter("@NewStatus", (object?)row.NewStatus ?? DBNull.Value),
                new SqlParameter("@Actor", (object?)row.Actor ?? DBNull.Value),
                new SqlParameter("@Note", (object?)row.Note ?? DBNull.Value),
                new SqlParameter("@Detail", (object?)row.Detail ?? DBNull.Value),
                new SqlParameter("@AtUtc", row.AtUtc)
            }));
        }

        public DataTable Query(
            Guid? eventId,
            long? historyId,
            string? fingerprint,
            byte? action,
            DateTime? fromUtc,
            DateTime? toUtc,
            int top)
        {
            if (top <= 0) top = 100;
            if (top > 500) top = 500;

            const string sql = @"
SELECT TOP (@Top)
    Id, HistoryId, EventId, Fingerprint, EventType, Action,
    PreviousStatus, NewStatus, Actor, Note, Detail, AtUtc
FROM CrmDecisionAudit
WHERE (@EventId IS NULL OR EventId = @EventId)
  AND (@HistoryId IS NULL OR HistoryId = @HistoryId)
  AND (@Fingerprint IS NULL OR Fingerprint = @Fingerprint)
  AND (@Action IS NULL OR Action = @Action)
  AND (@FromUtc IS NULL OR AtUtc >= @FromUtc)
  AND (@ToUtc IS NULL OR AtUtc <= @ToUtc)
ORDER BY AtUtc DESC, Id DESC";

            return db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@Top", top),
                new SqlParameter("@EventId", (object?)eventId ?? DBNull.Value),
                new SqlParameter("@HistoryId", (object?)historyId ?? DBNull.Value),
                new SqlParameter("@Fingerprint", (object?)fingerprint ?? DBNull.Value),
                new SqlParameter("@Action", (object?)action ?? DBNull.Value),
                new SqlParameter("@FromUtc", (object?)fromUtc ?? DBNull.Value),
                new SqlParameter("@ToUtc", (object?)toUtc ?? DBNull.Value)
            });
        }
    }

    public sealed class DecisionAuditRow
    {
        public long? HistoryId { get; init; }
        public Guid? EventId { get; init; }
        public string? Fingerprint { get; init; }
        public string? EventType { get; init; }
        public byte Action { get; init; }
        public byte? PreviousStatus { get; init; }
        public byte? NewStatus { get; init; }
        public string? Actor { get; init; }
        public string? Note { get; init; }
        public string? Detail { get; init; }
        public DateTime AtUtc { get; init; }
    }
}
