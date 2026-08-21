using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ventas por producto (FASE 9.10).</summary>
    public static class SalesByProductPolicy
    {
        public const string Definition =
            "FASE 9.10: por producto — Unidades, Ingresos, Ganancia, Margen, ROI, " +
            "Transacciones, Ticket asociado, Tendencia MoM, Clase FASE 8. " +
            "Métricas etiquetadas por separado.";

        public const string TicketNote =
            "Ticket asociado = Ingresos producto / tickets que lo incluyen (DISTINCT VentaId). " +
            "≠ ticket global de la tienda.";

        public const string Fase8 =
            "PerformanceClass y Trend reutilizan FASE 8 — no recalcular estrella aquí.";
    }

    /// <summary>Composición pura ventas×producto (FASE 9.10).</summary>
    public static class SalesByProductComposer
    {
        public static SalesProductRow Compose(
            ProfitGroupRow profit,
            decimal totalRevenue,
            ProductTrendRow? trend,
            ProductClassificationRow? classification,
            int rank)
        {
            int txns = profit.TransactionCount;
            return new SalesProductRow
            {
                Rank = rank,
                ProductId = profit.ProductId ?? 0,
                ProductName = profit.ProductName ?? profit.GroupName,
                Category = classification?.Category ?? string.Empty,
                UnitsSold = profit.UnitsSold,
                RevenueTotal = profit.RevenueTotal,
                RealizedProfit = profit.RealizedProfit,
                MarginPct = profit.MarginPct,
                RoiPct = profit.RoiPct,
                TransactionCount = txns,
                AverageTicket = SalesAnalyticsMath.AverageTicket(profit.RevenueTotal, txns),
                UnitsPerTransaction = SalesAnalyticsMath.UnitsPerTransaction(profit.UnitsSold, txns),
                RevenueSharePct = SalesAnalyticsMath.SharePct(profit.RevenueTotal, totalRevenue),
                Trend = trend?.PrimaryTrend,
                UnitsChangePct = trend?.UnitsChangePct,
                PerformanceClass = classification?.Class
            };
        }

        public static SalesByProductReport Build(
            IReadOnlyList<ProfitGroupRow> products,
            IReadOnlyDictionary<int, ProductTrendRow> trends,
            IReadOnlyDictionary<int, ProductClassificationRow> classifications,
            ProfitPeriodKind periodKind)
        {
            decimal totalRevenue = products.Sum(p => p.RevenueTotal);
            decimal totalProfit = products.Sum(p => p.RealizedProfit);

            var rows = new List<SalesProductRow>(products.Count);
            int rank = 0;
            foreach (ProfitGroupRow p in products
                         .OrderByDescending(x => x.RevenueTotal)
                         .ThenByDescending(x => x.RealizedProfit)
                         .ThenBy(x => x.GroupName))
            {
                rank++;
                int id = p.ProductId ?? 0;
                trends.TryGetValue(id, out ProductTrendRow? tr);
                classifications.TryGetValue(id, out ProductClassificationRow? cls);
                rows.Add(Compose(p, totalRevenue, tr, cls, rank));
            }

            return new SalesByProductReport
            {
                PeriodKind = periodKind,
                ProductCount = rows.Count,
                TotalRevenue = InventoryFinancialMath.RoundMoney(totalRevenue),
                TotalRealizedProfit = InventoryFinancialMath.RoundMoney(totalProfit),
                Products = rows
            };
        }
    }
}
