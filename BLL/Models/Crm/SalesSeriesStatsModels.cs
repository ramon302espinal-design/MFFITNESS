namespace BLL.Models.Crm
{
    /// <summary>
    /// Estadísticos de una serie (FASE 9.6). Promedio ≠ mediana.
    /// </summary>
    public sealed class SalesSeriesStats
    {
        public int PointCount { get; init; }
        public decimal Total { get; init; }
        public decimal? Average { get; init; }
        public decimal? Median { get; init; }
        public decimal? Min { get; init; }
        public decimal? Max { get; init; }
    }

    /// <summary>
    /// Promedios/medianas del período a partir de días con operación (FASE 9.6).
    /// </summary>
    public sealed class SalesDailyStatsReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }

        /// <summary>Días calendario del rango (puede &gt; días con venta).</summary>
        public int? CalendarDays { get; init; }

        /// <summary>Días con al menos una transacción.</summary>
        public int OperatingDays { get; init; }

        public SalesSeriesStats Revenue { get; init; } = null!;
        public SalesSeriesStats RealizedProfit { get; init; } = null!;
        public SalesSeriesStats Units { get; init; } = null!;
        public SalesSeriesStats Transactions { get; init; } = null!;

        /// <summary>Total ingresos / días calendario (null si sin días).</summary>
        public decimal? AverageRevenuePerCalendarDay { get; init; }

        /// <summary>Total ingresos / días con operación.</summary>
        public decimal? AverageRevenuePerOperatingDay { get; init; }
    }
}
