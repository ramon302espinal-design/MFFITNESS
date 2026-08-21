namespace BLL.Models.Crm
{
    /// <summary>
    /// Estado de idle de ventas (FASE 7.4). Distinto de antigüedad de entrada (7.3).
    /// Alcance = el mismo filtro de ventas de la consulta.
    /// </summary>
    public enum InventoryIdleKind
    {
        /// <summary>Hubo al menos una venta; IdleDays = días desde LastSale.</summary>
        HasSales = 0,

        /// <summary>Sin ventas; IdleDays = días desde FirstEntry (si existe).</summary>
        NeverSold = 1,

        /// <summary>Sin ventas y sin ENTRADA; IdleDays = null.</summary>
        Unknown = 2
    }

    /// <summary>
    /// Salud de capital en inventario (FASE 7.8).
    /// Independiente de FlagStockoutRisk (quiebre = problema opuesto).
    /// </summary>
    public enum InventoryHealthStatus
    {
        /// <summary>Sin stock/capital evaluable o sin fechas para decidir.</summary>
        InsufficientData = 0,

        /// <summary>Dentro del período de gracia post-primera ENTRADA.</summary>
        New = 1,

        /// <summary>Buena rotación / idle bajo.</summary>
        Healthy = 2,

        /// <summary>Demanda débil o cobertura elevada, aún no congelado material.</summary>
        Slow = 3,

        /// <summary>Capital material + idle/cobertura prolongados.</summary>
        Frozen = 4,

        /// <summary>Congelado agravado (capital alto, never-sold largo, potencial &lt; 0).</summary>
        Critical = 5
    }

    /// <summary>
    /// Fila de lectura financiera de inventario (FASE 4.5).
    /// Derivados calculados en servicio; no persistidos en Productos.
    /// </summary>
    public sealed class InventoryFinancialRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public bool Activo { get; init; }

        public int Stock { get; init; }
        public int StockMinimo { get; init; }
        public decimal UnitCost { get; init; }
        public decimal SalePrice { get; init; }

        /// <summary>Stock × costo (0 si stock≤0 o costo no definido). Alias histórico.</summary>
        public decimal InventoryCost { get; init; }

        /// <summary>
        /// Capital en inventario (FASE 7.2) = Stock × PrecioCompra.
        /// Mismo valor que <see cref="InventoryCost"/>. No es capital congelado clasificado.
        /// </summary>
        public decimal InventoryCapital { get; init; }

        /// <summary>Stock × precio (0 si stock≤0 o precio no definido). ≠ capital.</summary>
        public decimal PotentialSalesValue { get; init; }

        /// <summary>Valor potencial − capital inventario (0 si no calculable). ≠ realizada.</summary>
        public decimal PotentialProfit { get; init; }

        public int UnitsSold { get; init; }
        public decimal Revenue { get; init; }

        /// <summary>COGS solo de líneas con CostoUnitario snapshot.</summary>
        public decimal Cogs { get; init; }

        /// <summary>Ganancia realizada solo con snapshot confiable.</summary>
        public decimal RealizedProfit { get; init; }

        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public DateTime? LastSaleDate { get; init; }

        /// <summary>
        /// Días desde LastSale si hubo venta. Null si NeverSold / Unknown.
        /// No usar sola para ranking de idle: preferir <see cref="IdleDays"/>.
        /// </summary>
        public int? DaysWithoutSale { get; init; }

        /// <summary>Primera venta registrada (MIN Ventas.Fecha en alcance). FASE 7.3.</summary>
        public DateTime? FirstSaleDate { get; init; }

        /// <summary>FASE 7.4: sin ventas en el alcance de la consulta.</summary>
        public bool FlagNeverSold { get; init; }

        /// <summary>FASE 7.4: HasSales | NeverSold | Unknown.</summary>
        public InventoryIdleKind IdleKind { get; init; }

        /// <summary>
        /// Idle de ventas (FASE 7.4): días de idle efectivo (LastSale o FirstEntry).
        /// Null solo si Unknown.
        /// </summary>
        public int? IdleDays { get; init; }

        /// <summary>Días de la ventana usada para velocidad (default 30). FASE 7.5.</summary>
        public int VelocityWindowDays { get; init; }

        /// <summary>Unidades vendidas dentro de la ventana de velocidad.</summary>
        public int UnitsSoldInVelocityWindow { get; init; }

        /// <summary>Unidades / día en la ventana. 0 si no hubo ventas (no null).</summary>
        public decimal? UnitsPerDay { get; init; }

        /// <summary>UnitsPerDay × 7.</summary>
        public decimal? UnitsPerWeek { get; init; }

        /// <summary>UnitsPerDay × 30 (mes comercial).</summary>
        public decimal? UnitsPerMonth { get; init; }

        /// <summary>COGS con snapshot en la ventana de velocidad (FASE 7.6).</summary>
        public decimal CogsInVelocityWindow { get; init; }

        /// <summary>
        /// PROXY: CogsInVelocityWindow / InventoryCapital.
        /// No es rotación contable (sin inventario promedio). Null si sin capital.
        /// </summary>
        public decimal? TurnoverProxy { get; init; }

        /// <summary>PROXY: UnitsSoldInVelocityWindow / Stock. Null si stock ≤ 0.</summary>
        public decimal? UnitTurnoverProxy { get; init; }

        /// <summary>
        /// Días de cobertura (FASE 7.7) = Stock / UnitsPerDay.
        /// Null si velocidad ≤ 0 (sin demanda medible).
        /// </summary>
        public decimal? DaysOfCover { get; init; }

        /// <summary>
        /// Cobertura ≥ umbral overstock (default 90d) y hay demanda.
        /// No implica automáticamente “congelado” (eso es 7.8).
        /// </summary>
        public bool FlagOverstock { get; init; }

        /// <summary>
        /// Stock ≤ StockMinimo con velocidad &gt; 0 (riesgo de quiebre).
        /// Opuesto conceptual a capital congelado.
        /// </summary>
        public bool FlagStockoutRisk { get; init; }

        /// <summary>Clasificación de salud de capital (FASE 7.8).</summary>
        public InventoryHealthStatus HealthStatus { get; init; }

        /// <summary>Primera ENTRADA en MovimientosStock. Null si no hay historial de entrada.</summary>
        public DateTime? FirstEntryDate { get; init; }

        /// <summary>Última ENTRADA (no confundir con última venta).</summary>
        public DateTime? LatestEntryDate { get; init; }

        /// <summary>Días desde FirstEntryDate hasta asOf. Null si no hay entrada.</summary>
        public int? DaysSinceFirstEntry { get; init; }

        /// <summary>Días desde LatestEntryDate. Null si no hay entrada.</summary>
        public int? DaysSinceLatestEntry { get; init; }

        /// <summary>True si no existe ENTRADA en MovimientosStock (antigüedad N/D).</summary>
        public bool FlagNoEntryHistory { get; init; }

        public bool FlagNoCost { get; init; }
        public bool FlagNoPrice { get; init; }
        public bool FlagNoStock { get; init; }
        public bool FlagNegativeStock { get; init; }
        public bool FlagNoRotation { get; init; }
        public bool FlagUncostedSales { get; init; }
        public bool HasReliableRealizedProfit { get; init; }
    }

    public sealed class InventoryFinancialSummary
    {
        public int ProductCount { get; init; }
        public int ProductsWithStock { get; init; }
        public int ProductsNoCost { get; init; }
        public int ProductsNoPrice { get; init; }
        public int ProductsNegativeStock { get; init; }
        public int ProductsNoRotation { get; init; }
        public int ProductsNoEntryHistory { get; init; }
        public int ProductsNeverSold { get; init; }

        /// <summary>Días de ventana de velocidad aplicada al snapshot (FASE 7.5).</summary>
        public int VelocityWindowDays { get; init; }

        /// <summary>Σ COGS confiable en la ventana de velocidad.</summary>
        public decimal CogsInVelocityWindowTotal { get; init; }

        /// <summary>
        /// PROXY global: CogsInVelocityWindowTotal / InventoryCapitalTotal.
        /// Etiquetado proxy — sin inventario promedio histórico.
        /// </summary>
        public decimal? TurnoverProxy { get; init; }

        public int ProductsOverstock { get; init; }
        public int ProductsStockoutRisk { get; init; }

        public int ProductsHealthy { get; init; }
        public int ProductsSlow { get; init; }
        public int ProductsFrozen { get; init; }
        public int ProductsCritical { get; init; }
        public int ProductsNew { get; init; }

        /// <summary>Σ capital en inventario (FASE 7.2) — todo stock×costo.</summary>
        public decimal InventoryCapitalTotal { get; init; }

        public decimal HealthyCapital { get; init; }
        public decimal SlowCapital { get; init; }
        public decimal NewCapital { get; init; }

        /// <summary>Capital con HealthStatus = Frozen (naranja).</summary>
        public decimal FrozenStatusCapital { get; init; }

        /// <summary>Capital con HealthStatus = Critical (rojo).</summary>
        public decimal CriticalCapital { get; init; }

        /// <summary>
        /// Capital congelado clasificado (FASE 7.9) = Frozen + Critical.
        /// Ya NO es alias de InventoryCapitalTotal.
        /// </summary>
        public decimal FrozenCapitalTotal { get; init; }

        /// <summary>FrozenCapitalTotal / InventoryCapitalTotal × 100.</summary>
        public decimal? FrozenSharePct { get; init; }

        public decimal PotentialSalesValueTotal { get; init; }
        public decimal PotentialProfitTotal { get; init; }

        public decimal RevenueTotal { get; init; }
        public decimal CogsTotal { get; init; }
        public decimal RealizedProfitTotal { get; init; }

        public IReadOnlyList<InventoryFinancialRow> Rows { get; init; }
            = Array.Empty<InventoryFinancialRow>();
    }

    /// <summary>
    /// Capital de inventario por producto (FASE 7.2).
    /// </summary>
    public sealed class InventoryCapitalItem
    {
        public int Rank { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int Stock { get; init; }
        public decimal UnitCost { get; init; }

        /// <summary>Stock × costo.</summary>
        public decimal InventoryCapital { get; init; }

        /// <summary>Participación % sobre el capital inventario total (null si total = 0).</summary>
        public decimal? SharePct { get; init; }

        public int? DaysWithoutSale { get; init; }
        public bool FlagNoRotation { get; init; }
        public bool FlagNoCost { get; init; }
    }

    public sealed class InventoryCapitalReport
    {
        /// <summary>Σ stock × costo (productos con stock&gt;0 y costo&gt;0).</summary>
        public decimal TotalInventoryCapital { get; init; }

        public int ProductsWithInventoryCapital { get; init; }
        public int ProductsExcludedNoCostWithStock { get; init; }
        public int ProductsExcludedNoStock { get; init; }
        public int ProductsNegativeStock { get; init; }

        /// <summary>Ordenados por capital inventario descendente.</summary>
        public IReadOnlyList<InventoryCapitalItem> Items { get; init; }
            = Array.Empty<InventoryCapitalItem>();
    }

    /// <summary>
    /// FASE 7.9: capital congelado = productos Frozen o Critical (no todo el inventario).
    /// </summary>
    public sealed class FrozenCapitalItem
    {
        public int Rank { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int Stock { get; init; }
        public decimal UnitCost { get; init; }

        /// <summary>Capital del producto (status Frozen o Critical).</summary>
        public decimal FrozenCapital { get; init; }

        public InventoryHealthStatus HealthStatus { get; init; }
        public decimal? SharePct { get; init; }
        public int? IdleDays { get; init; }
        public int? DaysWithoutSale { get; init; }
        public bool FlagNoRotation { get; init; }
        public bool FlagNoCost { get; init; }
        public bool FlagNeverSold { get; init; }
    }

    /// <summary>FASE 7.9: reporte de capital congelado clasificado.</summary>
    public sealed class FrozenCapitalReport
    {
        /// <summary>Σ capital Frozen + Critical.</summary>
        public decimal TotalFrozenCapital { get; init; }

        /// <summary>Capital inventario total (denominador del %).</summary>
        public decimal TotalInventoryCapital { get; init; }

        /// <summary>TotalFrozenCapital / TotalInventoryCapital.</summary>
        public decimal? FrozenSharePct { get; init; }

        public decimal FrozenStatusCapital { get; init; }
        public decimal CriticalCapital { get; init; }

        public int ProductsWithFrozenCapital { get; init; }
        public int ProductsExcludedNoCostWithStock { get; init; }
        public int ProductsExcludedNoStock { get; init; }
        public int ProductsNegativeStock { get; init; }

        public IReadOnlyList<FrozenCapitalItem> Items { get; init; }
            = Array.Empty<FrozenCapitalItem>();
    }

    /// <summary>Resumen de buckets de capital por salud (FASE 7.9).</summary>
    public sealed class InventoryCapitalHealthReport
    {
        public decimal InventoryCapitalTotal { get; init; }
        public decimal HealthyCapital { get; init; }
        public decimal SlowCapital { get; init; }
        public decimal NewCapital { get; init; }
        public decimal FrozenStatusCapital { get; init; }
        public decimal CriticalCapital { get; init; }

        /// <summary>Frozen + Critical.</summary>
        public decimal ImmobilizedCapital { get; init; }

        public decimal? ImmobilizedSharePct { get; init; }

        public FrozenCapitalReport Frozen { get; init; } = new();
    }

    /// <summary>
    /// Valor de venta potencial y ganancia potencial por producto (FASE 4.7).
    /// </summary>
    public sealed class PotentialValueItem
    {
        public int Rank { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int Stock { get; init; }
        public decimal UnitCost { get; init; }
        public decimal SalePrice { get; init; }

        /// <summary>Capital inventario (stock × costo).</summary>
        public decimal InventoryCapital { get; init; }

        /// <summary>LEGACY FASE 4 = InventoryCapital.</summary>
        public decimal FrozenCapital { get; init; }

        public decimal PotentialSalesValue { get; init; }
        public decimal PotentialProfit { get; init; }

        /// <summary>Participación % de ganancia potencial sobre el total (null si total = 0).</summary>
        public decimal? PotentialProfitSharePct { get; init; }

        public bool FlagNoCost { get; init; }
        public bool FlagNoPrice { get; init; }
    }

    public sealed class PotentialValueReport
    {
        /// <summary>Σ stock × precio (stock&gt;0 y precio&gt;0).</summary>
        public decimal TotalPotentialSalesValue { get; init; }

        /// <summary>Σ (valor potencial − capital) solo donde ambos son calculables.</summary>
        public decimal TotalPotentialProfit { get; init; }

        /// <summary>Capital inventario alineado (mismos productos con costo+stock).</summary>
        public decimal TotalInventoryCapital { get; init; }

        /// <summary>LEGACY = TotalInventoryCapital.</summary>
        public decimal TotalFrozenCapital { get; init; }

        public int ProductsWithPotentialProfit { get; init; }
        public int ProductsExcludedNoPriceWithStock { get; init; }
        public int ProductsExcludedNoCostWithStock { get; init; }
        public int ProductsExcludedNoStock { get; init; }

        /// <summary>Ordenados por ganancia potencial descendente.</summary>
        public IReadOnlyList<PotentialValueItem> Items { get; init; }
            = Array.Empty<PotentialValueItem>();
    }
}
