namespace BLL.Services.Crm
{
    /// <summary>
    /// Contrato oficial de capital de inventario (FASE 7.2).
    /// Separación semántica: inventario ≠ congelado (congelado se clasifica en 7.8–7.9).
    /// </summary>
    public static class InventoryCapitalPolicy
    {
        public const string InventoryCapitalDefinition =
            "Capital en inventario = StockActual × PrecioCompra (costo vigente). " +
            "Solo si Stock > 0 y PrecioCompra > 0. Usa COSTO, nunca PrecioVenta.";

        public const string PotentialSalesDefinition =
            "Valor potencial de venta = StockActual × PrecioVenta. " +
            "No es capital invertido ni ganancia realizada.";

        public const string PotentialProfitDefinition =
            "Ganancia potencial = Valor potencial − Capital en inventario. " +
            "Requiere stock, costo y precio válidos. Nunca se presenta como realizada.";

        public const string FrozenVsInventoryNote =
            "FASE 7.9: FrozenCapitalTotal = Σ capital con HealthStatus Frozen o Critical. " +
            "InventoryCapitalTotal = todo stock × costo. " +
            "INVENTARIO ≠ CONGELADO. GetFrozenCapitalReport lista solo inmovilizado clasificado. " +
            "InvestmentSummary.FrozenCapital (FASE 6) sigue siendo pool FIFO de ENTRADAS.";

        public const string InvestmentFrozenNote =
            "InvestmentSummary.FrozenCapital (FASE 6) = costo restante atribuible a ENTRADAS " +
            "etiquetadas (FIFO). Es distinto del capital de inventario global.";

        public const string CostSourceNote =
            "Fuente de costo para capital de inventario: Productos.PrecioCompra (vigente). " +
            "No usa DetalleVentas.CostoUnitario ni MovimientosStock para el total global. " +
            "Si mañana cambia el costo, el capital instantáneo cambia; no hay snapshot persistido.";

        public const string AgeDefinition =
            "Antigüedad (FASE 7.3) = días desde la primera ENTRADA (MIN MovimientosStock.Fecha). " +
            "Días desde última ENTRADA = MAX Fecha ENTRADA. " +
            "No confundir con días sin venta (LastSale). " +
            "Sin ENTRADA registrada → FlagNoEntryHistory; antigüedad = N/D (no inventar).";

        public const string IdleDefinition =
            "Idle ventas (FASE 7.4): HasSales → IdleDays = Hoy − LastSale; " +
            "NeverSold → IdleDays = Hoy − FirstEntry (etiqueta NUNCA VENDIDO); " +
            "Unknown → sin venta y sin ENTRADA (IdleDays N/D). " +
            "DaysWithoutSale solo aplica con HasSales. " +
            "FlagNoRotation (FASE 4) = stock>0 y 0 uds vendidas en alcance; complementa, no sustituye IdleKind.";

        public const string VelocityDefinition =
            "Velocidad (FASE 7.5): unidades vendidas en ventana fija (default 30 días) / días de ventana. " +
            "UnitsPerDay; UnitsPerWeek = ×7; UnitsPerMonth = ×30 (mes comercial). " +
            "La ventana es independiente del filtro de P&L del APPLY S. " +
            "0 uds en ventana → velocidad 0 (no inventar demanda).";

        public const string TurnoverProxyDefinition =
            "Rotación PROXY (FASE 7.6) = COGS_ventana (líneas con CostoUnitario) / CapitalInventario_hoy. " +
            "NO es rotación contable COGS/inventario_promedio: no existe inventario promedio histórico fiable. " +
            "UnitTurnoverProxy = uds_ventana / stock complementa. " +
            "Mostrar siempre como PROXY / estimado.";

        public const string CoverageDefinition =
            "Cobertura / días de inventario (FASE 7.7) = Stock / UnitsPerDay. " +
            "Null si UnitsPerDay ≤ 0 (no inventar cobertura infinita). " +
            "FlagOverstock: cobertura ≥ OverstockCoverDays (default 90) con demanda. " +
            "FlagStockoutRisk: Stock ≤ StockMinimo y velocidad > 0. " +
            "Sobreinventario ≠ congelado automático; quiebre es el problema opuesto.";

        public const string HealthClassificationDefinition =
            "Salud capital (FASE 7.8): New (gracia post-entrada) → Healthy → Slow → Frozen → Critical. " +
            "Frozen requiere capital ≥ MinMaterialCapital + (idle≥30d o cobertura≥90d o NeverSold post-gracia). " +
            "Critical = Frozen + (capital≥CriticalCapitalMin | NeverSold≥60d | potencial<0). " +
            "Capital bajo sin materialidad no es Critical. Quiebre no fuerza Frozen. " +
            "Umbrales: InventoryHealthThresholds (configurables).";

        public const string RiskAndLiberableDefinition =
            "Riesgo (FASE 7.10): AtRisk = Critical + Frozen con potencial<0. " +
            "No todo capital congelado está perdido. " +
            "Liberable = capital a costo del alcance en riesgo (aún no recuperado). " +
            "Simulación liquidación: descuentos 0/5/10/20/30/50% sobre PVP; " +
            "calcula ingreso/ganancia-pérdida/capital liberable. NUNCA modifica PrecioVenta.";

        public const string AlertDefinition =
            "Alertas capital (FASE 7.11): CriticalCapital, FrozenCapital, NeverSold, Overstock, " +
            "StockoutRisk, AtRiskLoss, SlowCapital, HighImmobilizedShare. " +
            "Prioridad por capital+tipo (Critica/Alta/Media/Baja). " +
            "No duplica anomalías de integridad FASE 4.8 (InventoryFinancialValidationService). " +
            "New products no generan NeverSold/Frozen.";

        public const string RankingDefinition =
            "Rankings capital (FASE 7.12): InventoryCapital, Immobilized, AtRisk, IdleDays, " +
            "DaysOfCover, TurnoverProxy↑/↓, UnitsPerDay, PotentialProfit. " +
            "Criterios separados — no hay un único ranking universal.";

        public const string InvestmentBridgeDefinition =
            "Puente inversiones (FASE 7.13): TrappedCapital = InvestmentSummary.FrozenCapital (FIFO). " +
            "≠ FrozenCapital clasificado de inventario global. " +
            "Ranking ByFrozenCapitalDesc + salud FASE 7 de productos vinculados. " +
            "TotalTrapped vs GlobalImmobilized pueden diferir (solo ENTRADAS etiquetadas).";

        public const int DefaultVelocityWindowDays =
            InventoryFinancialMath.DefaultVelocityWindowDays;

        public const int DefaultHealthyCoverDays =
            InventoryFinancialMath.DefaultHealthyCoverDays;

        public const int DefaultOverstockCoverDays =
            InventoryFinancialMath.DefaultOverstockCoverDays;
    }
}
