using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Carga segura de analítica de ventas FASE 9 (sin lógica financiera en Forms).
    /// </summary>
    public static class CrmSalesUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Money(decimal value)
            => "RD$ " + value.ToString("N2", Cultura);

        public static string MoneyRange(decimal? low, decimal? mid, decimal? high)
        {
            if (!mid.HasValue)
                return "N/D";
            if (low.HasValue && high.HasValue)
                return $"{Money(low.Value)} – {Money(high.Value)} (base {Money(mid.Value)})";
            return Money(mid.Value);
        }

        public static string Pct(decimal? value)
            => value.HasValue ? value.Value.ToString("N2", Cultura) + " %" : "N/D";

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static string VariationDisplay(SalesVariationLabel? label)
            => label?.Display ?? "N/D";

        public static string TrendLabel(SalesSeriesTrendKind kind) => kind switch
        {
            SalesSeriesTrendKind.Growing => "Creciendo",
            SalesSeriesTrendKind.Stable => "Estable",
            SalesSeriesTrendKind.Declining => "Cayendo",
            SalesSeriesTrendKind.Volatile => "Volátil",
            _ => "N/D"
        };

        public static string AccelerationLabel(SalesAccelerationKind kind) => kind switch
        {
            SalesAccelerationKind.Accelerating => "Acelerando",
            SalesAccelerationKind.Decelerating => "Desacelerando",
            SalesAccelerationKind.Steady => "Estable",
            _ => "N/D"
        };

        public static string ConfidenceLabel(SalesForecastConfidence c) => c switch
        {
            SalesForecastConfidence.High => "ALTA",
            SalesForecastConfidence.Medium => "MEDIA",
            SalesForecastConfidence.Low => "BAJA",
            _ => "N/D"
        };

        public static string PeriodLabel(ProfitPeriodKind kind) => kind switch
        {
            ProfitPeriodKind.ThisMonth => "Este mes",
            ProfitPeriodKind.Last30Days => "Últimos 30 días",
            ProfitPeriodKind.Last14Days => "Últimos 14 días",
            ProfitPeriodKind.Last7Days => "Últimos 7 días",
            ProfitPeriodKind.ThisQuarter => "Este trimestre",
            _ => kind.ToString()
        };

        public static SalesDashboardReport? TryLoadDashboard(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int topLists = 5,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            error = null;
            try
            {
                return new SalesDashboardService().GetReport(
                    periodKind,
                    topLists: topLists,
                    customFrom: customFrom,
                    customToExclusive: customToExclusive);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static SalesDecisionReport? TryLoadDecisions(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days)
        {
            error = null;
            try
            {
                return new SalesDecisionService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static SalesSeriesTrendReport? TryLoadSeriesTrend(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days)
        {
            error = null;
            try
            {
                return new SalesSeriesTrendService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static SalesAccelerationReport? TryLoadAcceleration(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days)
        {
            error = null;
            try
            {
                return new SalesAccelerationService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static SalesForecastReport? TryLoadForecast(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days,
            int horizonDays = 30)
        {
            error = null;
            try
            {
                return new SalesForecastService().GetEstimate(periodKind, horizonDays);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static SalesStarMixReport? TryLoadStarMix(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth)
        {
            error = null;
            try
            {
                return new SalesStarMixService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
    }
}
