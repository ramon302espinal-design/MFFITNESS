namespace BLL.Models.Crm
{
    /// <summary>Señales ventas ↔ capital (FASE 9.19). ≠ score.</summary>
    public enum SalesCapitalSignalKind
    {
        None = 0,

        /// <summary>Ingresos ↑ y ROI ↓ (§52) — más capital sin retorno proporcional.</summary>
        RevenueUpRoiDown = 1,

        /// <summary>Ingresos ↑ y ganancia ↓ (§50) con contexto de capital.</summary>
        RevenueUpProfitDown = 2,

        /// <summary>Declining + capital inmovilizado / overstock (§48).</summary>
        CapitalRisk = 3,

        /// <summary>Quiebre con capital aún atado al SKU (operativo).</summary>
        StockoutWithCapital = 4
    }

    public sealed class SalesCapitalSignal
    {
        public SalesCapitalSignalKind Kind { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>Producto: ventas del período + capital snapshot (FASE 9.19).</summary>
    public sealed class SalesCapitalBridgeRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public decimal? RevenueChangePct { get; init; }
        public decimal? ProfitChangePct { get; init; }
        public decimal? RoiChangePct { get; init; }

        public int Stock { get; init; }
        public decimal InventoryCapital { get; init; }
        public decimal ImmobilizedCapital { get; init; }
        public InventoryHealthStatus HealthStatus { get; init; }
        public bool IsImmobilized => ImmobilizedCapital > 0m;

        public ProductTrendDirection? Trend { get; init; }
        public SalesStockSignalKind? StockSignal { get; init; }

        public IReadOnlyList<SalesCapitalSignal> Signals { get; init; }
            = Array.Empty<SalesCapitalSignal>();

        public SalesCapitalSignalKind PrimarySignal { get; init; }
    }

    public sealed class SalesCapitalBridgeReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public decimal TotalRevenue { get; init; }
        public decimal TotalRealizedProfit { get; init; }
        public decimal TotalInventoryCapital { get; init; }
        public decimal TotalImmobilizedCapital { get; init; }

        /// <summary>Ganancia período / capital inventario (null si capital ≤ 0). Etiqueta: eficiencia, no ROI FASE 6.</summary>
        public decimal? PeriodProfitOnInventoryCapitalPct { get; init; }

        public int RevenueUpRoiDownCount { get; init; }
        public int RevenueUpProfitDownCount { get; init; }
        public int CapitalRiskCount { get; init; }
        public int StockoutWithCapitalCount { get; init; }

        /// <summary>Capital inventario en productos con señal CapitalRisk.</summary>
        public decimal CapitalAtRisk { get; init; }

        public IReadOnlyList<SalesCapitalBridgeRow> Rows { get; init; }
            = Array.Empty<SalesCapitalBridgeRow>();

        public IReadOnlyList<SalesCapitalBridgeRow> Flagged { get; init; }
            = Array.Empty<SalesCapitalBridgeRow>();

        public string Caution { get; init; } = string.Empty;
    }
}
