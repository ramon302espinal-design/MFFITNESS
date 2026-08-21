using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato riesgo de quiebre / señales stock↔ventas (FASE 9.18).</summary>
    public static class SalesStockRiskPolicy
    {
        public const string Definition =
            "FASE 9.18: integrar proyección de demanda, stock y velocidad (FASE 7). " +
            "Cobertura estimada = DaysOfCover (Stock / UnitsPerDay). " +
            "Si demanda proyectada > stock → RIESGO DE QUIEBRE.";

        public const string Stockout =
            "🔴 RIESGO DE QUIEBRE si FlagStockoutRisk (FASE 7) o demanda(horizonte) > stock " +
            "o cobertura < LowCoverDays.";

        public const string Replenishment =
            "🟡 OPORTUNIDAD DE REABASTECIMIENTO si Growing + stock bajo + rotación alta. " +
            "NO ejecutar compra automática.";

        public const string Capital =
            "🔴 RIESGO DE CAPITAL si Declining + (Overstock o capital inmovilizado). " +
            "Integrar con capital congelado FASE 7.";

        public const string Healthy =
            "🔥 CRECIMIENTO SALUDABLE si Growing + cobertura saludable + velocidad > 0.";
    }

    public sealed class SalesStockRiskThresholds
    {
        public static SalesStockRiskThresholds Default { get; } = new();

        public int HorizonDays { get; init; } = 30;
        public int LowCoverDays { get; init; } = 7;
        public int HealthyCoverMinDays { get; init; } = 7;
        public int HealthyCoverMaxDays { get; init; } =
            InventoryFinancialMath.DefaultHealthyCoverDays;

        /// <summary>Rotación “alta” mínima (uds/día) para oportunidad de reabastecimiento.</summary>
        public decimal HighVelocityUnitsPerDay { get; init; } = 0.5m;
    }

    /// <summary>Clasificación pura stock↔ventas (FASE 9.18).</summary>
    public static class SalesStockRiskMath
    {
        public static decimal? ProjectedDemandUnits(decimal? unitsPerDay, int horizonDays)
        {
            if (!unitsPerDay.HasValue || unitsPerDay.Value <= 0 || horizonDays <= 0)
                return null;
            return InventoryFinancialMath.RoundPct(unitsPerDay.Value * horizonDays);
        }

        public static bool DemandExceedsStock(int stock, decimal? projectedDemand)
            => projectedDemand.HasValue && projectedDemand.Value > stock;

        public static bool IsLowStock(
            int stock,
            int stockMinimo,
            decimal? daysOfCover,
            bool flagStockoutRisk,
            SalesStockRiskThresholds t)
        {
            if (flagStockoutRisk)
                return true;
            if (stockMinimo > 0 && stock <= stockMinimo)
                return true;
            if (daysOfCover.HasValue && daysOfCover.Value < t.LowCoverDays)
                return true;
            return false;
        }

        public static SalesStockSignalRow Classify(
            int productId,
            string productName,
            string category,
            int stock,
            int stockMinimo,
            decimal? unitsPerDay,
            decimal? turnoverProxy,
            bool flagStockoutRisk,
            bool flagOverstock,
            bool isImmobilized,
            ProductTrendDirection? trend = null,
            SalesStockRiskThresholds? thresholds = null)
        {
            SalesStockRiskThresholds t = thresholds ?? SalesStockRiskThresholds.Default;
            decimal? cover = InventoryFinancialMath.DaysOfCover(stock, unitsPerDay);
            decimal? demand = ProjectedDemandUnits(unitsPerDay, t.HorizonDays);
            bool exceeds = DemandExceedsStock(stock, demand);

            var signals = new List<SalesStockSignalKind>();
            var reasons = new List<string>();

            bool stockout = flagStockoutRisk
                || exceeds
                || (cover.HasValue && cover.Value < t.LowCoverDays && unitsPerDay > 0);

            if (stockout)
            {
                signals.Add(SalesStockSignalKind.StockoutRisk);
                if (exceeds)
                    reasons.Add($"Demanda ~{demand:N0} uds/{t.HorizonDays}d > stock {stock}");
                else if (flagStockoutRisk)
                    reasons.Add($"FlagStockoutRisk FASE 7 (stock {stock} ≤ mín {stockMinimo})");
                else
                    reasons.Add($"Cobertura {cover:N1}d < {t.LowCoverDays}d");
            }

            bool lowStock = IsLowStock(stock, stockMinimo, cover, flagStockoutRisk, t);
            bool highRotation = (unitsPerDay.HasValue && unitsPerDay.Value >= t.HighVelocityUnitsPerDay)
                || (turnoverProxy.HasValue && turnoverProxy.Value > 0);

            if (trend == ProductTrendDirection.Growing && lowStock && highRotation)
            {
                signals.Add(SalesStockSignalKind.ReplenishmentOpportunity);
                reasons.Add("Growing + stock bajo + rotación alta — NO compra automática");
            }

            if (trend == ProductTrendDirection.Declining
                && (flagOverstock || isImmobilized
                    || (cover.HasValue
                        && cover.Value >= InventoryFinancialMath.DefaultOverstockCoverDays)))
            {
                signals.Add(SalesStockSignalKind.CapitalRisk);
                reasons.Add("Declining + sobreinventario/inmovilizado → riesgo de capital");
            }

            bool healthyCover = cover.HasValue
                && cover.Value >= t.HealthyCoverMinDays
                && cover.Value <= t.HealthyCoverMaxDays;

            if (trend == ProductTrendDirection.Growing
                && !stockout
                && healthyCover
                && unitsPerDay.HasValue
                && unitsPerDay.Value > 0)
            {
                signals.Add(SalesStockSignalKind.HealthyGrowth);
                reasons.Add($"Growing + cobertura {cover:N1}d saludable + velocidad");
            }

            SalesStockSignalKind primary = PickPrimary(signals);
            return new SalesStockSignalRow
            {
                ProductId = productId,
                ProductName = productName,
                Category = category,
                PrimarySignal = primary,
                Signals = signals,
                Stock = stock,
                StockMinimo = stockMinimo,
                UnitsPerDay = unitsPerDay,
                TurnoverProxy = turnoverProxy,
                DaysOfCover = cover,
                ProjectedDemandUnits = demand,
                HorizonDays = t.HorizonDays,
                DemandExceedsStock = exceeds,
                FlagStockoutRisk = flagStockoutRisk,
                FlagOverstock = flagOverstock,
                IsImmobilized = isImmobilized,
                Trend = trend,
                DisplayLabel = DisplayLabel(primary),
                Reason = reasons.Count == 0 ? "Sin señal" : string.Join(" · ", reasons)
            };
        }

        public static SalesStockSignalRow FromInventory(
            InventoryFinancialRow inv,
            ProductTrendDirection? trend = null,
            SalesStockRiskThresholds? thresholds = null)
            => Classify(
                inv.ProductId,
                inv.ProductName,
                inv.Category,
                inv.Stock,
                inv.StockMinimo,
                inv.UnitsPerDay,
                inv.TurnoverProxy,
                inv.FlagStockoutRisk,
                inv.FlagOverstock,
                inv.HealthStatus is InventoryHealthStatus.Frozen or InventoryHealthStatus.Critical,
                trend,
                thresholds);

        public static SalesStockRiskReport Compose(
            IEnumerable<SalesStockSignalRow> rows,
            ProfitPeriodKind periodKind,
            SalesStockRiskThresholds? thresholds = null)
        {
            SalesStockRiskThresholds t = thresholds ?? SalesStockRiskThresholds.Default;
            var list = rows
                .OrderByDescending(r => Priority(r.PrimarySignal))
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return new SalesStockRiskReport
            {
                PeriodKind = periodKind,
                HorizonDays = t.HorizonDays,
                ProductCount = list.Count,
                StockoutRiskCount = list.Count(r =>
                    r.Signals.Contains(SalesStockSignalKind.StockoutRisk)),
                ReplenishmentOpportunityCount = list.Count(r =>
                    r.Signals.Contains(SalesStockSignalKind.ReplenishmentOpportunity)),
                CapitalRiskCount = list.Count(r =>
                    r.Signals.Contains(SalesStockSignalKind.CapitalRisk)),
                HealthyGrowthCount = list.Count(r =>
                    r.Signals.Contains(SalesStockSignalKind.HealthyGrowth)),
                Rows = list,
                StockoutRisks = list
                    .Where(r => r.Signals.Contains(SalesStockSignalKind.StockoutRisk))
                    .ToList(),
                ReplenishmentOpportunities = list
                    .Where(r => r.Signals.Contains(SalesStockSignalKind.ReplenishmentOpportunity))
                    .ToList(),
                PolicyNote = SalesStockRiskPolicy.Replenishment
            };
        }

        private static SalesStockSignalKind PickPrimary(IReadOnlyList<SalesStockSignalKind> signals)
        {
            if (signals.Contains(SalesStockSignalKind.StockoutRisk))
                return SalesStockSignalKind.StockoutRisk;
            if (signals.Contains(SalesStockSignalKind.CapitalRisk))
                return SalesStockSignalKind.CapitalRisk;
            if (signals.Contains(SalesStockSignalKind.ReplenishmentOpportunity))
                return SalesStockSignalKind.ReplenishmentOpportunity;
            if (signals.Contains(SalesStockSignalKind.HealthyGrowth))
                return SalesStockSignalKind.HealthyGrowth;
            return SalesStockSignalKind.None;
        }

        private static int Priority(SalesStockSignalKind kind)
            => kind switch
            {
                SalesStockSignalKind.StockoutRisk => 4,
                SalesStockSignalKind.CapitalRisk => 3,
                SalesStockSignalKind.ReplenishmentOpportunity => 2,
                SalesStockSignalKind.HealthyGrowth => 1,
                _ => 0
            };

        private static string DisplayLabel(SalesStockSignalKind kind)
            => kind switch
            {
                SalesStockSignalKind.StockoutRisk => "🔴 RIESGO DE QUIEBRE",
                SalesStockSignalKind.ReplenishmentOpportunity => "🟡 OPORTUNIDAD DE REABASTECIMIENTO",
                SalesStockSignalKind.CapitalRisk => "🔴 RIESGO DE CAPITAL",
                SalesStockSignalKind.HealthyGrowth => "🔥 CRECIMIENTO SALUDABLE",
                _ => "Sin señal"
            };
    }
}
