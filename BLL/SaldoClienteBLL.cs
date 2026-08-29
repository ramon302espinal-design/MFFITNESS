using BLL.Models;
using BLL.Services;
using CORE;
using DL;
using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Saldo a favor: cobro anticipado + reserva; despacho = venta sin nuevo ingreso a caja.
    /// </summary>
    public class SaldoClienteBLL
    {
        private readonly SaldoClienteDAL dal = new SaldoClienteDAL();
        private readonly VentasBLL ventasBLL = new VentasBLL();
        private readonly ClienteDAL clienteDal = new ClienteDAL();

        public DataTable ObtenerActivos() => dal.ObtenerActivos();

        public DataTable ObtenerDetalle(int saldoClienteId)
        {
            if (saldoClienteId <= 0)
                throw new Exception("Saldo inválido.");

            return dal.ObtenerDetalle(saldoClienteId);
        }

        public DataRow? ObtenerCabeceraActiva(int saldoClienteId) =>
            dal.ObtenerCabeceraActiva(saldoClienteId);

        public int? ObtenerIdActivoPorCliente(int clienteId) =>
            clienteId > 0 ? dal.ObtenerIdActivoPorCliente(clienteId) : null;

        public bool TieneSaldoActivo(int clienteId) =>
            ObtenerIdActivoPorCliente(clienteId).HasValue;

        /// <summary>Cobra y reserva. No descuenta stock ni crea venta.</summary>
        public int CobrarSaldoReserva(
            int clienteId,
            string clienteNombre,
            DataTable carrito,
            decimal montoCobrado,
            string metodoPago,
            string? usuario)
        {
            if (clienteId <= 0)
                throw new Exception("Seleccione un miembro válido.");

            if (!clienteDal.EsMiembroRegistrado(clienteId))
                throw new Exception("Solo miembros registrados pueden recibir abono de saldo a favor.");

            if (string.IsNullOrWhiteSpace(clienteNombre))
                clienteNombre = "Cliente";

            if (carrito == null || carrito.Rows.Count == 0)
                throw new Exception("El carrito está vacío.");

            if (montoCobrado <= 0)
                throw new Exception("El monto cobrado debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new Exception("Método de pago requerido.");

            var lineas = carrito.Clone();
            decimal total = 0m;

            foreach (DataRow row in carrito.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                int productoId = Convert.ToInt32(row["ProductoId"]);
                string producto = Convert.ToString(row["Producto"])?.Trim() ?? "Producto";
                decimal precio = Convert.ToDecimal(row["Precio"]);
                int cantidad = Convert.ToInt32(row["Cantidad"]);
                decimal lineaTotal = Convert.ToDecimal(row["Total"]);

                if (productoId <= 0 || cantidad <= 0)
                    continue;

                lineas.Rows.Add(productoId, producto, precio, cantidad, lineaTotal);
                total += lineaTotal;
            }

            if (lineas.Rows.Count == 0)
                throw new Exception("No hay líneas válidas para reservar.");

            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            if (total <= 0)
                throw new Exception("El total a reservar debe ser mayor a cero.");

            if (montoCobrado < total)
                throw new Exception(
                    $"El monto cobrado (RD$ {montoCobrado:N2}) no cubre la reserva (RD$ {total:N2}).");

            string nombre = clienteNombre.Trim();
            if (nombre.Length > 200)
                nombre = nombre.Substring(0, 200);

            string user = string.IsNullOrWhiteSpace(usuario) ? "ADMIN" : usuario.Trim();

            string conceptoCaja = CajaConceptoHelper.IngresoSaldoAFavor(clienteId, nombre, total);

            int saldoId = dal.CobrarSaldoConCaja(
                clienteId,
                nombre,
                total,
                montoCobrado,
                user,
                metodoPago,
                conceptoCaja,
                lineas);

            MovimientoFinancieroNotifier.PagoConCaja();

            return saldoId;
        }

        /// <summary>Despacha productos reservados: venta + stock, sin caja.</summary>
        public VentaOperacionResult DespacharSaldo(int saldoClienteId, string? usuario)
        {
            if (saldoClienteId <= 0)
                throw new Exception("Seleccione un miembro con saldo a favor.");

            var cabecera = dal.ObtenerCabeceraActiva(saldoClienteId)
                ?? throw new Exception("El saldo a favor ya no está activo.");

            int clienteId = Convert.ToInt32(cabecera["ClienteId"]);
            DataTable detalle = dal.ObtenerDetalle(saldoClienteId);
            if (detalle.Rows.Count == 0)
                throw new Exception("La reserva no tiene productos.");

            var carrito = new DataTable();
            carrito.Columns.Add("ProductoId", typeof(int));
            carrito.Columns.Add("Producto", typeof(string));
            carrito.Columns.Add("Precio", typeof(decimal));
            carrito.Columns.Add("Cantidad", typeof(int));
            carrito.Columns.Add("Total", typeof(decimal));

            decimal total = 0m;
            foreach (DataRow row in detalle.Rows)
            {
                decimal linea = Convert.ToDecimal(row["Total"]);
                carrito.Rows.Add(
                    Convert.ToInt32(row["ProductoId"]),
                    row["Producto"]?.ToString() ?? "Producto",
                    Convert.ToDecimal(row["Precio"]),
                    Convert.ToInt32(row["Cantidad"]),
                    linea);
                total += linea;
            }

            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            string user = string.IsNullOrWhiteSpace(usuario) ? "ADMIN" : usuario.Trim();

            VentaOperacionResult operacion;
            try
            {
                operacion = ventasBLL.RegistrarVentaDespachoSaldoAFavor(
                    clienteId,
                    total,
                    user,
                    carrito,
                    saldoClienteId);
            }
            catch
            {
                throw;
            }

            try
            {
                dal.MarcarDespachado(saldoClienteId, operacion.VentaId, user);
            }
            catch
            {
                try { ventasBLL.RevertirVenta(operacion, user); }
                catch { /* best effort */ }
                throw;
            }

            MovimientoFinancieroNotifier.VentaSinCaja();

            return operacion;
        }
    }
}
