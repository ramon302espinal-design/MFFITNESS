using BLL.Services;
using CORE;
using DL;
using System;
using System.Data;

namespace BLL
{
    public class PagoBLL
    {
        private readonly PagoDAL pagoDAL = new PagoDAL();
        private readonly ClienteDAL clienteDAL = new ClienteDAL();

        public DataTable ListarPagos()
        {
            return pagoDAL.ObtenerPagos();
        }

        public (int pagoId, int cajaMovimientoId) RegistrarPagoConResultado(
            int clienteId,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            decimal monto,
            string metodoPago,
            string concepto,
            string usuario)
            => RegistrarPagoConResultado(
                clienteId, fechaPago, fechaVencimiento, monto, metodoPago, concepto, usuario,
                permitirMontoCero: false);

        public (int pagoId, int cajaMovimientoId) RegistrarPagoConResultado(
            int clienteId,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            decimal monto,
            string metodoPago,
            string concepto,
            string usuario,
            bool permitirMontoCero)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente inválido.");

            if (monto < 0)
                throw new Exception("El monto no puede ser negativo.");

            // Oferta/cortesía: RD$ 0 registra pago + historial sin movimiento de caja.
            bool esCortesiaCero = monto == 0;
            if (monto == 0 && !permitirMontoCero)
                throw new Exception("El monto debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new Exception("Método de pago requerido.");

            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario requerido.");

            var txService = new CajaTransaccionService();
            int pagoId = 0;
            int cajaMovId = 0;
            string? nombreCliente = clienteDAL.ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();
            string conceptoCaja = CajaConceptoHelper.IngresoPagoMembresia(clienteId, nombreCliente, concepto);

            txService.Ejecutar((conn, tx) =>
            {
                pagoId = pagoDAL.RegistrarPagoConId(conn, tx,
                    clienteId, fechaPago, fechaVencimiento,
                    monto, metodoPago, concepto, usuario);

                if (!esCortesiaCero)
                {
                    cajaMovId = txService.RegistrarIngresoConId(
                        conn, tx, monto, conceptoCaja, usuario, metodoPago, clienteId);
                }
            });

            MovimientoFinancieroNotifier.Notificar(huboPago: true, huboCaja: cajaMovId > 0);

            return (pagoId, cajaMovId);
        }

        public void RevertirPagoMembresia(int pagoId, int cajaMovimientoId, string usuario)
        {
            if (pagoId <= 0)
                throw new Exception("Pago inválido.");

            pagoDAL.RevertirPagoMembresia(pagoId, cajaMovimientoId, usuario);
            MovimientoFinancieroNotifier.ReversoPago();
        }
    }
}
