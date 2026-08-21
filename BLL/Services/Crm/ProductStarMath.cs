using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Checklist de producto estrella (FASE 8.13). Tres pilares booleanos — sin score.
    /// </summary>
    public static class ProductStarMath
    {
        /// <summary>
        /// True si cumple impacto + eficiencia + bajo riesgo y no está excluido.
        /// </summary>
        public static bool TryBuildStar(
            ProductPerformanceRow performance,
            ProductTrendRow? trend,
            out ProductClassificationRow starRow,
            ProductStarThresholds? thresholds = null)
        {
            ProductStarThresholds t = thresholds ?? ProductStarThresholds.Default;
            var reasons = new List<string>();
            var gaps = new List<string>();

            if (IsExcluded(performance, reasons))
            {
                starRow = default!;
                return false;
            }

            bool impact = MeetsImpact(performance, t, reasons, gaps);
            bool efficiency = MeetsEfficiency(performance, t, reasons, gaps);
            bool lowRisk = MeetsLowRisk(performance, trend, t, reasons, gaps);

            if (!(impact && efficiency && lowRisk))
            {
                starRow = default!;
                return false;
            }

            if (performance.FlagStockoutRisk)
                reasons.Add("⚠️ StockoutRisk — estrella con riesgo de quiebre (reabastecer)");

            starRow = new ProductClassificationRow
            {
                ProductId = performance.ProductId,
                ProductName = performance.ProductName,
                Category = performance.Category,
                Class = ProductPerformanceClass.Star,
                Reasons = reasons,
                Performance = performance,
                Trend = trend?.PrimaryTrend
            };
            return true;
        }

        private static bool IsExcluded(ProductPerformanceRow p, List<string> reasons)
        {
            if (!p.HasPeriodActivity && !p.HasInventorySnapshot)
                return true;
            if (p.HealthStatus == InventoryHealthStatus.New)
                return true;
            if (p.HealthStatus == InventoryHealthStatus.InsufficientData
                && !p.HasPeriodActivity)
                return true;
            if (p.HealthStatus == InventoryHealthStatus.Critical)
                return true;
            return false;
        }

        private static bool MeetsImpact(
            ProductPerformanceRow p,
            ProductStarThresholds t,
            List<string> reasons,
            List<string> gaps)
        {
            bool byProfit = p.HasReliableRealizedProfit
                && p.RealizedProfit >= t.MinRealizedProfit;
            bool byRevenue = p.RevenueTotal >= t.MinRevenue;
            bool byUnits = p.UnitsSold >= t.MinUnitsSold;

            if (byProfit || byRevenue || byUnits)
            {
                if (byProfit)
                    reasons.Add($"Impacto: ganancia {p.RealizedProfit:N0} ≥ {t.MinRealizedProfit:N0}");
                else if (byRevenue)
                    reasons.Add($"Impacto: ingresos {p.RevenueTotal:N0} ≥ {t.MinRevenue:N0}");
                else
                    reasons.Add($"Impacto: {p.UnitsSold} uds ≥ {t.MinUnitsSold}");
                return true;
            }

            gaps.Add("Sin impacto suficiente (ganancia/ingresos/unidades)");
            return false;
        }

        private static bool MeetsEfficiency(
            ProductPerformanceRow p,
            ProductStarThresholds t,
            List<string> reasons,
            List<string> gaps)
        {
            bool marginOk = p.HasReliableRealizedProfit
                && p.MarginPct.HasValue
                && p.MarginPct.Value >= t.MinMarginPct;
            bool roiOk = p.HasReliableRealizedProfit
                && p.RoiPct.HasValue
                && p.RoiPct.Value >= t.MinRoiPct;

            if (!marginOk && !roiOk)
            {
                gaps.Add("Sin eficiencia (margen/ROI bajo umbral o no confiable)");
                return false;
            }

            bool rotation = (p.UnitsPerDay ?? 0m) > 0
                || (p.TurnoverProxy ?? 0m) > 0
                || p.HasPeriodActivity;

            if (!rotation)
            {
                gaps.Add("Sin señal de rotación/demanda");
                return false;
            }

            if (marginOk)
                reasons.Add($"Eficiencia: margen {p.MarginPct:N1}% ≥ {t.MinMarginPct:N0}%");
            if (roiOk)
                reasons.Add($"Eficiencia: ROI {p.RoiPct:N1}% ≥ {t.MinRoiPct:N0}%");
            reasons.Add("Eficiencia: rotación/demanda presente");
            return true;
        }

        private static bool MeetsLowRisk(
            ProductPerformanceRow p,
            ProductTrendRow? trend,
            ProductStarThresholds t,
            List<string> reasons,
            List<string> gaps)
        {
            if (p.IsImmobilized
                || p.HealthStatus is InventoryHealthStatus.Frozen
                    or InventoryHealthStatus.Slow
                    or InventoryHealthStatus.Critical)
            {
                gaps.Add($"Riesgo de capital: {p.HealthStatus}");
                return false;
            }

            if (p.PotentialProfit < 0)
            {
                gaps.Add("Ganancia potencial negativa");
                return false;
            }

            if (trend?.PrimaryTrend == ProductTrendDirection.Declining
                && (trend.UnitsChangePct ?? 0m) <= t.StrongDeclinePct)
            {
                gaps.Add("Tendencia Declining fuerte");
                return false;
            }

            reasons.Add("Bajo riesgo: capital no inmovilizado / salud ok");
            if (trend?.PrimaryTrend == ProductTrendDirection.Growing)
                reasons.Add("Tendencia Growing");
            else if (trend?.PrimaryTrend == ProductTrendDirection.Stable)
                reasons.Add("Tendencia Stable");
            return true;
        }
    }
}
