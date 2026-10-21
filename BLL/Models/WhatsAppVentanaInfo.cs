using System;

namespace BLL.Models
{
    public sealed class WhatsAppVentanaInfo
    {
        public bool Abierta { get; init; }
        public DateTime? UltimaEntrada { get; init; }
        public DateTime? CierraEn { get; init; }

        public string Resumen
        {
            get
            {
                if (Abierta && CierraEn.HasValue)
                    return $"Ventana de atención abierta hasta {CierraEn:dd/MM HH:mm}. Puede enviar texto libre.";

                if (UltimaEntrada == null)
                    return "Ventana cerrada: este miembro no ha escrito en el chat. El texto libre requiere una plantilla aprobada.";

                return $"Ventana cerrada (último mensaje del miembro: {UltimaEntrada:dd/MM HH:mm}). El texto libre está bloqueado.";
            }
        }
    }

    public sealed class WhatsAppAutomatizacionDto
    {
        public string Tipo { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public bool Activa { get; init; } = true;
        public string Evento { get; init; } = string.Empty;
        public string Condicion { get; init; } = string.Empty;
        public int? DiasAntes { get; init; }
        public int? DiasUrgente { get; init; }
        public int EvitarDuplicadoHoras { get; init; } = 24;
        public string? UltimoResultado { get; init; }
        public DateTime? UltimaEjecucion { get; init; }
        public string Plantilla { get; init; } = string.Empty;
    }
}
