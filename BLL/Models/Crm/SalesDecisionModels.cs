namespace BLL.Models.Crm
{
    public enum SalesDecisionSeverity
    {
        Info = 0,
        Watch = 1,
        Action = 2
    }

    /// <summary>Señal narrativa para Centro de decisiones (FASE 9.22 / §63).</summary>
    public sealed class SalesDecisionSignal
    {
        public string Code { get; init; } = string.Empty;
        public SalesDecisionSeverity Severity { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public int Priority { get; init; }
    }

    public sealed class SalesDecisionReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public IReadOnlyList<SalesDecisionSignal> Signals { get; init; }
            = Array.Empty<SalesDecisionSignal>();

        /// <summary>Señal de mayor prioridad (para tarjeta principal UI).</summary>
        public SalesDecisionSignal? Primary { get; init; }

        public int SignalCount => Signals.Count;

        public string PolicyNote { get; init; } = string.Empty;
    }
}
