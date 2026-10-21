namespace BLL.Models
{
    public sealed class ChatEnvioResult
    {
        public bool Exito { get; init; }
        public string Detalle { get; init; } = string.Empty;
        public int? MensajeId { get; init; }
        public bool RequierePlantilla { get; init; }
        public bool Simulado { get; init; }
        public string Estado { get; init; } = string.Empty;
    }
}
