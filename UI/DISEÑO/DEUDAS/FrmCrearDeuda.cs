using BLL;
using BLL.Commands;
using CORE;
using System;
using System.Data;
using System.Windows.Forms;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCrearDeuda : Form
    {
        private ClienteBLL clienteBLL = new ClienteBLL();


        public FrmCrearDeuda()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }
        private void txtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }    
        private void txtMonto_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto))
            {
                txtMonto.Text = monto.ToString("N2");
            }
        }
        private void FrmCrearDeuda_Load(object sender, EventArgs e)
        {
           
            cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;

            CargarClientes();
        }

        // ============================
        // CARGAR CLIENTES
        // ============================
        private void CargarClientes()
        {
            try
            {
                DataTable dt = clienteBLL.ObtenerClientes();

                cbClientes.DataSource = dt;
                cbClientes.DisplayMember = "Nombre";
                cbClientes.ValueMember = "Id";  // ← Nombre correcto de la columna
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================
        // VALIDACIONES
        // ============================
        private bool Validar()
        {
            if (cbClientes.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un cliente");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                MessageBox.Show("Ingrese un concepto");
                return false;
            }

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Monto inválido");
                return false;
            }

            if (dtpVencimiento.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Fecha inválida");
                return false;
            }

            return true;
        }

        // ============================
        // GUARDAR
        // ============================
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                int clienteId = Convert.ToInt32(cbClientes.SelectedValue);
                string concepto = txtConcepto.Text;
                decimal monto = Convert.ToDecimal(txtMonto.Text);
                DateTime vencimiento = dtpVencimiento.Value;
                string usuario = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "ADMIN" : Sesion.Usuario;

                var result = DeudaCommandService.CrearDeuda(
                    clienteId, concepto, monto, vencimiento, usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppEventos.PagoRegistrado();

                MessageBox.Show(result.Message, "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        // ============================
        // CANCELAR
        // ============================
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}