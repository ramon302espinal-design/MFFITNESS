using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ventas por día (FASE 9.7).</summary>
    public static class SalesByDayPolicy
    {
        public const string Definition =
            "FASE 9.7: serie diaria con Fecha, Ingresos, Ganancia, Transacciones, Unidades, Ticket. " +
            "Mejor/peor día por INGRESOS y por GANANCIA por separado — no confundir.";

        public const string ExcludeEmpty =
            "Días sin operación (TransactionCount = 0) se excluyen del ranking mejor/peor. " +
            "No inventar 'peor día' = día cerrado.";

        public const string Source =
            "Fuente: ProfitAnalyticsService.GetByDay → CAST(Ventas.Fecha AS date).";
    }

    /// <summary>Composición pura ventas por día (FASE 9.7).</summary>
    public static class SalesByDayComposer
    {
        public static SalesDayRow FromProfitDay(ProfitDayRow d)
            => new()
            {
                Date = d.Date.Date,
                TransactionCount = d.TransactionCount,
                UnitsSold = d.UnitsSold,
                RevenueTotal = d.RevenueTotal,
                RealizedProfit = d.RealizedProfit,
                MarginPct = d.MarginPct,
                AverageTicket = SalesAnalyticsMath.AverageTicket(
                    d.RevenueTotal, d.TransactionCount),
                HasReliableRealizedProfit = d.HasReliableRealizedProfit
            };

        public static SalesByDayReport Build(
            IReadOnlyList<ProfitDayRow> source,
            ProfitPeriodKind periodKind,
            DateTime? periodFrom,
            DateTime? periodToExclusive)
        {
            var days = source
                .Where(d => d.TransactionCount > 0)
                .Select(FromProfitDay)
                .OrderBy(d => d.Date)
                .ToList();

            SalesDayRow? bestRev = days
                .OrderByDescending(d => d.RevenueTotal)
                .ThenByDescending(d => d.RealizedProfit)
                .ThenBy(d => d.Date)
                .FirstOrDefault();

            SalesDayRow? bestProfit = days
                .OrderByDescending(d => d.RealizedProfit)
                .ThenByDescending(d => d.RevenueTotal)
                .ThenBy(d => d.Date)
                .FirstOrDefault();

            SalesDayRow? worstRev = days
                .OrderBy(d => d.RevenueTotal)
                .ThenBy(d => d.RealizedProfit)
                .ThenBy(d => d.Date)
                .FirstOrDefault();

            SalesDayRow? worstProfit = days
                .OrderBy(d => d.RealizedProfit)
                .ThenBy(d => d.RevenueTotal)
                .ThenBy(d => d.Date)
                .FirstOrDefault();

            return new SalesByDayReport
            {
                PeriodKind = periodKind,
                PeriodFrom = periodFrom,
                PeriodToExclusive = periodToExclusive,
                OperatingDayCount = days.Count,
                Days = days,
                BestDayByRevenue = bestRev,
                BestDayByProfit = bestProfit,
                WorstDayByRevenue = worstRev,
                WorstDayByProfit = worstProfit
            };
        }
    }
}
