using System;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Fórmulas financieras puras (FASE 4.3/4.9). Sin I/O.
    /// </summary>
    public static class InventoryFinancialMath
    {
        public static decimal RoundMoney(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        public static decimal RoundCost(decimal v)
            => Math.Round(v, 4, MidpointRounding.AwayFromZero);

        public static decimal RoundPct(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Capital en inventario a costo (FASE 7.2 / alias histórico FASE 4).
        /// Stock × costo. No es precio de venta ni “congelado clasificado”.
        /// </summary>
        public static decimal InventoryCost(int stock, decimal unitCost)
        {
            if (stock <= 0 || unitCost <= 0)
                return 0m;
            return RoundMoney(stock * unitCost);
        }

        /// <summary>Nombre canónico FASE 7.2 = <see cref="InventoryCost"/>.</summary>
        public static decimal InventoryCapital(int stock, decimal unitCost)
            => InventoryCost(stock, unitCost);

        /// <summary>Valor potencial de venta (≠ capital).</summary>
        public static decimal PotentialSalesValue(int stock, decimal salePrice)
        {
            if (stock <= 0 || salePrice <= 0)
                return 0m;
            return RoundMoney(stock * salePrice);
        }

        /// <summary>Ganancia potencial = valor potencial − capital inventario.</summary>
        public static decimal PotentialProfit(int stock, decimal unitCost, decimal salePrice)
        {
            if (stock <= 0 || unitCost <= 0 || salePrice <= 0)
                return 0m;
            return RoundMoney(PotentialSalesValue(stock, salePrice) - InventoryCapital(stock, unitCost));
        }

        /// <summary>
        /// Días calendario entre dos fechas (solo Date). Null si falta origen.
        /// Nunca negativo. FASE 7.3.
        /// </summary>
        public static int? DaysSince(DateTime? fromDate, DateTime asOf)
        {
            if (!fromDate.HasValue)
                return null;
            return Math.Max(0, (asOf.Date - fromDate.Value.Date).Days);
        }

        /// <summary>
        /// Idle de ventas (FASE 7.4): LastSale si existe; si no, FirstEntry; si no, null.
        /// NeverSold no inventa fecha: usa entrada real.
        /// </summary>
        public static (InventoryIdleKind Kind, int? IdleDays, int? DaysWithoutSale) ResolveIdle(
            DateTime? lastSaleDate,
            DateTime? firstEntryDate,
            DateTime asOf)
        {
            if (lastSaleDate.HasValue)
            {
                int days = DaysSince(lastSaleDate, asOf)!.Value;
                return (InventoryIdleKind.HasSales, days, days);
            }

            if (firstEntryDate.HasValue)
            {
                int days = DaysSince(firstEntryDate, asOf)!.Value;
                return (InventoryIdleKind.NeverSold, days, null);
            }

            return (InventoryIdleKind.Unknown, null, null);
        }

        /// <summary>Ventana por defecto de velocidad (FASE 7.5): 30 días calendario.</summary>
        public const int DefaultVelocityWindowDays = 30;

        /// <summary>
        /// Velocidad media (FASE 7.5).
        /// UnitsPerDay = unidades en ventana / días de ventana.
        /// Semana = ×7; mes = ×30 (mes comercial, no calendario).
        /// Null si windowDays ≤ 0.
        /// </summary>
        public static (decimal? UnitsPerDay, decimal? UnitsPerWeek, decimal? UnitsPerMonth) ResolveVelocity(
            int unitsSoldInWindow,
            int windowDays)
        {
            if (windowDays <= 0)
                return (null, null, null);

            decimal perDay = RoundPct((decimal)Math.Max(0, unitsSoldInWindow) / windowDays);
            return (
                perDay,
                RoundPct(perDay * 7m),
                RoundPct(perDay * 30m));
        }

        /// <summary>
        /// Rotación PROXY (FASE 7.6) = COGS_ventana / CapitalInventario_hoy.
        /// NO es rotación contable (falta inventario promedio histórico).
        /// Null si capital ≤ 0. 0 si no hubo COGS confiable en la ventana.
        /// </summary>
        public static decimal? TurnoverProxy(decimal cogsInWindow, decimal inventoryCapital)
        {
            if (inventoryCapital <= 0)
                return null;
            return RoundPct(Math.Max(0m, cogsInWindow) / inventoryCapital);
        }

        /// <summary>
        /// Proxy de vueltas de unidades: uds_ventana / stock (si stock &gt; 0).
        /// Complementa TurnoverProxy; no lo sustituye.
        /// </summary>
        public static decimal? UnitTurnoverProxy(int unitsSoldInWindow, int stock)
        {
            if (stock <= 0)
                return null;
            return RoundPct((decimal)Math.Max(0, unitsSoldInWindow) / stock);
        }

        /// <summary>Umbral default cobertura “saludable” (días). FASE 7.7 / usado en 7.8.</summary>
        public const int DefaultHealthyCoverDays = 30;

        /// <summary>Umbral default sobreinventario (días de cobertura).</summary>
        public const int DefaultOverstockCoverDays = 90;

        /// <summary>
        /// Días de cobertura / inventario (FASE 7.7) = Stock / UnitsPerDay.
        /// Null si no hay velocidad &gt; 0 (no inventar demanda infinita).
        /// </summary>
        public static decimal? DaysOfCover(int stock, decimal? unitsPerDay)
        {
            if (stock < 0)
                return null;
            if (!unitsPerDay.HasValue || unitsPerDay.Value <= 0)
                return null;
            return RoundPct(stock / unitsPerDay.Value);
        }

        /// <summary>
        /// % capital congelado clasificado / capital inventario (FASE 7.9+).
        /// Null si no hay capital inventario. No usar con el alias legacy Frozen=todo.
        /// </summary>
        public static decimal? FrozenShareOfInventoryPct(decimal frozenCapital, decimal inventoryCapital)
        {
            if (inventoryCapital <= 0)
                return null;
            return RoundPct(frozenCapital / inventoryCapital * 100m);
        }

        /// <summary>Descuentos estándar de simulación (FASE 7.10). No mutan precios reales.</summary>
        public static IReadOnlyList<decimal> DefaultLiquidationDiscounts { get; } =
            new decimal[] { 0m, 5m, 10m, 20m, 30m, 50m };

        /// <summary>
        /// Simulación de liquidación por producto (FASE 7.10).
        /// Precio simulado = SalePrice × (1 − discount/100). No escribe en BD.
        /// </summary>
        public static LiquidationScenarioResult SimulateLiquidation(
            int stock,
            decimal unitCost,
            decimal salePrice,
            decimal discountPct)
        {
            decimal capital = InventoryCapital(stock, unitCost);
            decimal disc = Math.Clamp(discountPct, 0m, 100m);
            decimal unitSim = salePrice > 0
                ? RoundMoney(salePrice * (1m - disc / 100m))
                : 0m;
            decimal revenue = stock > 0 && unitSim > 0
                ? RoundMoney(stock * unitSim)
                : 0m;

            return new LiquidationScenarioResult
            {
                DiscountPct = disc,
                SimulatedUnitPrice = unitSim,
                SimulatedRevenue = revenue,
                CapitalAtCost = capital,
                ProfitOrLoss = RoundMoney(revenue - capital),
                CapitalLiberable = capital
            };
        }

        /// <summary>Simulación agregada desde totales (alcance en riesgo).</summary>
        public static LiquidationScenarioResult SimulateLiquidationFromTotals(
            decimal capitalAtCost,
            decimal listSalesValue,
            decimal discountPct)
        {
            decimal disc = Math.Clamp(discountPct, 0m, 100m);
            decimal revenue = listSalesValue > 0
                ? RoundMoney(listSalesValue * (1m - disc / 100m))
                : 0m;
            decimal capital = Math.Max(0m, RoundMoney(capitalAtCost));

            return new LiquidationScenarioResult
            {
                DiscountPct = disc,
                SimulatedUnitPrice = 0m,
                SimulatedRevenue = revenue,
                CapitalAtCost = capital,
                ProfitOrLoss = RoundMoney(revenue - capital),
                CapitalLiberable = capital
            };
        }

        public static decimal RealizedLineProfit(int quantity, decimal salePrice, decimal costSnapshot)
        {
            if (quantity <= 0 || costSnapshot < 0)
                return 0m;
            decimal revenue = quantity * salePrice;
            decimal cogs = quantity * costSnapshot;
            return RoundMoney(revenue - cogs);
        }

        public static decimal LineCogs(int quantity, decimal costSnapshot)
        {
            if (quantity <= 0 || costSnapshot < 0)
                return 0m;
            return RoundMoney(quantity * costSnapshot);
        }

        /// <summary>
        /// Margen % (FASE 5.6) = Ganancia / Ingreso × 100.
        /// Denominador = ingreso de líneas CON costo (no RevenueTotal bruto).
        /// No es ROI. Null si ingreso ≤ 0. Negativo si hay pérdida.
        /// </summary>
        public static decimal? MarginPct(decimal realizedProfit, decimal revenue)
        {
            if (revenue <= 0)
                return null;
            return RoundPct(realizedProfit / revenue * 100m);
        }

        /// <summary>
        /// ROI % (FASE 5.7) = Ganancia / COGS × 100.
        /// Denominador = capital a costo vendido (no ingreso, no MontoPagado).
        /// No es margen. Null si COGS ≤ 0. Negativo si hay pérdida.
        /// </summary>
        public static decimal? RoiPct(decimal realizedProfit, decimal cogs)
        {
            if (cogs <= 0)
                return null;
            return RoundPct(realizedProfit / cogs * 100m);
        }

        /// <summary>Promedio ponderado tras una entrada con costo.</summary>
        public static decimal WeightedAverageUnitCost(
            int stockBefore,
            decimal costBefore,
            int qtyIn,
            decimal costIn)
        {
            if (qtyIn <= 0 || costIn <= 0)
                throw new ArgumentException("Entrada y costo deben ser mayores a cero.");

            int stockAfter = stockBefore + qtyIn;
            if (stockAfter <= 0)
                return RoundCost(costIn);

            if (stockBefore <= 0)
                return RoundCost(costIn);

            decimal valueBefore = stockBefore * Math.Max(costBefore, 0m);
            decimal valueIn = qtyIn * costIn;
            return RoundCost((valueBefore + valueIn) / stockAfter);
        }
    }
}
