using BLL;
using System;
using System.Data;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmHistorialMembresia : Form
    {
        private readonly HistorialMembresiaBLL historialBLL = new HistorialMembresiaBLL();
        private int? clienteIdSeleccionado = null;

        public FrmHistorialMembresia()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            CargarHistorial();
        }

        public FrmHistorialMembresia(int clienteId) : this()
        {
            clienteIdSeleccionado = clienteId;
        }

        private void CargarHistorial()
        {
            dgvHistorialMembresia.DataSource = historialBLL.ObtenerHistorial();

            if (clienteIdSeleccionado.HasValue)
            {
                SeleccionarClientePorId(clienteIdSeleccionado.Value);
            }
        }

        private void SeleccionarClientePorId(int clienteId)
        {
            try
            {
                foreach (DataGridViewRow row in dgvHistorialMembresia.Rows)
                {
                    if (row.Cells["ClienteId"] != null && 
                        row.Cells["ClienteId"].Value != null &&
                        Convert.ToInt32(row.Cells["ClienteId"].Value) == clienteId)
                    {
                        dgvHistorialMembresia.ClearSelection();
                        row.Selected = true;
                        dgvHistorialMembresia.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar el cliente: " + ex.Message, 
                                "Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Warning);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarHistorial();
        }
    }
}
