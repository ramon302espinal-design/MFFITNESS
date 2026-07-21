using DL;
using System;

namespace BLL
{
    public class DashboardBLL
    {
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly MembresiaDAL membDAL = new MembresiaDAL();

        public decimal IngresosHoy()
        {
            return cajaDAL.ObtenerTotalIngresosPorFecha(DateTime.Now);
        }

        public decimal IngresosMes()
        {
            return cajaDAL.ObtenerTotalIngresosMesActual();
        }

        public int ClientesActivos()
        {
            return membDAL.ObtenerTotalActivos();
        }

        public int ClientesVencidos()
        {
            return membDAL.ClientesVencidos();
        }
    }
}
