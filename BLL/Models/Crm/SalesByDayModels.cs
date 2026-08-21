namespace BLL.Models.Crm
{
    /// <summary>Día de ventas enriquecido (FASE 9.7).</summary>
    public sealed class SalesDayRow
    {
        public DateTime Date { get; init; }
        public int TransactionCount { get; init; }
        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? AverageTicket { get; init; }
        public bool HasReliableRealizedProfit { get; init; }
    }

    /// <summary>Informe de ventas por día + extremos (FASE 9.7).</summary>
    public sealed class SalesByDayReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }

        public int OperatingDayCount { get; init; }

        /// <summary>Ordenados por fecha ascendente (solo días con operación).</summary>
        public IReadOnlyList<SalesDayRow> Days { get; init; }
            = Array.Empty<SalesDayRow>();

        /// <summary>Mayor ingreso (entre días con operación).</summary>
        public SalesDayRow? BestDayByRevenue { get; init; }

        /// <summary>Mayor ganancia realizada (puede ≠ mejor por ingreso).</summary>
        public SalesDayRow? BestDayByProfit { get; init; }

        /// <summary>Menor ingreso (días con operación; excluye días sin venta).</summary>
        public SalesDayRow? WorstDayByRevenue { get; init; }

        public SalesDayRow? WorstDayByProfit { get; init; }
    }
}
