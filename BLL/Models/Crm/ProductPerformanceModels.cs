namespace BLL.Models.Crm
{
    /// <summary>
    /// Criterios de ranking de performance (FASE 8.2 contrato; ordenación en 8.3+).
    /// Cada ranking usa UNA métrica — nunca mezclar sin etiquetar.
    /// </summary>
    public enum ProductPerformanceMetricKind
    {
        UnitsSold = 0,
        Revenue = 1,
        RealizedProfit = 2,
        MarginPct = 3,
        RoiPct = 4,
        TurnoverProxy = 5,
        InventoryCapital = 6,
        ImmobilizedCapital = 7,
        PotentialProfit = 8,
        UnitsPerDay = 9
    }

    /// <summary>
    /// Métricas base por producto (FASE 8.2). Sin score compuesto.
    /// P&amp;L = período; capital/rotación/salud = snapshot.
    /// </summary>
    public sealed class ProductPerformanceRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        // --- IMPACTO (período) ---
        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal Cogs { get; init; }
        public decimal RevenueWithCost { get; init; }

        // --- EFICIENCIA (período / snapshot) ---
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }
        public decimal? UnitsPerDay { get; init; }
        public decimal? TurnoverProxy { get; init; }

        // --- CAPITAL / POTENCIAL (snapshot) ---
        public int Stock { get; init; }
        public decimal InventoryCapital { get; init; }
        public decimal ImmobilizedCapital { get; init; }
        public decimal PotentialSalesValue { get; init; }
        public decimal PotentialProfit { get; init; }

        // --- RIESGO / CONTEXTO (snapshot) ---
        public InventoryHealthStatus HealthStatus { get; init; }
        public int? IdleDays { get; init; }
        public bool FlagStockoutRisk { get; init; }
        public bool FlagOverstock { get; init; }

        public bool HasReliableRealizedProfit { get; init; }
        public bool HasPeriodActivity { get; init; }
        public bool HasInventorySnapshot { get; init; }

        /// <summary>True si Frozen o Critical con capital &gt; 0.</summary>
        public bool IsImmobilized { get; init; }
    }

    /// <summary>Informe de métricas base (FASE 8.2).</summary>
    public sealed class ProductPerformanceReport
    {
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }
        public ProfitPeriodKind PeriodKind { get; init; }

        public int ProductCount { get; init; }
        public int ProductsWithPeriodActivity { get; init; }
        public int ProductsWithImmobilizedCapital { get; init; }

        public decimal TotalUnitsSold { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal TotalRealizedProfit { get; init; }
        public decimal TotalInventoryCapital { get; init; }
        public decimal TotalImmobilizedCapital { get; init; }

        public IReadOnlyList<ProductPerformanceRow> Rows { get; init; }
            = Array.Empty<ProductPerformanceRow>();
    }

    /// <summary>Fila de ranking etiquetada (FASE 8.3+). Una métrica por lista.</summary>
    public sealed class ProductPerformanceRankRow
    {
        public int Rank { get; init; }
        public ProductPerformanceMetricKind Kind { get; init; }
        public ProductPerformanceRow Row { get; init; } = null!;
        public string MetricLabel { get; init; } = string.Empty;
        public decimal? MetricValue { get; init; }
    }
}
