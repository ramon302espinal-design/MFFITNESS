using System;

namespace CORE
{
    /// <summary>
    /// Ancla = día real del freeze (reloj del PC), solo lunes a viernes.
    /// Congela el 11 → se reactiva el 11 o después (hábil), hasta fin de mes.
    /// Al reactivar, curso de pago: ciclo 15 → vence el 15 del mismo mes (si aún no pasó).
    /// </summary>
    public static class CongelacionHelper
    {
        public static DateTime HoyPc() => DateTime.Today;

        public static bool EsFinDeSemana(DateTime fecha)
        {
            var d = fecha.DayOfWeek;
            return d == DayOfWeek.Saturday || d == DayOfWeek.Sunday;
        }

        public static int CalcularDiaAncla(DateTime fechaCongelacion)
        {
            int dia = fechaCongelacion.Day;
            if (dia < 1) dia = 1;
            if (dia > 31) dia = 31;
            return dia;
        }

        public static int DiaLimiteDelMes(int diaAncla, DateTime fecha)
        {
            int diasMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);
            if (diaAncla < 1) diaAncla = 1;
            return Math.Min(diaAncla, diasMes);
        }

        public static bool PuedeActivarHoy(int diaAncla, DateTime? hoy = null)
        {
            DateTime fecha = (hoy ?? HoyPc()).Date;
            if (EsFinDeSemana(fecha))
                return false;
            return fecha.Day >= DiaLimiteDelMes(diaAncla, fecha);
        }

        public static string MensajeActivacionBloqueada(string nombreCliente, int diaAncla, DateTime? hoy = null)
        {
            string nombre = string.IsNullOrWhiteSpace(nombreCliente)
                ? "El cliente"
                : nombreCliente.Trim();
            DateTime fecha = (hoy ?? HoyPc()).Date;

            if (EsFinDeSemana(fecha))
                return $"{nombre} solo se puede activar de lunes a viernes (no sábado ni domingo).";

            return $"{nombre} solo se puede activar el {diaAncla} de este mes o del otro mes.";
        }

        /// <summary>
        /// Primera fecha del mes actual en la que el día ya alcanza el ancla
        /// (si hoy ya lo alcanzó, hoy). Usada en aviso WhatsApp.
        /// </summary>
        public static DateTime CalcularFechaReactivacionDesde(int diaAncla, DateTime? hoy = null)
        {
            DateTime fecha = (hoy ?? HoyPc()).Date;
            int limite = DiaLimiteDelMes(diaAncla, fecha);
            if (fecha.Day >= limite)
                return fecha;
            return new DateTime(fecha.Year, fecha.Month, limite);
        }

        public static int CalcularDiasRestantes(DateTime fechaFin, DateTime fechaCongelacion)
        {
            int dias = (fechaFin.Date - fechaCongelacion.Date).Days;
            return Math.Max(0, dias);
        }

        /// <summary>
        /// Si el vencimiento original aún no llega, se conserva.
        /// Si ya pasó: ciclo 15 → día 15 del mismo mes (o del siguiente si hoy ya es después del 15);
        /// ciclo fin de mes → último día del mes de activación.
        /// </summary>
        public static DateTime CalcularFechaFinAlActivar(
            DateTime fechaActivacion,
            DateTime? fechaFinOriginal,
            int diasRestantes = 0)
        {
            DateTime act = fechaActivacion.Date;

            if (fechaFinOriginal.HasValue && fechaFinOriginal.Value.Date >= act)
                return fechaFinOriginal.Value.Date;

            if (fechaFinOriginal.HasValue)
            {
                DateTime orig = fechaFinOriginal.Value.Date;
                bool ciclo15 = orig.Day == 15;
                bool cicloFinMes = orig.Day == DateTime.DaysInMonth(orig.Year, orig.Month);

                if (ciclo15)
                {
                    if (act.Day <= 15)
                        return new DateTime(act.Year, act.Month, 15);
                    DateTime mesSig = new DateTime(act.Year, act.Month, 1).AddMonths(1);
                    return new DateTime(mesSig.Year, mesSig.Month, 15);
                }

                if (cicloFinMes)
                {
                    int ultimo = DateTime.DaysInMonth(act.Year, act.Month);
                    return new DateTime(act.Year, act.Month, ultimo);
                }
            }

            return act.AddDays(Math.Max(0, diasRestantes));
        }
    }
}
