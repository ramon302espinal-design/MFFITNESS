namespace UI
{
    partial class FrmCrearDeuda
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
            label1 = new Label();
            cbClientes = new ComboBox();
            labelTipoPlan = new Label();
            cmbTipoPlan = new ComboBox();
            label2 = new Label();
            txtConcepto = new TextBox();
            label3 = new Label();
            txtMonto = new TextBox();
            dtpVencimiento = new DateTimePicker();
            pnlCrearDeuda = new Panel();
            lblPagoDeInicio = new Label();
            txtPagodeinicio = new TextBox();
            lblSaldoPendienteTitulo = new Label();
            lblSaldorestante = new Label();
            lblFechaLimiteDeuda = new Label();
            dtpFechaVencimientodeuda = new DateTimePicker();
            btnGuardar = new Button();
            btnCancelar = new Button();
            pnlCrearDeuda.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(90, 28);
            label1.TabIndex = 0;
            label1.Text = "CLIENTE";
            // 
            // cbClientes
            // 
            cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
            cbClientes.FormattingEnabled = true;
            cbClientes.Location = new Point(120, 15);
            cbClientes.Name = "cbClientes";
            cbClientes.Size = new Size(280, 28);
            cbClientes.TabIndex = 1;
            // 
            // labelTipoPlan
            // 
            labelTipoPlan.AutoSize = true;
            labelTipoPlan.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTipoPlan.Location = new Point(12, 58);
            labelTipoPlan.Name = "labelTipoPlan";
            labelTipoPlan.Size = new Size(102, 28);
            labelTipoPlan.TabIndex = 2;
            labelTipoPlan.Text = "TIPO PLAN";
            // 
            // cmbTipoPlan
            // 
            cmbTipoPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoPlan.FormattingEnabled = true;
            cmbTipoPlan.Location = new Point(120, 58);
            cmbTipoPlan.Name = "cmbTipoPlan";
            cmbTipoPlan.Size = new Size(280, 28);
            cmbTipoPlan.TabIndex = 3;
            cmbTipoPlan.SelectedIndexChanged += cmbTipoPlan_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(12, 101);
            label2.Name = "label2";
            label2.Size = new Size(116, 28);
            label2.TabIndex = 4;
            label2.Text = "CONCEPTO";
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(134, 102);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(266, 27);
            txtConcepto.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label3.Location = new Point(12, 144);
            label3.Name = "label3";
            label3.Size = new Size(148, 28);
            label3.TabIndex = 6;
            label3.Text = "MONTO PLAN";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(166, 145);
            txtMonto.Name = "txtMonto";
            txtMonto.ReadOnly = true;
            txtMonto.Size = new Size(150, 27);
            txtMonto.TabIndex = 7;
            txtMonto.TextAlign = HorizontalAlignment.Right;
            // 
            // dtpVencimiento
            // 
            dtpVencimiento.Location = new Point(12, 520);
            dtpVencimiento.Name = "dtpVencimiento";
            dtpVencimiento.Size = new Size(200, 27);
            dtpVencimiento.TabIndex = 99;
            dtpVencimiento.Visible = false;
            // 
            // pnlCrearDeuda
            // 
            pnlCrearDeuda.BackColor = Color.FromArgb(240, 248, 255);
            pnlCrearDeuda.BorderStyle = BorderStyle.FixedSingle;
            pnlCrearDeuda.Controls.Add(lblPagoDeInicio);
            pnlCrearDeuda.Controls.Add(txtPagodeinicio);
            pnlCrearDeuda.Controls.Add(lblSaldoPendienteTitulo);
            pnlCrearDeuda.Controls.Add(lblSaldorestante);
            pnlCrearDeuda.Controls.Add(lblFechaLimiteDeuda);
            pnlCrearDeuda.Controls.Add(dtpFechaVencimientodeuda);
            pnlCrearDeuda.Location = new Point(12, 190);
            pnlCrearDeuda.Name = "pnlCrearDeuda";
            pnlCrearDeuda.Size = new Size(400, 180);
            pnlCrearDeuda.TabIndex = 8;
            // 
            // lblPagoDeInicio
            // 
            lblPagoDeInicio.AutoSize = true;
            lblPagoDeInicio.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPagoDeInicio.Location = new Point(12, 16);
            lblPagoDeInicio.Name = "lblPagoDeInicio";
            lblPagoDeInicio.Size = new Size(138, 25);
            lblPagoDeInicio.TabIndex = 0;
            lblPagoDeInicio.Text = "Pago de inicio:";
            // 
            // txtPagodeinicio
            // 
            txtPagodeinicio.Font = new Font("Segoe UI", 12F);
            txtPagodeinicio.Location = new Point(170, 12);
            txtPagodeinicio.Name = "txtPagodeinicio";
            txtPagodeinicio.Size = new Size(150, 34);
            txtPagodeinicio.TabIndex = 1;
            txtPagodeinicio.Text = "0";
            txtPagodeinicio.TextAlign = HorizontalAlignment.Right;
            txtPagodeinicio.TextChanged += txtPagodeinicio_TextChanged;
            txtPagodeinicio.KeyPress += txtPagodeinicio_KeyPress;
            // 
            // lblSaldoPendienteTitulo
            // 
            lblSaldoPendienteTitulo.AutoSize = true;
            lblSaldoPendienteTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSaldoPendienteTitulo.Location = new Point(12, 64);
            lblSaldoPendienteTitulo.Name = "lblSaldoPendienteTitulo";
            lblSaldoPendienteTitulo.Size = new Size(161, 25);
            lblSaldoPendienteTitulo.TabIndex = 2;
            lblSaldoPendienteTitulo.Text = "Saldo pendiente:";
            // 
            // lblSaldorestante
            // 
            lblSaldorestante.AutoSize = true;
            lblSaldorestante.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSaldorestante.ForeColor = Color.FromArgb(244, 67, 54);
            lblSaldorestante.Location = new Point(170, 60);
            lblSaldorestante.Name = "lblSaldorestante";
            lblSaldorestante.Size = new Size(76, 32);
            lblSaldorestante.TabIndex = 3;
            lblSaldorestante.Text = "$0.00";
            // 
            // lblFechaLimiteDeuda
            // 
            lblFechaLimiteDeuda.AutoSize = true;
            lblFechaLimiteDeuda.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblFechaLimiteDeuda.Location = new Point(12, 118);
            lblFechaLimiteDeuda.Name = "lblFechaLimiteDeuda";
            lblFechaLimiteDeuda.Size = new Size(204, 25);
            lblFechaLimiteDeuda.TabIndex = 4;
            lblFechaLimiteDeuda.Text = "Fecha límite de pago:";
            // 
            // dtpFechaVencimientodeuda
            // 
            dtpFechaVencimientodeuda.Font = new Font("Segoe UI", 11F);
            dtpFechaVencimientodeuda.Format = DateTimePickerFormat.Short;
            dtpFechaVencimientodeuda.Location = new Point(220, 114);
            dtpFechaVencimientodeuda.Name = "dtpFechaVencimientodeuda";
            dtpFechaVencimientodeuda.Size = new Size(160, 32);
            dtpFechaVencimientodeuda.TabIndex = 5;
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.Location = new Point(70, 390);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 9;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancelar.Location = new Point(220, 390);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 10;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmCrearDeuda
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(430, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(pnlCrearDeuda);
            Controls.Add(dtpVencimiento);
            Controls.Add(txtMonto);
            Controls.Add(label3);
            Controls.Add(txtConcepto);
            Controls.Add(label2);
            Controls.Add(cmbTipoPlan);
            Controls.Add(labelTipoPlan);
            Controls.Add(cbClientes);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmCrearDeuda";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Crear deuda / Financiamiento";
            Load += FrmCrearDeuda_Load;
            pnlCrearDeuda.ResumeLayout(false);
            pnlCrearDeuda.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox cbClientes;
        private Label labelTipoPlan;
        private ComboBox cmbTipoPlan;
        private Label label2;
        private TextBox txtConcepto;
        private Label label3;
        private TextBox txtMonto;
        private DateTimePicker dtpVencimiento;
        private Panel pnlCrearDeuda;
        private Label lblPagoDeInicio;
        private TextBox txtPagodeinicio;
        private Label lblSaldoPendienteTitulo;
        private Label lblSaldorestante;
        private Label lblFechaLimiteDeuda;
        private DateTimePicker dtpFechaVencimientodeuda;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}
