namespace CORE
{
    /// <summary>
    /// Punto único de publicación tras movimientos de dinero (Fase 3 — bus unificado).
    /// </summary>
    public static class MovimientoFinancieroNotifier
    {
        public static void Notificar(bool huboPago = true, bool huboCaja = false, bool huboDeuda = false)
        {
            if (huboPago)
                AppEventos.PagoRegistrado();

            if (huboCaja)
                AppEventos.CajaCambiada();

            if (huboDeuda)
                AppEventos.DeudaModificada();
        }

        /// <summary>Pago/abono a deuda existente (siempre caja + deuda).</summary>
        public static void PagoDeuda() =>
            Notificar(huboPago: true, huboCaja: true, huboDeuda: true);

        /// <summary>Reverso de pago de deuda o membresía con movimiento en caja.</summary>
        public static void ReversoPago() =>
            Notificar(huboPago: true, huboCaja: true, huboDeuda: true);

        /// <summary>Edición de financiamiento; caja solo si hubo reverso o ingreso.</summary>
        public static void EdicionFinanciamiento(bool huboMovimientoCaja) =>
            Notificar(huboPago: true, huboCaja: huboMovimientoCaja, huboDeuda: true);

        /// <summary>Nueva deuda registrada (sin movimiento de caja en InsertarDeuda).</summary>
        public static void DeudaCreada(bool huboCaja = false) =>
            Notificar(huboPago: true, huboCaja: huboCaja, huboDeuda: true);

        /// <summary>Deuda anulada sin cobro.</summary>
        public static void DeudaAnulada() =>
            Notificar(huboPago: false, huboCaja: false, huboDeuda: true);

        /// <summary>Venta POS de producto.</summary>
        public static void VentaProducto(int cajaMovimientoId, int deudaId) =>
            Notificar(
                huboPago: true,
                huboCaja: cajaMovimientoId > 0,
                huboDeuda: deudaId > 0);

        /// <summary>Pago de membresía / ingreso operativo en caja.</summary>
        public static void PagoConCaja(bool huboDeuda = false) =>
            Notificar(huboPago: true, huboCaja: true, huboDeuda: huboDeuda);

        /// <summary>Membresía financiada (deuda + caja opcional).</summary>
        public static void MembresiaFinanciada(int cajaMovimientoId, int deudaId) =>
            Notificar(
                huboPago: true,
                huboCaja: cajaMovimientoId > 0,
                huboDeuda: deudaId > 0);

        /// <summary>Solo apertura/cierre/gasto de caja.</summary>
        public static void SoloCaja() =>
            Notificar(huboPago: false, huboCaja: true, huboDeuda: false);

        /// <summary>Venta/despacho sin movimiento de caja (p. ej. saldo a favor).</summary>
        public static void VentaSinCaja() =>
            Notificar(huboPago: true, huboCaja: false, huboDeuda: false);
    }
}
