using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato forecast / estimación (FASE 9.17).</summary>
    public static class SalesForecastPolicy
    {
        public const string Language =
            "FASE 9.17: toda proyección es ESTIMACIÓN / ESCENARIO. " +
            "Nunca presentar como certeza ni probabilidad exacta.";

        public const string Simple =
            "Proyección simple = promedio diario histórico (días con operación) × horizonte. " +
            "Ej.: RD$10,000/día × 30 = RD$300,000.";

        public const string Trend =
            "Ajuste por tendencia 9.14 (Growing/Stable/Declining/Volatile). " +
            "Sin modelos complejos.";

        public const string Scenarios =
            "Siempre Low / Base / High. No mostrar un único número como 'ventas futuras'.";

        public const string Confidence =
            "Confianza ALTA/MEDIA/BAJA según cantidad de datos, CV y consistencia. " +
            "NO inventar probabilidades.";

        public const string Profit =
            "Ganancia estimada = ingresos proyectados × margen histórico % (si confiable). " +
            "Etiquetar ESTIMADA.";
    }

    public sealed class SalesForecastThresholds
    {
        public static SalesForecastThresholds Default { get; } = new();

        public int MinOperatingDays { get; init; } = 4;
        public int MediumOperatingDays { get; init; } = 10;
        public int HighOperatingDays { get; init; } = 20;

        public decimal HighMaxCvPct { get; init; } = 25m;
        public decimal MediumMaxCvPct { get; init; } = 40m;

        public decimal GrowingFactor { get; init; } = 1.10m;
        public decimal DecliningFactor { get; init; } = 0.90m;

        /// <summary>Escenario bajo = base × LowFactor (brief ≈ 280/320).</summary>
        public decimal LowFactor { get; init; } = 0.875m;

        /// <summary>Escenario alto = base × HighFactor (brief ≈ 370/320).</summary>
        public decimal HighFactor { get; init; } = 1.15625m;

        /// <summary>Si Volatile, ensanchar banda.</summary>
        public decimal VolatileLowFactor { get; init; } = 0.80m;
        public decimal VolatileHighFactor { get; init; } = 1.25m;
    }

    /// <summary>Motor puro de estimación (FASE 9.17).</summary>
    public static class SalesForecastMath
    {
        public static decimal TrendFactor(
            SalesSeriesTrendKind trend,
            SalesForecastThresholds? thresholds = null)
        {
            SalesForecastThresholds t = thresholds ?? SalesForecastThresholds.Default;
            return trend switch
            {
                SalesSeriesTrendKind.Growing => t.GrowingFactor,
                SalesSeriesTrendKind.Declining => t.DecliningFactor,
                _ => 1.00m
            };
        }

        public static SalesForecastConfidence ClassifyConfidence(
            int operatingDays,
            decimal? cvPct,
            SalesSeriesTrendKind trend,
            SalesForecastThresholds? thresholds = null)
        {
            SalesForecastThresholds t = thresholds ?? SalesForecastThresholds.Default;

            if (operatingDays < t.MinOperatingDays)
                return SalesForecastConfidence.InsufficientData;

            if (trend is SalesSeriesTrendKind.Volatile or SalesSeriesTrendKind.InsufficientData)
                return SalesForecastConfidence.Low;

            if (operatingDays >= t.HighOperatingDays
                && cvPct.HasValue
                && cvPct.Value <= t.HighMaxCvPct)
                return SalesForecastConfidence.High;

            if (operatingDays >= t.MediumOperatingDays
                && (!cvPct.HasValue || cvPct.Value <= t.MediumMaxCvPct))
                return SalesForecastConfidence.Medium;

            return SalesForecastConfidence.Low;
        }

        public static string ConfidenceReason(
            SalesForecastConfidence confidence,
            int operatingDays,
            decimal? cvPct,
            SalesSeriesTrendKind trend)
        {
            return confidence switch
            {
                SalesForecastConfidence.InsufficientData =>
                    $"Datos insuficientes ({operatingDays} días con operación)",
                SalesForecastConfidence.High =>
                    $"ALTA: {operatingDays} días, CV {(cvPct?.ToString("N0") ?? "N/D")}% , tendencia {trend}",
                SalesForecastConfidence.Medium =>
                    $"MEDIA: {operatingDays} días, CV {(cvPct?.ToString("N0") ?? "N/D")}%",
                _ =>
                    $"BAJA: {operatingDays} días, CV {(cvPct?.ToString("N0") ?? "N/D")}%, tendencia {trend}"
            };
        }

        public static SalesForecastScenario Scenario(
            string key,
            string label,
            decimal revenue,
            decimal? marginPct)
        {
            decimal rev = InventoryFinancialMath.RoundMoney(revenue);
            decimal? profit = null;
            if (marginPct.HasValue)
            {
                profit = InventoryFinancialMath.RoundMoney(rev * marginPct.Value / 100m);
            }

            return new SalesForecastScenario
            {
                Key = key,
                Label = label,
                EstimatedRevenue = rev,
                EstimatedProfit = profit
            };
        }

        public static SalesForecastReport Build(
            IReadOnlyList<decimal> operatingDayRevenues,
            int horizonDays,
            SalesSeriesTrendResult? trendResult = null,
            decimal? historicalMarginPct = null,
            ProfitPeriodKind sourcePeriodKind = ProfitPeriodKind.Last30Days,
            SalesForecastThresholds? thresholds = null)
        {
            SalesForecastThresholds t = thresholds ?? SalesForecastThresholds.Default;
            int n = operatingDayRevenues?.Count ?? 0;
            SalesSeriesTrendKind trend = trendResult?.Kind ?? SalesSeriesTrendKind.InsufficientData;
            decimal? cv = trendResult?.CoefficientOfVariationPct
                          ?? SalesSeriesTrendMath.CoefficientOfVariationPct(
                              operatingDayRevenues ?? Array.Empty<decimal>());

            if (n == 0 || horizonDays <= 0)
            {
                return Empty(sourcePeriodKind, horizonDays, trend, n, cv);
            }

            decimal avg = operatingDayRevenues!.Average();
            decimal simple = avg * horizonDays;
            decimal factor = TrendFactor(trend, t);
            decimal baseRev = simple * factor;

            decimal lowF = trend == SalesSeriesTrendKind.Volatile ? t.VolatileLowFactor : t.LowFactor;
            decimal highF = trend == SalesSeriesTrendKind.Volatile ? t.VolatileHighFactor : t.HighFactor;

            var confidence = ClassifyConfidence(n, cv, trend, t);

            return new SalesForecastReport
            {
                SourcePeriodKind = sourcePeriodKind,
                HorizonDays = horizonDays,
                OperatingDaysUsed = n,
                HistoricalDailyAverageRevenue = InventoryFinancialMath.RoundMoney(avg),
                HistoricalMarginPct = historicalMarginPct,
                TrendUsed = trend,
                TrendAdjustmentFactor = factor,
                SimpleProjectionRevenue = InventoryFinancialMath.RoundMoney(simple),
                Low = Scenario("low", "Escenario bajo (estimación)", baseRev * lowF, historicalMarginPct),
                Base = Scenario("base", "Escenario base (estimación)", baseRev, historicalMarginPct),
                High = Scenario("high", "Escenario alto (estimación)", baseRev * highF, historicalMarginPct),
                Confidence = confidence,
                ConfidenceReason = ConfidenceReason(confidence, n, cv, trend),
                LanguageNote = SalesForecastPolicy.Language
            };
        }

        public static SalesForecastReport FromDays(
            IReadOnlyList<ProfitDayRow> days,
            int horizonDays = 30,
            decimal? historicalMarginPct = null,
            ProfitPeriodKind sourcePeriodKind = ProfitPeriodKind.Last30Days,
            SalesForecastThresholds? thresholds = null)
        {
            var operating = days
                .Where(d => d.TransactionCount > 0)
                .OrderBy(d => d.Date)
                .Select(d => d.RevenueTotal)
                .ToList();

            var trend = SalesSeriesTrendMath.Classify(operating, thresholds: null);
            return Build(
                operating,
                horizonDays,
                trend,
                historicalMarginPct,
                sourcePeriodKind,
                thresholds);
        }

        private static SalesForecastReport Empty(
            ProfitPeriodKind sourcePeriodKind,
            int horizonDays,
            SalesSeriesTrendKind trend,
            int n,
            decimal? cv)
        {
            return new SalesForecastReport
            {
                SourcePeriodKind = sourcePeriodKind,
                HorizonDays = horizonDays,
                OperatingDaysUsed = n,
                TrendUsed = trend,
                TrendAdjustmentFactor = 1m,
                Low = Scenario("low", "Escenario bajo (estimación)", 0m, null),
                Base = Scenario("base", "Escenario base (estimación)", 0m, null),
                High = Scenario("high", "Escenario alto (estimación)", 0m, null),
                Confidence = SalesForecastConfidence.InsufficientData,
                ConfidenceReason = ConfidenceReason(
                    SalesForecastConfidence.InsufficientData, n, cv, trend),
                LanguageNote = SalesForecastPolicy.Language
            };
        }
    }
}
