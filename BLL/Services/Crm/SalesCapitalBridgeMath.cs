using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato puente ventas ↔ capital (FASE 9.19).</summary>
    public static class SalesCapitalBridgePolicy
    {
        public const string Definition =
            "FASE 9.19: integrar motor de ventas con capital inventario (FASE 7) e ROI de producto. " +
            "InventoryCapital ≠ ImmobilizedCapital ≠ FrozenCapital inversión (FASE 6).";

        public const string RevenueUpRoi =
            "VENTAS↑ + ROI↓ (§52): puede indicar más capital invertido sin retorno proporcional. " +
            "Integrar lectura con FASE 6/7 — no es score.";

        public const string CapitalRisk =
            "RIESGO DE CAPITAL (§48): Declining + overstock/inmovilizado. " +
            "CapitalAtRisk = Σ InventoryCapital de productos con señal CapitalRisk.";

        public const string Separation =
            "PeriodProfitOnInventoryCapitalPct = ganancia período / capital inventario. " +
            "≠ RoiPct de línea de venta ≠ ROI de inversión FASE 6.";
    }

    /// <summary>Composición pura ventas↔capital (FASE 9.19).</summary>
    public static class SalesCapitalBridgeMath
    {
        public static bool IsUp(decimal? changePct, decimal flatBandPct = 2m)
            => changePct.HasValue && changePct.Value > flatBandPct;

        public static bool IsDown(decimal? changePct, decimal flatBandPct = 2m)
            => changePct.HasValue && changePct.Value < -flatBandPct;

        public static IReadOnlyList<SalesCapitalSignal> DetectSignals(
            decimal? revenueChangePct,
            decimal? profitChangePct,
            decimal? roiChangePct,
            decimal inventoryCapital,
            decimal immobilizedCapital,
            ProductTrendDirection? trend,
            SalesStockSignalKind? stockSignal,
            bool flagOverstock)
        {
            var list = new List<SalesCapitalSignal>();

            if (IsUp(revenueChangePct) && IsDown(roiChangePct))
            {
                list.Add(new SalesCapitalSignal
                {
                    Kind = SalesCapitalSignalKind.RevenueUpRoiDown,
                    Message = "⚠️ Ventas ↑ + ROI ↓ — capital sin retorno proporcional (§52)"
                });
            }

            if (IsUp(revenueChangePct) && IsDown(profitChangePct))
            {
                list.Add(new SalesCapitalSignal
                {
                    Kind = SalesCapitalSignalKind.RevenueUpProfitDown,
                    Message = "⚠️ Ventas ↑ + ganancia ↓ — revisar margen/capital (§50)"
                });
            }

            bool capitalRisk = trend == ProductTrendDirection.Declining
                && (immobilizedCapital > 0m || flagOverstock);

            if (capitalRisk)
            {
                list.Add(new SalesCapitalSignal
                {
                    Kind = SalesCapitalSignalKind.CapitalRisk,
                    Message = $"🔴 Riesgo de capital — inmovilizado {immobilizedCapital:N0} / stock alto (§48)"
                });
            }

            if (stockSignal == SalesStockSignalKind.StockoutRisk && inventoryCapital > 0m)
            {
                list.Add(new SalesCapitalSignal
                {
                    Kind = SalesCapitalSignalKind.StockoutWithCapital,
                    Message = $"🔴 Quiebre con capital atado {inventoryCapital:N0}"
                });
            }

            return list;
        }

        public static SalesCapitalSignalKind PickPrimary(IReadOnlyList<SalesCapitalSignal> signals)
        {
            if (signals.Any(s => s.Kind == SalesCapitalSignalKind.CapitalRisk))
                return SalesCapitalSignalKind.CapitalRisk;
            if (signals.Any(s => s.Kind == SalesCapitalSignalKind.StockoutWithCapital))
                return SalesCapitalSignalKind.StockoutWithCapital;
            if (signals.Any(s => s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown))
                return SalesCapitalSignalKind.RevenueUpRoiDown;
            if (signals.Any(s => s.Kind == SalesCapitalSignalKind.RevenueUpProfitDown))
                return SalesCapitalSignalKind.RevenueUpProfitDown;
            return SalesCapitalSignalKind.None;
        }

        public static SalesCapitalBridgeRow Compose(
            ProductPerformanceRow performance,
            decimal? revenueChangePct = null,
            decimal? profitChangePct = null,
            decimal? roiChangePct = null,
            ProductTrendDirection? trend = null,
            SalesStockSignalKind? stockSignal = null)
        {
            var signals = DetectSignals(
                revenueChangePct,
                profitChangePct,
                roiChangePct,
                performance.InventoryCapital,
                performance.ImmobilizedCapital,
                trend,
                stockSignal,
                performance.FlagOverstock);

            return new SalesCapitalBridgeRow
            {
                ProductId = performance.ProductId,
                ProductName = performance.ProductName,
                Category = performance.Category,
                RevenueTotal = performance.RevenueTotal,
                RealizedProfit = performance.RealizedProfit,
                MarginPct = performance.MarginPct,
                RoiPct = performance.RoiPct,
                RevenueChangePct = revenueChangePct,
                ProfitChangePct = profitChangePct,
                RoiChangePct = roiChangePct,
                Stock = performance.Stock,
                InventoryCapital = performance.InventoryCapital,
                ImmobilizedCapital = performance.ImmobilizedCapital,
                HealthStatus = performance.HealthStatus,
                Trend = trend,
                StockSignal = stockSignal,
                Signals = signals,
                PrimarySignal = PickPrimary(signals)
            };
        }

        public static SalesCapitalBridgeReport BuildReport(
            IEnumerable<SalesCapitalBridgeRow> rows,
            ProfitPeriodKind periodKind)
        {
            var list = rows
                .OrderByDescending(r => r.InventoryCapital)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            decimal totalCap = InventoryFinancialMath.RoundMoney(list.Sum(r => r.InventoryCapital));
            decimal totalProfit = InventoryFinancialMath.RoundMoney(list.Sum(r => r.RealizedProfit));
            decimal totalImm = InventoryFinancialMath.RoundMoney(list.Sum(r => r.ImmobilizedCapital));
            decimal totalRev = InventoryFinancialMath.RoundMoney(list.Sum(r => r.RevenueTotal));

            decimal? efficiency = totalCap > 0
                ? InventoryFinancialMath.RoundPct(totalProfit / totalCap * 100m)
                : null;

            var flagged = list.Where(r => r.PrimarySignal != SalesCapitalSignalKind.None).ToList();

            return new SalesCapitalBridgeReport
            {
                PeriodKind = periodKind,
                TotalRevenue = totalRev,
                TotalRealizedProfit = totalProfit,
                TotalInventoryCapital = totalCap,
                TotalImmobilizedCapital = totalImm,
                PeriodProfitOnInventoryCapitalPct = efficiency,
                RevenueUpRoiDownCount = list.Count(r =>
                    r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown)),
                RevenueUpProfitDownCount = list.Count(r =>
                    r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.RevenueUpProfitDown)),
                CapitalRiskCount = list.Count(r =>
                    r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.CapitalRisk)),
                StockoutWithCapitalCount = list.Count(r =>
                    r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.StockoutWithCapital)),
                CapitalAtRisk = InventoryFinancialMath.RoundMoney(
                    list.Where(r => r.Signals.Any(s => s.Kind == SalesCapitalSignalKind.CapitalRisk))
                        .Sum(r => r.InventoryCapital)),
                Rows = list,
                Flagged = flagged
                    .OrderByDescending(r => Priority(r.PrimarySignal))
                    .ThenByDescending(r => r.InventoryCapital)
                    .ToList(),
                Caution = SalesCapitalBridgePolicy.Separation
            };
        }

        private static int Priority(SalesCapitalSignalKind kind)
            => kind switch
            {
                SalesCapitalSignalKind.CapitalRisk => 4,
                SalesCapitalSignalKind.StockoutWithCapital => 3,
                SalesCapitalSignalKind.RevenueUpRoiDown => 2,
                SalesCapitalSignalKind.RevenueUpProfitDown => 1,
                _ => 0
            };
    }
}
