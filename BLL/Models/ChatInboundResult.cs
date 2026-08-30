namespace BLL.Models
{
    public sealed class ChatInboundResult
    {
        public bool Exito { get; init; }
        public bool Duplicado { get; init; }
        public int? MensajeId { get; init; }
        public int? ClienteId { get; init; }
        public string Detalle { get; init; } = string.Empty;
        public string? Telefono { get; init; }
    }
}
