namespace BLL.Services.Crm
{
    /// <summary>Puente capital (FASE 7) ↔ clasificación de producto (FASE 8.17).</summary>
    public static class ProductCapitalPerformancePolicy
    {
        public const string Definition =
            "FASE 8.17: el capital de inventario se agrupa por ProductPerformanceClass (FASE 8) " +
            "sin recalcular salud FASE 7 ni FIFO. " +
            "ImmobilizedCapital = InventoryCapital si HealthStatus Frozen o Critical; si no, 0. " +
            "≠ todo InventoryCapital. Capital de Star ≠ capital de riesgo.";

        public const string Question =
            "¿Cuánto capital está en estrellas vs oportunidad vs riesgo vs lento? " +
            "→ buckets por Class + TopImmobilized con etiqueta de clase.";

        public const string Separation =
            "HealthStatus (FASE 7) ≠ Class (FASE 8). " +
            "Critical class puede incluir Frozen con agravantes; " +
            "Frozen sin agravante sigue siendo Slow en FASE 8.";

        public const string NoScore =
            "Sin score compuesto ni pesos. Solo sumas de capital por clase.";
    }
}
