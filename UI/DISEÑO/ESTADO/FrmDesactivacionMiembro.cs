using BLL.Models;
using System;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>
    /// Diálogo visual (Designer) para desactivar o reactivar miembro.
    /// VENCIDO lo marca el sistema por FechaFin; no se elige aquí.
    /// Tag "classic": fondo y controles editables tal cual WinForms original.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmDesactivacionMiembro : Form
    {
        private string _nombreCliente = string.Empty;
        private bool _esDesactivado;

        public ModoDesactivacionMiembro? ModoSeleccionado { get; private set; }
        public bool SolicitaActivacion { get; private set; }

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmDesactivacionMiembro()
        {
            InitializeComponent();
        }

        public FrmDesactivacionMiembro(string nombreCliente, bool esDesactivado) : this()
        {
            _nombreCliente = nombreCliente?.Trim() ?? string.Empty;
            _esDesactivado = esDesactivado;
        }

        /// <summary>
        /// Misma API que el diálogo anterior (cableado de FrmEstadoClientes).
        /// </summary>
        public static DesactivacionMiembroResult Mostrar(
            IWin32Window? owner,
            string nombreCliente,
            bool esDesactivado)
        {
            using var frm = new FrmDesactivacionMiembro(nombreCliente, esDesactivado);
            if (frm.ShowDialog(owner) != DialogResult.OK)
                return DesactivacionMiembroResult.Cancelar();

            return frm.SolicitaActivacion
                ? DesactivacionMiembroResult.Activacion()
                : DesactivacionMiembroResult.Desactivacion(
                    frm.ModoSeleccionado ?? ModoDesactivacionMiembro.SinMembresia);
        }

        private void FrmDesactivacionMiembro_Load(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            lblCliente.Text = string.IsNullOrWhiteSpace(_nombreCliente)
                ? "Cliente"
                : _nombreCliente;

            if (_esDesactivado)
            {
                Text = string.IsNullOrWhiteSpace(_nombreCliente)
                    ? "Activar"
                    : "Activar - " + _nombreCliente;
                lblTitulo.Text = "¿Reactivar al miembro?";
                btnDesactivado.Text = "ACTIVAR";
            }
            else
            {
                Text = string.IsNullOrWhiteSpace(_nombreCliente)
                    ? "Desactivar"
                    : "Desactivar - " + _nombreCliente;
                lblTitulo.Text = "¿Confirmar baja del miembro?";
                btnDesactivado.Text = "DESACTIVAR";
            }
        }

        private void btnDesactivado_Click(object? sender, EventArgs e)
        {
            if (_esDesactivado)
            {
                SolicitaActivacion = true;
                ModoSeleccionado = null;
            }
            else
            {
                SolicitaActivacion = false;
                ModoSeleccionado = ModoDesactivacionMiembro.SinMembresia;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            SolicitaActivacion = false;
            ModoSeleccionado = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
