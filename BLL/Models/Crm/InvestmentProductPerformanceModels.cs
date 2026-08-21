namespace BLL.Models.Crm
{
    /// <summary>
    /// Producto de inversión + performance FASE 8 (8.16).
    /// </summary>
    public sealed class InvestmentProductPerformanceRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;

        /// <summary>Capital atribuido en la inversión (FIFO FASE 6).</summary>
        public decimal CapitalAssignedInInvestment { get; init; }

        public ProductPerformanceClass Class { get; init; }
        public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();

        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public decimal ProductInventoryCapital { get; init; }
        public bool IsStar => Class == ProductPerformanceClass.Star;
        public bool IsOpportunity => Class == ProductPerformanceClass.Opportunity;
        public bool IsRisk => Class == ProductPerformanceClass.Critical;
    }

    public sealed class InvestmentPerformanceBridgeRow
    {
        public int Rank { get; init; }
        public InvestmentSummary Summary { get; init; } = null!;

        public int ProductsLinked { get; init; }
        public int StarsCount { get; init; }
        public int OpportunityCount { get; init; }
        public int RiskCount { get; init; }

        /// <summary>Σ ganancia realizada del período de productos vinculados.</summary>
        public decimal LinkedPeriodProfit { get; init; }

        public IReadOnlyList<InvestmentProductPerformanceRow> Products { get; init; }
            = Array.Empty<InvestmentProductPerformanceRow>();
    }

    public sealed class InvestmentPerformanceBridgeReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public int InvestmentCount { get; init; }
        public int TotalStarsAcrossInvestments { get; init; }
        public int TotalRisksAcrossInvestments { get; init; }
        public decimal TotalLinkedPeriodProfit { get; init; }

        public IReadOnlyList<InvestmentPerformanceBridgeRow> Investments { get; init; }
            = Array.Empty<InvestmentPerformanceBridgeRow>();
    }
}
