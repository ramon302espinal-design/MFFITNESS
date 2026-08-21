using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Checklist de oportunidad (FASE 8.14). Sin auto-compra.</summary>
    public static class ProductOpportunityMath
    {
        public static bool TryBuildOpportunity(
            ProductPerformanceRow performance,
            ProductTrendRow? trend,
            out ProductClassificationRow row,
            ProductOpportunityThresholds? thresholds = null)
        {
            ProductOpportunityThresholds t = thresholds ?? ProductOpportunityThresholds.Default;
            var reasons = new List<string>();

            if (trend?.PrimaryTrend != ProductTrendDirection.Growing)
            {
                row = default!;
                return false;
            }

            if (performance.IsImmobilized
                || performance.HealthStatus is InventoryHealthStatus.Frozen
                    or InventoryHealthStatus.Critical
                    or InventoryHealthStatus.Slow)
            {
                row = default!;
                return false;
            }

            if (!performance.HasReliableRealizedProfit)
            {
                row = default!;
                return false;
            }

            bool marginOk = performance.MarginPct.HasValue
                && performance.MarginPct.Value >= t.MinMarginPct;
            bool roiOk = performance.RoiPct.HasValue
                && performance.RoiPct.Value >= t.MinRoiPct;
            if (!marginOk && !roiOk)
            {
                row = default!;
                return false;
            }

            if (performance.InventoryCapital > t.MaxInventoryCapital)
            {
                row = default!;
                return false;
            }

            if (performance.Stock > t.MaxStock)
            {
                row = default!;
                return false;
            }

            reasons.Add($"Tendencia Growing ({trend.UnitsChangePct:N0}% uds)");
            if (marginOk)
                reasons.Add($"Margen {performance.MarginPct:N1}% ≥ {t.MinMarginPct:N0}%");
            if (roiOk)
                reasons.Add($"ROI {performance.RoiPct:N1}% ≥ {t.MinRoiPct:N0}%");
            reasons.Add(
                $"Capital {performance.InventoryCapital:N0} ≤ {t.MaxInventoryCapital:N0} · " +
                $"Stock {performance.Stock} ≤ {t.MaxStock}");
            reasons.Add("Señal de oportunidad — no es orden de compra automática");

            row = new ProductClassificationRow
            {
                ProductId = performance.ProductId,
                ProductName = performance.ProductName,
                Category = performance.Category,
                Class = ProductPerformanceClass.Opportunity,
                Reasons = reasons,
                Performance = performance,
                Trend = trend.PrimaryTrend
            };
            return true;
        }
    }
}
