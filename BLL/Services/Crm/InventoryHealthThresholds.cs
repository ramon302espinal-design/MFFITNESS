namespace BLL.Services.Crm
{
    /// <summary>
    /// Umbrales configurables de salud de capital (FASE 7.8).
    /// Defaults alineados a la propuesta aprobada en 7.1.
    /// </summary>
    public sealed class InventoryHealthThresholds
    {
        public static InventoryHealthThresholds Default { get; } = new();

        /// <summary>Días desde 1ª ENTRADA sin marcar congelado.</summary>
        public int NewProductGraceDays { get; init; } = 14;

        public int HealthyIdleDaysMax { get; init; } = 7;
        public int SlowIdleDaysMax { get; init; } = 30;

        public int HealthyCoverDaysMax { get; init; } = 30;
        public int SlowCoverDaysMax { get; init; } = 60;

        /// <summary>Cobertura ≥ este valor contribuye a Frozen (con materialidad).</summary>
        public int FrozenCoverDaysMin { get; init; } = 90;

        public int CriticalNeverSoldDays { get; init; } = 60;

        /// <summary>Capital mínimo para Frozen/Critical (evita alertar RD$500).</summary>
        public decimal MinMaterialCapital { get; init; } = 1_000m;

        /// <summary>Capital que agrava Frozen → Critical.</summary>
        public decimal CriticalCapitalMin { get; init; } = 10_000m;
    }
}
