namespace UI.DISEÑO
{
    partial class FrmRegistrarGasto
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitulo = new Label();
            lblTipoMovimiento = new Label();
            cmbTipoMovimiento = new ComboBox();
            lblConcepto = new Label();
            txtConcepto = new TextBox();
            lblMonto = new Label();
            txtMonto = new TextBox();
            btnLeerFactura = new Button();
            btnTomarFotoFactura = new Button();
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
            lblTitulo.Padding = new Padding(12, 12, 12, 0);
            lblTitulo.Size = new Size(299, 49);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Registrar Movimiento";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTipoMovimiento
            // 
            lblTipoMovimiento.AutoSize = true;
            lblTipoMovimiento.Location = new Point(24, 60);
            lblTipoMovimiento.Name = "lblTipoMovimiento";
            lblTipoMovimiento.Size = new Size(140, 20);
            lblTipoMovimiento.TabIndex = 1;
            lblTipoMovimiento.Text = "Tipo de movimiento";
            // 
            // cmbTipoMovimiento
            // 
            cmbTipoMovimiento.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoMovimiento.FormattingEnabled = true;
            cmbTipoMovimiento.Location = new Point(24, 86);
            cmbTipoMovimiento.Name = "cmbTipoMovimiento";
            cmbTipoMovimiento.Size = new Size(200, 28);
            cmbTipoMovimiento.TabIndex = 2;
            // 
            // lblConcepto
            // 
            lblConcepto.AutoSize = true;
            lblConcepto.Location = new Point(24, 130);
            lblConcepto.Name = "lblConcepto";
            lblConcepto.Size = new Size(280, 20);
            lblConcepto.TabIndex = 3;
            lblConcepto.Text = "Concepto (comercio + detalle factura)";
            // 
            // txtConcepto
            // 
            txtConcepto.AcceptsReturn = true;
            txtConcepto.Location = new Point(24, 156);
            txtConcepto.MaxLength = 1000;
            txtConcepto.Multiline = true;
            txtConcepto.Name = "txtConcepto";
            txtConcepto.ScrollBars = ScrollBars.Vertical;
            txtConcepto.Size = new Size(400, 110);
            txtConcepto.TabIndex = 4;
            // 
            // lblMonto
            // 
            lblMonto.AutoSize = true;
            lblMonto.Location = new Point(24, 278);
            lblMonto.Name = "lblMonto";
            lblMonto.Size = new Size(220, 20);
            lblMonto.TabIndex = 5;
            lblMonto.Text = "Monto (TOTAL a pagar)";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(24, 304);
            txtMonto.Name = "txtMonto";
            txtMonto.Size = new Size(200, 27);
            txtMonto.TabIndex = 6;
            // 
            // btnTomarFotoFactura
            // 
            btnTomarFotoFactura.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTomarFotoFactura.Location = new Point(24, 350);
            btnTomarFotoFactura.Name = "btnTomarFotoFactura";
            btnTomarFotoFactura.Size = new Size(170, 40);
            btnTomarFotoFactura.TabIndex = 7;
            btnTomarFotoFactura.Tag = "classic";
            btnTomarFotoFactura.Text = "Tomar foto (Iriun)";
            btnTomarFotoFactura.UseVisualStyleBackColor = true;
            btnTomarFotoFactura.Click += btnTomarFotoFactura_Click;
            // 
            // btnLeerFactura
            // 
            btnLeerFactura.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLeerFactura.Location = new Point(204, 350);
            btnLeerFactura.Name = "btnLeerFactura";
            btnLeerFactura.Size = new Size(130, 40);
            btnLeerFactura.TabIndex = 8;
            btnLeerFactura.Tag = "classic";
            btnLeerFactura.Text = "Desde archivo";
            btnLeerFactura.UseVisualStyleBackColor = true;
            btnLeerFactura.Click += btnLeerFactura_Click;
            // 
            // btnRegistrar
            // 
            btnRegistrar.BackColor = Color.LightGreen;
            btnRegistrar.FlatStyle = FlatStyle.Flat;
            btnRegistrar.Location = new Point(24, 402);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(150, 40);
            btnRegistrar.TabIndex = 9;
            btnRegistrar.Text = "Registrar";
            btnRegistrar.UseVisualStyleBackColor = false;
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.IndianRed;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(184, 402);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(150, 40);
            btnCancelar.TabIndex = 10;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmRegistrarGasto
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(450, 460);
            Controls.Add(btnCancelar);
            Controls.Add(btnRegistrar);
            Controls.Add(btnLeerFactura);
            Controls.Add(btnTomarFotoFactura);
            Controls.Add(txtMonto);
            Controls.Add(lblMonto);
            Controls.Add(txtConcepto);
            Controls.Add(lblConcepto);
            Controls.Add(cmbTipoMovimiento);
            Controls.Add(lblTipoMovimiento);
            Controls.Add(lblTitulo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRegistrarGasto";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Registrar Gasto";
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
        private Button btnTomarFotoFactura;
        private Button btnLeerFactura;
        private Button btnRegistrar;
        private Button btnCancelar;
    }
}
