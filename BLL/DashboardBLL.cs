using DL;
using System;

namespace BLL
{
    public class DashboardBLL
    {
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly EstadoDAL estadoDAL = new EstadoDAL();

        public decimal IngresosHoy()
        {
            return cajaDAL.ObtenerTotalIngresosPorFecha(DateTime.Now);
        }

        public decimal IngresosMes()
        {
            return cajaDAL.ObtenerTotalIngresosMesActual();
        }

        /// <summary>ACTIVO + ACTIVO Y PROGRAMADO (SSOT = dgvEstado / dgvClientes).</summary>
        public int ClientesActivos()
        {
            return estadoDAL.ObtenerConteosDashboard().Activos;
        }

        /// <summary>VENCIDO (SSOT = dgvEstado / dgvClientes).</summary>
        public int ClientesVencidos()
        {
            return estadoDAL.ObtenerConteosDashboard().Vencidos;
        }
    }
}
