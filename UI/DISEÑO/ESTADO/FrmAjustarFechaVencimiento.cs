using System;
using System.Windows.Forms;

using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>Diálogo simple para ajustar FechaFin (sin cobro).</summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAjustarFechaVencimiento : Form
    {
        public DateTime FechaNueva => dtpNueva.Value.Date;

        public FrmAjustarFechaVencimiento()
        {
            InitializeComponent();
        }

        public FrmAjustarFechaVencimiento(string nombreCliente, DateTime fechaActual)
            : this()
        {
            lblCliente.Text = string.IsNullOrWhiteSpace(nombreCliente) ? "Miembro" : nombreCliente.Trim();
            lblActual.Text = "Vence actualmente: " + fechaActual.ToString("dd/MM/yyyy");
            dtpNueva.Value = fechaActual.Date < dtpNueva.MinDate
                ? DateTime.Today
                : fechaActual.Date;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
