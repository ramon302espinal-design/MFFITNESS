
using BLL;
using CORE;
using System;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmMovimientosCaja : Form
    {

        private CajaBLL cajaBLL = new CajaBLL();

        private Form? formularioAnterior;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmMovimientosCaja()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        public FrmMovimientosCaja(Form frm)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            formularioAnterior = frm;
            if (ThemeHost.IsDesignTime())
                return;

            CargarMovimientos();
            ConfigurarEventos();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void CargarMovimientos()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(CargarMovimientos));
                    return;
                }

                dgvMovimientos.DataSource = cajaBLL.MovimientosHoy();
                dgvMovimientos.ClearSelection();
                DataGridViewHelper.HideColumn(dgvMovimientos, "ClienteId");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando movimientos: " + ex.Message);
            }
        }

        private void ConfigurarEventos()
        {
            dgvMovimientos.CellDoubleClick += DgvMovimientos_CellDoubleClick;
        }

        private void DgvMovimientos_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                var row = dgvMovimientos.Rows[e.RowIndex];

                if (row.Cells["ClienteId"].Value == null || row.Cells["ClienteId"].Value == DBNull.Value)
                {
                    MessageBox.Show("Este movimiento no está asociado a ningún cliente.", 
                                    "Sin cliente", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                    return;
                }

                int clienteId = Convert.ToInt32(row.Cells["ClienteId"].Value);
                string nombreCliente = row.Cells["NombreCliente"].Value?.ToString() ?? "Desconocido";

                var result = MessageBox.Show(
                    $"¿Desea ver el historial de membresía de {nombreCliente}?",
                    "Ir a Historial de Membresía",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    AbrirHistorialMembresia(clienteId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar el movimiento: " + ex.Message, 
                                "Error", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
            }
        }

        private void AbrirHistorialMembresia(int clienteId)
        {
            FrmHistorialMembresia frmHistorial = new FrmHistorialMembresia(clienteId);
            frmHistorial.ShowDialog();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }






    }
}