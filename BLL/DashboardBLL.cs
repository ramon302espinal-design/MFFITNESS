using DL;
using System;

namespace BLL
{
    public class DashboardBLL
    {
        private readonly CajaBLL cajaBLL = new CajaBLL();
        private readonly EstadoDAL estadoDAL = new EstadoDAL();

        public decimal IngresosHoy()
        {
            return cajaBLL.IngresosNetosDelDia();
        }

        public decimal IngresosMes()
        {
            return cajaBLL.IngresosNetosMes();
        }

        /// <summary>ACTIVO + ACTIVO Y PROGRAMADO (SSOT = dgvEstado / dgvClientes).</summary>
        public int ClientesActivos() => EstadoConteosSSOT.Obtener().Activos;

        /// <summary>VENCIDO (SSOT = dgvEstado / dgvClientes).</summary>
        public int ClientesVencidos() => EstadoConteosSSOT.Obtener().Vencidos;
    }
}
