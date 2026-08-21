using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Detección de producto en riesgo → Critical (FASE 8.15).</summary>
    public static class ProductRiskMath
    {
        public static bool TryBuildRisk(
            ProductPerformanceRow performance,
            ProductTrendRow? trend,
            out ProductClassificationRow row,
            ProductRiskThresholds? thresholds = null)
        {
            ProductRiskThresholds t = thresholds ?? ProductRiskThresholds.Default;
            var reasons = new List<string>();

            if (performance.HealthStatus == InventoryHealthStatus.Critical)
            {
                reasons.Add("HealthStatus Critical (FASE 7)");
                row = Build(performance, trend, reasons);
                return true;
            }

            if (performance.IsImmobilized && performance.PotentialProfit < 0)
            {
                reasons.Add("Inmovilizado con ganancia potencial negativa");
                row = Build(performance, trend, reasons);
                return true;
            }

            if (performance.IsImmobilized
                && performance.IdleDays.HasValue
                && performance.IdleDays.Value >= t.CriticalIdleDays
                && performance.ImmobilizedCapital >= t.MinMaterialCapital)
            {
                reasons.Add(
                    $"Inmovilizado {performance.IdleDays}d idle · " +
                    $"capital {performance.ImmobilizedCapital:N0}");
                row = Build(performance, trend, reasons);
                return true;
            }

            // Brief §29: alto capital + ventas/ganancia débiles + idle
            if (performance.InventoryCapital >= t.MinMaterialCapital
                && performance.IdleDays.HasValue
                && performance.IdleDays.Value >= t.CriticalIdleDays
                && (!performance.HasReliableRealizedProfit
                    || performance.RealizedProfit <= t.MaxWeakProfit)
                && performance.HasInventorySnapshot)
            {
                reasons.Add(
                    $"Capital {performance.InventoryCapital:N0} · idle {performance.IdleDays}d · " +
                    "ganancia débil/nula en período");
                row = Build(performance, trend, reasons);
                return true;
            }

            if (performance.InventoryCapital >= t.MinMaterialCapital
                && trend?.PrimaryTrend == ProductTrendDirection.Declining
                && (trend.UnitsChangePct ?? 0m) <= t.StrongDeclinePct
                && (!performance.HasReliableRealizedProfit
                    || performance.RealizedProfit <= t.MaxWeakProfit))
            {
                reasons.Add(
                    $"Declining {trend.UnitsChangePct:N0}% · capital material · impacto débil");
                row = Build(performance, trend, reasons);
                return true;
            }

            row = default!;
            return false;
        }

        private static ProductClassificationRow Build(
            ProductPerformanceRow p,
            ProductTrendRow? trend,
            List<string> reasons)
        {
            if (p.FlagStockoutRisk)
                reasons.Add("Nota: también FlagStockoutRisk (operativo, no causa de Critical)");

            return new ProductClassificationRow
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Category = p.Category,
                Class = ProductPerformanceClass.Critical,
                Reasons = reasons,
                Performance = p,
                Trend = trend?.PrimaryTrend
            };
        }
    }
}
