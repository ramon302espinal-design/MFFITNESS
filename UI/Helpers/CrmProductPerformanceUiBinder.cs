using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Carga segura de performance FASE 8 (sin lógica financiera en Forms).
    /// </summary>
    public static class CrmProductPerformanceUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Money(decimal value)
            => "RD$ " + value.ToString("N2", Cultura);

        public static string Pct(decimal? value)
            => value.HasValue ? value.Value.ToString("N2", Cultura) + " %" : "N/D";

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static string ClassLabel(ProductPerformanceClass cls) => cls switch
        {
            ProductPerformanceClass.Star => "Estrella",
            ProductPerformanceClass.Healthy => "Saludable",
            ProductPerformanceClass.Opportunity => "Oportunidad",
            ProductPerformanceClass.Slow => "Lento",
            ProductPerformanceClass.Critical => "Crítico",
            ProductPerformanceClass.New => "Nuevo",
            _ => "N/D"
        };

        public static string TrendLabel(ProductTrendDirection d) => d switch
        {
            ProductTrendDirection.Growing => "Creciendo",
            ProductTrendDirection.Stable => "Estable",
            ProductTrendDirection.Declining => "Cayendo",
            _ => "N/D"
        };

        public static ProductPerformanceDashboardReport? TryLoadDashboard(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int topLists = 5)
        {
            try
            {
                error = null;
                return new ProductPerformanceDashboardService().GetReport(periodKind, topLists: topLists);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static ProductClassificationReport? TryLoadClassification(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth)
        {
            try
            {
                error = null;
                return new ProductClassificationService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProductClassificationRow>? TryLoadStars(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 15)
        {
            try
            {
                error = null;
                return new ProductClassificationService().GetStars(periodKind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProductClassificationRow>? TryLoadOpportunities(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10)
        {
            try
            {
                error = null;
                return new ProductClassificationService().GetOpportunities(periodKind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProductClassificationRow>? TryLoadRisks(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10)
        {
            try
            {
                error = null;
                return new ProductClassificationService().GetRisks(periodKind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<ProductPerformanceRankRow>? TryLoadRanking(
            ProductPerformanceMetricKind kind,
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 5)
        {
            try
            {
                error = null;
                return new ProductPerformanceService().GetRanking(kind, periodKind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static ProductTrendReport? TryLoadTrends(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth)
        {
            try
            {
                error = null;
                return new ProductTrendService().GetTrends(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static ProductCapitalPerformanceReport? TryLoadCapitalByClass(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth)
        {
            try
            {
                error = null;
                return new ProductCapitalPerformanceBridgeService().GetReport(periodKind);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string FormatRankLine(ProductPerformanceRankRow row)
            => $"{row.Rank}. {row.Row.ProductName} · {row.MetricLabel}";

        public static string FormatClassLine(ProductClassificationRow row)
            => $"{ClassLabel(row.Class)}: {row.ProductName}";

        public static string RankHeadline(ProductPerformanceRankRow? row)
            => row?.Row.ProductName ?? "—";

        public static string RankMetric(ProductPerformanceRankRow? row)
            => row?.MetricLabel ?? "Sin datos";

        public static string ExplainStar(ProductClassificationRow star)
        {
            string reasons = star.Reasons.Count == 0
                ? "checklist impacto + eficiencia + bajo riesgo"
                : string.Join("; ", star.Reasons.Take(4));
            return $"{star.ProductName}: {reasons}";
        }
    }
}
