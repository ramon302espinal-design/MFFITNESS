using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ventas por hora (FASE 9.8).</summary>
    public static class SalesByHourPolicy
    {
        public const string Definition =
            "FASE 9.8: agregar por DATEPART(hour, Ventas.Fecha). " +
            "Fuente = DateTime.Now al registrar venta. NO inventar horarios.";

        public const string Reliability =
            "HourDataReliable=false si no hay actividad o el 100% cae en hora 0 " +
            "(sospechoso de Fecha sin hora). UI debe mostrar N/D / datos insuficientes.";

        public const string Peaks =
            "Picos separados: mayor ingreso ≠ mayor transacciones ≠ mayor unidades.";
    }

    /// <summary>Composición pura por hora (FASE 9.8).</summary>
    public static class SalesByHourComposer
    {
        public static SalesHourRow Compose(
            int hour,
            int transactions,
            int units,
            decimal revenue,
            decimal profit,
            bool reliableProfit)
            => new()
            {
                Hour = hour,
                TransactionCount = transactions,
                UnitsSold = units,
                RevenueTotal = revenue,
                RealizedProfit = reliableProfit ? profit : 0m,
                AverageTicket = SalesAnalyticsMath.AverageTicket(revenue, transactions),
                HasReliableRealizedProfit = reliableProfit
            };

        /// <summary>
        /// True si hay ≥1 bucket con txn y no todo está concentrado en hora 0.
        /// </summary>
        public static bool EvaluateReliability(IReadOnlyList<SalesHourRow> hours, out string note)
        {
            var active = hours.Where(h => h.TransactionCount > 0).ToList();
            if (active.Count == 0)
            {
                note = "Sin ventas en el período — datos insuficientes para horario.";
                return false;
            }

            int txnTotal = active.Sum(h => h.TransactionCount);
            int txnAtMidnight = active.Where(h => h.Hour == 0).Sum(h => h.TransactionCount);
            if (txnTotal > 0 && txnAtMidnight == txnTotal)
            {
                note = "Toda la actividad en 00:00 — Fecha posiblemente sin hora real. No inventar picos.";
                return false;
            }

            note = "Hora tomada de Ventas.Fecha (registro POS).";
            return true;
        }

        public static SalesByHourReport Build(
            IReadOnlyList<SalesHourRow> hours,
            ProfitPeriodKind periodKind,
            DateTime? periodFrom,
            DateTime? periodToExclusive)
        {
            var ordered = hours
                .Where(h => h.Hour is >= 0 and <= 23)
                .OrderBy(h => h.Hour)
                .ToList();

            bool reliable = EvaluateReliability(ordered, out string note);
            var active = ordered.Where(h => h.TransactionCount > 0).ToList();

            return new SalesByHourReport
            {
                PeriodKind = periodKind,
                PeriodFrom = periodFrom,
                PeriodToExclusive = periodToExclusive,
                HourDataReliable = reliable,
                ReliabilityNote = note,
                Hours = ordered,
                PeakByRevenue = reliable
                    ? active.OrderByDescending(h => h.RevenueTotal)
                        .ThenByDescending(h => h.TransactionCount)
                        .FirstOrDefault()
                    : null,
                PeakByTransactions = reliable
                    ? active.OrderByDescending(h => h.TransactionCount)
                        .ThenByDescending(h => h.RevenueTotal)
                        .FirstOrDefault()
                    : null,
                PeakByUnits = reliable
                    ? active.OrderByDescending(h => h.UnitsSold)
                        .ThenByDescending(h => h.RevenueTotal)
                        .FirstOrDefault()
                    : null
            };
        }
    }
}
