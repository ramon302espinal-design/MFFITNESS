namespace BLL.Models.Crm
{
    /// <summary>
    /// Fase de la ventana de evaluación post-acción (FASE 11.7 / brief §18).
    /// La evaluación de resultado (Exitosa/…) = 11.9 — aquí solo el calendario.
    /// </summary>
    public enum BusinessActionEvaluationPhase
    {
        /// <summary>No aplica (pendiente, en proceso, cancelada, sin resultado).</summary>
        NotApplicable = 0,

        /// <summary>Días planificados; acción aún no completada.</summary>
        Planned = 1,

        /// <summary>Completada; aún dentro de la ventana (asOf &lt; DueAt).</summary>
        InWindow = 2,

        /// <summary>Completada; ventana vencida y sin outcome evaluado.</summary>
        Ready = 3,

        /// <summary>Ya tiene ActualImpact con outcome distinto de Unspecified.</summary>
        Evaluated = 4
    }

    /// <summary>Vista calculada de la ventana (sin I/O).</summary>
    public sealed class BusinessActionEvaluationWindow
    {
        public BusinessActionEvaluationPhase Phase { get; init; }

        /// <summary>Ancla: CompletedAt (post-complete) o CreatedAt (planned).</summary>
        public DateTime? WindowStart { get; init; }

        public DateTime? WindowEnd { get; init; }

        public int? EvaluationDays { get; init; }

        /// <summary>Días restantes hasta DueAt (negativo = vencida). Null si N/A.</summary>
        public int? DaysRemaining { get; init; }

        public bool IsReady => Phase == BusinessActionEvaluationPhase.Ready;

        public string Label { get; init; } = string.Empty;
    }
}
