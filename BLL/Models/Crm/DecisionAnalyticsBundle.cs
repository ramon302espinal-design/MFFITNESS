namespace BLL.Models.Crm
{
    /// <summary>
    /// Métricas agregadas compartidas para un run del motor (FASE 10.27 / brief §88).
    /// Se cargan una vez; las reglas evalúan en memoria.
    /// </summary>
    public sealed class DecisionAnalyticsBundle
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? AsOf { get; init; }

        public SalesVariationReport? SalesVariation { get; init; }
        public SalesShareReport? SalesShare { get; init; }
        public InventoryAlertReport? InventoryAlerts { get; init; }
        public SalesCapitalBridgeReport? CapitalBridge { get; init; }
        public ProductClassificationReport? ProductClassification { get; init; }
        public SalesStarMixReport? StarMix { get; init; }
        public SalesStockRiskReport? StockRisk { get; init; }
        public SalesAccelerationReport? Acceleration { get; init; }
        public SalesSeriesTrendReport? SeriesTrend { get; init; }
        public SalesForecastReport? Forecast { get; init; }
        public InvestmentCapitalBridgeReport? TrappedCapital { get; init; }
        public IReadOnlyList<InvestmentSummary>? InvestmentSummaries { get; init; }

        public DecisionAnalyticsLoadStats Stats { get; init; } = new();
    }

    /// <summary>Contadores de carga (una llamada por fuente SSOT).</summary>
    public sealed class DecisionAnalyticsLoadStats
    {
        public int ServiceCalls { get; init; }
        public IReadOnlyList<string> SourcesLoaded { get; init; } = Array.Empty<string>();
        public long ElapsedMs { get; init; }
        public string PolicyNote { get; init; } = string.Empty;
    }
}
