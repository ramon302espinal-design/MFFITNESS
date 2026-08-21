using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Clasificador de salud de capital (FASE 7.8). Sin I/O.
    /// Quiebre (FlagStockoutRisk) no fuerza Frozen/Critical.
    /// </summary>
    public static class InventoryHealthClassifier
    {
        public static InventoryHealthStatus Classify(
            int stock,
            decimal inventoryCapital,
            decimal potentialProfit,
            InventoryIdleKind idleKind,
            int? idleDays,
            int? daysSinceFirstEntry,
            decimal? daysOfCover,
            decimal? unitsPerDay,
            InventoryHealthThresholds? thresholds = null)
        {
            InventoryHealthThresholds t = thresholds ?? InventoryHealthThresholds.Default;

            if (stock <= 0 || inventoryCapital <= 0)
                return InventoryHealthStatus.InsufficientData;

            if (daysSinceFirstEntry.HasValue && daysSinceFirstEntry.Value < t.NewProductGraceDays)
                return InventoryHealthStatus.New;

            if (idleKind == InventoryIdleKind.Unknown && !daysOfCover.HasValue)
                return InventoryHealthStatus.InsufficientData;

            bool material = inventoryCapital >= t.MinMaterialCapital;
            bool frozenSignal = IsFrozenSignal(idleKind, idleDays, daysOfCover, t);

            if (material && frozenSignal)
            {
                if (IsCriticalAggravation(
                        inventoryCapital, potentialProfit, idleKind, idleDays, t))
                    return InventoryHealthStatus.Critical;

                return InventoryHealthStatus.Frozen;
            }

            if (IsSlow(idleDays, daysOfCover, unitsPerDay, t))
                return InventoryHealthStatus.Slow;

            if (IsHealthy(idleDays, daysOfCover, unitsPerDay, t))
                return InventoryHealthStatus.Healthy;

            // Capital bajo + idle largo sin materialidad → Slow (observar), no Critical.
            if (frozenSignal && !material)
                return InventoryHealthStatus.Slow;

            return InventoryHealthStatus.Healthy;
        }

        private static bool IsFrozenSignal(
            InventoryIdleKind idleKind,
            int? idleDays,
            decimal? daysOfCover,
            InventoryHealthThresholds t)
        {
            if (daysOfCover.HasValue && daysOfCover.Value >= t.FrozenCoverDaysMin)
                return true;

            if (idleDays.HasValue && idleDays.Value >= t.SlowIdleDaysMax)
                return true;

            if (idleKind == InventoryIdleKind.NeverSold
                && idleDays.HasValue
                && idleDays.Value >= t.SlowIdleDaysMax)
                return true;

            return false;
        }

        private static bool IsCriticalAggravation(
            decimal inventoryCapital,
            decimal potentialProfit,
            InventoryIdleKind idleKind,
            int? idleDays,
            InventoryHealthThresholds t)
        {
            if (inventoryCapital >= t.CriticalCapitalMin)
                return true;

            if (potentialProfit < 0)
                return true;

            if (idleKind == InventoryIdleKind.NeverSold
                && idleDays.HasValue
                && idleDays.Value >= t.CriticalNeverSoldDays)
                return true;

            return false;
        }

        private static bool IsSlow(
            int? idleDays,
            decimal? daysOfCover,
            decimal? unitsPerDay,
            InventoryHealthThresholds t)
        {
            if (idleDays.HasValue
                && idleDays.Value > t.HealthyIdleDaysMax
                && idleDays.Value < t.SlowIdleDaysMax)
                return true;

            if (daysOfCover.HasValue
                && daysOfCover.Value > t.HealthyCoverDaysMax
                && daysOfCover.Value < t.FrozenCoverDaysMin)
                return true;

            // Demanda presente pero idle medio sin cobertura calculable
            if (unitsPerDay.HasValue
                && unitsPerDay.Value > 0
                && idleDays.HasValue
                && idleDays.Value > t.HealthyIdleDaysMax
                && idleDays.Value < t.SlowIdleDaysMax)
                return true;

            return false;
        }

        private static bool IsHealthy(
            int? idleDays,
            decimal? daysOfCover,
            decimal? unitsPerDay,
            InventoryHealthThresholds t)
        {
            bool demand = unitsPerDay.HasValue && unitsPerDay.Value > 0;

            if (demand && daysOfCover.HasValue && daysOfCover.Value <= t.HealthyCoverDaysMax)
                return true;

            if (idleDays.HasValue && idleDays.Value <= t.HealthyIdleDaysMax)
                return true;

            if (demand && (!idleDays.HasValue || idleDays.Value <= t.HealthyIdleDaysMax))
                return true;

            return false;
        }
    }
}
