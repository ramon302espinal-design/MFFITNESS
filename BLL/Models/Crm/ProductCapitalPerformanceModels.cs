namespace BLL.Models.Crm
{
    /// <summary>
    /// Producto con capital + clase FASE 8 (8.17).
    /// ImmobilizedCapital sigue regla FASE 7.9 / 8.10.
    /// </summary>
    public sealed class ProductCapitalClassRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public ProductPerformanceClass Class { get; init; }
        public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();

        /// <summary>Salud de capital FASE 7 (independiente de Class).</summary>
        public InventoryHealthStatus HealthStatus { get; init; }

        public int Stock { get; init; }
        public decimal InventoryCapital { get; init; }
        public decimal ImmobilizedCapital { get; init; }
        public decimal PotentialProfit { get; init; }
        public int? IdleDays { get; init; }

        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public bool IsImmobilized => ImmobilizedCapital > 0m;
    }

    /// <summary>Bucket de capital por clase de performance.</summary>
    public sealed class ProductCapitalClassBucket
    {
        public ProductPerformanceClass Class { get; init; }
        public int ProductCount { get; init; }
        public decimal InventoryCapital { get; init; }
        public decimal ImmobilizedCapital { get; init; }
        public decimal PeriodProfit { get; init; }
    }

    /// <summary>Informe puente capital ↔ performance (FASE 8.17).</summary>
    public sealed class ProductCapitalPerformanceReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public decimal TotalInventoryCapital { get; init; }

        /// <summary>Σ ImmobilizedCapital (Frozen∪Critical FASE 7) — ≠ TotalInventoryCapital.</summary>
        public decimal TotalImmobilizedCapital { get; init; }

        public decimal StarCapital { get; init; }
        public decimal OpportunityCapital { get; init; }
        public decimal CriticalClassCapital { get; init; }
        public decimal SlowCapital { get; init; }
        public decimal HealthyCapital { get; init; }
        public decimal NewCapital { get; init; }

        public IReadOnlyList<ProductCapitalClassBucket> Buckets { get; init; }
            = Array.Empty<ProductCapitalClassBucket>();

        /// <summary>Top por ImmobilizedCapital desc, con Class FASE 8 etiquetada.</summary>
        public IReadOnlyList<ProductCapitalClassRow> TopImmobilized { get; init; }
            = Array.Empty<ProductCapitalClassRow>();

        public IReadOnlyList<ProductCapitalClassRow> Products { get; init; }
            = Array.Empty<ProductCapitalClassRow>();
    }
}
