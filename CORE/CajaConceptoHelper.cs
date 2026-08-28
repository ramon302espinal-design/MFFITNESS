using System;

namespace CORE
{
    /// <summary>
    /// Conceptos de DetalleCaja estandarizados para poder resolver NombreCliente en el grid.
    /// Formato: siempre incluye "(Cliente {id})" para el JOIN.
    /// </summary>
    public static class CajaConceptoHelper
    {
        public static string NombreSeguro(string? nombreCliente, int clienteId)
        {
            if (!string.IsNullOrWhiteSpace(nombreCliente))
                return nombreCliente.Trim();
            return $"#{clienteId}";
        }

        public static string IngresoPagoMembresia(int clienteId, string? nombreCliente, string detalle)
        {
            string nombre = NombreSeguro(nombreCliente, clienteId);
            string det = string.IsNullOrWhiteSpace(detalle) ? "Membresía" : detalle.Trim();
            return $"Pago membresía - {nombre} (Cliente {clienteId}) - {det}";
        }

        public static string IngresoRenovacion(int clienteId, string? nombreCliente)
        {
            string nombre = NombreSeguro(nombreCliente, clienteId);
            return $"Renovación - {nombre} (Cliente {clienteId})";
        }

        public static string IngresoPagoInicialFinanciado(int clienteId, string? nombreCliente, string detalle)
        {
            string nombre = NombreSeguro(nombreCliente, clienteId);
            string det = string.IsNullOrWhiteSpace(detalle) ? "Pago inicial" : detalle.Trim();
            return $"Pago membresía - {nombre} (Cliente {clienteId}) - {det}";
        }

        public static string IngresoSaldoAFavor(int clienteId, string? nombreCliente, decimal totalReserva)
        {
            string nombre = NombreSeguro(nombreCliente, clienteId);
            return $"Saldo a favor - {nombre} (Cliente {clienteId}) - reserva RD${totalReserva:N2}";
        }

        /// <summary>
        /// True si el movimiento es un reverso (deshacer / corrección de pago inicial),
        /// no un gasto operativo. No debe sumar al panel de Gastos.
        /// </summary>
        public static bool EsReverso(string? concepto, string? metodoPago = null)
        {
            if (!string.IsNullOrWhiteSpace(metodoPago) &&
                metodoPago.Trim().Equals("REVERSO", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.IsNullOrWhiteSpace(concepto))
                return false;

            string c = concepto.Trim();
            return c.StartsWith("REVERSO", StringComparison.OrdinalIgnoreCase)
                || c.StartsWith("Reverso", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Etiqueta de tipo para el grid: REVERSO en lugar de EGRESO.</summary>
        public static string TipoVisible(string? tipoMovimiento, string? concepto, string? metodoPago = null)
        {
            if (EsReverso(concepto, metodoPago))
                return "REVERSO";

            return string.IsNullOrWhiteSpace(tipoMovimiento)
                ? string.Empty
                : tipoMovimiento.Trim().ToUpperInvariant();
        }
    }
}
