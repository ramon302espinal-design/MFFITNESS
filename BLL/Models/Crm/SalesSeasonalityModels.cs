namespace BLL.Models.Crm
{
    /// <summary>Modo de comparación estacional (FASE 9.16).</summary>
    public enum SalesSeasonalityMode
    {
        SameMonthYoY = 1,
        SameWeekYoY = 2,
        SameCalendarDayYoY = 3
    }

    /// <summary>
    /// Banda estacional heurística por mes (no certeza).
    /// No confundir con crecimiento permanente.
    /// </summary>
    public enum SalesSeasonBand
    {
        Low = 0,
        Normal = 1,
        Elevated = 2,
        High = 3
    }

    public sealed class SalesSeasonDayOfWeekRow
    {
        public DayOfWeek DayOfWeek { get; init; }
        public string DayName { get; init; } = string.Empty;
        public int OperatingDays { get; init; }
        public decimal RevenueTotal { get; init; }
        public int TransactionCount { get; init; }
        public decimal? AvgRevenuePerOperatingDay { get; init; }
    }

    public sealed class SalesSeasonalityReport
    {
        public SalesSeasonalityMode Mode { get; init; }
        public DateTime AsOf { get; init; }

        public ProfitPeriodRange CurrentRange { get; init; }
        public ProfitPeriodRange PriorYearRange { get; init; }

        /// <summary>Comparación YoY (mismo mes/semana/día vs año anterior).</summary>
        public SalesComparisonReport YoY { get; init; } = null!;

        /// <summary>
        /// Comparación secuencial (p. ej. mes vs mes anterior). Opcional para contrastar distorsión.
        /// </summary>
        public SalesComparisonReport? Sequential { get; init; }

        public SalesSeasonBand CurrentSeasonBand { get; init; }
        public string SeasonLabel { get; init; } = string.Empty;

        /// <summary>
        /// True si YoY y MoM discrepan en signo de ingresos (posible distorsión estacional).
        /// </summary>
        public bool PossibleSeasonalDistortion { get; init; }

        public string Caution { get; init; } = string.Empty;

        public IReadOnlyList<SalesSeasonDayOfWeekRow> DayOfWeekProfile { get; init; }
            = Array.Empty<SalesSeasonDayOfWeekRow>();
    }
}
