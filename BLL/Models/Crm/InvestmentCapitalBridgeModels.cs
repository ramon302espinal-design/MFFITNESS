namespace BLL.Models.Crm
{
    /// <summary>
    /// Producto de una inversión cruzado con salud de inventario (FASE 7.13).
    /// FrozenCapital de inversión (FIFO) ≠ InventoryCapital global del producto.
    /// </summary>
    public sealed class InvestmentProductHealthRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal CapitalAssignedInInvestment { get; init; }
        public decimal ProductInventoryCapital { get; init; }
        public InventoryHealthStatus HealthStatus { get; init; }
        public int? IdleDays { get; init; }
        public bool FlagNeverSold { get; init; }
        public bool FlagStockoutRisk { get; init; }
    }

    /// <summary>Inversión con capital atrapado + salud de productos vinculados.</summary>
    public sealed class InvestmentTrappedCapitalRow
    {
        public int Rank { get; init; }
        public InvestmentSummary Summary { get; init; } = null!;

        /// <summary>Alias de Summary.FrozenCapital (FIFO FASE 6).</summary>
        public decimal TrappedCapital { get; init; }

        public int ProductsLinked { get; init; }
        public int ProductsFrozenOrCritical { get; init; }

        public IReadOnlyList<InvestmentProductHealthRow> Products { get; init; }
            = Array.Empty<InvestmentProductHealthRow>();
    }

    public sealed class InvestmentCapitalBridgeReport
    {
        public decimal TotalTrappedCapital { get; init; }
        public decimal GlobalImmobilizedCapital { get; init; }
        public int InvestmentsWithTrappedCapital { get; init; }

        /// <summary>Ordenados por capital atrapado descendente.</summary>
        public IReadOnlyList<InvestmentTrappedCapitalRow> Investments { get; init; }
            = Array.Empty<InvestmentTrappedCapitalRow>();
    }
}
