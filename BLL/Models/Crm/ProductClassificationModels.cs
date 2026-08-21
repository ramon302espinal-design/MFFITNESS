namespace BLL.Models.Crm
{
    /// <summary>
    /// Clasificación de performance de producto (FASE 8.12).
    /// Star / Opportunity se refinan en 8.13–8.14; aquí la taxonomía + reglas base.
    /// </summary>
    public enum ProductPerformanceClass
    {
        InsufficientData = 0,
        New = 1,
        Healthy = 2,
        Opportunity = 3,
        Slow = 4,
        Critical = 5,
        /// <summary>Asignado solo desde FASE 8.13 (reglas multi-dimensión).</summary>
        Star = 6
    }

    public sealed class ProductClassificationRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public ProductPerformanceClass Class { get; init; }

        /// <summary>Razones explicables (sin score numérico).</summary>
        public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();

        public ProductPerformanceRow? Performance { get; init; }
        public ProductTrendDirection? Trend { get; init; }
    }

    public sealed class ProductClassificationReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public int ProductCount { get; init; }
        public int NewCount { get; init; }
        public int HealthyCount { get; init; }
        public int OpportunityCount { get; init; }
        public int SlowCount { get; init; }
        public int CriticalCount { get; init; }
        public int StarCount { get; init; }
        public int InsufficientCount { get; init; }

        public IReadOnlyList<ProductClassificationRow> Rows { get; init; }
            = Array.Empty<ProductClassificationRow>();
    }
}
