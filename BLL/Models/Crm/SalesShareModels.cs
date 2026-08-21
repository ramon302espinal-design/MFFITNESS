namespace BLL.Models.Crm
{
    /// <summary>Participación de un ítem sobre el total (FASE 9.12).</summary>
    public sealed class SalesShareRow
    {
        public int Rank { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? ProductId { get; init; }
        public int? CategoryId { get; init; }

        public decimal Amount { get; init; }
        public decimal? SharePct { get; init; }

        /// <summary>Participación acumulada hasta este rank (Pareto prep 9.13).</summary>
        public decimal? CumulativeSharePct { get; init; }
    }

    public sealed class SalesShareReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        /// <summary>Ingresos o ganancia según Metric.</summary>
        public string MetricLabel { get; init; } = "Ingresos";

        public decimal TotalAmount { get; init; }
        public int ItemCount { get; init; }

        /// <summary>Σ share de los top N (concentración).</summary>
        public decimal? TopNSharePct { get; init; }
        public int TopN { get; init; }

        public IReadOnlyList<SalesShareRow> Items { get; init; }
            = Array.Empty<SalesShareRow>();
    }

    public enum SalesShareMetric
    {
        Revenue = 0,
        RealizedProfit = 1,
        Units = 2
    }
}
