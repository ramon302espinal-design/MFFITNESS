using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// Reglas del centro que no deben cambiar el POS ni los dos plazos de App.config.
    /// </summary>
    public static class WhatsAppAutomatizacionPolitica
    {
        public static bool PuedeDesactivarse(string? tipo) =>
            WhatsAppPlantillaCatalogo.Buscar(tipo) != null;

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
