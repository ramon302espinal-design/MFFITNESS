using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ticket promedio (FASE 9.9).</summary>
    public static class SalesTicketPolicy
    {
        public const string Definition =
            "TICKET = Ingresos (Σ Subtotal) / Transacciones. " +
            "≠ MontoPagado / cobrado. ≠ ganancia. ≠ ROI.";

        public const string UnitsPerTxn =
            "UNIDADES/TRANSACCIÓN = Unidades / Transacciones. " +
            "Señal de cross-sell / bundles — no compra automática.";

        public const string Comparison =
            "Comparar ticket actual vs período equivalente (9.4). " +
            "Previous txn=0 o ticket null → N/D.";
    }

    /// <summary>Composición pura de ticket (FASE 9.9).</summary>
    public static class SalesTicketComposer
    {
        public static SalesTicketReport Build(
            ProfitPeriodKind periodKind,
            SalesSummary current,
            SalesSummary? previous,
            ProfitPeriodRange currentRange,
            ProfitPeriodRange? previousRange)
        {
            decimal? ticketVar = null;
            decimal? uptVar = null;
            bool comparable = previous != null
                && previous.AverageTicket.HasValue
                && previous.TransactionCount > 0;

            if (comparable)
            {
                ticketVar = SalesAnalyticsMath.VariationPct(
                    current.AverageTicket ?? 0m,
                    previous!.AverageTicket!.Value);
                uptVar = SalesAnalyticsMath.VariationPct(
                    current.UnitsPerTransaction ?? 0m,
                    previous.UnitsPerTransaction ?? 0m);
            }

            return new SalesTicketReport
            {
                PeriodKind = periodKind,
                CurrentFrom = currentRange.From,
                CurrentToExclusive = currentRange.ToExclusive,
                PreviousFrom = previousRange?.From,
                PreviousToExclusive = previousRange?.ToExclusive,
                CurrentTransactions = current.TransactionCount,
                CurrentUnits = current.UnitsSold,
                CurrentRevenue = current.RevenueTotal,
                CurrentTicket = current.AverageTicket,
                CurrentUnitsPerTransaction = current.UnitsPerTransaction,
                PreviousTransactions = previous?.TransactionCount ?? 0,
                PreviousRevenue = previous?.RevenueTotal ?? 0m,
                PreviousTicket = previous?.AverageTicket,
                PreviousUnitsPerTransaction = previous?.UnitsPerTransaction,
                TicketVariationPct = ticketVar,
                UnitsPerTxnVariationPct = uptVar,
                HasComparablePrevious = comparable,
                TicketLabel = SalesVariationMath.Label(ticketVar),
                UnitsPerTxnLabel = comparable
                    ? SalesVariationMath.Label(uptVar)
                    : null
            };
        }
    }
}
