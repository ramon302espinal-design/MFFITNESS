using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Fórmulas de capital/ROI de inversión (FASE 6.5+). Sin I/O.
    /// Distinto de InventoryFinancialMath / ProfitAnalytics (FASE 4–5).
    /// </summary>
    public static class InvestmentMath
    {
        public static decimal RoundMoney(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        public static decimal RoundPct(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Capital de una línea ENTRADA: CostoTotal si existe; si no qty×unit; si no 0.
        /// </summary>
        public static decimal LineCapital(int quantity, decimal? unitCost, decimal? costTotal)
        {
            if (costTotal.HasValue && costTotal.Value > 0)
                return RoundMoney(costTotal.Value);

            if (unitCost.HasValue && unitCost.Value > 0 && quantity > 0)
                return RoundMoney(quantity * unitCost.Value);

            return 0m;
        }

        /// <summary>Σ capital de líneas (FASE 6.5).</summary>
        public static decimal CapitalInvested(IEnumerable<decimal> lineCapitals)
        {
            decimal sum = 0m;
            foreach (decimal c in lineCapitals)
                sum += c;
            return RoundMoney(sum);
        }

        public static decimal CapitalPending(decimal invested, decimal recovered)
            => RoundMoney(Math.Max(0m, invested - recovered));

        public static decimal? RecoveryPct(decimal recovered, decimal invested)
        {
            if (invested <= 0)
                return null;
            return RoundPct(recovered / invested * 100m);
        }

        /// <summary>ROI inversión = ganancia / capital invertido (no COGS, no ventas).</summary>
        public static decimal? RoiPct(decimal profit, decimal capitalInvested)
        {
            if (capitalInvested <= 0)
                return null;
            return RoundPct(profit / capitalInvested * 100m);
        }

        /// <summary>ROI proyectado = (realizada + potencial) / capital invertido.</summary>
        public static decimal? RoiProjectedPct(decimal realizedProfit, decimal potentialProfit, decimal capitalInvested)
            => RoiPct(realizedProfit + potentialProfit, capitalInvested);

        /// <summary>
        /// Payback (FASE 6.10): días desde startDate hasta la primera fecha en que
        /// el capital recuperado (FIFO/COGS) alcanza el capital invertido.
        /// Null si aún no se recupera. No es ROI.
        /// </summary>
        public static int? PaybackDays(
            DateTime startDate,
            decimal capitalInvested,
            IReadOnlyList<InvestmentFifoEntry> entries,
            IReadOnlyList<InvestmentFifoSale> sales)
        {
            if (capitalInvested <= 0)
                return null;

            var pools = entries
                .Where(e => e.Quantity > 0 && e.UnitCost > 0)
                .OrderBy(e => e.EntryDate)
                .ThenBy(e => e.EntryId)
                .Select(e => new Pool(e.EntryId, e.ProductId, e.EntryDate, e.Quantity, e.UnitCost))
                .ToList();

            if (pools.Count == 0)
                return null;

            decimal recovered = 0m;
            DateTime start = startDate.Date;

            foreach (InvestmentFifoSale sale in sales.OrderBy(s => s.SaleDate).ThenBy(s => s.SaleLineId))
            {
                if (sale.Quantity <= 0)
                    continue;

                int remaining = sale.Quantity;
                foreach (Pool pool in pools)
                {
                    if (remaining <= 0)
                        break;
                    if (pool.ProductId != sale.ProductId || pool.Remaining <= 0)
                        continue;
                    if (pool.EntryDate > sale.SaleDate)
                        continue;

                    int take = Math.Min(remaining, pool.Remaining);
                    recovered += take * pool.UnitCost;
                    pool.Remaining -= take;
                    remaining -= take;
                }

                if (RoundMoney(recovered) >= RoundMoney(capitalInvested))
                {
                    int days = (sale.SaleDate.Date - start).Days;
                    return Math.Max(0, days);
                }
            }

            return null;
        }

        /// <summary>
        /// Capital recuperado (FASE 6.6): costo de unidades consumidas de las ENTRADAS
        /// por FIFO (fecha entrada ≤ fecha venta). No usa ingresos ni cobros.
        /// </summary>
        public static decimal CapitalRecoveredFifo(
            IReadOnlyList<InvestmentFifoEntry> entries,
            IReadOnlyList<InvestmentFifoSale> sales)
            => RunFifo(entries, sales).Recovered;

        /// <summary>
        /// FASE 6.6–6.8: recuperado, congelado, ingreso/COGS/ganancia atribuibles.
        /// Ganancia = ingreso atribuido − costo ENTRADA consumido (no capital recuperado).
        /// </summary>
        public static InvestmentFifoResult RunFifo(
            IReadOnlyList<InvestmentFifoEntry> entries,
            IReadOnlyList<InvestmentFifoSale> sales,
            IReadOnlyDictionary<int, decimal>? currentSalePricesByProduct = null)
        {
            var pools = entries
                .Where(e => e.Quantity > 0 && e.UnitCost > 0)
                .OrderBy(e => e.EntryDate)
                .ThenBy(e => e.EntryId)
                .Select(e => new Pool(e.EntryId, e.ProductId, e.EntryDate, e.Quantity, e.UnitCost))
                .ToList();

            if (pools.Count == 0)
                return InvestmentFifoResult.Empty;

            decimal recovered = 0m;
            decimal attributedRevenue = 0m;

            foreach (InvestmentFifoSale sale in sales.OrderBy(s => s.SaleDate).ThenBy(s => s.SaleLineId))
            {
                if (sale.Quantity <= 0)
                    continue;

                int remaining = sale.Quantity;
                decimal unitRevenue = sale.Quantity > 0
                    ? sale.Revenue / sale.Quantity
                    : 0m;

                foreach (Pool pool in pools)
                {
                    if (remaining <= 0)
                        break;
                    if (pool.ProductId != sale.ProductId)
                        continue;
                    if (pool.Remaining <= 0)
                        continue;
                    if (pool.EntryDate > sale.SaleDate)
                        continue;

                    int take = Math.Min(remaining, pool.Remaining);
                    recovered += take * pool.UnitCost;
                    attributedRevenue += take * unitRevenue;
                    pool.Remaining -= take;
                    remaining -= take;
                }
            }

            decimal frozen = 0m;
            decimal potential = 0m;
            int unitsLeft = 0;
            foreach (Pool pool in pools)
            {
                if (pool.Remaining <= 0)
                    continue;

                frozen += pool.Remaining * pool.UnitCost;
                unitsLeft += pool.Remaining;

                if (currentSalePricesByProduct != null
                    && currentSalePricesByProduct.TryGetValue(pool.ProductId, out decimal pvp)
                    && pvp > 0)
                {
                    potential += pool.Remaining * (pvp - pool.UnitCost);
                }
            }

            decimal revenue = RoundMoney(attributedRevenue);
            decimal cogs = RoundMoney(recovered);
            decimal profit = RoundMoney(revenue - cogs);

            return new InvestmentFifoResult(
                Recovered: cogs,
                Frozen: RoundMoney(frozen),
                UnitsRemaining: unitsLeft,
                AttributedRevenue: revenue,
                AttributedCogs: cogs,
                RealizedProfit: profit,
                PotentialProfit: RoundMoney(potential));
        }

        private sealed class Pool
        {
            public int EntryId { get; }
            public int ProductId { get; }
            public DateTime EntryDate { get; }
            public decimal UnitCost { get; }
            public int Remaining { get; set; }

            public Pool(int entryId, int productId, DateTime entryDate, int qty, decimal unitCost)
            {
                EntryId = entryId;
                ProductId = productId;
                EntryDate = entryDate;
                Remaining = qty;
                UnitCost = unitCost;
            }
        }
    }

    public readonly record struct InvestmentFifoResult(
        decimal Recovered,
        decimal Frozen,
        int UnitsRemaining,
        decimal AttributedRevenue = 0m,
        decimal AttributedCogs = 0m,
        decimal RealizedProfit = 0m,
        decimal PotentialProfit = 0m)
    {
        public static InvestmentFifoResult Empty { get; } = new(0m, 0m, 0);
    }

    /// <summary>ENTRADA etiquetada para FIFO de recuperación.</summary>
    public readonly record struct InvestmentFifoEntry(
        int EntryId,
        int ProductId,
        DateTime EntryDate,
        int Quantity,
        decimal UnitCost);

    /// <summary>Línea de venta para consumir pool de inversión.</summary>
    public readonly record struct InvestmentFifoSale(
        int SaleLineId,
        int ProductId,
        DateTime SaleDate,
        int Quantity,
        decimal Revenue);
}
