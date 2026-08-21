using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Composición pura de métricas de producto (FASE 8.2). Sin I/O ni score.
    /// </summary>
    public static class ProductPerformanceComposer
    {
        /// <summary>
        /// Capital inmovilizado del producto: capital si Frozen/Critical; si no, 0.
        /// </summary>
        public static decimal ImmobilizedCapitalOf(InventoryFinancialRow? inv)
        {
            if (inv == null || inv.InventoryCapital <= 0)
                return 0m;
            if (inv.HealthStatus is InventoryHealthStatus.Frozen
                or InventoryHealthStatus.Critical)
                return InventoryFinancialMath.RoundMoney(inv.InventoryCapital);
            return 0m;
        }

        public static bool IsImmobilized(InventoryFinancialRow? inv)
            => ImmobilizedCapitalOf(inv) > 0m;

        /// <summary>Une fila de P&amp;L del período + snapshot de inventario.</summary>
        public static ProductPerformanceRow Compose(
            ProfitGroupRow? period,
            InventoryFinancialRow? inventory)
        {
            if (period == null && inventory == null)
                throw new ArgumentException("Se requiere al menos una fuente (período o inventario).");

            int id = period?.ProductId ?? inventory!.ProductId;
            string name = !string.IsNullOrEmpty(period?.ProductName)
                ? period!.ProductName!
                : inventory!.ProductName;
            string category = inventory?.Category ?? string.Empty;

            decimal immobilized = ImmobilizedCapitalOf(inventory);
            bool periodReliable = period?.HasReliableRealizedProfit == true;

            return new ProductPerformanceRow
            {
                ProductId = id,
                ProductName = name,
                Category = category,

                UnitsSold = period?.UnitsSold ?? 0,
                RevenueTotal = period?.RevenueTotal ?? 0m,
                RealizedProfit = periodReliable ? period!.RealizedProfit : 0m,
                Cogs = periodReliable ? period!.Cogs : 0m,
                RevenueWithCost = period?.RevenueWithCost ?? 0m,

                MarginPct = periodReliable ? period!.MarginPct : null,
                RoiPct = periodReliable ? period!.RoiPct : null,
                UnitsPerDay = inventory?.UnitsPerDay,
                TurnoverProxy = inventory?.TurnoverProxy,

                Stock = inventory?.Stock ?? 0,
                InventoryCapital = inventory?.InventoryCapital ?? 0m,
                ImmobilizedCapital = immobilized,
                PotentialSalesValue = inventory?.PotentialSalesValue ?? 0m,
                PotentialProfit = inventory?.PotentialProfit ?? 0m,

                HealthStatus = inventory?.HealthStatus ?? InventoryHealthStatus.InsufficientData,
                IdleDays = inventory?.IdleDays,
                FlagStockoutRisk = inventory?.FlagStockoutRisk ?? false,
                FlagOverstock = inventory?.FlagOverstock ?? false,

                HasReliableRealizedProfit = periodReliable,
                HasPeriodActivity = period != null && period.UnitsSold > 0,
                HasInventorySnapshot = inventory != null,
                IsImmobilized = immobilized > 0m
            };
        }

        /// <summary>
        /// Une diccionarios por ProductId (unión). Orden: nombre.
        /// </summary>
        public static IReadOnlyList<ProductPerformanceRow> ComposeAll(
            IEnumerable<ProfitGroupRow> periodRows,
            IEnumerable<InventoryFinancialRow> inventoryRows)
        {
            var byProfit = periodRows
                .Where(p => p.ProductId.HasValue)
                .GroupBy(p => p.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var byInv = inventoryRows
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            var ids = byProfit.Keys.Union(byInv.Keys).ToList();
            var rows = new List<ProductPerformanceRow>(ids.Count);

            foreach (int id in ids)
            {
                byProfit.TryGetValue(id, out ProfitGroupRow? p);
                byInv.TryGetValue(id, out InventoryFinancialRow? i);
                rows.Add(Compose(p, i));
            }

            return rows
                .OrderBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }
}
