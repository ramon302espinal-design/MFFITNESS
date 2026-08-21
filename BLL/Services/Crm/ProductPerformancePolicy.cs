namespace BLL.Services.Crm
{
    /// <summary>
    /// Contrato oficial de performance de producto (FASE 8.2).
    /// Rankings por métrica = 8.3+; estrella global / score = posterior (sin pesos).
    /// </summary>
    public static class ProductPerformancePolicy
    {
        public const string StarRule =
            "Producto estrella ≠ producto más vendido. " +
            "Estrella requiere múltiples dimensiones (impacto + eficiencia + riesgo). " +
            "FASE 8.2 solo define métricas base; no clasifica estrella ni asigna score.";

        public const string ImpactDefinition =
            "IMPACTO (absoluto): Unidades, Ingresos (Subtotal), Ganancia realizada, Capital inventario. " +
            "Miden cuánto mueve el negocio, no qué tan eficiente es.";

        public const string EfficiencyDefinition =
            "EFICIENCIA (relativa): Margen % (Ganancia/Ingreso c/costo), ROI producto (Ganancia/COGS), " +
            "Rotación PROXY (FASE 7.6) y UnitsPerDay (FASE 7.5). " +
            "ROI alto con ganancia mínima ≠ mayor impacto.";

        public const string RiskDefinition =
            "RIESGO: Capital inmovilizado (Frozen∪Critical FASE 7.9), IdleDays, Overstock, " +
            "FlagStockoutRisk, potencial &lt; 0. " +
            "No todo capital alto es riesgo si rota bien.";

        public const string UnitsDefinition =
            "Unidades = Σ DetalleVentas.Cantidad en período (Ventas.Fecha).";

        public const string RevenueDefinition =
            "Ingresos = Σ DetalleVentas.Subtotal. ≠ MontoPagado / cobrado en caja.";

        public const string ProfitDefinition =
            "Ganancia realizada = Σ (Subtotal − Cant×CostoUnitario) solo líneas con snapshot. " +
            "Sin costo confiable → no inventar ganancia.";

        public const string MarginDefinition =
            "Margen % = Ganancia / RevenueWithCost × 100. Null si ingreso c/costo ≤ 0. ≠ ROI.";

        public const string RoiProductDefinition =
            "ROI producto (FASE 5.7) = Ganancia / COGS × 100. Null si COGS ≤ 0. " +
            "≠ ROI inversión (FASE 6: Ganancia / CapitalInvertido).";

        public const string RotationDefinition =
            "Rotación = UnitsPerDay y TurnoverProxy (FASE 7). PROXY — no inventario promedio.";

        public const string CapitalDefinition =
            "Capital = Stock × PrecioCompra. Instantáneo (no filtrado por período de P&amp;L).";

        public const string ImmobilizedDefinition =
            "Capital congelado por producto = InventoryCapital si HealthStatus Frozen o Critical; " +
            "si no, 0. ≠ capital inventario total.";

        public const string PotentialDefinition =
            "Ganancia potencial = Stock×PVP − capital. ≠ ganancia realizada.";

        public const string PeriodDefinition =
            "P&amp;L usa ProfitPeriodKind (Hoy/7d/30d/mes/mes ant./año/custom). " +
            "Capital/salud/rotación son snapshot a asOf. Trimestre = pendiente.";

        public const string ScoreNote =
            "ProductPerformanceScore: arquitectura reservada. " +
            "NO implementar pesos ni score compuesto en FASE 8.2. " +
            "Cuando exista, debe ser explicable (no caja negra).";

        public const string DataQualityNote =
            "Sin costo → margen/ROI/ganancia no confiables. " +
            "Sin ventas en período → impacto 0 (no inventar). " +
            "Anulación DELETE deja de contar. Sin módulo de devoluciones.";

        public const string ComposeNote =
            "ProductPerformanceService une ProfitAnalytics (período) + InventoryFinancial (snapshot) " +
            "por ProductId. Una pasada agregada — evitar N+1.";

        public const string UnitsRankingDefinition =
            "Ranking TOP UNIDADES (FASE 8.3) = orden por UnitsSold desc. " +
            "Etiqueta: TOP VENTAS (unidades). ≠ TOP ingresos ≠ TOP ganancia ≠ producto estrella. " +
            "Solo productos con UnitsSold > 0. Empate: mayor RevenueTotal, luego nombre.";

        public const string RevenueRankingDefinition =
            "Ranking TOP INGRESOS (FASE 8.4) = orden por RevenueTotal (Σ Subtotal) desc. " +
            "≠ unidades ≠ ganancia ≠ cobrado en caja ≠ producto estrella. " +
            "Solo RevenueTotal > 0. Empate: mayor UnitsSold, luego nombre.";

        public const string ProfitRankingDefinition =
            "Ranking TOP GANANCIA (FASE 8.5) = orden por RealizedProfit desc. " +
            "Solo líneas con costo confiable (HasReliableRealizedProfit). " +
            "≠ ROI ≠ margen ≠ ingresos ≠ producto estrella. " +
            "Empate: mayor RevenueTotal, luego nombre.";

        public const string MarginRankingDefinition =
            "Ranking TOP MARGEN (FASE 8.6) = orden por MarginPct desc. " +
            "Requiere MarginPct con costo confiable. " +
            "≠ ganancia absoluta ≠ ROI ≠ producto estrella. " +
            "Empate: mayor RealizedProfit, luego nombre. " +
            "Margen alto + ganancia baja = eficiencia, no impacto.";

        public const string RoiRankingDefinition =
            "Ranking TOP ROI (FASE 8.7) = orden por RoiPct (Ganancia/COGS) desc. " +
            "Requiere RoiPct válido y COGS > 0 (HasReliableRealizedProfit). " +
            "≠ margen ≠ ganancia absoluta ≠ ROI inversión (FASE 6) ≠ estrella. " +
            "Empate: mayor RealizedProfit, luego nombre. " +
            "ROI alto con capital/COGS mínimo = eficiencia, no impacto.";

        public const string RotationRankingDefinition =
            "Ranking TOP ROTACIÓN (FASE 8.8) = orden por TurnoverProxy desc (FASE 7.6 PROXY). " +
            "Solo TurnoverProxy con valor. ≠ rotación contable (sin inventario promedio). " +
            "UnitsPerDay es ranking paralelo (velocidad). ≠ estrella. " +
            "Empate Turnover: mayor UnitsPerDay, luego nombre.";

        public const string CapitalRankingDefinition =
            "Ranking TOP CAPITAL (FASE 8.9) = orden por InventoryCapital (Stock×PrecioCompra) desc. " +
            "Snapshot instantáneo — no filtrado por período P&amp;L. " +
            "Solo InventoryCapital > 0. ≠ capital congelado ≠ mejor producto ≠ estrella. " +
            "Empate: mayor Stock, luego nombre.";

        public const string ImmobilizedRankingDefinition =
            "Ranking TOP CONGELADO (FASE 8.10) = orden por ImmobilizedCapital desc. " +
            "ImmobilizedCapital = capital si HealthStatus Frozen o Critical; si no, 0. " +
            "Solo IsImmobilized. ≠ InventoryCapital total ≠ estrella. " +
            "Empate: mayor IdleDays, luego nombre. " +
            "PotentialProfit ranking paralelo (mayor potencial snapshot).";
    }
}
