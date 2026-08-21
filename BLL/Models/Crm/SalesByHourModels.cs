namespace BLL.Models.Crm
{
    /// <summary>Bucket horario 0–23 (FASE 9.8).</summary>
    public sealed class SalesHourRow
    {
        /// <summary>Hora inicio (0 = 00:00–01:00, …, 23 = 23:00–24:00).</summary>
        public int Hour { get; init; }

        public string Label => $"{Hour:00}:00–{(Hour + 1) % 24:00}:00";

        public int TransactionCount { get; init; }
        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? AverageTicket { get; init; }
        public bool HasReliableRealizedProfit { get; init; }
    }

    public sealed class SalesByHourReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }

        /// <summary>
        /// False si no hay datos o toda la actividad cae en hora 0
        /// (posible Fecha truncada a medianoche — no inventar picos).
        /// </summary>
        public bool HourDataReliable { get; init; }

        public string ReliabilityNote { get; init; } = string.Empty;

        public IReadOnlyList<SalesHourRow> Hours { get; init; }
            = Array.Empty<SalesHourRow>();

        public SalesHourRow? PeakByRevenue { get; init; }
        public SalesHourRow? PeakByTransactions { get; init; }
        public SalesHourRow? PeakByUnits { get; init; }
    }
}
