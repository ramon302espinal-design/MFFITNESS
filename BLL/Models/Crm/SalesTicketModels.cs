namespace BLL.Models.Crm
{
    /// <summary>Ticket y unidades/transacción del período (FASE 9.9).</summary>
    public sealed class SalesTicketReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? CurrentFrom { get; init; }
        public DateTime? CurrentToExclusive { get; init; }
        public DateTime? PreviousFrom { get; init; }
        public DateTime? PreviousToExclusive { get; init; }

        public int CurrentTransactions { get; init; }
        public int CurrentUnits { get; init; }
        public decimal CurrentRevenue { get; init; }

        /// <summary>Ingresos / transacciones (null si txn = 0).</summary>
        public decimal? CurrentTicket { get; init; }

        /// <summary>Unidades / transacciones.</summary>
        public decimal? CurrentUnitsPerTransaction { get; init; }

        public int PreviousTransactions { get; init; }
        public decimal PreviousRevenue { get; init; }
        public decimal? PreviousTicket { get; init; }
        public decimal? PreviousUnitsPerTransaction { get; init; }

        /// <summary>Variación % del ticket (null si sin base).</summary>
        public decimal? TicketVariationPct { get; init; }

        public decimal? UnitsPerTxnVariationPct { get; init; }

        public bool HasComparablePrevious { get; init; }

        public SalesVariationLabel TicketLabel { get; init; } = null!;
        public SalesVariationLabel? UnitsPerTxnLabel { get; init; }
    }
}
