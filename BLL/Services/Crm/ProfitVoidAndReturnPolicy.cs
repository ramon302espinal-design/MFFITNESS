namespace BLL.Services.Crm
{
    /// <summary>
    /// Política FASE 5.9: devoluciones y anulaciones frente al motor de ganancias.
    /// Congelada según el POS actual — no inventa módulo de returns.
    /// </summary>
    public static class ProfitVoidAndReturnPolicy
    {
        /// <summary>
        /// El POS no tiene devolución parcial/completa ni nota de crédito de productos.
        /// </summary>
        public const bool HasProductReturnModule = false;

        /// <summary>
        /// Anulación de venta = DELETE de DetalleVentas + Ventas (VentasDAL.AnularVenta).
        /// Usado en rollback mid-proceso (VentasBLL.RevertirVenta), no como soft-delete.
        /// </summary>
        public const bool AnnulmentDeletesHistory = true;

        /// <summary>
        /// Regla del motor: solo cuentan filas que existen en Ventas/DetalleVentas.
        /// Una venta borrada deja de existir → no aporta ingreso, COGS ni ganancia.
        /// No hay filtro Estado=ANULADA porque no hay columna de estado.
        /// </summary>
        public const string RealizedProfitSource =
            "DetalleVentas INNER JOIN Ventas (filas vigentes). Sin estado ANULADA.";

        /// <summary>
        /// MovimientosStock REVERSO y DetalleCaja REVERSO NO son fuente de ganancia.
        /// Ajustan inventario/caja; el P&amp;L de productos usa solo el detalle de venta.
        /// </summary>
        public const bool StockOrCashReversalAffectsRealizedProfit = false;

        /// <summary>
        /// Recomendación senior (no implementada en 5.9): soft-delete / Estado ANULADA
        /// para conservar auditoría sin contar en métricas.
        /// </summary>
        public const string RecommendedFuture =
            "Marcar venta ANULADA en lugar de DELETE; filtrar Estado <> 'ANULADA' en el motor.";

        public static string DescribeForUi() =>
            "Ganancia realizada = ventas vigentes. No hay módulo de devoluciones. " +
            "Anulaciones que borran la venta dejan de contar. " +
            "Reversos de stock/caja no recalculan ganancia.";
    }
}
