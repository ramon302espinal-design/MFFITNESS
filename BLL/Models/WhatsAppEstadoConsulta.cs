namespace BLL.Models
{
    /// <summary>
    /// Estado real de un mensaje consultado a Twilio despues del envio.
    /// </summary>
    public sealed class WhatsAppEstadoConsulta
    {
        /// <summary>False si Twilio no respondio: no se debe cambiar el registro.</summary>
        public bool Consultado { get; init; }

        public string Status { get; init; } = string.Empty;

        public bool Entregado { get; init; }

        public bool Fallido { get; init; }

        public string Detalle { get; init; } = string.Empty;
    }
}
