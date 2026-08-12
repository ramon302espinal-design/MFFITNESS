namespace UI.DISEÑO
{
    partial class FrmDesactivacionMiembro
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitulo = new Label();
            panelOpciones = new Panel();
            btnDesactivado = new Button();
            lblDesactivado = new Label();
            panelAcciones = new Panel();
            btnCancelar = new Button();
            panelOpciones.SuspendLayout();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(20, 16);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(452, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "¿Confirmar baja del miembro?";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelOpciones
            // 
            panelOpciones.Controls.Add(btnDesactivado);
            panelOpciones.Controls.Add(lblDesactivado);
            panelOpciones.Dock = DockStyle.Top;
            panelOpciones.Location = new Point(20, 48);
            panelOpciones.Name = "panelOpciones";
            panelOpciones.Padding = new Padding(0, 8, 0, 0);
            panelOpciones.Size = new Size(452, 120);
            panelOpciones.TabIndex = 1;
            // 
            // btnDesactivado
            // 
            btnDesactivado.BackColor = Color.White;
            btnDesactivado.Cursor = Cursors.Hand;
            btnDesactivado.FlatAppearance.BorderColor = Color.FromArgb(27, 146, 255);
            btnDesactivado.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 245, 255);
            btnDesactivado.FlatStyle = FlatStyle.Flat;
            btnDesactivado.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            btnDesactivado.ForeColor = Color.FromArgb(27, 146, 255);
            btnDesactivado.Location = new Point(17, 8);
            btnDesactivado.Name = "btnDesactivado";
            btnDesactivado.Size = new Size(195, 36);
            btnDesactivado.TabIndex = 0;
            btnDesactivado.Text = "DESACTIVADO";
            btnDesactivado.UseVisualStyleBackColor = false;
            btnDesactivado.Click += btnDesactivado_Click;
            // 
            // lblDesactivado
            // 
            lblDesactivado.Font = new Font("Segoe UI", 9F);
            lblDesactivado.ForeColor = Color.FromArgb(100, 116, 139);
            lblDesactivado.Location = new Point(0, 52);
            lblDesactivado.Name = "lblDesactivado";
            lblDesactivado.Size = new Size(430, 56);
            lblDesactivado.TabIndex = 2;
            lblDesactivado.Text = "El cliente se va por otra razón.\r\nQueda como CLIENTE DESACTIVADO (no cuenta en vencidos).\r\nEl vencimiento lo marca el sistema automáticamente.";
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(20, 178);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(452, 44);
            panelAcciones.TabIndex = 2;
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
            btnCancelar.Location = new Point(294, 5);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 34);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmDesactivacionMiembro
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(492, 238);
            Controls.Add(panelOpciones);
            Controls.Add(lblTitulo);
            Controls.Add(panelAcciones);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmDesactivacionMiembro";
            Padding = new Padding(20, 16, 20, 16);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Desactivar";
            panelOpciones.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitulo;
        private Panel panelOpciones;
        private Button btnDesactivado;
        private Label lblDesactivado;
        private Panel panelAcciones;
        private Button btnCancelar;
    }
}
