namespace UI.DISEÑO
{
    partial class FrmDesactivacionMiembro
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
            panelAcciones = new Panel();
            btnDesactivado = new Button();
            btnCancelar = new Button();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.Location = new Point(16, 16);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(279, 25);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "¿Confirmar baja del miembro?";
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.Location = new Point(16, 48);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(66, 23);
            lblCliente.TabIndex = 1;
            lblCliente.Text = "Cliente";
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnDesactivado);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 168);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(12, 8, 12, 8);
            panelAcciones.Size = new Size(444, 52);
            panelAcciones.TabIndex = 3;
            panelAcciones.Tag = "classic";
            // 
            // btnDesactivado
            // 
            btnDesactivado.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDesactivado.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnDesactivado.Location = new Point(176, 8);
            btnDesactivado.Name = "btnDesactivado";
            btnDesactivado.Size = new Size(130, 34);
            btnDesactivado.TabIndex = 0;
            btnDesactivado.Text = "DESACTIVAR";
            btnDesactivado.UseVisualStyleBackColor = true;
            btnDesactivado.Click += btnDesactivado_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Font = new Font("Segoe UI", 9.75F);
            btnCancelar.Location = new Point(312, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 34);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmDesactivacionMiembro
            // 
            AcceptButton = btnDesactivado;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(444, 220);
            Controls.Add(lblCliente);
            Controls.Add(lblTitulo);
            Controls.Add(panelAcciones);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmDesactivacionMiembro";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Tag = "classic";
            Text = "Desactivar";
            Load += FrmDesactivacionMiembro_Load;
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblCliente;
        private Panel panelAcciones;
        private Button btnDesactivado;
        private Button btnCancelar;
    }
}
