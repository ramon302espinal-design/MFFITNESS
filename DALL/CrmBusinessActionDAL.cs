using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    /// <summary>Persistencia ActionRecord (FASE 11.4). Sin dependencia BLL.</summary>
    public class CrmBusinessActionDAL
    {
        private readonly DBHelper db = new();

        private const string SelectCols = @"
    Id, ActionId, DecisionEventId, DecisionHistoryId, ActionType, Area, EntityType,
    EntityId, EntityName, Description, Reason, Notes, QuantityInvolved, CapitalInvolved,
    CreatedAt, CreatedBy, Status, StartedAt, EvaluationDays, EvaluationDueAt,
    CompletedAt, CompletedBy, ExpectedSummary, ExpectedNotes, ExpectedMetricKeys,
    Outcome, Confidence, ActualSummary, ActualNotes, BaselinePayload, DeltasPayload";

        public long Insert(BusinessActionRow row)
        {
            const string sql = @"
INSERT INTO CrmBusinessActions
(
    ActionId, DecisionEventId, DecisionHistoryId, ActionType, Area, EntityType,
    EntityId, EntityName, Description, Reason, Notes, QuantityInvolved, CapitalInvolved,
    CreatedAt, CreatedBy, Status, StartedAt, EvaluationDays, EvaluationDueAt,
    CompletedAt, CompletedBy, ExpectedSummary, ExpectedNotes, ExpectedMetricKeys,
    Outcome, Confidence, ActualSummary, ActualNotes, BaselinePayload, DeltasPayload
)
OUTPUT INSERTED.Id
VALUES
(
    @ActionId, @DecisionEventId, @DecisionHistoryId, @ActionType, @Area, @EntityType,
    @EntityId, @EntityName, @Description, @Reason, @Notes, @QuantityInvolved, @CapitalInvolved,
    @CreatedAt, @CreatedBy, @Status, @StartedAt, @EvaluationDays, @EvaluationDueAt,
    @CompletedAt, @CompletedBy, @ExpectedSummary, @ExpectedNotes, @ExpectedMetricKeys,
    @Outcome, @Confidence, @ActualSummary, @ActualNotes, @BaselinePayload, @DeltasPayload
)";
            return Convert.ToInt64(db.ExecuteScalar(sql, ToParams(row)));
        }

        public int Update(BusinessActionRow row)
        {
            const string sql = @"
UPDATE CrmBusinessActions
SET DecisionEventId = @DecisionEventId,
    DecisionHistoryId = @DecisionHistoryId,
    ActionType = @ActionType,
    Area = @Area,
    EntityType = @EntityType,
    EntityId = @EntityId,
    EntityName = @EntityName,
    Description = @Description,
    Reason = @Reason,
    Notes = @Notes,
    QuantityInvolved = @QuantityInvolved,
    CapitalInvolved = @CapitalInvolved,
    CreatedBy = @CreatedBy,
    Status = @Status,
    StartedAt = @StartedAt,
    EvaluationDays = @EvaluationDays,
    EvaluationDueAt = @EvaluationDueAt,
    CompletedAt = @CompletedAt,
    CompletedBy = @CompletedBy,
    ExpectedSummary = @ExpectedSummary,
    ExpectedNotes = @ExpectedNotes,
    ExpectedMetricKeys = @ExpectedMetricKeys,
    Outcome = @Outcome,
    Confidence = @Confidence,
    ActualSummary = @ActualSummary,
    ActualNotes = @ActualNotes,
    BaselinePayload = @BaselinePayload,
    DeltasPayload = @DeltasPayload
WHERE ActionId = @ActionId";
            return db.ExecuteNonQuery(sql, ToParams(row));
        }

        public DataRow? GetByActionId(Guid actionId)
        {
            string sql = $@"
SELECT TOP 1 {SelectCols}
FROM CrmBusinessActions
WHERE ActionId = @ActionId";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@ActionId", actionId) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public DataRow? GetById(long id)
        {
            string sql = $@"
SELECT {SelectCols}
FROM CrmBusinessActions
WHERE Id = @Id";
            DataTable t = db.ExecuteQuery(sql, new[] { new SqlParameter("@Id", id) });
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public DataTable Query(
            byte? status,
            int? actionType,
            Guid? decisionEventId,
            byte? entityType,
            string? entityId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int top)
        {
            if (top <= 0) top = 100;
            if (top > 500) top = 500;

            string sql = $@"
SELECT TOP (@Top) {SelectCols}
FROM CrmBusinessActions
WHERE (@Status IS NULL OR Status = @Status)
  AND (@ActionType IS NULL OR ActionType = @ActionType)
  AND (@DecisionEventId IS NULL OR DecisionEventId = @DecisionEventId)
  AND (@EntityType IS NULL OR EntityType = @EntityType)
  AND (@EntityId IS NULL OR EntityId = @EntityId)
  AND (@FromUtc IS NULL OR CreatedAt >= @FromUtc)
  AND (@ToUtc IS NULL OR CreatedAt <= @ToUtc)
ORDER BY CreatedAt DESC, Id DESC";

            return db.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@Top", top),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@ActionType", (object?)actionType ?? DBNull.Value),
                new SqlParameter("@DecisionEventId", (object?)decisionEventId ?? DBNull.Value),
                new SqlParameter("@EntityType", (object?)entityType ?? DBNull.Value),
                new SqlParameter("@EntityId", (object?)entityId ?? DBNull.Value),
                new SqlParameter("@FromUtc", (object?)fromUtc ?? DBNull.Value),
                new SqlParameter("@ToUtc", (object?)toUtc ?? DBNull.Value)
            });
        }

        private static SqlParameter[] ToParams(BusinessActionRow row) =>
        [
            new("@ActionId", row.ActionId),
            new("@DecisionEventId", (object?)row.DecisionEventId ?? DBNull.Value),
            new("@DecisionHistoryId", (object?)row.DecisionHistoryId ?? DBNull.Value),
            new("@ActionType", row.ActionType),
            new("@Area", row.Area),
            new("@EntityType", row.EntityType),
            new("@EntityId", (object?)row.EntityId ?? DBNull.Value),
            new("@EntityName", (object?)row.EntityName ?? DBNull.Value),
            new("@Description", row.Description),
            new("@Reason", (object?)row.Reason ?? DBNull.Value),
            new("@Notes", (object?)row.Notes ?? DBNull.Value),
            new("@QuantityInvolved", (object?)row.QuantityInvolved ?? DBNull.Value),
            new("@CapitalInvolved", (object?)row.CapitalInvolved ?? DBNull.Value),
            new("@CreatedAt", row.CreatedAt),
            new("@CreatedBy", (object?)row.CreatedBy ?? DBNull.Value),
            new("@Status", row.Status),
            new("@StartedAt", (object?)row.StartedAt ?? DBNull.Value),
            new("@EvaluationDays", (object?)row.EvaluationDays ?? DBNull.Value),
            new("@EvaluationDueAt", (object?)row.EvaluationDueAt ?? DBNull.Value),
            new("@CompletedAt", (object?)row.CompletedAt ?? DBNull.Value),
            new("@CompletedBy", (object?)row.CompletedBy ?? DBNull.Value),
            new("@ExpectedSummary", (object?)row.ExpectedSummary ?? DBNull.Value),
            new("@ExpectedNotes", (object?)row.ExpectedNotes ?? DBNull.Value),
            new("@ExpectedMetricKeys", (object?)row.ExpectedMetricKeys ?? DBNull.Value),
            new("@Outcome", (object?)row.Outcome ?? DBNull.Value),
            new("@Confidence", (object?)row.Confidence ?? DBNull.Value),
            new("@ActualSummary", (object?)row.ActualSummary ?? DBNull.Value),
            new("@ActualNotes", (object?)row.ActualNotes ?? DBNull.Value),
            new("@BaselinePayload", (object?)row.BaselinePayload ?? DBNull.Value),
            new("@DeltasPayload", (object?)row.DeltasPayload ?? DBNull.Value)
        ];
    }

    /// <summary>DTO plano DAL (sin dependencia BLL).</summary>
    public sealed class BusinessActionRow
    {
        public long Id { get; init; }
        public Guid ActionId { get; init; }
        public Guid? DecisionEventId { get; init; }
        public long? DecisionHistoryId { get; init; }
        public int ActionType { get; init; }
        public byte Area { get; init; }
        public byte EntityType { get; init; }
        public string? EntityId { get; init; }
        public string? EntityName { get; init; }
        public string Description { get; init; } = string.Empty;
        public string? Reason { get; init; }
        public string? Notes { get; init; }
        public decimal? QuantityInvolved { get; init; }
        public decimal? CapitalInvolved { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public byte Status { get; init; }
        public DateTime? StartedAt { get; init; }
        public int? EvaluationDays { get; init; }
        public DateTime? EvaluationDueAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public string? CompletedBy { get; init; }
        public string? ExpectedSummary { get; init; }
        public string? ExpectedNotes { get; init; }
        public string? ExpectedMetricKeys { get; init; }
        public byte? Outcome { get; init; }
        public byte? Confidence { get; init; }
        public string? ActualSummary { get; init; }
        public string? ActualNotes { get; init; }
        /// <summary>Codec baseline v1 (FASE 11.6). Null si Schema &lt; 13 o sin snapshot.</summary>
        public string? BaselinePayload { get; init; }
        /// <summary>Codec deltas (FASE 11.8). Null si Schema &lt; 14 o sin deltas.</summary>
        public string? DeltasPayload { get; init; }
    }
}
