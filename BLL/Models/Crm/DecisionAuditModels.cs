namespace BLL.Models.Crm
{
    /// <summary>Acción auditada (FASE 10.23).</summary>
    public enum DecisionAuditAction
    {
        Detected = 1,
        StartReview = 2,
        Resolve = 3,
        Ignore = 4,
        Reopen = 5,
        DuplicateSuppressed = 6
    }

    /// <summary>Entrada append-only de auditoría.</summary>
    public sealed class DecisionAuditEntry
    {
        public long Id { get; init; }
        public long? HistoryId { get; init; }
        public Guid? EventId { get; init; }
        public string? Fingerprint { get; init; }
        public string? EventType { get; init; }
        public DecisionAuditAction Action { get; init; }
        public DecisionEventStatus? PreviousStatus { get; init; }
        public DecisionEventStatus? NewStatus { get; init; }
        public string? Actor { get; init; }
        public string? Note { get; init; }
        public string? Detail { get; init; }
        public DateTime AtUtc { get; init; }
    }

    /// <summary>Filtro de consulta de auditoría.</summary>
    public sealed class DecisionAuditQuery
    {
        public Guid? EventId { get; init; }
        public long? HistoryId { get; init; }
        public string? Fingerprint { get; init; }
        public DecisionAuditAction? Action { get; init; }
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public int Top { get; init; } = 100;
    }
}
