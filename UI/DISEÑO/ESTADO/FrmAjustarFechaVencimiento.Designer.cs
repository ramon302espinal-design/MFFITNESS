namespace UI.DISEÑO
{
    partial class FrmAjustarFechaVencimiento
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblCliente = new Label();
            lblActual = new Label();
            lblNueva = new Label();
            dtpNueva = new DateTimePicker();
            panelAcciones = new Panel();
            btnGuardar = new Button();
            btnCancelar = new Button();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblCliente
            // 
            lblCliente.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCliente.Location = new Point(16, 16);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(388, 28);
            lblCliente.TabIndex = 0;
            lblCliente.Text = "Cliente";
            // 
            // lblActual
            // 
            lblActual.ForeColor = Color.DimGray;
            lblActual.Location = new Point(16, 52);
            lblActual.Name = "lblActual";
            lblActual.Size = new Size(388, 24);
            lblActual.TabIndex = 1;
            lblActual.Text = "Vence actualmente: —";
            // 
            // lblNueva
            // 
            lblNueva.AutoSize = true;
            lblNueva.Location = new Point(16, 92);
            lblNueva.Name = "lblNueva";
            lblNueva.Size = new Size(211, 23);
            lblNueva.TabIndex = 2;
            lblNueva.Text = "Nueva fecha de vencimiento:";
            // 
            // dtpNueva
            // 
            dtpNueva.Format = DateTimePickerFormat.Short;
            dtpNueva.Location = new Point(16, 120);
            dtpNueva.Name = "dtpNueva";
            dtpNueva.ShowCheckBox = false;
            dtpNueva.Size = new Size(200, 27);
            dtpNueva.TabIndex = 3;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 162);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(420, 48);
            panelAcciones.TabIndex = 4;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(22, 163, 74);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(186, 8);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 32);
            btnGuardar.TabIndex = 0;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Location = new Point(310, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(90, 32);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FrmAjustarFechaVencimiento
            // 
            AcceptButton = btnGuardar;
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancelar;
            ClientSize = new Size(420, 210);
            Controls.Add(panelAcciones);
            Controls.Add(dtpNueva);
            Controls.Add(lblNueva);
            Controls.Add(lblActual);
            Controls.Add(lblCliente);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmAjustarFechaVencimiento";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Modificar fecha de vencimiento";
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCliente;
        private Label lblActual;
        private Label lblNueva;
        private DateTimePicker dtpNueva;
        private Panel panelAcciones;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}
