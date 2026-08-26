using DL;
using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Hold de carrito POS. No descuenta stock ni mueve caja/deudas.
    /// </summary>
    public class VentaPausadaBLL
    {
        private readonly VentaPausadaDAL dal = new VentaPausadaDAL();

        public DataTable ObtenerPausadasActivas() => dal.ObtenerPausadasActivas();

        public DataTable ObtenerDetalle(int ventaPausadaId)
        {
            if (ventaPausadaId <= 0)
                throw new Exception("Venta pausada inválida.");

            return dal.ObtenerDetalle(ventaPausadaId);
        }

        public DataRow? ObtenerCabeceraActiva(int ventaPausadaId) =>
            dal.ObtenerCabeceraActiva(ventaPausadaId);

        public int? ObtenerIdPausaActivaPorCliente(int clienteId) =>
            clienteId > 0 ? dal.ObtenerIdPausaActivaPorCliente(clienteId) : null;

        public bool TienePausaActiva(int clienteId) =>
            ObtenerIdPausaActivaPorCliente(clienteId).HasValue;

        public int PausarCarrito(
            int clienteId,
            string clienteNombre,
            DataTable carrito,
            string? usuario)
        {
            if (clienteId <= 0)
                throw new Exception("Seleccione un miembro válido para pausar la venta.");

            if (string.IsNullOrWhiteSpace(clienteNombre))
                clienteNombre = "Cliente";

            if (carrito == null || carrito.Rows.Count == 0)
                throw new Exception("El carrito está vacío.");

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
                throw new Exception("No hay líneas válidas para pausar.");

            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            if (total <= 0)
                throw new Exception("El total a pausar debe ser mayor a cero.");

            string nombre = clienteNombre.Trim();
            if (nombre.Length > 200)
                nombre = nombre.Substring(0, 200);

            return dal.Pausar(
                clienteId,
                nombre,
                total,
                string.IsNullOrWhiteSpace(usuario) ? "ADMIN" : usuario.Trim(),
                lineas);
        }

        public DataTable Despausar(int ventaPausadaId)
        {
            if (ventaPausadaId <= 0)
                throw new Exception("Seleccione un miembro en pausa.");

            var cabecera = dal.ObtenerCabeceraActiva(ventaPausadaId)
                ?? throw new Exception("La venta pausada ya no está activa.");

            DataTable detalle = dal.ObtenerDetalle(ventaPausadaId);
            if (detalle.Rows.Count == 0)
                throw new Exception("La venta pausada no tiene productos.");

            dal.MarcarEstado(ventaPausadaId, "DESPAUSADA");
            _ = cabecera;
            return detalle;
        }

        public void CancelarPorCliente(int clienteId)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente inválido.");

            if (!TienePausaActiva(clienteId))
                throw new Exception("Ese miembro no tiene una venta en pausa.");

            dal.CancelarPorCliente(clienteId);
        }
    }
}
