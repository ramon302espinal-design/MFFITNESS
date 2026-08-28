using System;
using System.Collections.Generic;
using System.Data;
using DL;

namespace BLL
{
    public class EstadoBLL
    {
        private readonly EstadoDAL estadoDAL = new EstadoDAL();
        private readonly PlanBLL planBLL = new PlanBLL();

        public DataTable ObtenerEstadoClientes()
        {
            return estadoDAL.ObtenerEstadoClientes();
        }

        public DataTable ObtenerKpisPlanesPorMes(int anio, int mes)
        {
            if (mes < 1 || mes > 12)
                throw new ArgumentOutOfRangeException(nameof(mes), "Mes inválido.");

            return estadoDAL.ObtenerKpisPlanesPorMes(anio, mes);
        }

        public DataTable ObtenerDetalleMembresiasPorMes(int anio, int mes)
        {
            if (mes < 1 || mes > 12)
                throw new ArgumentOutOfRangeException(nameof(mes), "Mes inválido.");

            return estadoDAL.ObtenerDetalleMembresiasPorMes(anio, mes);
        }

        public DataTable ObtenerDetalleActivosReporte()
        {
            var detalle = new DataTable();
            detalle.Columns.Add("Fecha", typeof(DateTime));
            detalle.Columns.Add("Cliente", typeof(string));
            detalle.Columns.Add("Plan", typeof(string));
            detalle.Columns.Add("Movimiento", typeof(string));
            detalle.Columns.Add("Monto", typeof(decimal));

            DataTable estado = estadoDAL.ObtenerEstadoClientes();
            Dictionary<string, decimal> precios = ObtenerPreciosPlanes();

            foreach (DataRow row in estado.Rows)
            {
                string est = Convert.ToString(row["Estado"])?.Trim() ?? string.Empty;
                if (!string.Equals(est, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                    continue;

                string plan = Convert.ToString(row["Membresia"])?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(plan)
                    || string.Equals(plan, "SIN MEMBRESIA", StringComparison.OrdinalIgnoreCase))
                    continue;

                string cliente = Convert.ToString(row["Nombre"])?.Trim() ?? string.Empty;
                decimal monto = precios.TryGetValue(plan, out decimal p) ? p : 0m;
                DateTime fecha = DateTime.Today;
                if (row["FechaFin"] != null && row["FechaFin"] != DBNull.Value)
                    fecha = Convert.ToDateTime(row["FechaFin"]).Date;

                detalle.Rows.Add(fecha, cliente, plan, "ACTIVO", monto);
            }

            return detalle;
        }

        public DataTable ObtenerResumenDesdeDetalle(DataTable detalle)
        {
            var filas = new List<(string Plan, int Cantidad, decimal Monto)>();
            foreach (DataRow row in detalle.Rows)
            {
                string plan = Convert.ToString(row["Plan"])?.Trim() ?? string.Empty;
                decimal monto = row["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Monto"]);
                filas.Add((plan, 1, monto));
            }

            return EstadoReporteHelper.AgregarResumenDesdeFilas(filas);
        }

        public DataTable ObtenerResumenPlanesPorMes(int anio, int mes)
        {
            DataTable kpis = ObtenerKpisPlanesPorMes(anio, mes);
            var filas = new List<(string Plan, int Cantidad, decimal Monto)>();
            foreach (DataRow row in kpis.Rows)
            {
                string plan = Convert.ToString(row["PlanNombre"])?.Trim() ?? string.Empty;
                int cantidad = row["Cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(row["Cantidad"]);
                decimal monto = row["MontoTotal"] == DBNull.Value ? 0m : Convert.ToDecimal(row["MontoTotal"]);
                filas.Add((plan, cantidad, monto));
            }

            return EstadoReporteHelper.AgregarResumenDesdeFilas(filas);
        }

        private Dictionary<string, decimal> ObtenerPreciosPlanes()
        {
            var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            DataTable planes = planBLL.ObtenerPlanes();
            if (planes == null || !planes.Columns.Contains("Nombre") || !planes.Columns.Contains("Precio"))
                return map;

            foreach (DataRow row in planes.Rows)
            {
                string nombre = Convert.ToString(row["Nombre"])?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;
                decimal precio = row["Precio"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Precio"]);
                map[nombre] = precio;
            }

            return map;
        }
    }
}
