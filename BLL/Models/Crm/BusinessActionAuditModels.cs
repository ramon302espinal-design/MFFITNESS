namespace BLL.Models.Crm
{
    /// <summary>Acción auditada de negocio (FASE 11.12).</summary>
    public enum BusinessActionAuditAction
    {
        Register = 1,
        Start = 2,
        Complete = 3,
        Cancel = 4,
        MarkNoResult = 5,
        CaptureBaseline = 6,
        CapturePostMetrics = 7,
        Evaluate = 8,
        SetEvaluationWindow = 9
    }

    /// <summary>Entrada append-only de auditoría de acciones.</summary>
    public sealed class BusinessActionAuditEntry
    {
        public long Id { get; init; }
        public Guid ActionId { get; init; }
        public Guid? DecisionEventId { get; init; }
        public BusinessActionType? ActionType { get; init; }
        public BusinessActionAuditAction AuditAction { get; init; }
        public BusinessActionStatus? PreviousStatus { get; init; }
        public BusinessActionStatus? NewStatus { get; init; }
        public BusinessActionOutcome? Outcome { get; init; }
        public string? Actor { get; init; }
        public int? ActorUserId { get; init; }
        public string? Note { get; init; }
        public string? Detail { get; init; }
        public DateTime AtUtc { get; init; }
    }

    public sealed class BusinessActionAuditQuery
    {
        public Guid? ActionId { get; init; }
        public Guid? DecisionEventId { get; init; }
        public BusinessActionAuditAction? AuditAction { get; init; }
        public string? Actor { get; init; }
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public int Top { get; init; } = 100;
    }
}
