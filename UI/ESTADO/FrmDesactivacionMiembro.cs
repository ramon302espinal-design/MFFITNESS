using BLL.Models;
using System;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>
    /// Diálogo visual (Designer) para confirmar baja manual (DESACTIVADO).
    /// VENCIDO lo marca el sistema por FechaFin; no se elige aquí.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmDesactivacionMiembro : Form
    {
        public ModoDesactivacionMiembro? ModoSeleccionado { get; private set; }

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmDesactivacionMiembro()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        public FrmDesactivacionMiembro(string nombreCliente) : this()
        {
            Text = string.IsNullOrWhiteSpace(nombreCliente)
                ? "Desactivar"
                : "Desactivar - " + nombreCliente.Trim();
        }

        /// <summary>
        /// Misma API que el diálogo anterior (cableado de FrmEstadoClientes).
        /// </summary>
        public static ModoDesactivacionMiembro? Mostrar(IWin32Window? owner, string nombreCliente)
        {
            using var frm = new FrmDesactivacionMiembro(nombreCliente);
            return frm.ShowDialog(owner) == DialogResult.OK
                ? frm.ModoSeleccionado
                : null;
        }

        private void btnDesactivado_Click(object sender, EventArgs e)
        {
            ModoSeleccionado = ModoDesactivacionMiembro.SinMembresia;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            ModoSeleccionado = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
