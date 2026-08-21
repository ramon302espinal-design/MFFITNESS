using System.Data;
using BLL.Models.Crm;
using DL;

namespace BLL.Services.Crm
{
    /// <summary>Contrato persistencia acciones (FASE 11.4 / baseline 11.6).</summary>
    public static class BusinessActionPersistencePolicy
    {
        public const string Definition =
            "FASE 11.4: dbo.CrmBusinessActions (SchemaVersion 12) + CrmBusinessActionDAL. " +
            "FASE 11.6: BaselinePayload (SchemaVersion 13). " +
            "FASE 11.8: DeltasPayload (SchemaVersion 14). " +
            "Registro append/update de ActionRecord. No muta ventas/costos/stock. " +
            "FK opcional a CrmDecisionEvents.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Mapeo Record ↔ Row DAL (sin I/O).</summary>
    public static class BusinessActionPersistenceMapper
    {
        public static BusinessActionRow ToRow(BusinessActionRecord record, long id = 0)
        {
            ArgumentNullException.ThrowIfNull(record);

            return new BusinessActionRow
            {
                Id = id,
                ActionId = record.ActionId,
                DecisionEventId = record.DecisionEventId,
                DecisionHistoryId = record.DecisionHistoryId,
                ActionType = (int)record.ActionType,
                Area = (byte)record.Area,
                EntityType = (byte)record.EntityType,
                EntityId = record.EntityId,
                EntityName = string.IsNullOrEmpty(record.EntityName) ? null : record.EntityName,
                Description = record.Description,
                Reason = record.Reason,
                Notes = record.Notes,
                QuantityInvolved = record.QuantityInvolved,
                CapitalInvolved = record.CapitalInvolved,
                CreatedAt = record.CreatedAt,
                CreatedBy = record.CreatedBy,
                Status = (byte)record.Status,
                StartedAt = record.StartedAt,
                EvaluationDays = record.EvaluationDays,
                EvaluationDueAt = record.EvaluationDueAt,
                CompletedAt = record.CompletedAt,
                CompletedBy = record.CompletedBy,
                ExpectedSummary = record.ExpectedImpact?.Summary,
                ExpectedNotes = record.ExpectedImpact?.Notes,
                ExpectedMetricKeys = JoinKeys(record.ExpectedImpact?.TargetMetricKeys),
                Outcome = record.ActualImpact == null
                    ? null
                    : (byte)record.ActualImpact.Outcome,
                Confidence = record.ActualImpact == null
                    ? null
                    : (byte)record.ActualImpact.Confidence,
                ActualSummary = record.ActualImpact?.Summary,
                ActualNotes = record.ActualImpact?.Notes,
                BaselinePayload = BusinessActionBaselineCodec.Encode(record.Baseline),
                DeltasPayload = BusinessActionDeltaCodec.Encode(record.ActualImpact?.Deltas)
            };
        }

        public static BusinessActionRecord FromRow(BusinessActionRow row)
        {
            ArgumentNullException.ThrowIfNull(row);

            BusinessActionExpectedImpact? expected = null;
            if (!string.IsNullOrWhiteSpace(row.ExpectedSummary))
            {
                expected = new BusinessActionExpectedImpact
                {
                    Summary = row.ExpectedSummary.Trim(),
                    Notes = row.ExpectedNotes,
                    TargetMetricKeys = SplitKeys(row.ExpectedMetricKeys)
                };
            }

            BusinessActionActualImpact? actual = null;
            if (row.Outcome.HasValue || !string.IsNullOrWhiteSpace(row.ActualSummary)
                || !string.IsNullOrWhiteSpace(row.DeltasPayload))
            {
                actual = new BusinessActionActualImpact
                {
                    Outcome = row.Outcome.HasValue
                        ? (BusinessActionOutcome)row.Outcome.Value
                        : BusinessActionOutcome.Unspecified,
                    Confidence = row.Confidence.HasValue
                        ? (BusinessActionConfidence)row.Confidence.Value
                        : BusinessActionConfidence.Unspecified,
                    Summary = row.ActualSummary ?? string.Empty,
                    Notes = row.ActualNotes,
                    Deltas = BusinessActionDeltaCodec.Decode(row.DeltasPayload)
                };
            }

            return new BusinessActionRecord
            {
                ActionId = row.ActionId,
                DecisionEventId = row.DecisionEventId,
                DecisionHistoryId = row.DecisionHistoryId,
                ActionType = (BusinessActionType)row.ActionType,
                Area = (DecisionEventArea)row.Area,
                EntityType = (DecisionEntityType)row.EntityType,
                EntityId = row.EntityId,
                EntityName = row.EntityName ?? string.Empty,
                Description = row.Description,
                Reason = row.Reason,
                Notes = row.Notes,
                QuantityInvolved = row.QuantityInvolved,
                CapitalInvolved = row.CapitalInvolved,
                CreatedAt = row.CreatedAt,
                CreatedBy = row.CreatedBy,
                Status = (BusinessActionStatus)row.Status,
                StartedAt = row.StartedAt,
                EvaluationDays = row.EvaluationDays,
                EvaluationDueAt = row.EvaluationDueAt,
                CompletedAt = row.CompletedAt,
                CompletedBy = row.CompletedBy,
                ExpectedImpact = expected,
                ActualImpact = actual,
                Baseline = BusinessActionBaselineCodec.Decode(row.BaselinePayload)
            };
        }

