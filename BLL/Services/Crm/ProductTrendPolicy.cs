namespace BLL.Services.Crm
{
    /// <summary>Contrato de tendencia de producto (FASE 8.11).</summary>
    public static class ProductTrendPolicy
    {
        public const string TrendDefinition =
            "Tendencia (FASE 8.11) = comparar período actual vs período base (MoM / ventana previa). " +
            "PrimaryTrend = unidades. RevenueTrend es paralelo. " +
            "Growing / Stable / Declining / InsufficientData. Sin score.";

        public const string StableBandDefinition =
            "Banda estable default = ±10% de cambio relativo. " +
            "Configurable en ProductTrendThresholds.StableBandPct.";

        public const string InsufficientDefinition =
            "InsufficientData si ambos períodos tienen 0 unidades (o 0 ingresos para RevenueTrend). " +
            "No inventar tendencia sin actividad.";

        public const string AccelerationNote =
            "Aceleración/desaceleración: MoM de 2 puntos → Acceleration=Unknown. " +
            "Motor multi-período = SalesAccelerationMath (FASE 9.15). Sin score.";
        public const string PeriodPairNote =
            "Pares soportados: ThisMonth↔PreviousMonth, Last30Days↔30d previos, " +
            "Last7Days↔7d previos, Today↔Yesterday. Otros → no calcular (vacío/error de política).";
    }

    /// <summary>Umbrales de tendencia (FASE 8.11).</summary>
    public sealed class ProductTrendThresholds
    {
        public static ProductTrendThresholds Default { get; } = new();

        /// <summary>|changePct| ≤ este valor → Stable.</summary>
        public decimal StableBandPct { get; init; } = 10m;
    }
}
