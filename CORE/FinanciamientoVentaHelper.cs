using System;

namespace CORE
{
    /// <summary>
    /// Reglas Fase 11 — concepto estándar venta↔deuda e integridad post-venta producto.
    /// </summary>
    public static class FinanciamientoVentaHelper
    {
        public const string SufijoVentaId = "(Venta Id ";

        public static bool EsMetodoFinanciado(string? metodo)
        {
            if (string.IsNullOrWhiteSpace(metodo))
                return false;

            string m = metodo.Trim();
            return m.Equals("Financiado", StringComparison.OrdinalIgnoreCase)
                   || m.Equals("Credito", StringComparison.OrdinalIgnoreCase)
                   || m.Equals("Crédito", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Concepto determinístico con enlace <c>(Venta Id N)</c> (Fase 11.3).</summary>
        public static string FormatearConceptoDeudaVenta(string? conceptoBase, int ventaId)
        {
            if (ventaId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ventaId));

            string sufijo = $"{SufijoVentaId}{ventaId})";
            if (string.IsNullOrWhiteSpace(conceptoBase))
                return $"Venta de productos {sufijo}";

            string c = conceptoBase.Trim();
            if (c.Contains(sufijo, StringComparison.OrdinalIgnoreCase))
                return c;

            if (c.Contains("Venta Id ", StringComparison.OrdinalIgnoreCase))
                return c;

            return $"{c} {sufijo}";
        }

        /// <summary>
        /// Fase 11.1 / 11.2 — falla la TX si la venta financiada quedaría huérfana.
        /// </summary>
        public static void ValidarIntegridadPostVenta(
            decimal total,
            decimal montoPagado,
            string metodo,
            int ventaId,
            int deudaId,
            int cajaMovimientoId,
            bool pagoInicialEnHistorial)
        {
            if (ventaId <= 0)
                throw new InvalidOperationException("Integridad: venta sin Id válido.");

            decimal saldo = decimal.Round(total - montoPagado, 2);
            bool requiereDeuda = saldo > 0m || (EsMetodoFinanciado(metodo) && montoPagado < total);

            if (requiereDeuda)
            {
                if (deudaId <= 0)
                    throw new InvalidOperationException(
                        "Integridad: venta financiada sin deuda vinculada. Operación revertida.");

                if (saldo <= 0m)
                    throw new InvalidOperationException(
                        "Integridad: método financiado pero saldo de venta es cero.");
            }

            if (montoPagado > 0m)
            {
                if (cajaMovimientoId <= 0)
                    throw new InvalidOperationException(
                        "Integridad: cobro registrado sin movimiento de caja.");

                if (requiereDeuda && montoPagado > 0m && !pagoInicialEnHistorial)
                    throw new InvalidOperationException(
                        "Integridad: financiamiento con pago inicial sin fila PAGO_INICIAL en historial.");
            }
        }
    }
}
