using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Clasificador de productos (FASE 8.12). Sin score; Star no se asigna aquí.
    /// </summary>
    public static class ProductClassificationMath
    {
        public static ProductClassificationRow Classify(
            ProductPerformanceRow performance,
            ProductTrendRow? trend = null,
            ProductClassificationThresholds? thresholds = null)
        {
            ProductClassificationThresholds t = thresholds ?? ProductClassificationThresholds.Default;
            var reasons = new List<string>();

            if (!performance.HasPeriodActivity && !performance.HasInventorySnapshot)
            {
                reasons.Add("Sin ventas en período ni snapshot de inventario");
                return Result(performance, trend, ProductPerformanceClass.InsufficientData, reasons);
            }

            // Critical first
            if (ProductRiskMath.TryBuildRisk(
                    performance,
                    trend,
                    out ProductClassificationRow risk,
                    new ProductRiskThresholds
                    {
                        CriticalIdleDays = t.CriticalIdleDays,
                        MinMaterialCapital = t.MinMaterialCapital,
                        StrongDeclinePct = t.StrongDeclinePct
                    }))
                return risk;

            // New — do not treat as slow/frozen
            if (performance.HealthStatus == InventoryHealthStatus.New)
            {
                reasons.Add("Producto en gracia New (FASE 7)");
                return Result(performance, trend, ProductPerformanceClass.New, reasons);
            }

            if (IsSlow(performance, trend, t, reasons))
                return Result(performance, trend, ProductPerformanceClass.Slow, reasons);

            if (ProductStarMath.TryBuildStar(performance, trend, out ProductClassificationRow star))
                return star;

            if (ProductOpportunityMath.TryBuildOpportunity(
                    performance, trend, out ProductClassificationRow opportunity))
                return opportunity;

            if (performance.HealthStatus == InventoryHealthStatus.Healthy
                || performance.HasPeriodActivity)
            {
                reasons.Add(performance.HealthStatus == InventoryHealthStatus.Healthy
                    ? "Salud de capital Healthy"
                    : "Actividad en período sin señales adversas");
                return Result(performance, trend, ProductPerformanceClass.Healthy, reasons);
            }

            if (performance.HealthStatus == InventoryHealthStatus.InsufficientData)
            {
                reasons.Add("Datos de salud insuficientes");
                return Result(performance, trend, ProductPerformanceClass.InsufficientData, reasons);
            }

            reasons.Add("Sin señales fuertes — Healthy por defecto observacional");
            return Result(performance, trend, ProductPerformanceClass.Healthy, reasons);
        }

        private static bool IsSlow(
            ProductPerformanceRow p,
            ProductTrendRow? trend,
            ProductClassificationThresholds t,
            List<string> reasons)
        {
            if (p.HealthStatus is InventoryHealthStatus.Slow or InventoryHealthStatus.Frozen)
            {
                reasons.Add($"HealthStatus {p.HealthStatus}");
                return true;
            }

            if (trend?.PrimaryTrend == ProductTrendDirection.Declining
                && (trend.UnitsChangePct ?? 0m) <= t.StrongDeclinePct
                && p.InventoryCapital >= t.MinMaterialCapital)
            {
                reasons.Add($"Tendencia Declining fuerte ({trend.UnitsChangePct:N0}%) con capital material");
                return true;
            }

            return false;
        }

        private static ProductClassificationRow Result(
            ProductPerformanceRow p,
            ProductTrendRow? trend,
            ProductPerformanceClass cls,
            List<string> reasons)
            => new()
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Category = p.Category,
                Class = cls,
                Reasons = reasons.ToList(),
                Performance = p,
                Trend = trend?.PrimaryTrend
            };
    }
}
