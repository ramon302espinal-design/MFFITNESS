using BLL;
using BLL.Commands;
using CORE;
using CORE.Commands;
using System;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmRegistrarGasto : Form
    {
        private CajaBLL cajaBLL = new CajaBLL();

        public FrmRegistrarGasto()
        {
            InitializeComponent();
            cmbTipoMovimiento?.Items.AddRange(new string[] { "Ingreso", "Egreso", "Apertura" });
            if (cmbTipoMovimiento != null && cmbTipoMovimiento.Items.Count > 0)
                cmbTipoMovimiento.SelectedIndex = 0;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                string tipoMovimiento = (cmbTipoMovimiento?.SelectedItem as string) ?? string.Empty;
                string concepto = txtConcepto?.Text?.Trim() ?? string.Empty;
                string montoTexto = txtMonto?.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(tipoMovimiento))
                {
                    MessageBox.Show("Seleccione un tipo de movimiento.");
                    cmbTipoMovimiento?.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(concepto))
                {
                    MessageBox.Show("Ingrese un concepto válido.");
                    txtConcepto?.Focus();
                    return;
                }

                if (!decimal.TryParse(montoTexto, out decimal monto) || monto <= 0)
                {
                    MessageBox.Show("Ingrese un monto válido.");
                    txtMonto?.Focus();
                    return;
                }

                if (tipoMovimiento == "Apertura")
                {
                    cajaBLL.AbrirCajaSeguro(monto, Sesion.Usuario ?? "ADMIN");
                    MessageBox.Show("Caja abierta correctamente.");
                    AppEventos.CajaCambiada();
                    this.Close();
                    return;
                }

                CommandResult result = tipoMovimiento == "Ingreso"
                    ? CajaCommandService.RegistrarIngreso(concepto, monto, Sesion.Usuario)
                    : CajaCommandService.RegistrarGasto(concepto, monto, Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppEventos.CajaCambiada();
                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
