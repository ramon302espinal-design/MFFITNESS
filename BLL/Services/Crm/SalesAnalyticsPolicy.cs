namespace BLL.Services.Crm
{
    /// <summary>
    /// Contrato oficial de métricas de ventas (FASE 9.2).
    /// Períodos extendidos / comparaciones / forecast = 9.3+.
    /// </summary>
    public static class SalesAnalyticsPolicy
    {
        public const string SeparationRule =
            "NO confundir: VENTAS (transacciones) ≠ UNIDADES ≠ INGRESOS ≠ GANANCIA ≠ MARGEN ≠ ROI. " +
            "Cada métrica se etiqueta y se calcula por separado.";

        public const string TransactionsDefinition =
            "VENTAS / TRANSACCIONES = COUNT(Ventas) en el período (cabeceras vigentes).";

        public const string UnitsDefinition =
            "UNIDADES = Σ DetalleVentas.Cantidad. ≠ número de tickets.";

        public const string RevenueDefinition =
            "INGRESOS = Σ DetalleVentas.Subtotal. ≠ MontoPagado (cobrado) ≠ Ventas.Total cabecera.";

        public const string ProfitDefinition =
            "GANANCIA = Σ (Subtotal − Cant×CostoUnitario) solo líneas con snapshot. " +
            "Sin costo → no inventar ganancia.";

        public const string MarginDefinition =
            "MARGEN % = Ganancia / RevenueWithCost × 100. Null si ingreso c/costo ≤ 0. ≠ ROI.";

        public const string RoiProductDefinition =
            "ROI producto = Ganancia / COGS × 100. Null si COGS ≤ 0. " +
            "≠ ROI inversión (FASE 6: Ganancia / CapitalInvertido).";

        public const string TicketDefinition =
            "TICKET PROMEDIO = Ingresos / Transacciones. Null si transacciones = 0.";

        public const string UnitsPerTxnDefinition =
            "UNIDADES POR TRANSACCIÓN = Unidades / Transacciones. Null si transacciones = 0.";

        public const string VariationDefinition =
            "VARIACIÓN % = (Current − Previous) / Previous × 100. " +
            "Si Previous = 0 → null (N/D / SIN BASE COMPARABLE).";

        public const string AverageDefinition =
            "PROMEDIO = suma / N. MEDIANA = valor central (evita días extremos). " +
            "N = 0 → null.";

        public const string VoidReturnNote =
            "Anulación = DELETE (deja de contar). Sin módulo de devoluciones. " +
            "Sin columna Descuento: Precio/Subtotal ya reflejan precio cobrado en línea.";

        public const string ForecastLanguage =
            "Forecast (9.17+): lenguaje ESTIMACIÓN / ESCENARIO / PROYECCIÓN — nunca certeza.";

        public const string ComposeNote =
            "SalesAnalyticsService reutiliza ProfitAnalyticsService. " +
            "No duplicar agregaciones SQL. Extensiones de período = 9.3+.";

        public const string PeriodsDefinition =
            "FASE 9.3 períodos: Hoy, Ayer, 7d, 14d, 30d, Mes, Mes ant., " +
            "Trimestre, Semestre, Año, Año ant., Custom. " +
            "Rangos half-open [From, ToExclusive) sobre Ventas.Fecha. " +
            "Comparación estacional mismo-mes YoY = 9.16.";
    }
}
