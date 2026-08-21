using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    /// <summary>Persistencia historial DecisionEvent (FASE 10.21).</summary>
    public class CrmDecisionHistoryDAL
    {
        private readonly DBHelper db = new();

        public bool HasActiveFingerprint(string fingerprint)
        {
            const string sql = @"
SELECT TOP 1 1
FROM CrmDecisionEvents
WHERE Fingerprint = @Fingerprint AND Status IN (1, 5)";
            object? o = db.ExecuteScalar(sql, new[]
            {
                new SqlParameter("@Fingerprint", fingerprint)
            });
            return o != null && o != DBNull.Value;
        }

        public DataRow? GetByEventId(Guid eventId)
        {
            const string sql = @"
SELECT TOP 1
    Id, EventId, Fingerprint, EventType, Area, EntityType, EntityId, EntityName, PeriodKey,
    Severity, Priority, Status, Title, Description, Reason, Impact, Recommendation,
    Source, GroupKey, DetectedAt, CreatedAt, ResolvedAt, ResolvedBy, ResolutionNote
FROM CrmDecisionEvents
WHERE EventId = @EventId
ORDER BY Id DESC";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@EventId", eventId) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public DataRow? GetById(long id)
        {
            const string sql = @"
SELECT
    Id, EventId, Fingerprint, EventType, Area, EntityType, EntityId, EntityName, PeriodKey,
    Severity, Priority, Status, Title, Description, Reason, Impact, Recommendation,
    Source, GroupKey, DetectedAt, CreatedAt, ResolvedAt, ResolvedBy, ResolutionNote
FROM CrmDecisionEvents
WHERE Id = @Id";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@Id", id) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public DataRow? GetOpenByFingerprint(string fingerprint)
        {
            const string sql = @"
SELECT TOP 1
    Id, EventId, Fingerprint, EventType, Area, EntityType, EntityId, EntityName, PeriodKey,
    Severity, Priority, Status, Title, Description, Reason, Impact, Recommendation,
    Source, GroupKey, DetectedAt, CreatedAt, ResolvedAt, ResolvedBy, ResolutionNote
FROM CrmDecisionEvents
WHERE Fingerprint = @Fingerprint AND Status IN (1, 5)
ORDER BY DetectedAt DESC, Id DESC";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@Fingerprint", fingerprint) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        /// <summary>Actualiza Status (+ resolución). Retorna filas afectadas.</summary>
        public int UpdateStatus(
            long id,
            byte newStatus,
            DateTime? resolvedAt,
            string? resolvedBy,
            string? resolutionNote)
        {
            const string sql = @"
UPDATE CrmDecisionEvents
SET Status = @Status,
    ResolvedAt = @ResolvedAt,
    ResolvedBy = @ResolvedBy,
    ResolutionNote = @ResolutionNote
WHERE Id = @Id";
            return db.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@Status", newStatus),
                new SqlParameter("@ResolvedAt", (object?)resolvedAt ?? DBNull.Value),
                new SqlParameter("@ResolvedBy", (object?)resolvedBy?.Trim() ?? DBNull.Value),
                new SqlParameter("@ResolutionNote", (object?)resolutionNote?.Trim() ?? DBNull.Value),
                new SqlParameter("@Id", id)
            });
        }

        public long Insert(DecisionHistoryRow row)
        {
            const string sql = @"
INSERT INTO CrmDecisionEvents
(
    EventId, Fingerprint, EventType, Area, EntityType, EntityId, EntityName, PeriodKey,
    Severity, Priority, Status, Title, Description, Reason, Impact, Recommendation,
    Source, GroupKey, DetectedAt, CreatedAt
)
OUTPUT INSERTED.Id
VALUES
(
    @EventId, @Fingerprint, @EventType, @Area, @EntityType, @EntityId, @EntityName, @PeriodKey,
    @Severity, @Priority, @Status, @Title, @Description, @Reason, @Impact, @Recommendation,
    @Source, @GroupKey, @DetectedAt, @CreatedAt
)";

            SqlParameter[] p =
            {
                new("@EventId", row.EventId),
                new("@Fingerprint", row.Fingerprint),
                new("@EventType", row.EventType),
                new("@Area", row.Area),
                new("@EntityType", row.EntityType),
                new("@EntityId", (object?)row.EntityId ?? DBNull.Value),
                new("@EntityName", (object?)row.EntityName ?? DBNull.Value),
                new("@PeriodKey", (object?)row.PeriodKey ?? DBNull.Value),
                new("@Severity", row.Severity),
                new("@Priority", row.Priority),
                new("@Status", row.Status),
                new("@Title", row.Title),
                new("@Description", (object?)row.Description ?? DBNull.Value),
                new("@Reason", (object?)row.Reason ?? DBNull.Value),
                new("@Impact", (object?)row.Impact ?? DBNull.Value),
                new("@Recommendation", (object?)row.Recommendation ?? DBNull.Value),
                new("@Source", (object?)row.Source ?? DBNull.Value),
                new("@GroupKey", (object?)row.GroupKey ?? DBNull.Value),
                new("@DetectedAt", row.DetectedAt),
                new("@CreatedAt", row.CreatedAt)
            };

            return Convert.ToInt64(db.ExecuteScalar(sql, p));
        }

        public DataTable Query(
            DateTime? fromUtc,
            DateTime? toUtc,
            byte? status,
            string? eventType,
            string? fingerprint,
            byte? entityType,
            string? entityId,
            int top)
        {
            if (top <= 0) top = 100;
            if (top > 500) top = 500;

            const string sql = @"
SELECT TOP (@Top)
    Id, EventId, Fingerprint, EventType, Area, EntityType, EntityId, EntityName, PeriodKey,
    Severity, Priority, Status, Title, Description, Reason, Impact, Recommendation,
    Source, GroupKey, DetectedAt, CreatedAt, ResolvedAt, ResolvedBy, ResolutionNote
FROM CrmDecisionEvents
WHERE (@FromUtc IS NULL OR DetectedAt >= @FromUtc)
  AND (@ToUtc IS NULL OR DetectedAt <= @ToUtc)
  AND (@Status IS NULL OR Status = @Status)
  AND (@EventType IS NULL OR EventType = @EventType)
  AND (@Fingerprint IS NULL OR Fingerprint = @Fingerprint)
  AND (@EntityType IS NULL OR EntityType = @EntityType)
  AND (@EntityId IS NULL OR EntityId = @EntityId)
ORDER BY DetectedAt DESC, Id DESC";

            return db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@Top", top),
                new SqlParameter("@FromUtc", (object?)fromUtc ?? DBNull.Value),
                new SqlParameter("@ToUtc", (object?)toUtc ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@EventType", (object?)eventType ?? DBNull.Value),
                new SqlParameter("@Fingerprint", (object?)fingerprint ?? DBNull.Value),
                new SqlParameter("@EntityType", (object?)entityType ?? DBNull.Value),
                new SqlParameter("@EntityId", (object?)entityId ?? DBNull.Value)
            });
        }

        public DataTable Recurrence(DateTime fromUtc, int minOccurrences)
        {
            const string sql = @"
SELECT EventType, EntityId,
       COUNT(*) AS OccurrenceCount,
       MIN(DetectedAt) AS FirstDetectedAt,
       MAX(DetectedAt) AS LastDetectedAt
FROM CrmDecisionEvents
WHERE DetectedAt >= @FromUtc
GROUP BY EventType, EntityId
HAVING COUNT(*) >= @MinOccurrences
ORDER BY COUNT(*) DESC";

            return db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@FromUtc", fromUtc),
                new SqlParameter("@MinOccurrences", minOccurrences)
            });
        }

        public DataRow? Metrics(DateTime? fromUtc, DateTime? toUtc)
        {
            const string sql = @"
SELECT
    COUNT(*) AS GeneratedCount,
    SUM(CASE WHEN Severity = 5 THEN 1 ELSE 0 END) AS CriticalCount,
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS ResolvedCount,
    SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS IgnoredCount,
    AVG(CASE
            WHEN Status = 2 AND ResolvedAt IS NOT NULL
            THEN CAST(DATEDIFF(MINUTE, DetectedAt, ResolvedAt) AS FLOAT) / 60.0
            ELSE NULL
        END) AS AvgResolutionHours
FROM CrmDecisionEvents
WHERE (@FromUtc IS NULL OR DetectedAt >= @FromUtc)
  AND (@ToUtc IS NULL OR DetectedAt <= @ToUtc)";

            DataTable t = db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@FromUtc", (object?)fromUtc ?? DBNull.Value),
                new SqlParameter("@ToUtc", (object?)toUtc ?? DBNull.Value)
            });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }
    }

    /// <summary>DTO plano DAL (sin dependencia BLL).</summary>
    public sealed class DecisionHistoryRow
    {
        public Guid EventId { get; init; }
        public string Fingerprint { get; init; } = string.Empty;
        public string EventType { get; init; } = string.Empty;
        public byte Area { get; init; }
        public byte EntityType { get; init; }
        public string? EntityId { get; init; }
        public string? EntityName { get; init; }
        public string? PeriodKey { get; init; }
        public byte Severity { get; init; }
        public byte Priority { get; init; }
        public byte Status { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Reason { get; init; }
        public string? Impact { get; init; }
        public string? Recommendation { get; init; }
        public string? Source { get; init; }
        public string? GroupKey { get; init; }
        public DateTime DetectedAt { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
