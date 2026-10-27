using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// Reglas del centro que no deben cambiar el POS ni los dos plazos de App.config.
    /// </summary>
    public static class WhatsAppAutomatizacionPolitica
    {
        /// <summary>
        /// Membresía ya vencida: un aviso cada 3 días de calendario, no uno por día.
        /// Lunes cuenta; el siguiente sale el jueves.
        /// </summary>
        public const int IntervaloDiasMembresiaVencida = 3;

        /// <summary>No sale antes de las 9:00, hora de República Dominicana.</summary>
        public static readonly TimeSpan HoraInicioMembresiaVencida = new(9, 0, 0);

        public static bool PuedeDesactivarse(string? tipo) =>
            WhatsAppPlantillaCatalogo.Buscar(tipo) != null;

        public static bool EsHorarioAvisoMembresiaVencida(DateTime ahoraLocal) =>
            ahoraLocal.TimeOfDay >= HoraInicioMembresiaVencida;

        /// <summary>
        /// True si el último envío exitoso todavía cubre el intervalo de 3 días.
        /// </summary>
        public static bool AvisoMembresiaVencidaAunVigente(DateTime fechaUltimoEnvio, DateTime ahoraLocal) =>
            (ahoraLocal.Date - fechaUltimoEnvio.Date).Days < IntervaloDiasMembresiaVencida;

        /// <summary>Conserva cada plazo distinto. No sustituye 10 y 3 por un solo número.</summary>
        public static IReadOnlyList<int> PlazosActivos(IEnumerable<int> diasConfigurados)
        {
            var vistos = new HashSet<int>();
            var lista = new List<int>();
            foreach (int dias in diasConfigurados)
            {
                if (dias > 0 && vistos.Add(dias))
                    lista.Add(dias);
            }

            return lista;
        }
    }
}
