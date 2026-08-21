namespace BLL.Models.Crm
{
    /// <summary>
    /// Resumen base de ventas del período (FASE 9.2).
    /// Separación explícita Ventas / Unidades / Ingresos / Ganancia / Margen / ROI / Ticket.
    /// </summary>
    public sealed class SalesSummary
    {
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }
        public ProfitPeriodKind PeriodKind { get; init; }

        /// <summary>VENTAS = cantidad de transacciones (tickets).</summary>
        public int TransactionCount { get; init; }

        /// <summary>UNIDADES vendidas.</summary>
        public int UnitsSold { get; init; }

        /// <summary>INGRESOS = Σ Subtotal líneas.</summary>
        public decimal RevenueTotal { get; init; }

        /// <summary>Σ Ventas.Total cabecera (referencia; ≠ ingresos de líneas).</summary>
        public decimal SalesHeaderTotal { get; init; }

        /// <summary>GANANCIA realizada confiable (0 si sin costo).</summary>
        public decimal RealizedProfit { get; init; }

        public decimal Cogs { get; init; }
        public decimal RevenueWithCost { get; init; }

        /// <summary>MARGEN % — null si no confiable.</summary>
        public decimal? MarginPct { get; init; }

        /// <summary>ROI producto % — null si no confiable. ≠ ROI inversión.</summary>
        public decimal? RoiPct { get; init; }

        /// <summary>TICKET = Ingresos / Transacciones.</summary>
        public decimal? AverageTicket { get; init; }

        /// <summary>Unidades / Transacciones.</summary>
        public decimal? UnitsPerTransaction { get; init; }

        public bool HasReliableRealizedProfit { get; init; }
        public decimal? CostCoveragePct { get; init; }

        /// <summary>Cobrado en venta (flujo) — no es ingreso de líneas.</summary>
        public decimal CollectedAtSale { get; init; }
    }
}
