using System.Windows.Forms;
using BLL;

namespace UI.Helpers
{
    /// <summary>
    /// Regla única de deuda previa al comprar/renovar plan:
    /// deuda de membresía bloquea; deuda de "producto a crédito (venta)" solo avisa.
    /// </summary>
    internal static class AvisoDeudaPendiente
    {
        /// <summary>
        /// Muestra el aviso correspondiente. Devuelve true solo cuando la operación
        /// debe abortarse (deuda de membresía pendiente).
        /// </summary>
        public static bool BloqueaOperacionDePlan(IWin32Window? owner, int clienteId, DeudaBLL? deudaBLL = null)
        {
            var bll = deudaBLL ?? new DeudaBLL();

            if (bll.ClienteBloqueadoPorDeudaPendiente(clienteId, out string motivo))
            {
                Mostrar(owner, motivo);
                return true;
            }

            if (bll.TieneAvisoDeudaProducto(clienteId, out string aviso))
                Mostrar(owner, aviso);

            return false;
        }

        private static void Mostrar(IWin32Window? owner, string mensaje)
        {
            if (owner != null)
                MessageBox.Show(owner, mensaje, "Deuda pendiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(mensaje, "Deuda pendiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
