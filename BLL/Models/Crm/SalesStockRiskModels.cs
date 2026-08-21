namespace BLL.Models.Crm
{
    /// <summary>
    /// Señales inventario↔ventas (FASE 9.18).
    /// No ejecuta compra automática.
    /// </summary>
    public enum SalesStockSignalKind
    {
        None = 0,

        /// <summary>🔴 RIESGO DE QUIEBRE — demanda &gt; stock o FlagStockoutRisk FASE 7.</summary>
        StockoutRisk = 1,

        /// <summary>🟡 OPORTUNIDAD DE REABASTECIMIENTO — crece + stock bajo + rotación alta.</summary>
        ReplenishmentOpportunity = 2,

        /// <summary>🔴 RIESGO DE CAPITAL — ventas ↓ + sobreinventario / congelado.</summary>
        CapitalRisk = 3,

        /// <summary>🔥 CRECIMIENTO SALUDABLE — ventas ↑ + stock suficiente + rotación.</summary>
        HealthyGrowth = 4
    }

    public sealed class SalesStockSignalRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public SalesStockSignalKind PrimarySignal { get; init; }
        public IReadOnlyList<SalesStockSignalKind> Signals { get; init; }
            = Array.Empty<SalesStockSignalKind>();

        public int Stock { get; init; }
        public int StockMinimo { get; init; }
        public decimal? UnitsPerDay { get; init; }
        public decimal? TurnoverProxy { get; init; }

        /// <summary>Cobertura FASE 7.7 = Stock / UnitsPerDay.</summary>
        public decimal? DaysOfCover { get; init; }

        /// <summary>Demanda estimada en horizonte (uds) = UnitsPerDay × HorizonDays.</summary>
        public decimal? ProjectedDemandUnits { get; init; }

        public int HorizonDays { get; init; }
        public bool DemandExceedsStock { get; init; }
        public bool FlagStockoutRisk { get; init; }
        public bool FlagOverstock { get; init; }
        public bool IsImmobilized { get; init; }

        public ProductTrendDirection? Trend { get; init; }
        public string DisplayLabel { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }

    public sealed class SalesStockRiskReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public int HorizonDays { get; init; }

        public int ProductCount { get; init; }
        public int StockoutRiskCount { get; init; }
        public int ReplenishmentOpportunityCount { get; init; }
        public int CapitalRiskCount { get; init; }
        public int HealthyGrowthCount { get; init; }

        public IReadOnlyList<SalesStockSignalRow> Rows { get; init; }
            = Array.Empty<SalesStockSignalRow>();

        public IReadOnlyList<SalesStockSignalRow> StockoutRisks { get; init; }
            = Array.Empty<SalesStockSignalRow>();

        public IReadOnlyList<SalesStockSignalRow> ReplenishmentOpportunities { get; init; }
            = Array.Empty<SalesStockSignalRow>();

        public string PolicyNote { get; init; } = string.Empty;
    }
}
