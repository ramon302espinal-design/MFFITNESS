namespace BLL.Services.Crm
{
    /// <summary>Puente inversión ↔ performance de producto (FASE 8.16).</summary>
    public static class InvestmentProductPerformancePolicy
    {
        public const string Definition =
            "FASE 8.16: productos de una inversión se cruzan con clasificación FASE 8 " +
            "(Star/Opportunity/Critical/…) y P&L del período. " +
            "CapitalAssigned (FIFO inversión) ≠ InventoryCapital global del producto.";

        public const string Question =
            "¿Qué productos de una inversión fueron realmente buenos? " +
            "→ StarsCount / OpportunityCount / RiskCount + ganancia de período de vinculados.";

        public const string NoRecalcFifo =
            "No recalcula FIFO. Reutiliza InvestmentService + ProductPerformance + Classification.";
    }
}
