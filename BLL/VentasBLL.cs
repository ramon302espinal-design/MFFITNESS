using BLL.Services;
using BLL.Models;
using DL;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class VentasBLL
    {
        private readonly VentasDAL ventasDAL = new VentasDAL();
        private readonly StockBLL stockBLL = new StockBLL();
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();

        private string usuarioActual = "admin";

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

            usuarioActual = usuario;

            var result = new VentaOperacionResult { MontoPagado = montoPagado };

            try
            {
                int ventaId = ventasDAL.RegistrarVenta(clienteId, total, montoPagado, metodo, usuario);
                result.VentaId = ventaId;

                foreach (DataRow row in carrito.Rows)
                {
                    int productoId = Convert.ToInt32(row["ProductoId"]);
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    decimal precio = Convert.ToDecimal(row["Precio"]);
                    decimal subtotal = Convert.ToDecimal(row["Total"]);

                    ventasDAL.RegistrarDetalleVenta(ventaId, productoId, cantidad, precio, subtotal);

                    int movId = stockBLL.RegistrarSalidaConId(
                        productoId,
                        cantidad,
                        usuario,
                        $"Venta Id {ventaId}");

                    result.StockMovimientoIds.Add(movId);
                }

                if (montoPagado > 0)
                    result.CajaMovimientoId = RegistrarIngresoEnCajaConId(montoPagado, ventaId);

                decimal saldo = total - montoPagado;
                if (saldo > 0)
                {
                    if (!clienteId.HasValue || clienteId.Value <= 0)
                        throw new Exception("Para ventas a crédito se requiere un cliente válido.");

                    DateTime fechaVencimiento = fechaVencimientoDeuda?.Date
                        ?? DateTime.Today.AddDays(30);
                    string concepto = string.IsNullOrWhiteSpace(conceptoDeuda)
                        ? $"Venta de productos (Id {ventaId})"
                        : conceptoDeuda.Trim();
                    result.DeudaId = deudaBLL.CrearDeudaConId(
                        clienteId.Value,
                        concepto,
                        saldo,
                        fechaVencimiento,
                        usuario);
                }

                return result;
            }
            catch
            {
                if (result.VentaId > 0)
                {
                    try
                    {
                        RevertirVenta(result, usuario);
                    }
                    catch
                    {
                        // Best effort: evitar dejar ventas huérfanas si algo falla a mitad de proceso.
                    }
                }

                throw;
            }
        }

        public void RevertirVenta(VentaOperacionResult operacion, string usuario)
        {
            if (operacion.VentaId <= 0)
                throw new Exception("Venta inválida.");

            foreach (int movId in operacion.StockMovimientoIds)
                stockBLL.RevertirMovimiento(movId, usuario);

            if (operacion.CajaMovimientoId > 0)
                cajaDAL.RevertirMovimiento(operacion.CajaMovimientoId, usuario);

            if (operacion.DeudaId > 0)
                deudaBLL.AnularDeuda(operacion.DeudaId, usuario);

            ventasDAL.AnularVenta(operacion.VentaId);
        }

        private int RegistrarIngresoEnCajaConId(decimal total, int ventaId)
        {
            if (total <= 0)
                return 0;

            var caja = cajaDAL.ObtenerCajaAbierta();

            if (caja == null)
                throw new Exception("No hay caja abierta para registrar la venta.");

            int cajaId = Convert.ToInt32(caja["Id"]);
            string concepto = $"Venta de productos (Id {ventaId})";

            var txService = new CajaTransaccionService();
            int movimientoId = 0;

            txService.Ejecutar((conn, tx) =>
            {
                movimientoId = txService.RegistrarIngresoConId(conn, tx, total, concepto, usuarioActual);
            });

            if (movimientoId <= 0)
                movimientoId = cajaDAL.ObtenerUltimoMovimientoIdPorConcepto(cajaId, concepto);

            return movimientoId;
        }
    }
}