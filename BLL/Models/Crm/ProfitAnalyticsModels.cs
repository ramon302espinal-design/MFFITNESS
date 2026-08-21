namespace BLL.Models.Crm
{
    /// <summary>
    /// Resumen financiero del motor de ganancias (FASE 5.5).
    /// Ventas ≠ Ingresos ≠ COGS ≠ Ganancia ≠ Margen ≠ ROI ≠ Cobrado.
    /// </summary>
    public sealed class ProfitSummary
    {
        public DateTime? PeriodFrom { get; init; }
        public DateTime? PeriodToExclusive { get; init; }

        /// <summary>Cantidad de tickets/ventas en el período (cabecera Ventas).</summary>
        public int TransactionCount { get; init; }

        /// <summary>Σ Ventas.Total (cabecera).</summary>
        public decimal SalesHeaderTotal { get; init; }

        /// <summary>Σ DetalleVentas.Subtotal (ingreso real de líneas).</summary>
        public decimal RevenueTotal { get; init; }

        /// <summary>Ingreso solo de líneas con CostoUnitario (base de margen).</summary>
        public decimal RevenueWithCost { get; init; }

        public int UnitsSold { get; init; }

        /// <summary>COGS solo líneas con snapshot.</summary>
        public decimal Cogs { get; init; }

        /// <summary>Ganancia realizada confiable (RevenueWithCost − Cogs).</summary>
        public decimal RealizedProfit { get; init; }

        /// <summary>
        /// Margen % (FASE 5.6) = RealizedProfit / RevenueWithCost × 100.
        /// Null si no hay líneas con costo o ingreso con costo ≤ 0.
        /// Puede ser negativo (venta bajo costo). No confundir con ROI.
        /// </summary>
        public decimal? MarginPct { get; init; }

        /// <summary>
        /// ROI % (FASE 5.7) = RealizedProfit / Cogs × 100.
        /// Null si no hay COGS confiable. Puede ser negativo.
        /// No confundir con margen ni con cobros en caja.
        /// </summary>
        public decimal? RoiPct { get; init; }

        public int LinesWithCost { get; init; }
        public int LinesWithoutCost { get; init; }

        /// <summary>Cobertura % de líneas con costo (null si no hay líneas).</summary>
        public decimal? CostCoveragePct { get; init; }

        public bool HasReliableRealizedProfit { get; init; }

        /// <summary>Σ MontoPagado de ventas del período (flujo parcial al momento de venta).</summary>
        public decimal CollectedAtSale { get; init; }

        /// <summary>Σ Saldo pendiente de ventas del período (CxC al momento de venta).</summary>
        public decimal ReceivableAtSale { get; init; }

        /// <summary>Instantáneo (no filtrado por período): ganancia potencial inventario.</summary>
        public decimal PotentialProfit { get; init; }

        /// <summary>Instantáneo: capital congelado clasificado (Frozen+Critical, FASE 7.9).</summary>
        public decimal FrozenCapital { get; init; }

        /// <summary>Instantáneo: capital inventario total (stock × costo).</summary>
        public decimal InventoryCapital { get; init; }

        /// <summary>Instantáneo: valor potencial de venta del stock.</summary>
        public decimal PotentialSalesValue { get; init; }
    }

    /// <summary>Presets de período (FASE 5.8 + FASE 9.3). Fecha = Ventas.Fecha.</summary>
    public enum ProfitPeriodKind
    {
        AllTime = 0,
        Today = 1,
        Yesterday = 2,
        Last7Days = 3,
        Last30Days = 4,
        ThisMonth = 5,
        PreviousMonth = 6,
        ThisYear = 7,
        Custom = 8,

        /// <summary>FASE 9.3: últimos 14 días (incluye asOf).</summary>
        Last14Days = 9,

        /// <summary>FASE 9.3: trimestre calendario que contiene asOf (Q1–Q4).</summary>
        ThisQuarter = 10,

        /// <summary>FASE 9.3: semestre calendario (Ene–Jun / Jul–Dic).</summary>
        ThisSemester = 11,

        /// <summary>FASE 9.3: año calendario anterior completo.</summary>
        PreviousYear = 12
    }

    /// <summary>Rango resuelto [From, ToExclusive).</summary>
    public readonly record struct ProfitPeriodRange(DateTime? From, DateTime? ToExclusive);

    /// <summary>Fila agrupada de ganancia realizada (producto o categoría).</summary>
    public sealed class ProfitGroupRow
    {
        public int Rank { get; init; }
        public int? ProductId { get; init; }
        public string? ProductName { get; init; }
        public int? CategoryId { get; init; }
        public string GroupName { get; init; } = string.Empty;

        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RevenueWithCost { get; init; }
        public decimal Cogs { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        /// <summary>Tickets distintos que incluyen el grupo (FASE 9.10).</summary>
        public int TransactionCount { get; init; }

        /// <summary>Participación % sobre ganancia realizada del conjunto (null si total ≤ 0).</summary>
        public decimal? ProfitSharePct { get; init; }

        public int LinesWithCost { get; init; }
        public int LinesWithoutCost { get; init; }
        public bool HasReliableRealizedProfit { get; init; }
        public bool IsLoss { get; init; }
    }

    /// <summary>Ganancia por día calendario (FASE 5.8).</summary>
    public sealed class ProfitDayRow
    {
        public DateTime Date { get; init; }
        public int TransactionCount { get; init; }
        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RevenueWithCost { get; init; }
        public decimal Cogs { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }
        public decimal CumulativeRealizedProfit { get; init; }
        public int LinesWithCost { get; init; }
        public int LinesWithoutCost { get; init; }
        public bool HasReliableRealizedProfit { get; init; }
    }
}
