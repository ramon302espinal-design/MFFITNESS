using System;
using System.Windows.Forms;
using BLL;
using UI.DISEÑO;

namespace UI.Helpers
{
    /// <summary>
    /// Punto de entrada para abrir <see cref="FrmRenovarMembresia"/> (formulario editable en diseñador).
    /// </summary>
    public static class RenovacionMembresiaDialog
    {
        public static bool Mostrar(
            IWin32Window? owner,
            int clienteId,
            string nombreCliente,
            Action? onRenovacionExitosa = null)
        {
            var deudaBLL = new DeudaBLL();
            if (AvisoDeudaPendiente.BloqueaOperacionDePlan(owner, clienteId, deudaBLL))
                return false;

            using var frm = new FrmRenovarMembresia(clienteId, nombreCliente);
            frm.ShowDialog(owner);

            if (frm.RenovacionCompletada)
                onRenovacionExitosa?.Invoke();

            return frm.RenovacionCompletada;
        }
    }
}
