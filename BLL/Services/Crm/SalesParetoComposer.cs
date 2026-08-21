using BLL.Models.Crm;
using System.Globalization;

namespace BLL.Services.Crm
{
    /// <summary>Contrato Pareto (FASE 9.13).</summary>
    public static class SalesParetoPolicy
    {
        public const string Definition =
            "FASE 9.13: calcular cuántos ítems hacen falta para alcanzar un umbral " +
            "de participación acumulada (default 80%). NO asumir exactamente 80/20.";

        public const string Summary =
            "Resumen = '{itemPct}% de ítems generan {achieved}% de {métrica}' con datos reales.";

        public const string Insufficient =
            "Sin ítems o total ≤ 0 → TargetReached=false, Summary = datos insuficientes.";
    }

    /// <summary>Cálculo puro Pareto (FASE 9.13).</summary>
    public static class SalesParetoComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static SalesParetoReport FromShareReport(
            SalesShareReport share,
            decimal targetCumulativePct = 80m)
        {
            decimal target = targetCumulativePct <= 0 ? 80m : targetCumulativePct;
            var items = share.Items;

            if (items.Count == 0 || share.TotalAmount <= 0)
            {
                return new SalesParetoReport
                {
                    PeriodKind = share.PeriodKind,
                    MetricLabel = share.MetricLabel,
                    ItemCount = 0,
                    TotalAmount = 0m,
                    TargetCumulativePct = target,
                    ItemsToReachTarget = 0,
                    ItemPctToReachTarget = null,
                    AchievedCumulativePct = null,
                    TargetReached = false,
                    Summary = "Datos insuficientes para Pareto.",
                    Items = items
                };
            }

            int cut = items.Count;
            decimal achieved = items[^1].CumulativeSharePct ?? 100m;
            bool reached = false;

            for (int i = 0; i < items.Count; i++)
            {
                decimal cum = items[i].CumulativeSharePct ?? 0m;
                if (cum >= target)
                {
                    cut = i + 1;
                    achieved = cum;
                    reached = true;
                    break;
                }
            }

            decimal? itemPct = SalesAnalyticsMath.SharePct(cut, items.Count);
            string summary = reached
                ? $"{FormatPct(itemPct)} de ítems generan {FormatPct(achieved)} de {share.MetricLabel.ToLowerInvariant()}"
                : $"Con todos los ítems se alcanza {FormatPct(achieved)} (umbral {FormatPct(target)} no cubierto)";

            return new SalesParetoReport
            {
                PeriodKind = share.PeriodKind,
                MetricLabel = share.MetricLabel,
                ItemCount = items.Count,
                TotalAmount = share.TotalAmount,
                TargetCumulativePct = target,
                ItemsToReachTarget = cut,
                ItemPctToReachTarget = itemPct,
                AchievedCumulativePct = achieved,
                TargetReached = reached,
                Summary = summary,
                Items = items
            };
        }

        private static string FormatPct(decimal? v)
            => v.HasValue ? v.Value.ToString("N0", Cultura) + "%" : "N/D";
    }
}
