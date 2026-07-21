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
        /// - Pago del 1 al 19: Vence el 15 del mes siguiente
        /// - Pago del 20 en adelante: Vence el último día del mes siguiente
        /// </summary>
        /// <param name="fechaPago">Fecha en que se realizó el pago</param>
        /// <returns>Fecha de vencimiento calculada</returns>
        public static DateTime CalcularFechaVencimiento(DateTime fechaPago)
        {
            int diaPago = fechaPago.Day;
            int mesPago = fechaPago.Month;
            int anoPago = fechaPago.Year;

            // Si pagan del 1 al 19 → Vence el 15 del mes siguiente
            if (diaPago >= 1 && diaPago <= 19)
            {
                DateTime primerDiaMesSiguiente = new DateTime(anoPago, mesPago, 1).AddMonths(1);
                DateTime fechaVence = new DateTime(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month, 15);
                return fechaVence;
            }
            // Si pagan del 20 en adelante → Vence el último día del mes siguiente
            else if (diaPago >= 20)
            {
                DateTime primerDiaMesSiguiente = new DateTime(anoPago, mesPago, 1).AddMonths(1);
                int ultimoDiaDelMes = DateTime.DaysInMonth(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month);
                DateTime fechaVence = new DateTime(primerDiaMesSiguiente.Year, primerDiaMesSiguiente.Month, ultimoDiaDelMes);
                return fechaVence;
            }

            // Fallback (no debería llegar aquí, pero por seguridad)
            return DateTime.Now.AddMonths(1);
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
