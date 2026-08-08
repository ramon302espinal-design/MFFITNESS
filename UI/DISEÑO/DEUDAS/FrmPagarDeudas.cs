using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPagarDeudas : Form
    {
        private string nombreCliente = "";
        private decimal saldo;
        private string estado = "";
        private DateTime? ultimoPago;

        public decimal Monto { get; private set; }
        public string Metodo { get; private set; } = "";

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmPagarDeudas()
        {
            InitializeComponent();
        }

        public FrmPagarDeudas(string nombre, decimal saldoActual, string estadoActual, DateTime? ultimoPagoFecha)
            : this()
        {
            nombreCliente = nombre;
            saldo = saldoActual;
            estado = estadoActual;
            ultimoPago = ultimoPagoFecha;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void FrmPagarDeudas_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            lblCliente.Text = nombreCliente;
            lblSaldo.Text = "RD$ " + saldo.ToString("N2");
            lblEstado.Text = estado;

            if (string.Equals(estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
                lblEstado.ForeColor = Color.Red;
            else if (string.Equals(estado, "PAGADA", StringComparison.OrdinalIgnoreCase))
                lblEstado.ForeColor = Color.Green;
            else
                lblEstado.ForeColor = Color.Black;

            lblUltimoPago.Text = "Último pago: " + (ultimoPago.HasValue
                ? ultimoPago.Value.ToString("dd/MM/yyyy")
                : "Sin pagos");

            cmbMetodo.Items.Clear();
            cmbMetodo.Items.AddRange(new string[] { "EFECTIVO", "TRANSFERENCIA" });
            cmbMetodo.SelectedIndex = 0;

            txtMonto.Focus();
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
                    throw new Exception("Ingrese un monto válido.");

                if (monto > saldo)
                    throw new Exception("No puede pagar más del saldo.");

                Monto = monto;
                Metodo = cmbMetodo.Text;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
