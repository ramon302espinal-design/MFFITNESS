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
            return IngresosCajaSSOT.IngresosNetosDelDia();
        }

        public decimal CalcularEgresosHoy()
        {
            DataRow? caja = cajaDAL.ObtenerCajaAbierta();
            if (caja == null)
                return 0m;

            return IngresosCajaSSOT.EgresosOperativosSesion(Convert.ToInt32(caja["Id"]));
        }

        // ===============================
        // CIERRE TOTAL PROFESIONAL
        // ===============================
        public (decimal ingresos, decimal egresos, decimal sistema, decimal diferencia)
            CerrarCajaConCuadre(decimal TotalContado)
        {
            DataRow? caja = cajaDAL.ObtenerCajaAbierta();

            if (caja == null)
                throw new Exception("No hay caja abierta.");

            int cajaId = Convert.ToInt32(caja["Id"]);
            string turno = ObtenerTurnoActual();
            decimal montoInicial = Convert.ToDecimal(caja["MontoInicial"]);

            decimal ingresos = cierreDAL.ObtenerTotalPorCaja(cajaId, "INGRESO");
            decimal egresos = cierreDAL.ObtenerTotalPorCaja(cajaId, "EGRESO");

            // SISTEMA = fondo inicial + ingresos − gastos
            decimal sistema = montoInicial + ingresos - egresos;
            decimal diferencia = TotalContado - sistema;

            // Registro de cuadre + cierre atómico: si falla uno, no queda caja "fantasma" abierta.
            cajaDAL.RegistrarCierreYCerrarCaja(
                cajaId,
                turno,
                ingresos,
                egresos,
                sistema,
                TotalContado,
                diferencia,
                ResolveUsuario());

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
        // Mañana 6:00–12:00 | Tarde 13:00–18:00 | Noche 19:00–22:00
        // (huecos/fuera de rango se asignan al turno continuo más cercano; sin EXTRA)
        // ===============================
        public static string ObtenerTurnoPorMomento(DateTime momento)
        {
            TimeSpan hora = momento.TimeOfDay;

            // 06:00 inclusive → antes de 13:00 → Mañana (cubre hasta 12:00 y el hueco a la 1pm)
            if (hora >= new TimeSpan(6, 0, 0) && hora < new TimeSpan(13, 0, 0))
                return "Mañana";

            // 13:00 → antes de 19:00 → Tarde (cubre hasta 18:00 y el hueco a las 7pm)
            if (hora >= new TimeSpan(13, 0, 0) && hora < new TimeSpan(19, 0, 0))
                return "Tarde";

            // 19:00–05:59 → Noche (incluye 19:00–22:00 y madrugada)
            return "Noche";
        }

        public static string ObtenerTurnoActual() => ObtenerTurnoPorMomento(DateTime.Now);

        /// <summary>Normaliza valores históricos (MAÑANA/TARDE/EXTRA) a Mañana/Tarde/Noche.</summary>
        public static string NormalizarNombreTurno(string? turno, DateTime? fechaCierre = null)
        {
            string t = (turno ?? string.Empty).Trim();
            if (t.Length == 0)
                return "—";

            if (t.Equals("Mañana", StringComparison.OrdinalIgnoreCase)
                || t.Equals("Manana", StringComparison.OrdinalIgnoreCase)
                || t.Equals("MAÑANA", StringComparison.OrdinalIgnoreCase))
                return "Mañana";

            if (t.Equals("Tarde", StringComparison.OrdinalIgnoreCase)
                || t.Equals("TARDE", StringComparison.OrdinalIgnoreCase))
                return "Tarde";

            if (t.Equals("Noche", StringComparison.OrdinalIgnoreCase)
                || t.Equals("NOCHE", StringComparison.OrdinalIgnoreCase))
                return "Noche";

            // EXTRA u otros: recalcular por hora de cierre si existe
            if (fechaCierre.HasValue)
                return ObtenerTurnoPorMomento(fechaCierre.Value);

            return "Noche";
        }
    }
}
