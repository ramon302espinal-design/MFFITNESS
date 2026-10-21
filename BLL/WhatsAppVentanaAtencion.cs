using System;

namespace BLL
{
    /// <summary>
    /// Ventana de atención de WhatsApp: 24 h desde el último mensaje entrante del miembro.
    /// </summary>
    public static class WhatsAppVentanaAtencion
    {
        public static readonly TimeSpan Duracion = TimeSpan.FromHours(24);

        public static bool EstaAbierta(DateTime? ultimaEntradaLocal, DateTime ahoraLocal)
        {
            if (ultimaEntradaLocal == null)
                return false;

            if (ultimaEntradaLocal.Value > ahoraLocal)
                return false;

            return ahoraLocal - ultimaEntradaLocal.Value <= Duracion;
        }

        public static DateTime? CierraEn(DateTime? ultimaEntradaLocal) =>
            ultimaEntradaLocal?.Add(Duracion);
    }
}
