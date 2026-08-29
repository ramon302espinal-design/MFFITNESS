using BLL.Models;
using BLL.Services;
using CORE;
using DL;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace BLL
{
    public class VentasBLL
    {
        private readonly VentasDAL ventasDAL = new VentasDAL();
        private readonly StockDAL stockDAL = new StockDAL();
        private readonly ProductoDAL productoDAL = new ProductoDAL();
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly DeudaDAL deudaDAL = new DeudaDAL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly CajaTransaccionService txService = new CajaTransaccionService();

        // ===============================
        // LISTAR VENTAS
        // ===============================
        public DataTable ListarVentas()
        {
            return ventasDAL.ObtenerVentas();
        }

        // ===============================
        // LISTAR DETALLE DE VENTA
        // ===============================
        public DataTable ListarDetalleVenta(int ventaId)
        {
            return ventasDAL.ObtenerDetalleVenta(ventaId);
        }

        /// <summary>Fase 11 — venta POS atómica: stock + caja + deuda + validación integridad.</summary>
        public VentaOperacionResult RegistrarVentaCompletaConResultado(
            int? clienteId,
            decimal total,
            decimal montoPagado,
            string metodo,
            string usuario,
            DataTable carrito,
            DateTime? fechaVencimientoDeuda = null,
            string? conceptoDeuda = null)
        {
            if (carrito.Rows.Count == 0)
                throw new Exception("El carrito está vacío.");

            if (total <= 0)
                throw new Exception("El total debe ser mayor a 0.");

            if (montoPagado < 0)
                throw new Exception("El monto pagado no puede ser negativo.");

            if (montoPagado > total)
                throw new Exception("El monto pagado no puede ser mayor al total.");

            if (string.IsNullOrWhiteSpace(metodo))
                throw new Exception("Método de pago requerido.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario requerido.");

            decimal saldo = decimal.Round(total - montoPagado, 2);
            DateTime fechaVencimiento = fechaVencimientoDeuda?.Date ?? DateTime.Today.AddDays(30);

            if (saldo > 0)
            {
                if (!clienteId.HasValue || clienteId.Value <= 0)
                    throw new Exception("Para ventas a crédito se requiere un cliente válido.");

                if (!new ClienteBLL().EsMiembroRegistrado(clienteId.Value))
                    throw new Exception(
                        "Las ventas a crédito solo aplican a miembros registrados (con historial de membresía).");
            }

            var result = new VentaOperacionResult { MontoPagado = montoPagado };
            string? conceptoDeudaFinal = null;
            int clienteDeuda = clienteId ?? 0;

            txService.Ejecutar((conn, tx) =>
            {
                int ventaId = ventasDAL.RegistrarVenta(
                    conn, tx, clienteId, total, montoPagado, metodo, usuario);
                result.VentaId = ventaId;

                foreach (DataRow row in carrito.Rows)
                {
                    int productoId = Convert.ToInt32(row["ProductoId"]);
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    decimal precio = Convert.ToDecimal(row["Precio"]);
                    decimal subtotal = Convert.ToDecimal(row["Total"]);

                    var (costoVigente, _) = productoDAL.ObtenerCostoYStock(productoId);
                    decimal? costoSnapshot = costoVigente > 0
                        ? Math.Round(costoVigente, 4, MidpointRounding.AwayFromZero)
                        : null;

                    ventasDAL.RegistrarDetalleVenta(
                        conn, tx, ventaId, productoId, cantidad, precio, subtotal, costoSnapshot);

                    int movId = stockDAL.RegistrarSalidaEnTransaccion(
                        conn, tx, productoId, cantidad, usuario, $"Venta Id {ventaId}");
                    result.StockMovimientoIds.Add(movId);
                }

                if (montoPagado > 0)
                {
                    string conceptoCaja = $"Venta de productos (Id {ventaId})";
                    result.CajaMovimientoId = txService.RegistrarIngresoConId(
                        conn, tx, montoPagado, conceptoCaja, usuario, metodo, clienteId);
                }

                if (saldo > 0)
                {
                    conceptoDeudaFinal = FinanciamientoVentaHelper.FormatearConceptoDeudaVenta(
                        conceptoDeuda, ventaId);
                    deudaBLL.ValidarDeudaNueva(clienteDeuda, conceptoDeudaFinal, saldo);

                    result.DeudaId = deudaDAL.InsertarDeuda(
                        conn, tx,
                        clienteDeuda,
                        conceptoDeudaFinal,
                        saldo,
                        fechaVencimiento,
                        usuario,
                        montoPagado,
                        total);
                }

                bool pagoInicialHist = result.DeudaId > 0
                    && montoPagado > 0
                    && DeudaDAL.ExistePagoInicialVigente(conn, tx, result.DeudaId);

                FinanciamientoVentaHelper.ValidarIntegridadPostVenta(
                    total,
                    montoPagado,
                    metodo,
                    result.VentaId,
                    result.DeudaId,
                    result.CajaMovimientoId,
                    pagoInicialHist);
            });

            if (result.DeudaId > 0 && conceptoDeudaFinal != null)
            {
                deudaBLL.NotificarDeudaCreadaPostCommit(
                    clienteDeuda, conceptoDeudaFinal, saldo, fechaVencimiento, result.DeudaId, montoPagado > 0);
            }

            return result;
        }

        /// <summary>
        /// Despacho de producto ya pagado con saldo a favor: venta + stock, sin movimiento de caja ni deuda.
        /// </summary>
        public VentaOperacionResult RegistrarVentaDespachoSaldoAFavor(
            int clienteId,
            decimal total,
            string usuario,
            DataTable carrito,
            int saldoClienteId)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente inválido para despacho.");

            if (carrito.Rows.Count == 0)
                throw new Exception("No hay productos para despachar.");

            if (total <= 0)
                throw new Exception("El total debe ser mayor a 0.");

            var result = new VentaOperacionResult { MontoPagado = total };

            txService.Ejecutar((conn, tx) =>
            {
                int ventaId = ventasDAL.RegistrarVenta(
                    conn, tx, clienteId, total, total, "Saldo a favor", usuario);
                result.VentaId = ventaId;

                foreach (DataRow row in carrito.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;

                    int productoId = Convert.ToInt32(row["ProductoId"]);
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    decimal precio = Convert.ToDecimal(row["Precio"]);
                    decimal subtotal = Convert.ToDecimal(row["Total"]);

                    var (costoVigente, _) = productoDAL.ObtenerCostoYStock(productoId);
                    decimal? costoSnapshot = costoVigente > 0
                        ? Math.Round(costoVigente, 4, MidpointRounding.AwayFromZero)
                        : null;

                    ventasDAL.RegistrarDetalleVenta(
                        conn, tx, ventaId, productoId, cantidad, precio, subtotal, costoSnapshot);

                    int movId = stockDAL.RegistrarSalidaEnTransaccion(
                        conn, tx,
                        productoId,
                        cantidad,
                        usuario,
                        $"Despacho saldo a favor Id {saldoClienteId} · Venta Id {ventaId}");
                    result.StockMovimientoIds.Add(movId);
                }
            });

            return result;
        }

        /// <summary>Fase 11.4 — revertir venta financiada/contado en una sola TX.</summary>
        public void RevertirVenta(VentaOperacionResult operacion, string usuario)
        {
            if (operacion.VentaId <= 0)
                throw new Exception("Venta inválida.");

            txService.Ejecutar((conn, tx) =>
            {
                foreach (int movId in operacion.StockMovimientoIds)
                    stockDAL.RevertirMovimientoEnTransaccion(conn, tx, movId, usuario);

                if (operacion.CajaMovimientoId > 0)
                    cajaDAL.RevertirMovimientoEnTransaccion(conn, tx, operacion.CajaMovimientoId, usuario);

                if (operacion.DeudaId > 0)
                    deudaDAL.AnularDeuda(conn, tx, operacion.DeudaId, usuario);

                ventasDAL.AnularVenta(conn, tx, operacion.VentaId);
            });
        }
    }
}
