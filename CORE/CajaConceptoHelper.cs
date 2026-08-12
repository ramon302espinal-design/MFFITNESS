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
    }
}
