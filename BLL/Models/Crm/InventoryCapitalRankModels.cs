namespace BLL.Models.Crm
{
    /// <summary>Criterios de ranking de capital (FASE 7.12). No hay un único “mejor”.</summary>
    public enum InventoryCapitalRankKind
    {
        ByInventoryCapitalDesc = 0,
        ByImmobilizedCapitalDesc = 1,
        ByAtRiskCapitalDesc = 2,
        ByIdleDaysDesc = 3,
        ByDaysOfCoverDesc = 4,
        ByTurnoverProxyDesc = 5,
        ByTurnoverProxyAsc = 6,
        ByUnitsPerDayDesc = 7,
        ByPotentialProfitDesc = 8
    }

    public sealed class InventoryCapitalRankRow
    {
        public int Rank { get; init; }
        public InventoryCapitalRankKind Kind { get; init; }
        public InventoryFinancialRow Row { get; init; } = null!;
        public string MetricLabel { get; init; } = string.Empty;
        public decimal? MetricValue { get; init; }
    }
}
