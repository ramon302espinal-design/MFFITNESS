using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>Diálogo simple para ajustar FechaFin (sin cobro).</summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public class FrmAjustarFechaVencimiento : Form
    {
        private readonly Label lblCliente = new();
        private readonly Label lblActual = new();
        private readonly Label lblNueva = new();
        private readonly DateTimePicker dtpNueva = new();
        private readonly Button btnGuardar = new();
        private readonly Button btnCancelar = new();

        public DateTime FechaNueva => dtpNueva.Value.Date;

        public FrmAjustarFechaVencimiento()
        {
            InicializarUi();
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

        private void InicializarUi()
        {
            Text = "Modificar fecha de vencimiento";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 210);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            lblCliente.AutoSize = false;
            lblCliente.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCliente.Location = new Point(16, 16);
            lblCliente.Size = new Size(388, 28);

            lblActual.AutoSize = false;
            lblActual.Location = new Point(16, 52);
            lblActual.Size = new Size(388, 24);
            lblActual.ForeColor = Color.DimGray;

            lblNueva.AutoSize = true;
            lblNueva.Text = "Nueva fecha de vencimiento:";
            lblNueva.Location = new Point(16, 92);

            dtpNueva.Format = DateTimePickerFormat.Short;
            dtpNueva.Location = new Point(16, 120);
            dtpNueva.Size = new Size(200, 27);
            dtpNueva.ShowCheckBox = false;

            btnGuardar.Text = "GUARDAR";
            btnGuardar.BackColor = Color.FromArgb(22, 163, 74);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.Size = new Size(110, 32);
            btnGuardar.Location = new Point(186, 162);
            btnGuardar.Click += (_, _) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(90, 32);
            btnCancelar.Location = new Point(310, 162);
            btnCancelar.DialogResult = DialogResult.Cancel;

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;

            Controls.Add(lblCliente);
            Controls.Add(lblActual);
            Controls.Add(lblNueva);
            Controls.Add(dtpNueva);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);
        }
    }
}
