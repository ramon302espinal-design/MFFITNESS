using DL;
using System;
using System.Data;
using CORE;

namespace BLL
{
    public class CajaServiceBLL
    { 
        private CajaDAL cajaDAL = new CajaDAL();
        private CierreCajaDAL cierreDAL = new CierreCajaDAL();

        // ===============================
        // CalCULAR TOTALES DEL DÍA
        // ===============================

        public decimal CalcularIngresosHoy()
        {
            return cierreDAL.ObtenerTotalPorTipoMovimientoHoy("INGRESO");
        }

        public decimal CalcularEgresosHoy()
        {
            return cierreDAL.ObtenerTotalPorTipoMovimientoHoy("EGRESO");
        }

        // ===============================
        // CIERRE TOTAL PROFESIONAL
        // ===============================
        public (decimal ingresos, decimal egresos, decimal sistema, decimal diferencia)
CerrarCajaConCuadre(decimal TotalContado)
        {
            string usuario = ResolveUsuario();
            DataRow? caja = cajaDAL.ObtenerCajaAbierta(usuario);

            if (caja == null)
                throw new Exception("No hay caja abierta.");

            int cajaId = Convert.ToInt32(caja["Id"]);

            string turno = ObtenerTurnoActual();

            if (!AppConfig.ModoPrueba)
            {
                if (cierreDAL.YaExisteCierreHoy(turno, usuario))
                    throw new Exception($"Ya se cerró el turno {turno} para este usuario.");
            }

            decimal ingresos = cierreDAL.ObtenerTotalPorCaja(cajaId, "INGRESO");
            decimal egresos = cierreDAL.ObtenerTotalPorCaja(cajaId, "EGRESO");

            decimal sistema = ingresos - egresos;
            decimal diferencia = TotalContado - sistema;

            cierreDAL.RegistrarCierreCompleto(
                cajaId,
                turno,
                ingresos,
                egresos,
                sistema,
                TotalContado,
                diferencia,
                usuario
            );

            cajaDAL.CerrarCaja(cajaId);

            return (ingresos, egresos, sistema, diferencia);
        }

        private static string ResolveUsuario()
        {
            if (!string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario;
            return "ADMIN";
        }

        // ===============================
        // TURNO AUTOMÁTICO
        // ===============================
        private string ObtenerTurnoActual()
        {
            TimeSpan hora = DateTime.Now.TimeOfDay;

            if (hora >= new TimeSpan(6, 0, 0) && hora <= new TimeSpan(11, 59, 59))
                return "MAÑANA";

            if (hora >= new TimeSpan(15, 0, 0) && hora <= new TimeSpan(22, 59, 59))
                return "TARDE";

            return "EXTRA"; // opcional
        }
    }
}