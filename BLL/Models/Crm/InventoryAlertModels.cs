namespace BLL.Models.Crm
{
    /// <summary>Tipos de alerta de capital (FASE 7.11). Sin duplicar anomalías de integridad FASE 4.8.</summary>
    public enum InventoryAlertKind
    {
        CriticalCapital = 1,
        FrozenCapital = 2,
        NeverSold = 3,
        Overstock = 4,
        StockoutRisk = 5,
        AtRiskLoss = 6,
        SlowCapital = 7,
        HighImmobilizedShare = 8
    }

    /// <summary>Prioridad: Critica &gt; Alta &gt; Media &gt; Baja.</summary>
    public enum InventoryAlertPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public sealed class InventoryAlert
    {
        public InventoryAlertKind Kind { get; init; }
        public InventoryAlertPriority Priority { get; init; }
        public int? ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public decimal? CapitalAmount { get; init; }
        public int? IdleDays { get; init; }
        public decimal? DaysOfCover { get; init; }
    }

    public sealed class InventoryAlertReport
    {
        public int TotalAlerts { get; init; }
        public int CriticalCount { get; init; }
        public int HighCount { get; init; }
        public int MediumCount { get; init; }
        public int LowCount { get; init; }

        public decimal ImmobilizedCapital { get; init; }
        public decimal AtRiskCapital { get; init; }
        public decimal? FrozenSharePct { get; init; }

        /// <summary>Orden: prioridad desc, capital desc, nombre.</summary>
        public IReadOnlyList<InventoryAlert> Alerts { get; init; }
            = Array.Empty<InventoryAlert>();
    }
}
