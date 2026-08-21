namespace BLL.Models.Crm
{
    /// <summary>Área de métrica para decisiones (FASE 10.2).</summary>
    public enum DecisionMetricArea
    {
        Sales = 1,
        Profit = 2,
        Margin = 3,
        Roi = 4,
        Inventory = 5,
        Capital = 6,
        Product = 7,
        Trend = 8,
        Forecast = 9,
        Investment = 10,
        Liquidity = 11,
        Concentration = 12
    }

    /// <summary>Unidad / forma de la métrica (FASE 10.2).</summary>
    public enum DecisionMetricUnit
    {
        Money = 1,
        Percent = 2,
        Count = 3,
        Days = 4,
        Ratio = 5,
        EnumLabel = 6,
        Flag = 7
    }

    /// <summary>Entrada del catálogo de métricas (FASE 10.2). Sin cálculo.</summary>
    public sealed class DecisionMetricDescriptor
    {
        public string Key { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DecisionMetricArea Area { get; init; }
        public DecisionMetricUnit Unit { get; init; }
        public string SourceService { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public bool RequiresComparablePeriod { get; init; }
        public bool AllowsInsufficientData { get; init; } = true;
    }
}
