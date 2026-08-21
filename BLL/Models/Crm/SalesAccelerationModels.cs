namespace BLL.Models.Crm
{
    /// <summary>
    /// Aceleración de ventas (FASE 9.15). ≠ tendencia Growing/Declining (9.14).
    /// Desaceleración puede coexistir con crecimiento (ej. +40 → +20 → +5).
    /// </summary>
    public enum SalesAccelerationKind
    {
        InsufficientData = 0,
        Accelerating = 1,
        Decelerating = 2,
        Steady = 3
    }

    public sealed class SalesAccelerationResult
    {
        public SalesAccelerationKind Kind { get; init; }
        public int ChangeCount { get; init; }

        /// <summary>Primera variación % del tramo (periodo₁ vs periodo₀).</summary>
        public decimal? FirstChangePct { get; init; }

        /// <summary>Última variación % del tramo.</summary>
        public decimal? LastChangePct { get; init; }

        /// <summary>Last − First (pp). Positivo = acelera.</summary>
        public decimal? AccelerationDeltaPp { get; init; }

        public IReadOnlyList<decimal> ChangePcts { get; init; } = Array.Empty<decimal>();

        public string Reason { get; init; } = string.Empty;
    }

    public sealed class SalesAccelerationReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public string SeriesLabel { get; init; } = "Semanas (ingresos)";

        public SalesAccelerationResult Revenue { get; init; } = null!;
        public SalesAccelerationResult RealizedProfit { get; init; } = null!;
        public SalesAccelerationResult Units { get; init; } = null!;
        public SalesAccelerationResult Transactions { get; init; } = null!;
    }
}
