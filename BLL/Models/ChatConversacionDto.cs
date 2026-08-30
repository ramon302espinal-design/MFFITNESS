using System;

namespace BLL.Models
{
    public sealed class ChatConversacionDto
    {
        public int ClienteId { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Telefono { get; init; } = string.Empty;
        public string? UltimoMensaje { get; init; }
        public DateTime? UltimaFecha { get; init; }
        public int NoLeidos { get; init; }
        public bool TelefonoValido { get; init; }
    }
}
