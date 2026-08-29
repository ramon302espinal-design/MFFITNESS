using System;

namespace CORE
{
    /// <summary>
    /// Helper centralizado para cálculos y validaciones de membresías
    /// </summary>
    public static class MembresiaHelper
    {
        /// <summary>
        /// Calcula la fecha de vencimiento de una membresía según las reglas del negocio:
        /// - Pago del 7 al 19: Vence el 15 del mes siguiente
        /// - Pago del 20 al 6 (cruza mes): Vence el último día del mes siguiente
        /// </summary>
        /// <param name="fechaPago">Fecha en que se realizó el pago</param>
        /// <returns>Fecha de vencimiento calculada</returns>
        public static DateTime CalcularFechaVencimiento(DateTime fechaPago)
        {
            int diaPago = fechaPago.Day;
            int mesPago = fechaPago.Month;
            int anoPago = fechaPago.Year;

            DateTime primerDiaMesSiguiente = new DateTime(anoPago, mesPago, 1).AddMonths(1);

            // Del 7 al 19 → vence el 15 del mes siguiente
            if (diaPago >= 7 && diaPago <= 19)
            {
                return new DateTime(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month, 15);
            }

            // Del 20 al 6 (20…fin de mes y 1…6) → último día del mes siguiente
            int ultimoDiaDelMes = DateTime.DaysInMonth(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month);
            return new DateTime(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month, ultimoDiaDelMes);
        }

        /// <summary>
        /// Calcula la fecha fin de una membresía programada a partir de la fecha de inicio elegida.
        /// A diferencia de <see cref="CalcularFechaVencimiento"/> (fecha de pago), el ancla es el inicio:
        /// - Inicio día 7–19 → vence el 15 del mes siguiente
        /// - Inicio día 1–6 → vence el último día del mismo mes de inicio
        /// - Inicio día 20 en adelante → vence el último día del mes siguiente
        /// </summary>
        public static DateTime CalcularFechaFinProgramacion(DateTime fechaInicio)
        {
            fechaInicio = fechaInicio.Date;
            int dia = fechaInicio.Day;
            int mes = fechaInicio.Month;
            int ano = fechaInicio.Year;

            if (dia >= 7 && dia <= 19)
            {
                DateTime mesSiguiente = new DateTime(ano, mes, 1).AddMonths(1);
                return new DateTime(mesSiguiente.Year, mesSiguiente.Month, 15);
            }

            if (dia >= 1 && dia <= 6)
            {
                int ultimoMesInicio = DateTime.DaysInMonth(ano, mes);
                return new DateTime(ano, mes, ultimoMesInicio);
            }

            DateTime mesSiguienteFin = new DateTime(ano, mes, 1).AddMonths(1);
            int ultimoDiaMesSiguiente = DateTime.DaysInMonth(mesSiguienteFin.Year, mesSiguienteFin.Month);
            return new DateTime(mesSiguienteFin.Year, mesSiguienteFin.Month, ultimoDiaMesSiguiente);
        }

        /// <summary>
        /// Indica si FechaFin refleja el período real del plan o fue truncada (p. ej. al desactivar).
        /// </summary>
        public static bool EsFechaFinPlanCoherente(DateTime fechaInicio, DateTime fechaFin)
        {
            fechaInicio = fechaInicio.Date;
            fechaFin = fechaFin.Date;

            if (fechaFin < fechaInicio)
                return false;

            int diasPlan = (fechaFin - fechaInicio).Days;
            if (diasPlan < 20)
                return false;

            DateTime esperada = CalcularFechaVencimiento(fechaInicio);
            return fechaFin == esperada;
        }

        /// <summary>
        /// Devuelve la fecha de vencimiento del plan según la regla del negocio.
        /// </summary>
        public static DateTime NormalizarFechaFinPlan(DateTime fechaInicio, DateTime? fechaFinActual = null)
        {
            if (fechaFinActual.HasValue &&
                EsFechaFinPlanCoherente(fechaInicio, fechaFinActual.Value))
            {
                return fechaFinActual.Value.Date;
            }

            return CalcularFechaVencimiento(fechaInicio);
        }
    }
}
