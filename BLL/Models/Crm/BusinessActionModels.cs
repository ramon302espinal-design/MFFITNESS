namespace BLL.Models.Crm
{
    /// <summary>
    /// Tipo de acción de negocio (FASE 11.2 / brief §4).
    /// Distinto de <see cref="DecisionResolutionAction"/> (cerrar evento).
    /// El usuario ejecuta; el sistema solo registra.
    /// </summary>
    public enum BusinessActionType
    {
        Unspecified = 0,
        Promotion = 1,
        PriceChange = 2,
        Replenishment = 3,
        StockReduction = 4,
        MixChange = 5,
        Campaign = 6,
        PurchasePause = 7,
        CostReview = 8,
        MarginReview = 9,
        StrategyChange = 10,
        Other = 99
    }

    /// <summary>Estado de ciclo de la acción (FASE 11.2 / brief §8).</summary>
    public enum BusinessActionStatus
    {
        Unspecified = 0,
        /// <summary>PENDIENTE — registrada, aún no iniciada.</summary>
        Pending = 1,
        /// <summary>EN PROCESO — en curso.</summary>
        InProgress = 2,
        /// <summary>COMPLETADA — lista para evaluar resultado.</summary>
        Completed = 3,
        /// <summary>CANCELADA — no evaluar como exitosa (TEST 8).</summary>
        Cancelled = 4,
        /// <summary>SIN RESULTADO — cerrada sin evaluación.</summary>
        NoResult = 5
    }

    /// <summary>
    /// Clasificación del resultado observado (FASE 11.2 / brief §12).
    /// Sin causalidad automática — evaluación = FASE 11.9+.
    /// </summary>
    public enum BusinessActionOutcome
    {
        Unspecified = 0,
        /// <summary>🟢 EXITOSA</summary>
        Successful = 1,
        /// <summary>🟡 PARCIAL</summary>
        Partial = 2,
        /// <summary>🔴 NO EFECTIVA</summary>
        Ineffective = 3,
        /// <summary>⚪ SIN DATOS — no inventar (TEST 7/12).</summary>
        InsufficientData = 4
    }

    /// <summary>Confianza en la evaluación (FASE 11.2 / brief §27).</summary>
    public enum BusinessActionConfidence
    {
        Unspecified = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    /// <summary>Descriptor de catálogo (sin umbrales ni I/O).</summary>
    public sealed class BusinessActionTypeDescriptor
    {
        public BusinessActionType Type { get; init; }
        public string Code { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        /// <summary>True si el tipo implica mutación POS — solo registro, nunca auto-ejecutar.</summary>
        public bool ImpliesManualPosChange { get; init; }
    }
}
