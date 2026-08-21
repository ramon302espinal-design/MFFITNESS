using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de comparaciones de ventas (FASE 9.4).</summary>
    public static class SalesComparisonPolicy
    {
        public const string Definition =
            "FASE 9.4: comparar período actual vs período anterior equivalente " +
            "(hoy/ayer, 7d/7d prev, mes/mes ant., trimestre/trim. ant., año/año ant.). " +
            "No confundir con comparación estacional mismo-mes YoY (9.16).";

        public const string Metrics =
            "Deltas etiquetados por separado: Ingresos, Ganancia, Unidades, Transacciones, Ticket, Margen. " +
            "Una métrica ≠ otra.";

        public const string NoPair =
            "Si el preset no tiene par (AllTime/Custom/PreviousYear solo), HasComparablePeriod=false.";
    }

    /// <summary>Composición pura de comparación (FASE 9.4).</summary>
    public static class SalesComparisonComposer
    {
        public static SalesMetricDelta Delta(decimal current, decimal previous)
            => new()
            {
                Current = current,
                Previous = previous,
                VariationPct = SalesAnalyticsMath.VariationPct(current, previous)
            };

        public static SalesMetricDelta? MarginDelta(decimal? current, decimal? previous)
        {
            if (!current.HasValue || !previous.HasValue)
                return null;
            return Delta(current.Value, previous.Value);
        }

        public static SalesComparisonReport Build(
            ProfitPeriodKind periodKind,
            ProfitPeriodRange currentRange,
            ProfitPeriodRange previousRange,
            SalesSummary current,
            SalesSummary previous)
        {
            return new SalesComparisonReport
            {
                PeriodKind = periodKind,
                CurrentFrom = currentRange.From,
                CurrentToExclusive = currentRange.ToExclusive,
                PreviousFrom = previousRange.From,
                PreviousToExclusive = previousRange.ToExclusive,
                Current = current,
                Previous = previous,
                Revenue = Delta(current.RevenueTotal, previous.RevenueTotal),
                RealizedProfit = Delta(current.RealizedProfit, previous.RealizedProfit),
                Units = Delta(current.UnitsSold, previous.UnitsSold),
                Transactions = Delta(current.TransactionCount, previous.TransactionCount),
                Ticket = Delta(
                    current.AverageTicket ?? 0m,
                    previous.AverageTicket ?? 0m),
                Margin = MarginDelta(current.MarginPct, previous.MarginPct),
                HasComparablePeriod = true
            };
        }
    }
}
