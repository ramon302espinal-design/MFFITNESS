namespace UI.DISEÑO
{
    partial class FrmRegistrarGasto
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
            lblTipoMovimiento = new Label();
            cmbTipoMovimiento = new ComboBox();
            lblConcepto = new Label();
            txtConcepto = new TextBox();
            lblMonto = new Label();
            txtMonto = new TextBox();
            btnRegistrar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(299, 37);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Registrar Movimiento";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTipoMovimiento
            // 
            lblTipoMovimiento.AutoSize = true;
            lblTipoMovimiento.Location = new Point(12, 41);
            lblTipoMovimiento.Name = "lblTipoMovimiento";
            lblTipoMovimiento.Size = new Size(224, 20);
            lblTipoMovimiento.TabIndex = 1;
            lblTipoMovimiento.Text = "TipoMovimiento de Movimiento";
            // 
            // cmbTipoMovimiento
            // 
            cmbTipoMovimiento.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoMovimiento.FormattingEnabled = true;
            cmbTipoMovimiento.Location = new Point(12, 74);
            cmbTipoMovimiento.Name = "cmbTipoMovimiento";
            cmbTipoMovimiento.Size = new Size(151, 28);
            cmbTipoMovimiento.TabIndex = 2;
            // 
            // lblConcepto
            // 
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new Point(23, 127);
            lblConcepto.Name = "lblConcepto";
            lblConcepto.Size = new Size(73, 20);
            lblConcepto.TabIndex = 3;
            lblConcepto.Text = "Concepto";
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(102, 127);
            txtConcepto.MaxLength = 200;
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(125, 27);
            txtConcepto.TabIndex = 4;
            // 
            // lblMonto
            // 
            lblMonto.AutoSize = true;
            lblMonto.Location = new Point(23, 176);
            lblMonto.Name = "lblMonto";
            lblMonto.Size = new Size(53, 20);
            lblMonto.TabIndex = 5;
            lblMonto.Text = "Monto";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(73, 176);
            txtMonto.Name = "txtMonto";
            txtMonto.Size = new Size(125, 27);
            txtMonto.TabIndex = 6;
            // 
            // btnRegistrar
            // 
            btnRegistrar.BackColor = Color.LightGreen;
            btnRegistrar.FlatStyle = FlatStyle.Flat;
            btnRegistrar.Location = new Point(12, 218);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(130, 40);
            btnRegistrar.TabIndex = 7;
            btnRegistrar.Text = "Registrar";
            btnRegistrar.UseVisualStyleBackColor = false;
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.IndianRed;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(152, 218);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(130, 40);
            btnCancelar.TabIndex = 8;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmRegistrarGasto
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(382, 323);
            Controls.Add(btnCancelar);
            Controls.Add(btnRegistrar);
            Controls.Add(txtMonto);
            Controls.Add(lblMonto);
            Controls.Add(txtConcepto);
            Controls.Add(lblConcepto);
            Controls.Add(cmbTipoMovimiento);
            Controls.Add(lblTipoMovimiento);
            Controls.Add(lblTitulo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FrmRegistrarGasto";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "L";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblTipoMovimiento;
        private ComboBox cmbTipoMovimiento;
        private Label lblConcepto;
        private TextBox txtConcepto;
        private Label lblMonto;
        private TextBox txtMonto;
        private Button btnRegistrar;
        private Button btnCancelar;
    }
}