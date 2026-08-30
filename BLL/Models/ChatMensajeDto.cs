using System;

namespace BLL.Models
{
    public sealed class ChatMensajeDto
    {
        public long Id { get; init; }
        public int ClienteId { get; init; }
        public string Direccion { get; init; } = "SALIDA";
        public string Cuerpo { get; init; } = string.Empty;
        public string Estado { get; init; } = string.Empty;
        public string? Usuario { get; init; }
        public DateTime Fecha { get; init; }
        public bool EsAutomatico { get; init; }
        public string? TipoAutomatico { get; init; }
    }
}
