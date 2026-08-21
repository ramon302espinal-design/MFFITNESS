using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato Single Source of Truth (FASE 10.3).</summary>
    public static class DecisionSourcePolicy
    {
        public const string Definition =
            "FASE 10.3: cada métrica tiene UN dueño canónico. " +
            "Dashboard / Alertas / Decisiones NO recalculan ventas, ganancia, ROI ni capital.";

        public const string Flow =
            "Analytics Owner → (Composer opcional) → DecisionEngine (10.8+) → UI binders → Forms.";

        public const string Forbidden =
            "Prohibido: Form calcula variación; Alertas reinventan DaysOfCover; " +
            "Decisiones duplican InventoryAlertService sin consumirlo.";

        public const string ExistingSignals =
            "InventoryAlertService, SalesDashboard alerts y SalesDecisionService " +
            "son señales legacy a consolidar en DecisionEngine — no borrar aún.";
    }

    /// <summary>Mapa de fuentes de verdad para el motor de decisiones.</summary>
    public static class DecisionSourceMap
    {
        public static IReadOnlyList<DecisionSourceDescriptor> All { get; } = Build();

        public static DecisionSourceDescriptor? Find(string serviceName)
            => All.FirstOrDefault(s =>
                string.Equals(s.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<DecisionSourceDescriptor> Owners
            => All.Where(s => s.Role == DecisionSourceRole.CanonicalOwner).ToList();

        public static IReadOnlyList<DecisionSourceDescriptor> Composers
            => All.Where(s => s.Role == DecisionSourceRole.Composer).ToList();

        public static IReadOnlyList<DecisionSourceDescriptor> UiConsumers
            => All.Where(s => s.Role == DecisionSourceRole.UiConsumer).ToList();

        /// <summary>
        /// Resuelve el dueño canónico de una métrica del catálogo 10.2.
        /// </summary>
        public static string? ResolveOwnerForMetric(string metricKey)
        {
            DecisionMetricDescriptor? metric = DecisionMetricsCatalog.Find(metricKey);
            return metric?.SourceService;
        }

        private static IReadOnlyList<DecisionSourceDescriptor> Build()
        {
            var list = new List<DecisionSourceDescriptor>();

            void Owner(string name, string owns, string mustNot, string consumedBy, string phase)
                => list.Add(new DecisionSourceDescriptor
                {
                    ServiceName = name,
                    Role = DecisionSourceRole.CanonicalOwner,
                    Owns = owns,
                    MustNot = mustNot,
                    ConsumedBy = consumedBy,
                    Phase = phase
                });

            void Composer(string name, string owns, string mustNot, string consumedBy, string phase)
                => list.Add(new DecisionSourceDescriptor
                {
                    ServiceName = name,
                    Role = DecisionSourceRole.Composer,
                    Owns = owns,
                    MustNot = mustNot,
                    ConsumedBy = consumedBy,
                    Phase = phase
                });

            void Ui(string name, string owns, string mustNot, string consumedBy, string phase)
                => list.Add(new DecisionSourceDescriptor
                {
                    ServiceName = name,
                    Role = DecisionSourceRole.UiConsumer,
                    Owns = owns,
                    MustNot = mustNot,
                    ConsumedBy = consumedBy,
                    Phase = phase
                });

            void Engine(string name, string owns, string mustNot, string consumedBy, string phase)
                => list.Add(new DecisionSourceDescriptor
                {
                    ServiceName = name,
                    Role = DecisionSourceRole.DecisionEngine,
                    Owns = owns,
                    MustNot = mustNot,
                    ConsumedBy = consumedBy,
                    Phase = phase
                });

            // ——— OWNERS ———
            Owner("ProfitAnalyticsService",
                "P&L período: revenue, units, profit, margin, ROI línea, by day/product/category/hour",
                "No emitir DecisionEvent; no umbrales de alerta de negocio",
                "Sales* · ProductPerformance · ProductTrend · DecisionEngine",
                "FASE 5");

            Owner("SalesAnalyticsService",
                "SalesSummary etiquetado (tickets ≠ uds ≠ ingresos)",
                "No SQL duplicado — delega en ProfitAnalytics",
                "Comparison · Ticket · Dashboard · DecisionEngine",
                "FASE 9.2");

            Owner("InventoryFinancialService",
                "Capital inventario, immobilized, velocidad, cobertura, health, risk snapshot",
                "No reglas de decisión multi-área; no forecast",
                "InventoryAlert · ProductPerformance · StockRisk · DecisionEngine",
                "FASE 7");

            Owner("InvestmentService",
                "Capital invertido/recuperado/frozen FIFO, ROI inversión, status",
                "No confundir FrozenCapital inv. con InventoryCapital producto",
                "Investment bridges · FrmAnaInversiones · DecisionEngine",
                "FASE 6");

            Owner("ProductTrendService",
                "MoM 2 puntos por producto (uds primaria)",
                "No tendencia multi-punto; Acceleration=Unknown en MoM",
                "Classification · StockRisk · DecisionEngine",
                "FASE 8.11");

            Owner("ProductClassificationService",
                "Clase Star/Healthy/Opp/Slow/Critical/New",
                "No score compuesto; no compra automática",
                "StarMix · Dashboard FASE 8 · DecisionEngine",
                "FASE 8.12–8.13");

            Owner("SalesSeriesTrendService",
                "Tendencia multi-punto + Volatile (≥4 pts)",
                "No MoM 2 puntos; no forecast",
                "Forecast · Dashboard · DecisionEngine",
                "FASE 9.14");

            Owner("SalesAccelerationService",
                "Aceleración/desaceleración (≥3 tasas)",
                "No clasificar Growing/Declining de serie",
                "Dashboard · DecisionEngine",
                "FASE 9.15");

            Owner("SalesSeasonalityService",
                "YoY mismo mes/semana/día + distorsión MoM",
                "No confundir estacionalidad con crecimiento permanente",
                "DecisionEngine (contexto anti-falso-positivo)",
                "FASE 9.16");

            Owner("SalesForecastService",
                "Estimación Low/Base/High + confianza cualitativa",
                "Nunca certeza ni probabilidad numérica inventada",
                "Dashboard · DecisionEngine",
                "FASE 9.17");

            // ——— COMPOSERS (agregan owners; no redefinen fórmulas base) ———
            Composer("SalesTicketService",
                "Ticket promedio + variación (ingresos / tickets)",
                "No inventar ingresos ni COUNT(Ventas) — usa SalesAnalytics",
                "SalesVariation · Dashboard · DecisionEngine",
                "FASE 9.9");

            Composer("SalesComparisonService",
                "Paquete current vs previous + deltas",
                "No inventar par de períodos fuera de ProductTrendMath",
                "SalesVariation · DecisionEngine",
                "FASE 9.4");

            Composer("SalesVariationService",
                "Labels Up/Down/Flat + cross signals",
                "No tendencia multi-punto",
                "SalesDecision · SalesDashboard · DecisionEngine",
                "FASE 9.5");

            Composer("SalesShareService",
                "Participación % + Top N",
                "No Pareto formal (eso es SalesPareto)",
                "SalesDecision · DecisionEngine",
                "FASE 9.12");

            Composer("SalesParetoService",
                "Corte Pareto real",
                "No asumir 80/20",
                "DecisionEngine (concentración)",
                "FASE 9.13");

            Composer("ProductPerformanceService",
                "Fila performance producto (P&L + snapshot inv.)",
                "No DecisionEvent; rankings = una métrica",
                "Classification · CapitalBridge · DecisionEngine",
                "FASE 8.2");

            Composer("ProductPerformanceDashboardService",
                "Buckets clase + tops + PortfolioHealthScore",
                "No alertas de ventas FASE 9",
                "FrmAnaDashboard · DecisionEngine (conteos)",
                "FASE 8.18");

            Composer("InventoryAlertService",
                "Alertas capital FASE 7.11 (prioridad)",
                "No ventas/margen/ROI inv.; no persistencia",
                "FrmAnaAlertas · DecisionEngine (consolidar)",
                "FASE 7.11");

            Composer("SalesStockRiskService",
                "Señales stock↔ventas 9.18",
                "No compra automática; reusa DaysOfCover",
                "CapitalBridge · DecisionEngine",
                "FASE 9.18");

            Composer("SalesCapitalBridgeService",
                "Puente ventas↔capital + señales §48/§52",
                "No ROI inversión FASE 6",
                "Dashboard · DecisionEngine",
                "FASE 9.19");

            Composer("SalesStarMixService",
                "Mix ingresos por clase + top estrellas",
                "No reclasificar — usa ProductClassification",
                "FrmAnaProductosEstrella · DecisionEngine",
                "FASE 9.20");

            Composer("SalesDashboardService",
                "Snapshot KPIs + alertas §62 + tops",
                "No lógica en Forms; forecast = estimación",
                "FrmAnaVentas/Dashboard/Alertas · DecisionEngine",
                "FASE 9.21");

            Composer("SalesDecisionService",
                "Narrativas §63 (legacy pre-motor)",
                "No acciones auto; migrar a DecisionEvent",
                "FrmAnaDecisiones · DecisionEngine",
                "FASE 9.22");

            Composer("InvestmentCapitalBridgeService",
                "Trapped capital inv. ↔ inventario producto",
                "No DecisionEvent aún",
                "DecisionEngine (inversiones)",
                "FASE 8.x");

            Engine("DecisionEngine",
                "Pipeline: reglas → materialidad → severidad → prioridad → fingerprint",
                "Recalcular ventas/ROI/capital; auto-comprar; persistir sin 10.21",
                "FrmAnaDecisiones · Alertas · Dashboard (vía binders)",
                "FASE 10.8");

            // ——— UI CONSUMERS ———
            Ui("CrmSalesUiBinder",
                "Formato + TryLoad* ventas/decisiones/forecast",
                "Cálculo financiero / umbrales / SQL",
                "FrmAnaVentas · Tendencias · Alertas · Decisiones · Dashboard",
                "FASE 9.24");

            Ui("CrmInventoryUiBinder",
                "Formato + TryLoad inventario/alertas capital",
                "Recalcular FrozenShare o DaysOfCover",
                "FrmAna* inventario/capital/alertas",
                "FASE 7.14");

            Ui("CrmProfitUiBinder",
                "Formato + TryLoad P&L",
                "Inventar variación sin SalesVariation",
                "FrmAnaGanancias · Dashboard",
                "FASE 5.11");

            Ui("CrmInvestmentUiBinder",
                "Formato + TryLoad inversiones",
                "Mutar status/caja; recalcular ROI",
                "FrmAnaInversiones · Decisiones",
                "FASE 6");

            Ui("CrmProductPerformanceUiBinder",
                "Formato + TryLoad estrellas/ranking/dash FASE 8",
                "Score de producto; reclasificar",
                "FrmAnaProductosEstrella · Ranking · Dashboard",
                "FASE 8.20");

            Composer("DecisionAuditService",
                "Auditoría append-only detecciones/resoluciones (FASE 10.23)",
                "Borrar audit; mutar stock/caja; alterar entradas previas",
                "Centro · Alertas · binders 10.24+",
                "FASE 10.23");

            Composer("DecisionResolutionService",
                "Resolve/Ignore/InReview de historial (FASE 10.22)",
                "Mutar stock/caja; borrar historial; auto-resolver",
                "FrmAnaDecisiones · Alertas (binders 10.24+)",
                "FASE 10.22");

            Composer("DecisionHistoryService",
                "Historial append-only DecisionEvents (FASE 10.21)",
                "Resolver/ignorar sin DecisionResolutionService; auto-acciones; recalcular métricas",
                "Centro · Alertas · Auditoría 10.23",
                "FASE 10.21");

            Composer("DecisionCenterService",
                "Resumen ejecutivo + PRIORIDADES DE HOY (FASE 10.20)",
                "Auto-acciones; persistir sin DecisionHistoryService; mutar stock/caja",
                "FrmAnaDecisiones · Dashboard (vía binders 10.24+)",
                "FASE 10.20");

            Composer("DecisionAnalyticsBundleLoader",
                "Carga SSOT una vez por run (FASE 10.27 / brief §88)",
                "N consultas por alerta; recalcular métricas en reglas",
                "DecisionCenterService.RunBuiltIn · DecisionEngine",
                "FASE 10.27");

            Composer("DecisionIntegrationService",
                "Orquestación final Centro+historial (FASE 10.28)",
                "Auto-acciones; mutar stock/caja; N consultas por alerta",
                "CrmDecisionUiBinder · Forms CRM",
                "FASE 10.28");

            Composer("BusinessActionCatalog",
                "Contratos ActionType/Status/Outcome/Confidence (FASE 11.2)",
                "Mutar POS; ML; confundir con DecisionResolutionAction",
                "ActionService (11.5+) · FrmAnaDecisiones",
                "FASE 11.2");

            Composer("BusinessActionRecordFactory",
                "ActionRecord + ExpectedImpact (FASE 11.3)",
                "Persistir; mutar POS; inventar ganancia esperada",
                "ActionService (11.5) · Evaluation (11.9)",
                "FASE 11.3");

            Composer("CrmBusinessActionDAL",
                "Persistencia CrmBusinessActions / Schema 12 (FASE 11.4)",
                "Mutar ventas/costos/stock; recalcular métricas",
                "BusinessActionPersistenceMapper · ActionService (11.5)",
                "FASE 11.4");

            Composer("BusinessActionService",
                "Registro + estados de acción (FASE 11.5)",
                "Mutar POS; auto-completar; Outcome Exitosa en Cancelada",
                "FrmAnaDecisiones · Evaluation (11.9)",
                "FASE 11.5");

            Composer("BusinessActionBaselineComposer",
                "Snapshot mínimo SSOT pre-acción (FASE 11.6)",
                "Inventar métricas; mutar POS; copiar Analytics completo",
                "ActionService · Evaluation (11.8–11.9)",
                "FASE 11.6");

            Composer("BusinessActionEvaluationWindowMath",
                "Ventana post-Complete InWindow/Ready (FASE 11.7)",
                "Clasificar Outcome; calcular deltas; mutar POS",
                "ActionService · Evaluation (11.9)",
                "FASE 11.7");

            Composer("BusinessActionMetricDeltaMath",
                "Post-métricas vs baseline; % y pp margen (FASE 11.8)",
                "Asignar Exitosa/Parcial; causalidad; mutar POS",
                "ActionService · Evaluation (11.9)",
                "FASE 11.8");

            Composer("BusinessActionEvaluationService",
                "Clasifica Outcome Exitosa/Parcial/No efectiva/Sin datos (FASE 11.9)",
                "ML; causalidad; Exitosa en Cancelada; inventar umbrales de negocio",
                "FrmAnaDecisiones · Learning (11.13)",
                "FASE 11.9");

            Composer("BusinessActionCapitalImpactComposer",
                "Capital liberado / incrementos observados (FASE 11.10)",
                "Afirmar causalidad; mutar POS; inventar montos sin deltas",
                "EvaluationService · UI Decisiones",
                "FASE 11.10");

            Composer("BusinessActionTimelineComposer",
                "Timeline decisión→acción→resultado (FASE 11.11)",
                "Mutar POS; inventar hitos; causalidad",
                "FrmAnaDecisiones · TimelineService",
                "FASE 11.11");

            Composer("BusinessActionTimelineService",
                "Timeline con prefetch batch decisiones (FASE 11.21 — sin N+1)",
                "1 FindById por acción en listados; mutar POS",
                "ListRecentBatch · GetByDecisionEventIdBatch · FrmAnaDecisiones",
                "FASE 11.21");

            Composer("BusinessActionSoftLanguageGuard",
                "Guard soft language / anti-causalidad acciones (FASE 11.23 / brief §86)",
                "Afirmar 'causó'; garantías; ML; mutar POS",
                "Evaluation · CapitalImpact · PostMetrics · Learning · BriefTests",
                "FASE 11.23");

            Composer("BusinessActionIntegrationService",
                "Closed-loop Alerta→Decisión→Acción→Resultado→Aprendizaje (FASE 11.24 / §88)",
                "Auto-ejecutar POS; ML; inventar impacto; tocar FrmReportes",
                "FrmAnaDecisiones · Dashboard · FinalIntegrationTests",
                "FASE 11.24");

            Composer("BusinessActionPhasePolicy",
                "Cierre formal FASE 11 (11.25) — IsComplete · closed-loop §88",
                "Reabrir fase; mutar POS; tocar FrmReportes; ML",
                "BusinessActionIntegrationService · UI banners · FinalIntegrationTests",
                "FASE 11.25");

            Composer("BusinessActionBriefPolicy",
                "Batería brief §80 TEST 1–12 (FASE 11.22)",
                "Mutar POS; inventar impacto; tratar histórico como garantía",
                "BusinessActionBriefTests · Evaluation · Learning",
                "FASE 11.22");

            Composer("BusinessActionAuditService",
                "Auditoría append-only acciones + actor Sesion (FASE 11.12)",
                "Borrar audit; mutar POS; inventar actor",
                "ActionService · EvaluationService · UI",
                "FASE 11.12");

            Composer("BusinessActionLearningComposer",
                "Tasas éxito/parcial/fracaso por tipo (FASE 11.13)",
                "ML; garantía futura; causalidad; mutar POS",
                "FrmAnaDecisiones · Learning UI",
                "FASE 11.13");

            Composer("BusinessActionContextualLearningComposer",
                "Learning producto/problema + señales recurrencia (FASE 11.14)",
                "ML; 'funcionará'; mutar POS; inventar causalidad",
                "LearningService · FrmAnaDecisiones",
                "FASE 11.14");

            Ui("CrmDomainHintUiBinder",
                "Hints suaves Forms dominio (FASE 11.20)",
                "Lógica financiera en Forms; mutar POS; saturar UI; auto-acción",
                "FrmAnaVentas · Ganancias · Roi · Inversiones · Inventario · Capital",
                "FASE 11.20");

            Ui("CrmBusinessActionUiBinder",
                "Binder SSOT acciones CRM completo (FASE 11.19)",
                "Lógica financiera en Forms; mutar POS; auto-ejecutar; new Service en Forms",
                "FrmAnaDecisiones · FrmAnaDashboard · FrmAnaAlertas",
                "FASE 11.19");

            Ui("CrmAlertLinkUiBinder",
                "Enlace alerta→decisión→acción→resultado (FASE 11.18)",
                "Mutar POS; inventar vínculos; auto-ejecutar",
                "FrmAnaAlertas",
                "FASE 11.18");

            Ui("FrmAnaDecisiones",
                "Centro de decisiones UI (FASE 10.25) + Acciones (11.15–11.16)",
                "Calcular métricas; acciones irreversibles",
                "Usuario",
                "FASE 10.25 / 11.15+");

            Ui("FrmAnaAlertas",
                "Buckets Centro + legacy inventario/ventas (FASE 10.25)",
                "Recalcular; persistir sin motor; auto-acciones",
                "Usuario",
                "FASE 10.25");

            Ui("CrmDecisionUiBinder",
                "Formato + TryLoad Centro de decisiones (FASE 10.24)",
                "Cálculo financiero; auto-acciones; SQL; persistir sin HistoryService",
                "FrmAnaDashboard · FrmAnaDecisiones (10.25)",
                "FASE 10.24");

            Ui("FrmAnaDashboard",
                "KPIs resumen + panel Centro + Ver/Analizar → Decisiones (10.28)",
                "DecisionEngine propio; recalcular ventas",
                "Usuario",
                "FASE 7–9 / 10.24 / 10.28");

            Ui("FrmReportes",
                "Reportes legacy POS",
                "Cualquier cambio FASE 10 — NO TOCAR",
                "Usuario",
                "Legacy");

            return list;
        }
    }
}
