using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato del mapa de métricas (FASE 10.2).</summary>
    public static class DecisionMetricsPolicy
    {
        public const string Definition =
            "FASE 10.2: catálogo de métricas que el Decision Engine puede consumir. " +
            "NO recalcular en Dashboard/Alertas/Decisiones — usar SourceService canónico.";

        public const string Separation =
            "VENTAS (tickets) ≠ UNIDADES ≠ INGRESOS ≠ GANANCIA ≠ MARGEN ≠ ROI. " +
            "InventoryCapital ≠ ImmobilizedCapital ≠ FrozenCapital inversión.";

        public const string NoEngineYet =
            "Este catálogo no evalúa reglas. Motor = 10.8+. Persistencia = 10.21+.";
    }

    /// <summary>
    /// Mapa de métricas disponibles para FASE 10 (inspección formalizada).
    /// Solo metadatos — sin I/O ni umbrales de alerta.
    /// </summary>
    public static class DecisionMetricsCatalog
    {
        public static IReadOnlyList<DecisionMetricDescriptor> All { get; } = Build();

        public static DecisionMetricDescriptor? Find(string key)
            => All.FirstOrDefault(m =>
                string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<DecisionMetricDescriptor> ByArea(DecisionMetricArea area)
            => All.Where(m => m.Area == area).ToList();

        public static IReadOnlyList<string> SourceServices
            => All.Select(m => m.SourceService).Distinct(StringComparer.Ordinal).OrderBy(s => s).ToList();

        private static IReadOnlyList<DecisionMetricDescriptor> Build()
        {
            var list = new List<DecisionMetricDescriptor>();

            void Add(
                string key, string name, DecisionMetricArea area, DecisionMetricUnit unit,
                string source, string notes,
                bool comparable = false, bool insuf = true)
            {
                list.Add(new DecisionMetricDescriptor
                {
                    Key = key,
                    DisplayName = name,
                    Area = area,
                    Unit = unit,
                    SourceService = source,
                    Notes = notes,
                    RequiresComparablePeriod = comparable,
                    AllowsInsufficientData = insuf
                });
            }

            // --- VENTAS ---
            Add("sales.revenue", "Ingresos", DecisionMetricArea.Sales, DecisionMetricUnit.Money,
                "SalesAnalyticsService", "Σ DetalleVentas.Subtotal ≠ MontoPagado");
            Add("sales.units", "Unidades", DecisionMetricArea.Sales, DecisionMetricUnit.Count,
                "SalesAnalyticsService", "Σ Cantidad ≠ tickets");
            Add("sales.transactions", "Tickets / ventas", DecisionMetricArea.Sales, DecisionMetricUnit.Count,
                "SalesAnalyticsService", "COUNT(Ventas)");
            Add("sales.ticket_avg", "Ticket promedio", DecisionMetricArea.Sales, DecisionMetricUnit.Money,
                "SalesTicketService", "Ingresos / tickets");
            Add("sales.revenue_var_pct", "Variación ingresos %", DecisionMetricArea.Sales, DecisionMetricUnit.Percent,
                "SalesVariationService", "Previous=0 → N/D", comparable: true);
            Add("sales.units_var_pct", "Variación unidades %", DecisionMetricArea.Sales, DecisionMetricUnit.Percent,
                "SalesVariationService", "MoM / par equivalente", comparable: true);
            Add("sales.tx_var_pct", "Variación tickets %", DecisionMetricArea.Sales, DecisionMetricUnit.Percent,
                "SalesVariationService", "", comparable: true);
            Add("sales.ticket_var_pct", "Variación ticket %", DecisionMetricArea.Sales, DecisionMetricUnit.Percent,
                "SalesVariationService", "", comparable: true);
            Add("sales.cross_rev_up_profit_down", "Ingresos↑ Ganancia↓", DecisionMetricArea.Sales, DecisionMetricUnit.Flag,
                "SalesVariationService", "Señal cruzada §50", comparable: true);
            Add("sales.cross_rev_up_margin_down", "Ingresos↑ Margen↓", DecisionMetricArea.Sales, DecisionMetricUnit.Flag,
                "SalesVariationService", "Señal cruzada §51", comparable: true);

            // --- GANANCIA / MARGEN ---
            Add("profit.realized", "Ganancia realizada", DecisionMetricArea.Profit, DecisionMetricUnit.Money,
                "ProfitAnalyticsService", "Requiere COGS snapshot confiable");
            Add("profit.var_pct", "Variación ganancia %", DecisionMetricArea.Profit, DecisionMetricUnit.Percent,
                "SalesVariationService", "", comparable: true);
            Add("profit.margin_pct", "Margen %", DecisionMetricArea.Margin, DecisionMetricUnit.Percent,
                "ProfitAnalyticsService", "Ganancia / ingresos con costo");
            Add("profit.margin_var_pct", "Variación margen %", DecisionMetricArea.Margin, DecisionMetricUnit.Percent,
                "SalesVariationService", "", comparable: true);

            // --- ROI (línea de venta) ---
            Add("roi.product_pct", "ROI producto %", DecisionMetricArea.Roi, DecisionMetricUnit.Percent,
                "ProfitAnalyticsService", "Ganancia / COGS · ≠ ROI inversión FASE 6");
            Add("roi.product_change_pp", "Cambio ROI (pp)", DecisionMetricArea.Roi, DecisionMetricUnit.Percent,
                "SalesCapitalBridgeService", "Actual − previo (puntos)", comparable: true);
            Add("roi.flag_rev_up_roi_down", "Ventas↑ ROI↓", DecisionMetricArea.Roi, DecisionMetricUnit.Flag,
                "SalesCapitalBridgeService", "§52 capital sin retorno proporcional", comparable: true);

            // --- INVENTARIO ---
            Add("inv.stock", "Stock", DecisionMetricArea.Inventory, DecisionMetricUnit.Count,
                "InventoryFinancialService", "Snapshot Productos");
            Add("inv.stock_min", "Stock mínimo", DecisionMetricArea.Inventory, DecisionMetricUnit.Count,
                "InventoryFinancialService", "StockMinimo");
            Add("inv.units_per_day", "Velocidad uds/día", DecisionMetricArea.Inventory, DecisionMetricUnit.Ratio,
                "InventoryFinancialService", "Ventana velocidad FASE 7");
            Add("inv.days_of_cover", "Días de cobertura", DecisionMetricArea.Inventory, DecisionMetricUnit.Days,
                "InventoryFinancialService", "Stock / UnitsPerDay · null si vel≤0");
            Add("inv.flag_overstock", "Sobreinventario", DecisionMetricArea.Inventory, DecisionMetricUnit.Flag,
                "InventoryFinancialService", "Cobertura ≥ umbral (default 90d)");
            Add("inv.flag_stockout", "Riesgo quiebre (FASE 7)", DecisionMetricArea.Inventory, DecisionMetricUnit.Flag,
                "InventoryFinancialService", "Stock ≤ mín + velocidad > 0");
            Add("inv.idle_days", "Días idle", DecisionMetricArea.Inventory, DecisionMetricUnit.Days,
                "InventoryFinancialService", "HasSales / NeverSold / Unknown");
            Add("inv.turnover_proxy", "Proxy rotación", DecisionMetricArea.Inventory, DecisionMetricUnit.Ratio,
                "InventoryFinancialService", "No es rotación contable formal");
            Add("inv.health_status", "Salud capital", DecisionMetricArea.Inventory, DecisionMetricUnit.EnumLabel,
                "InventoryFinancialService", "New/Healthy/Slow/Frozen/Critical");

            // --- CAPITAL ---
            Add("capital.inventory", "Capital inventario", DecisionMetricArea.Capital, DecisionMetricUnit.Money,
                "InventoryFinancialService", "Stock × PrecioCompra");
            Add("capital.immobilized", "Capital inmovilizado", DecisionMetricArea.Capital, DecisionMetricUnit.Money,
                "InventoryFinancialService", "Frozen ∪ Critical FASE 7.9");
            Add("capital.frozen_share_pct", "% congelado / inventario", DecisionMetricArea.Capital, DecisionMetricUnit.Percent,
                "InventoryFinancialService", "≠ todo el inventario");
            Add("capital.at_risk", "Capital en riesgo (bridge)", DecisionMetricArea.Capital, DecisionMetricUnit.Money,
                "SalesCapitalBridgeService", "Σ InventoryCapital con señal CapitalRisk");
            Add("capital.signal_decline_overstock", "Capital riesgo Declining+stock", DecisionMetricArea.Capital, DecisionMetricUnit.Flag,
                "SalesStockRiskService", "§48 ventas↓ + sobreinventario");

            // --- PRODUCTO / CLASE ---
            Add("product.class", "Clase performance", DecisionMetricArea.Product, DecisionMetricUnit.EnumLabel,
                "ProductClassificationService", "Star/Healthy/Opp/Slow/Critical/New");
            Add("product.is_star", "Es estrella", DecisionMetricArea.Product, DecisionMetricUnit.Flag,
                "ProductClassificationService", "FASE 8 checklist · sin score");
            Add("product.star_revenue_share_pct", "% ingresos estrellas", DecisionMetricArea.Product, DecisionMetricUnit.Percent,
                "SalesStarMixService", "Mix §53");
            Add("product.mom_units_trend", "Tendencia MoM uds", DecisionMetricArea.Product, DecisionMetricUnit.EnumLabel,
                "ProductTrendService", "Growing/Stable/Declining/Insufficient · 2 puntos");
            Add("product.mom_units_change_pct", "Cambio MoM uds %", DecisionMetricArea.Product, DecisionMetricUnit.Percent,
                "ProductTrendService", "", comparable: true);

            // --- STOCK↔VENTAS (9.18) ---
            Add("stock.signal", "Señal stock↔ventas", DecisionMetricArea.Inventory, DecisionMetricUnit.EnumLabel,
                "SalesStockRiskService", "Stockout/Replenish/CapitalRisk/HealthyGrowth");
            Add("stock.projected_demand", "Demanda proyectada uds", DecisionMetricArea.Inventory, DecisionMetricUnit.Count,
                "SalesStockRiskService", "UnitsPerDay × horizonte");
            Add("stock.demand_exceeds", "Demanda > stock", DecisionMetricArea.Inventory, DecisionMetricUnit.Flag,
                "SalesStockRiskService", "Quiebre proyectado");

            // --- TENDENCIAS SERIE (9.14–9.15) ---
            Add("trend.series_kind", "Tendencia serie ingresos", DecisionMetricArea.Trend, DecisionMetricUnit.EnumLabel,
                "SalesSeriesTrendService", "Incluye Volatile · ≥4 puntos");
            Add("trend.series_cv_pct", "CV serie %", DecisionMetricArea.Trend, DecisionMetricUnit.Percent,
                "SalesSeriesTrendService", "Volatilidad");
            Add("trend.accel_kind", "Aceleración", DecisionMetricArea.Trend, DecisionMetricUnit.EnumLabel,
                "SalesAccelerationService", "≠ crecimiento · ≥3 tasas");
            Add("trend.accel_delta_pp", "Δ tasas aceleración (pp)", DecisionMetricArea.Trend, DecisionMetricUnit.Percent,
                "SalesAccelerationService", "Last − First change %");

            // --- FORECAST (9.17) ---
            Add("forecast.base_revenue", "Forecast base (est.)", DecisionMetricArea.Forecast, DecisionMetricUnit.Money,
                "SalesForecastService", "ESTIMACIÓN · nunca certeza");
            Add("forecast.low_revenue", "Forecast bajo (est.)", DecisionMetricArea.Forecast, DecisionMetricUnit.Money,
                "SalesForecastService", "Escenario");
            Add("forecast.high_revenue", "Forecast alto (est.)", DecisionMetricArea.Forecast, DecisionMetricUnit.Money,
                "SalesForecastService", "Escenario");
            Add("forecast.confidence", "Confianza forecast", DecisionMetricArea.Forecast, DecisionMetricUnit.EnumLabel,
                "SalesForecastService", "ALTA/MEDIA/BAJA · no probabilidad");
            Add("forecast.margin_est_profit", "Ganancia estimada forecast", DecisionMetricArea.Forecast, DecisionMetricUnit.Money,
                "SalesForecastService", "Ingresos est. × margen histórico");

            // --- ESTACIONALIDAD ---
            Add("season.yoy_revenue_var_pct", "YoY mismo mes ingresos %", DecisionMetricArea.Sales, DecisionMetricUnit.Percent,
                "SalesSeasonalityService", "ago-N vs ago-N-1 · ≠ MoM", comparable: true);
            Add("season.distortion_flag", "Distorsión MoM vs YoY", DecisionMetricArea.Sales, DecisionMetricUnit.Flag,
                "SalesSeasonalityService", "Signos opuestos material");
            Add("season.band", "Banda estacional mes", DecisionMetricArea.Sales, DecisionMetricUnit.EnumLabel,
                "SalesSeasonalityService", "Heurística High/Elevated/Normal/Low");

            // --- CONCENTRACIÓN ---
            Add("conc.topn_share_pct", "Concentración Top N %", DecisionMetricArea.Concentration, DecisionMetricUnit.Percent,
                "SalesShareService", "Estratégico · no automáticamente malo");
            Add("conc.pareto_item_pct", "% ítems para umbral Pareto", DecisionMetricArea.Concentration, DecisionMetricUnit.Percent,
                "SalesParetoService", "Corte real · no asumir 80/20");

            // --- INVERSIONES ---
            Add("invst.capital_invested", "Capital invertido", DecisionMetricArea.Investment, DecisionMetricUnit.Money,
                "InvestmentService", "FASE 6");
            Add("invst.capital_recovered", "Capital recuperado", DecisionMetricArea.Investment, DecisionMetricUnit.Money,
                "InvestmentService", "");
            Add("invst.frozen_capital", "Frozen capital inv.", DecisionMetricArea.Investment, DecisionMetricUnit.Money,
                "InvestmentService", "FIFO · ≠ InventoryCapital global");
            Add("invst.realized_profit", "Ganancia inv.", DecisionMetricArea.Investment, DecisionMetricUnit.Money,
                "InvestmentService", "");
            Add("invst.roi_realized_pct", "ROI realizado inv. %", DecisionMetricArea.Investment, DecisionMetricUnit.Percent,
                "InvestmentService", "≠ ROI producto venta");
            Add("invst.status", "Estado inversión", DecisionMetricArea.Investment, DecisionMetricUnit.EnumLabel,
                "InvestmentService", "Activa/Recuperada/…");

            // --- LIQUIDEZ (agregados) ---
            Add("liq.immobilized_share_pct", "% capital inmovilizado", DecisionMetricArea.Liquidity, DecisionMetricUnit.Percent,
                "InventoryFinancialService", "Misma base FrozenShare");
            Add("liq.at_risk_capital", "Capital en riesgo inventario", DecisionMetricArea.Liquidity, DecisionMetricUnit.Money,
                "InventoryFinancialService", "GetInventoryRiskReport");

            // --- ALERTAS YA EMITIDAS (entrada al motor, no recalcular) ---
            Add("alert.inventory_kind", "Alerta inventario kind", DecisionMetricArea.Inventory, DecisionMetricUnit.EnumLabel,
                "InventoryAlertService", "Consumir · no duplicar cálculo");
            Add("alert.sales_dashboard_kind", "Alerta ventas dashboard", DecisionMetricArea.Sales, DecisionMetricUnit.EnumLabel,
                "SalesDashboardService", "§62 · consolidar en DecisionEngine");
            Add("alert.sales_decision_code", "Señal decisión ventas", DecisionMetricArea.Sales, DecisionMetricUnit.EnumLabel,
                "SalesDecisionService", "Narrativa 9.22 · migrar a DecisionEvent");

            return list;
        }
    }
}
