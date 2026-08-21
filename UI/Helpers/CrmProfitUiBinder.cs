using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Carga segura del motor de ganancias (FASE 5.11).
    /// Forms solo muestran; sin SQL ni fórmulas.
    /// </summary>
    public static class CrmProfitUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Money(decimal value)
            => "RD$ " + value.ToString("N2", Cultura);

        public static string Pct(decimal? value)
            => value.HasValue
                ? value.Value.ToString("N2", Cultura) + " %"
                : "N/D";

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static ProfitSummary? TryLoadSummary(
            ProfitPeriodKind kind,
            out string? error,
            DateTime? asOf = null)
        {
            try
            {
                error = null;
                return new ProfitAnalyticsService().GetForPeriod(kind, asOf);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static ProfitSummary? TryLoadAllTime(out string? error)
            => TryLoadSummary(ProfitPeriodKind.AllTime, out error);

        public static ProfitSummary? TryLoadThisMonth(out string? error)
            => TryLoadSummary(ProfitPeriodKind.ThisMonth, out error);

        public static IReadOnlyList<ProfitGroupRow>? TryLoadByProduct(
            ProfitPeriodKind kind,
            out string? error,
            int? top = 15)
        {
            try
            {
                error = null;
                return new ProfitAnalyticsService().GetByProduct(kind, top: top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProfitGroupRow>? TryLoadByCategory(
            ProfitPeriodKind kind,
            out string? error,
            int? top = 15)
        {
            try
            {
                error = null;
                return new ProfitAnalyticsService().GetByCategory(kind, top: top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProfitDayRow>? TryLoadByDay(
            ProfitPeriodKind kind,
            out string? error)
        {
            try
            {
                error = null;
                return new ProfitAnalyticsService().GetByDay(kind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string PolicyNote()
        {
            try
            {
                return new ProfitAnalyticsService().GetVoidAndReturnPolicyNote();
            }
            catch
            {
                return ProfitVoidAndReturnPolicy.DescribeForUi();
            }
        }
    }
}