        public static BusinessActionRecord FromDataRow(DataRow row)
        {
            ArgumentNullException.ThrowIfNull(row);
            return FromRow(new BusinessActionRow
            {
                Id = Convert.ToInt64(row["Id"]),
                ActionId = (Guid)row["ActionId"],
                DecisionEventId = row["DecisionEventId"] == DBNull.Value
                    ? null
                    : (Guid)row["DecisionEventId"],
                DecisionHistoryId = row["DecisionHistoryId"] == DBNull.Value
                    ? null
                    : Convert.ToInt64(row["DecisionHistoryId"]),
                ActionType = Convert.ToInt32(row["ActionType"]),
                Area = Convert.ToByte(row["Area"]),
                EntityType = Convert.ToByte(row["EntityType"]),
                EntityId = NullStr(row["EntityId"]),
                EntityName = NullStr(row["EntityName"]),
                Description = Convert.ToString(row["Description"]) ?? string.Empty,
                Reason = NullStr(row["Reason"]),
                Notes = NullStr(row["Notes"]),
                QuantityInvolved = NullDec(row["QuantityInvolved"]),
                CapitalInvolved = NullDec(row["CapitalInvolved"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                CreatedBy = NullStr(row["CreatedBy"]),
                Status = Convert.ToByte(row["Status"]),
                StartedAt = NullDt(row["StartedAt"]),
                EvaluationDays = row["EvaluationDays"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(row["EvaluationDays"]),
                EvaluationDueAt = NullDt(row["EvaluationDueAt"]),
                CompletedAt = NullDt(row["CompletedAt"]),
                CompletedBy = NullStr(row["CompletedBy"]),
                ExpectedSummary = NullStr(row["ExpectedSummary"]),
                ExpectedNotes = NullStr(row["ExpectedNotes"]),
                ExpectedMetricKeys = NullStr(row["ExpectedMetricKeys"]),
                Outcome = row["Outcome"] == DBNull.Value ? null : Convert.ToByte(row["Outcome"]),
                Confidence = row["Confidence"] == DBNull.Value
                    ? null
                    : Convert.ToByte(row["Confidence"]),
                ActualSummary = NullStr(row["ActualSummary"]),
                ActualNotes = NullStr(row["ActualNotes"]),
                BaselinePayload = row.Table.Columns.Contains("BaselinePayload")
                    ? NullStr(row["BaselinePayload"])
                    : null,
                DeltasPayload = row.Table.Columns.Contains("DeltasPayload")
                    ? NullStr(row["DeltasPayload"])
                    : null
            });
        }

        public static string? JoinKeys(IReadOnlyList<string>? keys)
        {
            if (keys == null || keys.Count == 0)
                return null;
            return string.Join('|', keys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()));
        }

        public static IReadOnlyList<string> SplitKeys(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<string>();
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string? NullStr(object o)
            => o == DBNull.Value ? null : Convert.ToString(o);

        private static decimal? NullDec(object o)
            => o == DBNull.Value ? null : Convert.ToDecimal(o);

        private static DateTime? NullDt(object o)
            => o == DBNull.Value ? null : Convert.ToDateTime(o);
    }
}
