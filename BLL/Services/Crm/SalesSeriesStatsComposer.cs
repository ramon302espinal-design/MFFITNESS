using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato promedios / medianas (FASE 9.6).</summary>
    public static class SalesSeriesStatsPolicy
    {
        public const string Definition =
            "FASE 9.6: promedio y mediana sobre serie diaria de ventas. " +
            "Mediana reduce distorsión por días extremos (brief TEST 7). " +
            "Promedio/día calendario ≠ promedio/día con operación.";

        public const string OperatingDays =
            "OperatingDays = días con TransactionCount > 0. " +
            "Días sin venta no entran en mediana de la serie operativa.";

        public const string NoInvent =
            "Sin puntos → stats null. No inventar promedio con N=0.";
    }

    /// <summary>Composición pura de estadísticas de serie (FASE 9.6).</summary>
    public static class SalesSeriesStatsComposer
    {
        public static SalesSeriesStats FromValues(IReadOnlyList<decimal> values)
        {
            if (values == null || values.Count == 0)
            {
                return new SalesSeriesStats
                {
                    PointCount = 0,
                    Total = 0m,
                    Average = null,
                    Median = null,
                    Min = null,
                    Max = null
                };
            }

            return new SalesSeriesStats
            {
                PointCount = values.Count,
                Total = InventoryFinancialMath.RoundMoney(values.Sum()),
                Average = SalesAnalyticsMath.Average(values),
                Median = SalesAnalyticsMath.Median(values),
                Min = InventoryFinancialMath.RoundMoney(values.Min()),
                Max = InventoryFinancialMath.RoundMoney(values.Max())
            };
        }

        public static SalesDailyStatsReport FromDays(
            IReadOnlyList<ProfitDayRow> days,
            ProfitPeriodKind periodKind,
            DateTime? periodFrom,
            DateTime? periodToExclusive)
        {
            var operating = days
                .Where(d => d.TransactionCount > 0)
                .OrderBy(d => d.Date)
                .ToList();

            var revenue = operating.Select(d => d.RevenueTotal).ToList();
            var profit = operating.Select(d => d.RealizedProfit).ToList();
            var units = operating.Select(d => (decimal)d.UnitsSold).ToList();
            var txns = operating.Select(d => (decimal)d.TransactionCount).ToList();

            int? calendarDays = SalesAnalyticsMath.CalendarDays(periodFrom, periodToExclusive);
            decimal revenueTotal = revenue.Sum();

            return new SalesDailyStatsReport
            {
                PeriodKind = periodKind,
                PeriodFrom = periodFrom,
                PeriodToExclusive = periodToExclusive,
                CalendarDays = calendarDays,
                OperatingDays = operating.Count,
                Revenue = FromValues(revenue),
                RealizedProfit = FromValues(profit),
                Units = FromValues(units),
                Transactions = FromValues(txns),
                AverageRevenuePerCalendarDay = SalesAnalyticsMath.AveragePerDay(
                    revenueTotal, calendarDays ?? 0),
                AverageRevenuePerOperatingDay = SalesAnalyticsMath.AveragePerDay(
                    revenueTotal, operating.Count)
            };
        }
    }
}
