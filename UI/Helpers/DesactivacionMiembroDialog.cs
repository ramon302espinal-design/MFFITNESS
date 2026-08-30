using System.Windows.Forms;
using UI.DISEÑO;

namespace UI.Helpers
{
    /// <summary>
    /// Fachada de compatibilidad: el UI real vive en <see cref="FrmDesactivacionMiembro"/> (Designer).
    /// </summary>
    public static class DesactivacionMiembroDialog
    {
        public static DesactivacionMiembroResult Mostrar(
            IWin32Window? owner,
            string nombreCliente,
            bool esDesactivado = false)
            => FrmDesactivacionMiembro.Mostrar(owner, nombreCliente, esDesactivado);
    }
}
