using System;

namespace BLL.Models
{
    public sealed class ChatNotificacionDto
    {
        public int MensajeId { get; init; }
        public int ClienteId { get; init; }
        public string ClienteNombre { get; init; } = string.Empty;
        public string Cuerpo { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
    }
}
