namespace UI.DISEÑO
{
    partial class FrmCongelarMiembro
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
            lblTitulo = new Label();
            lblCliente = new Label();
            lblInfo = new Label();
            lblMotivo = new Label();
            txtMotivo = new TextBox();
            panelAcciones = new Panel();
            btnConfirmar = new Button();
            btnCancelar = new Button();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(16, 12);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(428, 28);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Congelamiento de membresía";
            // 
            // lblCliente
            // 
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.ForeColor = Color.FromArgb(27, 146, 255);
            lblCliente.Location = new Point(16, 44);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(428, 24);
            lblCliente.TabIndex = 1;
            lblCliente.Text = "Cliente";
            // 
            // lblInfo
            // 
            lblInfo.Font = new Font("Segoe UI", 9F);
            lblInfo.ForeColor = Color.FromArgb(100, 116, 139);
            lblInfo.Location = new Point(16, 72);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(428, 48);
            lblInfo.TabIndex = 2;
            lblInfo.Text = "Reactivación: mismo día o después, lunes a viernes. Plan 15 → vence el 15 de ese mes.";
            // 
            // lblMotivo
            // 
            lblMotivo.AutoSize = true;
            lblMotivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMotivo.Location = new Point(16, 124);
            lblMotivo.Name = "lblMotivo";
            lblMotivo.Size = new Size(180, 21);
            lblMotivo.TabIndex = 3;
            lblMotivo.Text = "Motivo de congelamiento";
            // 
            // txtMotivo
            // 
            txtMotivo.Font = new Font("Segoe UI", 10F);
            txtMotivo.Location = new Point(16, 148);
            txtMotivo.Multiline = true;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.PlaceholderText = "Ej. viaje, lesión, trabajo...";
            txtMotivo.Size = new Size(428, 72);
            txtMotivo.TabIndex = 4;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnConfirmar);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(16, 232);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(428, 48);
            panelAcciones.TabIndex = 5;
            // 
            // btnConfirmar
            // 
            btnConfirmar.BackColor = Color.FromArgb(14, 165, 233);
            btnConfirmar.Cursor = Cursors.Hand;
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.FlatStyle = FlatStyle.Flat;
            btnConfirmar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.Location = new Point(186, 8);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new Size(120, 34);
            btnConfirmar.TabIndex = 0;
            btnConfirmar.Text = "CONGELAR";
            btnConfirmar.UseVisualStyleBackColor = false;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.FromArgb(241, 245, 249);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 9.5F);
            btnCancelar.ForeColor = Color.FromArgb(51, 65, 85);
            btnCancelar.Location = new Point(312, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 34);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmCongelarMiembro
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(460, 292);
            Controls.Add(txtMotivo);
            Controls.Add(lblMotivo);
            Controls.Add(lblInfo);
            Controls.Add(lblCliente);
            Controls.Add(lblTitulo);
            Controls.Add(panelAcciones);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmCongelarMiembro";
            Padding = new Padding(16, 12, 16, 12);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Congelar";
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblCliente;
        private Label lblInfo;
        private Label lblMotivo;
        private TextBox txtMotivo;
        private Panel panelAcciones;
        private Button btnConfirmar;
        private Button btnCancelar;
    }
}
