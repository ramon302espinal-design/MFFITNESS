using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Fórmulas puras del motor de ventas (FASE 9.2). Sin I/O.
    /// </summary>
    public static class SalesAnalyticsMath
    {
        /// <summary>
        /// Variación % = (current − previous) / previous × 100.
        /// Previous = 0 → null (N/D).
        /// </summary>
        public static decimal? VariationPct(decimal current, decimal previous)
        {
            if (previous == 0m)
                return null;
            return InventoryFinancialMath.RoundPct((current - previous) / previous * 100m);
        }

        /// <summary>Ticket = ingresos / transacciones. Transacciones ≤ 0 → null.</summary>
        public static decimal? AverageTicket(decimal revenue, int transactions)
        {
            if (transactions <= 0)
                return null;
            return InventoryFinancialMath.RoundMoney(revenue / transactions);
        }

        /// <summary>Unidades por ticket. Transacciones ≤ 0 → null.</summary>
        public static decimal? UnitsPerTransaction(int units, int transactions)
        {
            if (transactions <= 0)
                return null;
            return InventoryFinancialMath.RoundPct((decimal)units / transactions);
        }

        /// <summary>Promedio = suma / N. N ≤ 0 → null.</summary>
        public static decimal? Average(IReadOnlyList<decimal> values)
        {
            if (values == null || values.Count == 0)
                return null;
            return InventoryFinancialMath.RoundMoney(values.Sum() / values.Count);
        }

        /// <summary>
        /// Mediana (evita distorsión por extremos). N ≤ 0 → null.
        /// N par: promedio de los dos centrales.
        /// </summary>
        public static decimal? Median(IReadOnlyList<decimal> values)
        {
            if (values == null || values.Count == 0)
                return null;

            var sorted = values.OrderBy(v => v).ToList();
            int n = sorted.Count;
            if (n % 2 == 1)
                return InventoryFinancialMath.RoundMoney(sorted[n / 2]);

            return InventoryFinancialMath.RoundMoney(
                (sorted[n / 2 - 1] + sorted[n / 2]) / 2m);
        }

        /// <summary>Promedio diario = total / días. Días ≤ 0 → null.</summary>
        public static decimal? AveragePerDay(decimal total, int days)
        {
            if (days <= 0)
                return null;
            return InventoryFinancialMath.RoundMoney(total / days);
        }

        /// <summary>Participación % = part / total × 100. Total ≤ 0 → null.</summary>
        public static decimal? SharePct(decimal part, decimal total)
        {
            if (total <= 0m)
                return null;
            return InventoryFinancialMath.RoundPct(part / total * 100m);
        }

        /// <summary>Días calendario del rango [from, toExclusive). Null si rango incompleto.</summary>
        public static int? CalendarDays(DateTime? from, DateTime? toExclusive)
        {
            if (!from.HasValue || !toExclusive.HasValue)
                return null;
            int days = (toExclusive.Value.Date - from.Value.Date).Days;
            return days > 0 ? days : null;
        }
    }
}
