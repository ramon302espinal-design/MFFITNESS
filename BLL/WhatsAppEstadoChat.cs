using System;

namespace BLL
{
    /// <summary>
    /// Mapea el estado de Twilio al historial del chat.
    /// Aceptado por la API (queued/accepted/sent) no es entrega.
    /// </summary>
    public static class WhatsAppEstadoChat
    {
        public const string Pendiente = "PENDIENTE";
        public const string Enviado = "ENVIADO";
        public const string Entregado = "ENTREGADO";
        public const string Leido = "LEIDO";
        public const string Fallido = "FALLIDO";
        public const string Simulado = "SIMULADO";
        public const string Recibido = "RECIBIDO";

        public static string DesdeRespuestaApi(bool exitoApi, string? statusTwilio)
        {
            if (!exitoApi)
                return Fallido;

            return DesdeStatusTwilio(statusTwilio) ?? Enviado;
        }

        /// <summary>null si el status no se reconoce: el caller conserva ENVIADO (aceptado, no entregado).</summary>
        public static string? DesdeStatusTwilio(string? statusTwilio)
        {
            if (string.IsNullOrWhiteSpace(statusTwilio))
                return null;

            return statusTwilio.Trim().ToLowerInvariant() switch
            {
                "read" => Leido,
                "delivered" => Entregado,
                "failed" or "undelivered" => Fallido,
                "queued" or "accepted" or "sending" or "sent" or "receiving" => Enviado,
                _ => null
            };
        }

        public static bool EsEntregaConfirmada(string? estadoChat) =>
            string.Equals(estadoChat, Entregado, StringComparison.OrdinalIgnoreCase)
            || string.Equals(estadoChat, Leido, StringComparison.OrdinalIgnoreCase);

        /// <summary>No degrada leído/entregado con un status anterior. Un fallo sí reemplaza enviado.</summary>
        public static bool DebeActualizar(string? estadoActual, string estadoNuevo)
        {
            if (string.IsNullOrWhiteSpace(estadoNuevo))
                return false;

            string actual = (estadoActual ?? Pendiente).Trim().ToUpperInvariant();
            string nuevo = estadoNuevo.Trim().ToUpperInvariant();
            if (actual == nuevo)
                return false;

            if (actual == Leido)
                return false;

            if (nuevo == Fallido)
                return actual is Pendiente or Enviado or Entregado or "ERROR";

            return Rango(nuevo) > Rango(actual);
        }

        public static string Etiqueta(string? estado) => (estado ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            Pendiente => "pendiente",
            Enviado => "enviado",
            Entregado => "entregado",
            Leido => "leído",
            Fallido or "ERROR" => "fallido",
            Simulado => "prueba",
            Recibido => "recibido",
            _ => string.IsNullOrWhiteSpace(estado) ? string.Empty : estado.Trim().ToLowerInvariant()
        };

        private static int Rango(string estado) => estado switch
        {
            Pendiente => 0,
            Enviado => 1,
            Entregado => 2,
            Leido => 3,
            Fallido or "ERROR" => 1,
            _ => 0
        };
    }
}
