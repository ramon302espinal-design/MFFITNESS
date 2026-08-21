namespace BLL.Services.Crm
{
    /// <summary>Contrato de señales Dashboard FASE 8.18.</summary>
    public static class ProductPerformanceDashboardPolicy
    {
        public const string Definition =
            "FASE 8.18: Dashboard consume buckets FASE 8 (Star/Healthy/Opportunity/Slow/Critical) " +
            "y tops por métrica única (unidades, ganancia, ROI, margen, rotación). " +
            "Sin score de producto ni lógica financiera en Forms.";

        public const string Buckets =
            "Tarjetas: ESTRELLA · SALUDABLE · LENTOS · CRÍTICOS. " +
            "OPORTUNIDAD se reporta aparte (count + lista). " +
            "≠ buckets de HealthStatus FASE 7.";

        public const string PortfolioScore =
            "PortfolioHealthScore 0–100 = proporción (Star+Healthy+Opportunity+New) / clasificables " +
            "menos penalización por Critical. Explicable; no ranking de producto.";

        public const string Tops =
            "TOP VENTAS / GANANCIA / ROI / MARGEN / ROTACIÓN = ProductPerformanceRanker " +
            "(una métrica por lista). ≠ producto estrella automático.";
    }
}
