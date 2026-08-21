namespace BLL.Models.Crm
{
    /// <summary>Dirección de una variación % (FASE 9.5).</summary>
    public enum SalesVariationDirection
    {
        NoComparableBase = 0,
        Flat = 1,
        Up = 2,
        Down = 3
    }

    /// <summary>Fuerza orientativa — umbrales explicables, no score.</summary>
    public enum SalesVariationStrength
    {
        None = 0,
        Mild = 1,
        Strong = 2
    }

    /// <summary>Señal cruzada entre métricas (FASE 9.5 / brief §50–51).</summary>
    public enum SalesCrossSignalKind
    {
        None = 0,

        /// <summary>Ingresos ↑ y ganancia ↓.</summary>
        RevenueUpProfitDown = 1,

        /// <summary>Ingresos ↑ y margen ↓.</summary>
        RevenueUpMarginDown = 2,

        /// <summary>Ingresos ↓ y ganancia ↑ (eficiencia relativa).</summary>
        RevenueDownProfitUp = 3
    }

    public sealed class SalesVariationLabel
    {
        public decimal? VariationPct { get; init; }
        public SalesVariationDirection Direction { get; init; }
        public SalesVariationStrength Strength { get; init; }

        /// <summary>Ej. "+20.00 %", "N/D", "0.00 %".</summary>
        public string Display { get; init; } = "N/D";
    }

    public sealed class SalesCrossSignal
    {
        public SalesCrossSignalKind Kind { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>Lectura de variaciones sobre un SalesComparisonReport.</summary>
    public sealed class SalesVariationReport
    {
        public SalesVariationLabel Revenue { get; init; } = null!;
        public SalesVariationLabel RealizedProfit { get; init; } = null!;
        public SalesVariationLabel Units { get; init; } = null!;
        public SalesVariationLabel Transactions { get; init; } = null!;
        public SalesVariationLabel Ticket { get; init; } = null!;
        public SalesVariationLabel? Margin { get; init; }

        public IReadOnlyList<SalesCrossSignal> CrossSignals { get; init; }
            = Array.Empty<SalesCrossSignal>();
    }
}
