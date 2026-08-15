using System.Windows.Forms;

namespace UI.Helpers
{
    /// <summary>
    /// Los formularios de deudas viven incrustados en <see cref="FrmModuloDeudas"/>.
    /// Permite que pidan la pestaña "Nueva Deuda" sin conocer al contenedor.
    /// </summary>
    internal static class ModuloDeudasHost
    {
        /// <summary>
        /// Pide al módulo que hospeda al control la pantalla "Nueva Deuda".
        /// False solo si el control no está dentro del módulo.
        /// </summary>
        public static bool AbrirCrearDeuda(Control hijo)
        {
            return Localizar(hijo)?.AbrirCrearDeuda() == true;
        }

        private static FrmModuloDeudas? Localizar(Control? hijo)
        {
            for (Control? actual = hijo?.Parent; actual != null; actual = actual.Parent)
            {
                if (actual is FrmModuloDeudas modulo)
                    return modulo;
            }

            return null;
        }
    }
}
