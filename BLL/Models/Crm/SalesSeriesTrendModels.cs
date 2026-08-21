namespace BLL.Models.Crm
{
    /// <summary>
    /// Tendencia de serie multi-punto (FASE 9.14).
    /// Extiende Growing/Stable/Declining con Volatile; ≠ MoM de 2 puntos (FASE 8).
    /// </summary>
    public enum SalesSeriesTrendKind
    {
        InsufficientData = 0,
        Growing = 1,
        Stable = 2,
        Declining = 3,
        Volatile = 4
    }

    public sealed class SalesSeriesTrendResult
    {
        public SalesSeriesTrendKind Kind { get; init; }
        public int PointCount { get; init; }

        /// <summary>Pendiente lineal normalizada (aprox. cambio relativo por paso).</summary>
        public decimal? SlopePerStepPct { get; init; }

        /// <summary>Coeficiente de variación % = desv.est / promedio × 100.</summary>
        public decimal? CoefficientOfVariationPct { get; init; }

        public string Reason { get; init; } = string.Empty;
    }

    public sealed class SalesSeriesTrendReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public string SeriesLabel { get; init; } = "Ingresos diarios";

        public SalesSeriesTrendResult Revenue { get; init; } = null!;
        public SalesSeriesTrendResult RealizedProfit { get; init; } = null!;
        public SalesSeriesTrendResult Units { get; init; } = null!;
        public SalesSeriesTrendResult Transactions { get; init; } = null!;
    }
}
