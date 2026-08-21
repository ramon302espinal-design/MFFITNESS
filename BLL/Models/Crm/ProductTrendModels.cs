namespace BLL.Models.Crm
{
    /// <summary>Dirección de tendencia MoM (FASE 8.11). Sin score.</summary>
    public enum ProductTrendDirection
    {
        InsufficientData = 0,
        Growing = 1,
        Stable = 2,
        Declining = 3
    }

    /// <summary>
    /// Aceleración (FASE 8.11 concepto). Sin algoritmo de 3+ períodos todavía.
    /// </summary>
    public enum ProductAccelerationKind
    {
        Unknown = 0,
        /// <summary>Reservado — requiere ≥3 períodos.</summary>
        Accelerating = 1,
        /// <summary>Reservado — requiere ≥3 períodos.</summary>
        Decelerating = 2,
        Steady = 3
    }

    /// <summary>Tendencia de un producto entre período actual y base.</summary>
    public sealed class ProductTrendRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public int UnitsCurrent { get; init; }
        public int UnitsPrevious { get; init; }
        public decimal RevenueCurrent { get; init; }
        public decimal RevenuePrevious { get; init; }

        /// <summary>% cambio unidades (null si InsufficientData).</summary>
        public decimal? UnitsChangePct { get; init; }

        /// <summary>% cambio ingresos (null si InsufficientData).</summary>
        public decimal? RevenueChangePct { get; init; }

        public ProductTrendDirection UnitsTrend { get; init; }
        public ProductTrendDirection RevenueTrend { get; init; }

        /// <summary>Tendencia primaria = unidades (volumen). Ingresos es paralelo.</summary>
        public ProductTrendDirection PrimaryTrend { get; init; }

        public ProductAccelerationKind Acceleration { get; init; }
            = ProductAccelerationKind.Unknown;
    }

    public sealed class ProductTrendReport
    {
        public ProfitPeriodKind CurrentPeriodKind { get; init; }
        public DateTime? CurrentFrom { get; init; }
        public DateTime? CurrentToExclusive { get; init; }
        public DateTime? PreviousFrom { get; init; }
        public DateTime? PreviousToExclusive { get; init; }

        public int ProductCount { get; init; }
        public int GrowingCount { get; init; }
        public int StableCount { get; init; }
        public int DecliningCount { get; init; }
        public int InsufficientCount { get; init; }

        public IReadOnlyList<ProductTrendRow> Rows { get; init; }
            = Array.Empty<ProductTrendRow>();
    }
}
