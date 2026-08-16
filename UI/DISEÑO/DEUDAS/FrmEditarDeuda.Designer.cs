namespace UI
{
    partial class FrmEditarDeuda
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
            grpMiembro = new GroupBox();
            lblTituloCliente = new Label();
            lblCliente = new Label();
            lblTituloEstado = new Label();
            lblEstado = new Label();
            lblTituloRegistro = new Label();
            lblFechaCreacion = new Label();
            grpFinanciamiento = new GroupBox();
            lblTituloPlan = new Label();
            cmbPlan = new ComboBox();
            lblTituloConcepto = new Label();
            txtConcepto = new TextBox();
            lblTituloTotal = new Label();
            lblTotalFinanciado = new Label();
            lblPrecioPlan = new Label();
            lblTituloPagoInicial = new Label();
            txtPagoInicial = new TextBox();
            lblAvisoReverso = new Label();
            lblTituloVencimiento = new Label();
            dtpVencimiento = new DateTimePicker();
            lblTituloPagado = new Label();
            lblMontoPagado = new Label();
            lblTituloSaldo = new Label();
            lblSaldoResultante = new Label();
            lblNota = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            grpMiembro.SuspendLayout();
            grpFinanciamiento.SuspendLayout();
            SuspendLayout();
            // 
            // grpMiembro
            // 
            grpMiembro.Controls.Add(lblFechaCreacion);
            grpMiembro.Controls.Add(lblTituloRegistro);
            grpMiembro.Controls.Add(lblEstado);
            grpMiembro.Controls.Add(lblTituloEstado);
            grpMiembro.Controls.Add(lblCliente);
            grpMiembro.Controls.Add(lblTituloCliente);
            grpMiembro.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpMiembro.Location = new Point(12, 12);
            grpMiembro.Name = "grpMiembro";
            grpMiembro.Size = new Size(596, 92);
            grpMiembro.TabIndex = 0;
            grpMiembro.TabStop = false;
            grpMiembro.Text = "Miembro";
            // 
            // lblTituloCliente
            // 
            lblTituloCliente.AutoSize = true;
            lblTituloCliente.Font = new Font("Segoe UI", 9.5F);
            lblTituloCliente.Location = new Point(16, 30);
            lblTituloCliente.Name = "lblTituloCliente";
            lblTituloCliente.Size = new Size(72, 21);
            lblTituloCliente.TabIndex = 0;
            lblTituloCliente.Text = "Miembro:";
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.Location = new Point(140, 29);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(0, 23);
            lblCliente.TabIndex = 1;
            // 
            // lblTituloEstado
            // 
            lblTituloEstado.AutoSize = true;
            lblTituloEstado.Font = new Font("Segoe UI", 9.5F);
            lblTituloEstado.Location = new Point(16, 58);
            lblTituloEstado.Name = "lblTituloEstado";
            lblTituloEstado.Size = new Size(57, 21);
            lblTituloEstado.TabIndex = 2;
            lblTituloEstado.Text = "Estado:";
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblEstado.Location = new Point(140, 58);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(0, 21);
            lblEstado.TabIndex = 3;
            // 
            // lblTituloRegistro
            // 
            lblTituloRegistro.AutoSize = true;
            lblTituloRegistro.Font = new Font("Segoe UI", 9.5F);
            lblTituloRegistro.Location = new Point(330, 58);
            lblTituloRegistro.Name = "lblTituloRegistro";
            lblTituloRegistro.Size = new Size(84, 21);
            lblTituloRegistro.TabIndex = 4;
            lblTituloRegistro.Text = "Registrada:";
            // 
            // lblFechaCreacion
            // 
            lblFechaCreacion.AutoSize = true;
            lblFechaCreacion.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblFechaCreacion.Location = new Point(460, 58);
            lblFechaCreacion.Name = "lblFechaCreacion";
            lblFechaCreacion.Size = new Size(0, 21);
            lblFechaCreacion.TabIndex = 5;
            // 
            // grpFinanciamiento
            // 
            grpFinanciamiento.Controls.Add(lblSaldoResultante);
            grpFinanciamiento.Controls.Add(lblTituloSaldo);
            grpFinanciamiento.Controls.Add(lblMontoPagado);
            grpFinanciamiento.Controls.Add(lblTituloPagado);
            grpFinanciamiento.Controls.Add(dtpVencimiento);
            grpFinanciamiento.Controls.Add(lblTituloVencimiento);
            grpFinanciamiento.Controls.Add(lblAvisoReverso);
            grpFinanciamiento.Controls.Add(txtPagoInicial);
            grpFinanciamiento.Controls.Add(lblTituloPagoInicial);
            grpFinanciamiento.Controls.Add(lblPrecioPlan);
            grpFinanciamiento.Controls.Add(lblTotalFinanciado);
            grpFinanciamiento.Controls.Add(lblTituloTotal);
            grpFinanciamiento.Controls.Add(txtConcepto);
            grpFinanciamiento.Controls.Add(lblTituloConcepto);
            grpFinanciamiento.Controls.Add(cmbPlan);
            grpFinanciamiento.Controls.Add(lblTituloPlan);
            grpFinanciamiento.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grpFinanciamiento.Location = new Point(12, 112);
            grpFinanciamiento.Name = "grpFinanciamiento";
            grpFinanciamiento.Size = new Size(596, 268);
            grpFinanciamiento.TabIndex = 1;
            grpFinanciamiento.TabStop = false;
            grpFinanciamiento.Text = "Financiamiento";
            // 
            // lblTituloPlan
            // 
            lblTituloPlan.AutoSize = true;
            lblTituloPlan.Font = new Font("Segoe UI", 9.5F);
            lblTituloPlan.Location = new Point(16, 34);
            lblTituloPlan.Name = "lblTituloPlan";
            lblTituloPlan.Size = new Size(118, 21);
            lblTituloPlan.TabIndex = 0;
            lblTituloPlan.Text = "Plan / producto:";
            // 
            // cmbPlan
            // 
            cmbPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlan.Font = new Font("Segoe UI", 9.5F);
            cmbPlan.FormattingEnabled = true;
            cmbPlan.Location = new Point(170, 30);
            cmbPlan.Name = "cmbPlan";
            cmbPlan.Size = new Size(408, 29);
            cmbPlan.TabIndex = 1;
            cmbPlan.SelectedIndexChanged += cmbPlan_SelectedIndexChanged;
            // 
            // lblTituloConcepto
            // 
            lblTituloConcepto.AutoSize = true;
            lblTituloConcepto.Font = new Font("Segoe UI", 9.5F);
            lblTituloConcepto.Location = new Point(16, 74);
            lblTituloConcepto.Name = "lblTituloConcepto";
            lblTituloConcepto.Size = new Size(74, 21);
            lblTituloConcepto.TabIndex = 2;
            lblTituloConcepto.Text = "Concepto:";
            // 
            // txtConcepto
            // 
            txtConcepto.Font = new Font("Segoe UI", 9.5F);
            txtConcepto.Location = new Point(170, 70);
            txtConcepto.MaxLength = 200;
            txtConcepto.Name = "txtConcepto";
            txtConcepto.PlaceholderText = "Ej. Saldo plan M-A / Producto a crédito";
            txtConcepto.Size = new Size(408, 29);
            txtConcepto.TabIndex = 3;
            // 
            // lblTituloTotal
            // 
            lblTituloTotal.AutoSize = true;
            lblTituloTotal.Font = new Font("Segoe UI", 9.5F);
            lblTituloTotal.Location = new Point(16, 114);
            lblTituloTotal.Name = "lblTituloTotal";
            lblTituloTotal.Size = new Size(140, 21);
            lblTituloTotal.TabIndex = 4;
            lblTituloTotal.Text = "Total financiado:";
            // 
            // lblTotalFinanciado
            // 
            lblTotalFinanciado.AutoSize = true;
            lblTotalFinanciado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTotalFinanciado.Location = new Point(170, 113);
            lblTotalFinanciado.Name = "lblTotalFinanciado";
            lblTotalFinanciado.Size = new Size(20, 23);
            lblTotalFinanciado.TabIndex = 5;
            lblTotalFinanciado.Text = "—";
            // 
            // lblPrecioPlan
            // 
            lblPrecioPlan.AutoSize = true;
            lblPrecioPlan.Font = new Font("Segoe UI", 9F);
            lblPrecioPlan.ForeColor = SystemColors.GrayText;
            lblPrecioPlan.Location = new Point(324, 116);
            lblPrecioPlan.Name = "lblPrecioPlan";
            lblPrecioPlan.Size = new Size(0, 20);
            lblPrecioPlan.TabIndex = 6;
            // 
            // lblTituloPagoInicial
            // 
            lblTituloPagoInicial.AutoSize = true;
            lblTituloPagoInicial.Font = new Font("Segoe UI", 9.5F);
            lblTituloPagoInicial.Location = new Point(16, 154);
            lblTituloPagoInicial.Name = "lblTituloPagoInicial";
            lblTituloPagoInicial.Size = new Size(96, 21);
            lblTituloPagoInicial.TabIndex = 7;
            lblTituloPagoInicial.Text = "Pago inicial:";
            // 
            // txtPagoInicial
            // 
            txtPagoInicial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtPagoInicial.Location = new Point(170, 150);
            txtPagoInicial.MaxLength = 15;
            txtPagoInicial.Name = "txtPagoInicial";
            txtPagoInicial.Size = new Size(140, 30);
            txtPagoInicial.TabIndex = 8;
            txtPagoInicial.TextAlign = HorizontalAlignment.Right;
            txtPagoInicial.TextChanged += txtPagoInicial_TextChanged;
            txtPagoInicial.KeyPress += txtPagoInicial_KeyPress;
            txtPagoInicial.Leave += txtPagoInicial_Leave;
            // 
            // lblAvisoReverso
            // 
            lblAvisoReverso.Font = new Font("Segoe UI", 9F);
            lblAvisoReverso.ForeColor = Color.DarkOrange;
            lblAvisoReverso.Location = new Point(324, 148);
            lblAvisoReverso.Name = "lblAvisoReverso";
            lblAvisoReverso.Size = new Size(254, 36);
            lblAvisoReverso.TabIndex = 9;
            // 
            // lblTituloVencimiento
            // 
            lblTituloVencimiento.AutoSize = true;
            lblTituloVencimiento.Font = new Font("Segoe UI", 9.5F);
            lblTituloVencimiento.Location = new Point(16, 194);
            lblTituloVencimiento.Name = "lblTituloVencimiento";
            lblTituloVencimiento.Size = new Size(151, 21);
            lblTituloVencimiento.TabIndex = 10;
            lblTituloVencimiento.Text = "Fecha límite de pago:";
            // 
            // dtpVencimiento
            // 
            dtpVencimiento.Font = new Font("Segoe UI", 9.5F);
            dtpVencimiento.Format = DateTimePickerFormat.Short;
            dtpVencimiento.Location = new Point(170, 190);
            dtpVencimiento.Name = "dtpVencimiento";
            dtpVencimiento.Size = new Size(140, 29);
            dtpVencimiento.TabIndex = 11;
            dtpVencimiento.ValueChanged += dtpVencimiento_ValueChanged;
            // 
            // lblTituloPagado
            // 
            lblTituloPagado.AutoSize = true;
            lblTituloPagado.Font = new Font("Segoe UI", 9.5F);
            lblTituloPagado.Location = new Point(16, 232);
            lblTituloPagado.Name = "lblTituloPagado";
            lblTituloPagado.Size = new Size(150, 21);
            lblTituloPagado.TabIndex = 12;
            lblTituloPagado.Text = "Abonos a la deuda:";
            // 
            // lblMontoPagado
            // 
            lblMontoPagado.AutoSize = true;
            lblMontoPagado.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMontoPagado.Location = new Point(170, 232);
            lblMontoPagado.Name = "lblMontoPagado";
            lblMontoPagado.Size = new Size(66, 21);
            lblMontoPagado.TabIndex = 13;
            lblMontoPagado.Text = "RD$ 0.00";
            // 
            // lblTituloSaldo
            // 
            lblTituloSaldo.AutoSize = true;
            lblTituloSaldo.Font = new Font("Segoe UI", 9.5F);
            lblTituloSaldo.Location = new Point(324, 232);
            lblTituloSaldo.Name = "lblTituloSaldo";
            lblTituloSaldo.Size = new Size(126, 21);
            lblTituloSaldo.TabIndex = 14;
            lblTituloSaldo.Text = "Saldo resultante:";
            // 
            // lblSaldoResultante
            // 
            lblSaldoResultante.AutoSize = true;
            lblSaldoResultante.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSaldoResultante.ForeColor = Color.Firebrick;
            lblSaldoResultante.Location = new Point(456, 230);
            lblSaldoResultante.Name = "lblSaldoResultante";
            lblSaldoResultante.Size = new Size(20, 23);
            lblSaldoResultante.TabIndex = 15;
            lblSaldoResultante.Text = "—";
            // 
            // lblNota
            // 
            lblNota.Font = new Font("Segoe UI", 9F);
            lblNota.ForeColor = SystemColors.GrayText;
            lblNota.Location = new Point(12, 388);
            lblNota.Name = "lblNota";
            lblNota.Size = new Size(596, 40);
            lblNota.TabIndex = 2;
            lblNota.Text = "Al cambiar el pago inicial se reversa en caja el monto anterior y se registra el nuevo, y el saldo financiado se recalcula. No modifica la vigencia de la membresía.";
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnGuardar.Location = new Point(388, 434);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 34);
            btnGuardar.TabIndex = 3;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Font = new Font("Segoe UI", 9.5F);
            btnCancelar.Location = new Point(504, 434);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(104, 34);
            btnCancelar.TabIndex = 4;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FrmEditarDeuda
            // 
            AcceptButton = btnGuardar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(620, 482);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(lblNota);
            Controls.Add(grpFinanciamiento);
            Controls.Add(grpMiembro);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEditarDeuda";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Editar deuda";
            grpMiembro.ResumeLayout(false);
            grpMiembro.PerformLayout();
            grpFinanciamiento.ResumeLayout(false);
            grpFinanciamiento.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpMiembro;
        private Label lblTituloCliente;
        private Label lblCliente;
        private Label lblTituloEstado;
        private Label lblEstado;
        private Label lblTituloRegistro;
        private Label lblFechaCreacion;
        private GroupBox grpFinanciamiento;
        private Label lblTituloPlan;
        private ComboBox cmbPlan;
        private Label lblTituloConcepto;
        private TextBox txtConcepto;
        private Label lblTituloTotal;
        private Label lblTotalFinanciado;
        private Label lblPrecioPlan;
        private Label lblTituloPagoInicial;
        private TextBox txtPagoInicial;
        private Label lblAvisoReverso;
        private Label lblTituloVencimiento;
        private DateTimePicker dtpVencimiento;
        private Label lblTituloPagado;
        private Label lblMontoPagado;
        private Label lblTituloSaldo;
        private Label lblSaldoResultante;
        private Label lblNota;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}
