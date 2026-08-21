namespace BLL.Models.Crm
{
    /// <summary>
    /// Estados de inversión (FASE 6.2).
    /// Compactación financiera de capital — no es una ENTRADA de stock.
    /// </summary>
    public enum InvestmentStatus
    {
        /// <summary>Declarada; aún sin capital materializado (sin ENTRADAS asignadas).</summary>
        Planificada = 0,

        /// <summary>Tiene capital invertido; aún hay pendiente y/o stock atribuible.</summary>
        Activa = 1,

        /// <summary>Capital recuperado (COGS atribuido) ≥ capital invertido; puede quedar stock.</summary>
        Recuperada = 2,

        /// <summary>Cierre operativo (manual o sin stock atribuible restante).</summary>
        Cerrada = 3,

        /// <summary>Resultado con ganancia realizada &lt; 0 (puede coexistir con Activa/Cerrada en UI).</summary>
        ConPerdida = 4
    }

    /// <summary>
    /// Cabecera de inversión (FASE 6.2). Persistencia en etapa 6.3+.
    /// Métricas monetarias se calculan en servicio; no se inventan en el Form.
    /// </summary>
    public sealed class Investment
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? CloseDate { get; init; }
        public InvestmentStatus Status { get; init; }
        public string? Notes { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    /// <summary>
    /// Vínculo inversión → ENTRADA de stock (FASE 6.3).
    /// v1: un MovimientosStock solo puede estar en una inversión.
    /// </summary>
    public sealed class InvestmentLine
    {
        public int Id { get; init; }
        public int InvestmentId { get; init; }
        public int StockMovementId { get; init; }
        public DateTime AssignedAt { get; init; }

        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal? UnitCost { get; init; }
        public decimal? LineCapital { get; init; }
        public DateTime EntryDate { get; init; }
        public string? EntryDescription { get; init; }
        public string MovementType { get; init; } = string.Empty;
    }

    /// <summary>
    /// Producto dentro de una inversión (FASE 6.4).
    /// Agregado desde ENTRADAS asignadas; ventas/recuperación en 6.6+.
    /// </summary>
    public sealed class InvestmentProductRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        /// <summary>Σ cantidad de ENTRADAS asignadas.</summary>
        public int QuantityPurchased { get; init; }

        /// <summary>Costo unitario promedio ponderado de las líneas con costo.</summary>
        public decimal? AverageUnitCost { get; init; }

        /// <summary>Σ CostoTotal / capital de líneas con costo.</summary>
        public decimal CapitalAssigned { get; init; }

        public int EntryCount { get; init; }
        public int EntriesWithoutCost { get; init; }
        public bool HasReliableCost { get; init; }
    }

    /// <summary>
    /// Resumen de lectura financiera de una inversión (derivado; FASE 6.5–6.10).
    /// </summary>
    public sealed class InvestmentSummary
    {
        public int InvestmentId { get; init; }
        public string Name { get; init; } = string.Empty;
        public InvestmentStatus Status { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? CloseDate { get; init; }

        /// <summary>Σ costo de ENTRADAS asignadas.</summary>
        public decimal CapitalInvested { get; init; }

        /// <summary>Σ COGS de ventas atribuibles (no Σ ingresos).</summary>
        public decimal CapitalRecovered { get; init; }

        /// <summary>Invertido − Recuperado (puede ≠ congelado si hay mermas/ajustes).</summary>
        public decimal CapitalPending { get; init; }

        /// <summary>Costo del inventario restante atribuible a esta inversión.</summary>
        public decimal FrozenCapital { get; init; }

        /// <summary>Ingreso atribuible − COGS atribuible.</summary>
        public decimal RealizedProfit { get; init; }

        /// <summary>Ganancia latente del stock atribuible restante.</summary>
        public decimal PotentialProfit { get; init; }

        /// <summary>RealizedProfit / CapitalInvested × 100 (null si capital ≤ 0).</summary>
        public decimal? RoiRealizedPct { get; init; }

        /// <summary>PotentialProfit / CapitalInvested × 100.</summary>
        public decimal? RoiPotentialPct { get; init; }

        /// <summary>(Realized + Potential) / CapitalInvested × 100.</summary>
        public decimal? RoiProjectedPct { get; init; }

        /// <summary>CapitalRecovered / CapitalInvested × 100.</summary>
        public decimal? RecoveryPct { get; init; }

        public int? DaysActive { get; init; }
        public int? PaybackDays { get; init; }

        public bool HasReliableCost { get; init; }
        public bool IsLoss { get; init; }
    }

    /// <summary>Criterios de ranking de inversiones (FASE 6.12). No hay un único “mejor”.</summary>
    public enum InvestmentRankKind
    {
        ByRealizedProfit = 0,
        ByRoiRealized = 1,
        ByCapitalInvested = 2,
        ByRecoveryPct = 3,
        ByPaybackSpeed = 4,
        ByFrozenCapitalAsc = 5,
        ByPotentialProfit = 6,
        ByProjectedRoi = 7,

        /// <summary>FASE 7.13: mayor capital atrapado (FIFO restante) primero.</summary>
        ByFrozenCapitalDesc = 8
    }

    /// <summary>Fila de ranking de inversión.</summary>
    public sealed class InvestmentRankRow
    {
        public int Rank { get; init; }
        public InvestmentRankKind Kind { get; init; }
        public InvestmentSummary Summary { get; init; } = null!;
        public string SortLabel { get; init; } = string.Empty;
    }
}
