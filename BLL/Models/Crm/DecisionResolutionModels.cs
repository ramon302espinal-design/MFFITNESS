namespace BLL.Models.Crm
{
    /// <summary>Acción de resolución (FASE 10.22 / brief §107).</summary>
    public enum DecisionResolutionAction
    {
        StartReview = 1,
        Resolve = 2,
        Ignore = 3,
        /// <summary>Reabrir a Active (solo desde InReview).</summary>
        Reopen = 4
    }

    /// <summary>Solicitud de cambio de estado en historial.</summary>
    public sealed class DecisionResolutionRequest
    {
        public Guid? EventId { get; init; }
        public long? HistoryId { get; init; }
        /// <summary>Si se indica, actúa sobre el registro abierto (Active/InReview) de esa huella.</summary>
        public string? Fingerprint { get; init; }

        public DecisionResolutionAction Action { get; init; }

        public string? Actor { get; init; }
        public string? Note { get; init; }
        public DateTime? AtUtc { get; init; }
    }

    /// <summary>Resultado de Resolve/Ignore/StartReview.</summary>
    public sealed class DecisionResolutionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public DecisionHistoryRecord? Record { get; init; }
        public DecisionEventStatus? PreviousStatus { get; init; }
        public DecisionEventStatus? NewStatus { get; init; }
    }
}
