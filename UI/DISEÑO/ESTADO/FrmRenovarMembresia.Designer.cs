namespace UI.DISEÑO
{
    partial class FrmRenovarMembresia
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
            lblPlan = new Label();
            cmbPlan = new ComboBox();
            lblPrecio = new Label();
            chkFinanciamiento = new CheckBox();
            pnlFinanciamiento = new Panel();
            lblPagoInicial = new Label();
            txtPagoInicial = new TextBox();
            lblSaldo = new Label();
            lblSaldoValor = new Label();
            lblFechaLimite = new Label();
            dtpFechaLimitePago = new DateTimePicker();
            pnlOferta = new Panel();
            lblOfertaPct = new Label();
            txtDescuentoPorcental = new TextBox();
            lblOfertaMonto = new Label();
            txtDescuentoMonto = new TextBox();
            lblTotalPagarTitulo = new Label();
            lblTotalPagar = new Label();
            lblMotivoOferta = new Label();
            txtMotivo = new TextBox();
            panelAcciones = new Panel();
            btnConfirmar = new Button();
            btnCancelar = new Button();
            pnlFinanciamiento.SuspendLayout();
            pnlOferta.SuspendLayout();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.Location = new Point(16, 16);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(66, 23);
            lblCliente.TabIndex = 0;
            lblCliente.Text = "Cliente";
            // 
            // lblPlan
            // 
            lblPlan.AutoSize = true;
            lblPlan.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblPlan.Location = new Point(16, 52);
            lblPlan.Name = "lblPlan";
            lblPlan.Size = new Size(49, 23);
            lblPlan.TabIndex = 1;
            lblPlan.Text = "Plan:";
            // 
            // cmbPlan
            // 
            cmbPlan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlan.Font = new Font("Segoe UI", 10F);
            cmbPlan.FormattingEnabled = true;
            cmbPlan.Location = new Point(16, 78);
            cmbPlan.Name = "cmbPlan";
            cmbPlan.Size = new Size(381, 31);
            cmbPlan.TabIndex = 2;
            cmbPlan.SelectedIndexChanged += cmbPlan_SelectedIndexChanged;
            // 
            // lblPrecio
            // 
            lblPrecio.AutoSize = true;
            lblPrecio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPrecio.Location = new Point(16, 120);
            lblPrecio.Name = "lblPrecio";
            lblPrecio.Size = new Size(143, 23);
            lblPrecio.TabIndex = 3;
            lblPrecio.Text = "Precio: RD$ 0.00";
            // 
            // chkFinanciamiento
            // 
            chkFinanciamiento.AutoSize = true;
            chkFinanciamiento.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            chkFinanciamiento.ForeColor = Color.FromArgb(33, 150, 243);
            chkFinanciamiento.Location = new Point(16, 152);
            chkFinanciamiento.Name = "chkFinanciamiento";
            chkFinanciamiento.Size = new Size(171, 29);
            chkFinanciamiento.TabIndex = 4;
            chkFinanciamiento.Text = " Financiamiento";
            chkFinanciamiento.UseVisualStyleBackColor = true;
            chkFinanciamiento.CheckedChanged += chkFinanciamiento_CheckedChanged;
            // 
            // pnlFinanciamiento
            // 
            pnlFinanciamiento.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlFinanciamiento.BackColor = Color.FromArgb(240, 248, 255);
            pnlFinanciamiento.BorderStyle = BorderStyle.FixedSingle;
            pnlFinanciamiento.Controls.Add(lblPagoInicial);
            pnlFinanciamiento.Controls.Add(txtPagoInicial);
            pnlFinanciamiento.Controls.Add(lblSaldo);
            pnlFinanciamiento.Controls.Add(lblSaldoValor);
            pnlFinanciamiento.Controls.Add(lblFechaLimite);
            pnlFinanciamiento.Controls.Add(dtpFechaLimitePago);
            pnlFinanciamiento.Location = new Point(16, 188);
            pnlFinanciamiento.Name = "pnlFinanciamiento";
            pnlFinanciamiento.Size = new Size(381, 170);
            pnlFinanciamiento.TabIndex = 5;
            pnlFinanciamiento.Visible = false;
            // 
            // lblPagoInicial
            // 
            lblPagoInicial.AutoSize = true;
            lblPagoInicial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPagoInicial.Location = new Point(14, 16);
            lblPagoInicial.Name = "lblPagoInicial";
            lblPagoInicial.Size = new Size(108, 23);
            lblPagoInicial.TabIndex = 0;
            lblPagoInicial.Text = "Pago Inicial:";
            // 
            // txtPagoInicial
            // 
            txtPagoInicial.Font = new Font("Segoe UI", 12F);
            txtPagoInicial.Location = new Point(160, 12);
            txtPagoInicial.Name = "txtPagoInicial";
            txtPagoInicial.Size = new Size(140, 34);
            txtPagoInicial.TabIndex = 1;
            txtPagoInicial.Text = "0";
            txtPagoInicial.TextAlign = HorizontalAlignment.Right;
            txtPagoInicial.TextChanged += txtPagoInicial_TextChanged;
            // 
            // lblSaldo
            // 
            lblSaldo.AutoSize = true;
            lblSaldo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSaldo.Location = new Point(14, 62);
            lblSaldo.Name = "lblSaldo";
            lblSaldo.Size = new Size(146, 23);
            lblSaldo.TabIndex = 2;
            lblSaldo.Text = "Saldo Pendiente:";
            // 
            // lblSaldoValor
            // 
            lblSaldoValor.AutoSize = true;
            lblSaldoValor.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSaldoValor.ForeColor = Color.FromArgb(244, 67, 54);
            lblSaldoValor.Location = new Point(160, 56);
            lblSaldoValor.Name = "lblSaldoValor";
            lblSaldoValor.Size = new Size(77, 31);
            lblSaldoValor.TabIndex = 3;
            lblSaldoValor.Text = "$0.00";
            // 
            // lblFechaLimite
            // 
            lblFechaLimite.AutoSize = true;
            lblFechaLimite.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaLimite.Location = new Point(14, 112);
            lblFechaLimite.Name = "lblFechaLimite";
            lblFechaLimite.Size = new Size(183, 23);
            lblFechaLimite.TabIndex = 4;
            lblFechaLimite.Text = "Fecha Límite de Pago:";
            // 
            // dtpFechaLimitePago
            // 
            dtpFechaLimitePago.Enabled = false;
            dtpFechaLimitePago.Font = new Font("Segoe UI", 10F);
            dtpFechaLimitePago.Format = DateTimePickerFormat.Short;
            dtpFechaLimitePago.Location = new Point(203, 108);
            dtpFechaLimitePago.Name = "dtpFechaLimitePago";
            dtpFechaLimitePago.Size = new Size(150, 30);
            dtpFechaLimitePago.TabIndex = 5;
            // 
            // pnlOferta
            // 
            pnlOferta.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlOferta.BorderStyle = BorderStyle.FixedSingle;
            pnlOferta.Controls.Add(lblOfertaPct);
            pnlOferta.Controls.Add(txtDescuentoPorcental);
            pnlOferta.Controls.Add(lblOfertaMonto);
            pnlOferta.Controls.Add(txtDescuentoMonto);
            pnlOferta.Controls.Add(lblTotalPagarTitulo);
            pnlOferta.Controls.Add(lblTotalPagar);
            pnlOferta.Controls.Add(lblMotivoOferta);
            pnlOferta.Controls.Add(txtMotivo);
            pnlOferta.Location = new Point(16, 188);
            pnlOferta.Name = "pnlOferta";
            pnlOferta.Size = new Size(381, 245);
            pnlOferta.TabIndex = 6;
            pnlOferta.Tag = "classic";
            pnlOferta.Visible = false;
            // 
            // lblOfertaPct
            // 
            lblOfertaPct.AutoSize = true;
            lblOfertaPct.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblOfertaPct.Location = new Point(19, 25);
            lblOfertaPct.Name = "lblOfertaPct";
            lblOfertaPct.Size = new Size(100, 23);
            lblOfertaPct.TabIndex = 0;
            lblOfertaPct.Text = "Cortesía %:";
            // 
            // txtDescuentoPorcental
            // 
            txtDescuentoPorcental.Font = new Font("Segoe UI", 10F);
            txtDescuentoPorcental.Location = new Point(139, 21);
            txtDescuentoPorcental.Name = "txtDescuentoPorcental";
            txtDescuentoPorcental.Size = new Size(80, 30);
            txtDescuentoPorcental.TabIndex = 1;
            txtDescuentoPorcental.Text = "100";
            txtDescuentoPorcental.TextAlign = HorizontalAlignment.Right;
            txtDescuentoPorcental.TextChanged += txtDescuentoPorcental_TextChanged;
            // 
            // lblOfertaMonto
            // 
            lblOfertaMonto.AutoSize = true;
            lblOfertaMonto.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblOfertaMonto.Location = new Point(19, 65);
            lblOfertaMonto.Name = "lblOfertaMonto";
            lblOfertaMonto.Size = new Size(127, 23);
            lblOfertaMonto.TabIndex = 2;
            lblOfertaMonto.Text = "Valor ref. RD$:";
            // 
            // txtDescuentoMonto
            // 
            txtDescuentoMonto.Font = new Font("Segoe UI", 10F);
            txtDescuentoMonto.Location = new Point(139, 61);
            txtDescuentoMonto.Name = "txtDescuentoMonto";
            txtDescuentoMonto.Size = new Size(120, 30);
            txtDescuentoMonto.TabIndex = 3;
            txtDescuentoMonto.Text = "0.00";
            txtDescuentoMonto.TextAlign = HorizontalAlignment.Right;
            txtDescuentoMonto.TextChanged += txtDescuentoMonto_TextChanged;
            // 
            // lblTotalPagarTitulo
            // 
            lblTotalPagarTitulo.AutoSize = true;
            lblTotalPagarTitulo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblTotalPagarTitulo.Location = new Point(19, 105);
            lblTotalPagarTitulo.Name = "lblTotalPagarTitulo";
            lblTotalPagarTitulo.Size = new Size(121, 23);
            lblTotalPagarTitulo.TabIndex = 4;
            lblTotalPagarTitulo.Text = "Total a pagar:";
            // 
            // lblTotalPagar
            // 
            lblTotalPagar.AutoSize = true;
            lblTotalPagar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPagar.ForeColor = Color.FromArgb(22, 163, 74);
            lblTotalPagar.Location = new Point(139, 103);
            lblTotalPagar.Name = "lblTotalPagar";
            lblTotalPagar.Size = new Size(92, 25);
            lblTotalPagar.TabIndex = 5;
            lblTotalPagar.Text = "RD$ 0.00";
            // 
            // lblMotivoOferta
            // 
            lblMotivoOferta.AutoSize = true;
            lblMotivoOferta.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblMotivoOferta.Location = new Point(19, 137);
            lblMotivoOferta.Name = "lblMotivoOferta";
            lblMotivoOferta.Size = new Size(72, 23);
            lblMotivoOferta.TabIndex = 6;
            lblMotivoOferta.Text = "Motivo:";
            // 
            // txtMotivo
            // 
            txtMotivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMotivo.Font = new Font("Segoe UI", 10F);
            txtMotivo.Location = new Point(19, 163);
            txtMotivo.Multiline = true;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.PlaceholderText = "Ej. promo temporada, referido...";
            txtMotivo.Size = new Size(357, 56);
            txtMotivo.TabIndex = 7;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnConfirmar);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 448);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(12, 8, 12, 8);
            panelAcciones.Size = new Size(413, 52);
            panelAcciones.TabIndex = 7;
            panelAcciones.Tag = "classic";
            // 
            // btnConfirmar
            // 
            btnConfirmar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfirmar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnConfirmar.Location = new Point(193, 8);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new Size(120, 34);
            btnConfirmar.TabIndex = 0;
            btnConfirmar.Text = "CONFIRMAR";
            btnConfirmar.UseVisualStyleBackColor = true;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Font = new Font("Segoe UI", 9.75F);
            btnCancelar.Location = new Point(319, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(82, 34);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FrmRenovarMembresia
            // 
            AcceptButton = btnConfirmar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(413, 500);
            Controls.Add(pnlOferta);
            Controls.Add(pnlFinanciamiento);
            Controls.Add(chkFinanciamiento);
            Controls.Add(panelAcciones);
            Controls.Add(lblPrecio);
            Controls.Add(cmbPlan);
            Controls.Add(lblPlan);
            Controls.Add(lblCliente);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(380, 420);
            Name = "FrmRenovarMembresia";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Tag = "classic";
            Text = "Renovar membresía";
            Load += FrmRenovarMembresia_Load;
            pnlFinanciamiento.ResumeLayout(false);
            pnlFinanciamiento.PerformLayout();
            pnlOferta.ResumeLayout(false);
            pnlOferta.PerformLayout();
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCliente;
        private Label lblPlan;
        private ComboBox cmbPlan;
        private Label lblPrecio;
        private CheckBox chkFinanciamiento;
        private Panel pnlFinanciamiento;
        private Label lblPagoInicial;
        private TextBox txtPagoInicial;
        private Label lblSaldo;
        private Label lblSaldoValor;
        private Label lblFechaLimite;
        private DateTimePicker dtpFechaLimitePago;
        private Panel pnlOferta;
        private Label lblOfertaPct;
        private TextBox txtDescuentoPorcental;
        private Label lblOfertaMonto;
        private TextBox txtDescuentoMonto;
        private Label lblTotalPagarTitulo;
        private Label lblTotalPagar;
        private Label lblMotivoOferta;
        private TextBox txtMotivo;
        private Panel panelAcciones;
        private Button btnConfirmar;
        private Button btnCancelar;
    }
}
