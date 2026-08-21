namespace BLL.Models.Crm
{
    public enum InventoryFinancialAnomalyCode
    {
        NegativeStock = 1,
        CostNotDefined = 2,
        PriceNotDefined = 3,
        UncostedSales = 4,
        NoRotationWithStock = 5,
        BelowStockMinimum = 6
    }

    public enum InventoryFinancialAnomalySeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }

    public sealed class InventoryFinancialAnomaly
    {
        public InventoryFinancialAnomalyCode Code { get; init; }
        public InventoryFinancialAnomalySeverity Severity { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public decimal? Stock { get; init; }
        public decimal? UnitCost { get; init; }
        public decimal? SalePrice { get; init; }
        public decimal? FrozenCapitalAtRisk { get; init; }
    }

    public sealed class InventoryFinancialValidationReport
    {
        public int TotalAnomalies { get; init; }
        public int CriticalCount { get; init; }
        public int WarningCount { get; init; }
        public int InfoCount { get; init; }

        public bool HasBlockingIssues { get; init; }

        public IReadOnlyList<InventoryFinancialAnomaly> Anomalies { get; init; }
            = Array.Empty<InventoryFinancialAnomaly>();
    }
}
