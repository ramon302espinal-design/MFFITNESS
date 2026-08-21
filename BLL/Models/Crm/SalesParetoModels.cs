namespace BLL.Models.Crm
{
    /// <summary>Análisis Pareto calculado (FASE 9.13). No asume 80/20.</summary>
    public sealed class SalesParetoReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public string MetricLabel { get; init; } = "Ingresos";

        public int ItemCount { get; init; }
        public decimal TotalAmount { get; init; }

        /// <summary>Umbral de participación acumulada buscado (default 80).</summary>
        public decimal TargetCumulativePct { get; init; }

        /// <summary>Ítems necesarios para alcanzar el umbral (datos reales).</summary>
        public int ItemsToReachTarget { get; init; }

        /// <summary>% de ítems = ItemsToReachTarget / ItemCount × 100.</summary>
        public decimal? ItemPctToReachTarget { get; init; }

        /// <summary>Participación acumulada real al cortar (puede ser ≥ target).</summary>
        public decimal? AchievedCumulativePct { get; init; }

        /// <summary>True si se alcanzó el umbral con los datos.</summary>
        public bool TargetReached { get; init; }

        /// <summary>Ej. "20% de productos generan 75% de ingresos" (números reales).</summary>
        public string Summary { get; init; } = string.Empty;

        public IReadOnlyList<SalesShareRow> Items { get; init; }
            = Array.Empty<SalesShareRow>();
    }
}
