using BLL.Models;
using System.Windows.Forms;
using UI.DISEÑO;

namespace UI.Helpers
{
    /// <summary>
    /// Fachada de compatibilidad: el UI real vive en <see cref="FrmDesactivacionMiembro"/> (Designer).
    /// </summary>
    public static class DesactivacionMiembroDialog
    {
        public static ModoDesactivacionMiembro? Mostrar(IWin32Window? owner, string nombreCliente)
            => FrmDesactivacionMiembro.Mostrar(owner, nombreCliente);
    }
}
