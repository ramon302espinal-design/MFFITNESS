namespace BLL.Models
{
    public sealed class WhatsAppEnvioResult
    {
        public bool Exito { get; init; }
        public bool Entregado { get; init; }
        public string Detalle { get; init; } = string.Empty;
        public string? MessageSid { get; init; }
        public string? StatusFinal { get; init; }
        /// <summary>ContentSid que Twilio usó en este intento. Vacío en texto de sesión.</summary>
        public string? ContentSidUsado { get; set; }
    }
}
