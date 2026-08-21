using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato estacionalidad (FASE 9.16).</summary>
    public static class SalesSeasonalityPolicy
    {
        public const string Definition =
            "FASE 9.16: análisis estacional (mes / semana / día / temporada). " +
            "Comparación primaria = mismo período vs año anterior (ej. ago-2026 vs ago-2025), " +
            "no solo vs mes anterior (jul-2026).";

        public const string VsGrowth =
            "NO confundir estacionalidad con crecimiento permanente. " +
            "Diciembre alto ≠ tendencia Growing permanente.";

        public const string Distortion =
            "Si MoM y YoY discrepan en signo de ingresos → PossibleSeasonalDistortion. " +
            "Preferir lectura YoY para decisiones estacionales.";

        public const string SeasonBand =
            "Banda mensual heurística (High=Dic, Elevated=Nov/Ene). No certeza ni forecast.";
    }

    /// <summary>Resolución y composición estacional pura (FASE 9.16).</summary>
    public static class SalesSeasonalityMath
    {
        private static readonly CultureInfo EsDo = CultureInfo.GetCultureInfo("es-DO");

        public static ProfitPeriodRange ResolveSameMonth(DateTime asOf)
        {
            DateTime d = asOf.Date;
            var from = new DateTime(d.Year, d.Month, 1);
            return new ProfitPeriodRange(from, from.AddMonths(1));
        }

        public static ProfitPeriodRange ResolveSameMonthPriorYear(DateTime asOf)
        {
            DateTime d = asOf.Date;
            var from = new DateTime(d.Year - 1, d.Month, 1);
            return new ProfitPeriodRange(from, from.AddMonths(1));
        }

        public static (ProfitPeriodRange Current, ProfitPeriodRange PriorYear) ResolveSameMonthYoY(
            DateTime asOf)
            => (ResolveSameMonth(asOf), ResolveSameMonthPriorYear(asOf));

        /// <summary>Semana lun–dom que contiene asOf vs misma ventana −1 año.</summary>
        public static (ProfitPeriodRange Current, ProfitPeriodRange PriorYear) ResolveSameWeekYoY(
            DateTime asOf)
        {
            DateTime weekStart = StartOfWeek(asOf.Date);
            var current = new ProfitPeriodRange(weekStart, weekStart.AddDays(7));
            DateTime priorStart = weekStart.AddYears(-1);
            var prior = new ProfitPeriodRange(priorStart, priorStart.AddDays(7));
            return (current, prior);
        }

        public static (ProfitPeriodRange Current, ProfitPeriodRange PriorYear) ResolveSameCalendarDayYoY(
            DateTime asOf)
        {
            DateTime d = asOf.Date;
            var current = new ProfitPeriodRange(d, d.AddDays(1));
            DateTime prior = d.AddYears(-1);
            var priorRange = new ProfitPeriodRange(prior, prior.AddDays(1));
            return (current, priorRange);
        }

        public static SalesSeasonBand ClassifyMonthBand(int month)
            => month switch
            {
                12 => SalesSeasonBand.High,
                11 or 1 => SalesSeasonBand.Elevated,
                2 or 3 or 4 => SalesSeasonBand.Low,
                _ => SalesSeasonBand.Normal
            };

        public static string SeasonLabel(int month, SalesSeasonBand band)
        {
            string monthName = EsDo.DateTimeFormat.GetMonthName(month);
            return band switch
            {
                SalesSeasonBand.High => $"{monthName}: temporada alta (heurística)",
                SalesSeasonBand.Elevated => $"{monthName}: temporada elevada (heurística)",
                SalesSeasonBand.Low => $"{monthName}: temporada baja (heurística)",
                _ => $"{monthName}: temporada normal (heurística)"
            };
        }

        public static bool DetectSeasonalDistortion(
            decimal? yoyRevenueVariationPct,
            decimal? sequentialRevenueVariationPct)
        {
            if (!yoyRevenueVariationPct.HasValue || !sequentialRevenueVariationPct.HasValue)
                return false;

            // Signos opuestos con movimiento material (>2%)
            if (Math.Abs(yoyRevenueVariationPct.Value) <= 2m
                || Math.Abs(sequentialRevenueVariationPct.Value) <= 2m)
                return false;

            return Math.Sign(yoyRevenueVariationPct.Value)
                   != Math.Sign(sequentialRevenueVariationPct.Value);
        }

        public static IReadOnlyList<SalesSeasonDayOfWeekRow> BuildDayOfWeekProfile(
            IReadOnlyList<ProfitDayRow> days)
        {
            var operating = days.Where(d => d.TransactionCount > 0).ToList();
            var rows = new List<SalesSeasonDayOfWeekRow>();

            foreach (DayOfWeek dow in Enum.GetValues<DayOfWeek>())
            {
                var subset = operating.Where(d => d.Date.DayOfWeek == dow).ToList();
                decimal revenue = subset.Sum(d => d.RevenueTotal);
                int txns = subset.Sum(d => d.TransactionCount);
                int n = subset.Count;

                rows.Add(new SalesSeasonDayOfWeekRow
                {
                    DayOfWeek = dow,
                    DayName = EsDo.DateTimeFormat.GetDayName(dow),
                    OperatingDays = n,
                    RevenueTotal = revenue,
                    TransactionCount = txns,
                    AvgRevenuePerOperatingDay = n > 0
                        ? InventoryFinancialMath.RoundMoney(revenue / n)
                        : null
                });
            }

            // Lunes → domingo
            return rows
                .OrderBy(r => ((int)r.DayOfWeek + 6) % 7)
                .ToList();
        }

        public static SalesSeasonalityReport Compose(
            SalesSeasonalityMode mode,
            DateTime asOf,
            ProfitPeriodRange currentRange,
            ProfitPeriodRange priorYearRange,
            SalesSummary current,
            SalesSummary priorYear,
            SalesComparisonReport? sequential = null,
            IReadOnlyList<SalesSeasonDayOfWeekRow>? dayOfWeekProfile = null)
        {
            int month = asOf.Month;
            SalesSeasonBand band = ClassifyMonthBand(month);
            var yoy = SalesComparisonComposer.Build(
                ProfitPeriodKind.Custom,
                currentRange,
                priorYearRange,
                current,
                priorYear);

            bool distortion = DetectSeasonalDistortion(
                yoy.Revenue.VariationPct,
                sequential?.Revenue.VariationPct);

            return new SalesSeasonalityReport
            {
                Mode = mode,
                AsOf = asOf.Date,
                CurrentRange = currentRange,
                PriorYearRange = priorYearRange,
                YoY = yoy,
                Sequential = sequential,
                CurrentSeasonBand = band,
                SeasonLabel = SeasonLabel(month, band),
                PossibleSeasonalDistortion = distortion,
                Caution = SalesSeasonalityPolicy.VsGrowth,
                DayOfWeekProfile = dayOfWeekProfile ?? Array.Empty<SalesSeasonDayOfWeekRow>()
            };
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}
