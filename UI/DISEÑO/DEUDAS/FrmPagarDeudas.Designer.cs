namespace UI
{
    partial class FrmPagarDeudas
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
            txtMonto = new TextBox();
            cmbMetodo = new ComboBox();
            btnConfirmar = new Button();
            lblCliente = new Label();
            lblUltimoPago = new Label();
            lblSaldo = new Label();
            lblEstado = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label1.Location = new Point(12, 207);
            label1.Name = "label1";
            label1.Size = new Size(111, 35);
            label1.TabIndex = 0;
            label1.Text = "MONTO";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(119, 215);
            txtMonto.Name = "txtMonto";
            txtMonto.Size = new Size(125, 27);
            txtMonto.TabIndex = 1;
            // 
            // cmbMetodo
            // 
            cmbMetodo.FormattingEnabled = true;
            cmbMetodo.Location = new Point(12, 264);
            cmbMetodo.Name = "cmbMetodo";
            cmbMetodo.Size = new Size(151, 28);
            cmbMetodo.TabIndex = 2;
            // 
            // btnConfirmar
            // 
            btnConfirmar.BackColor = Color.FromArgb(22, 163, 74);
            btnConfirmar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.Location = new Point(169, 263);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new Size(94, 29);
            btnConfirmar.TabIndex = 3;
            btnConfirmar.Text = "COBRAR";
            btnConfirmar.UseVisualStyleBackColor = false;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCliente.Location = new Point(12, 18);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(0, 28);
            lblCliente.TabIndex = 4;
            // 
            // lblUltimoPago
            // 
            lblUltimoPago.AutoSize = true;
            lblUltimoPago.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUltimoPago.Location = new Point(24, 153);
            lblUltimoPago.Name = "lblUltimoPago";
            lblUltimoPago.Size = new Size(0, 28);
            lblUltimoPago.TabIndex = 5;
            // 
            // lblSaldo
            // 
            lblSaldo.AutoSize = true;
            lblSaldo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSaldo.Location = new Point(12, 101);
            lblSaldo.Name = "lblSaldo";
            lblSaldo.Size = new Size(24, 28);
            lblSaldo.TabIndex = 7;
            lblSaldo.Text = "0";
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblEstado.Location = new Point(303, 18);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(0, 28);
            lblEstado.TabIndex = 8;
            // 
            // FrmPagarDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(406, 357);
            Controls.Add(lblEstado);
            Controls.Add(lblSaldo);
            Controls.Add(lblUltimoPago);
            Controls.Add(lblCliente);
            Controls.Add(btnConfirmar);
            Controls.Add(cmbMetodo);
            Controls.Add(txtMonto);
            Controls.Add(label1);
            Name = "FrmPagarDeudas";
            Text = "Pagar deuda";
            Load += FrmPagarDeudas_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtMonto;
        private ComboBox cmbMetodo;
        private Button btnConfirmar;
        private Label lblCliente;
        private Label lblUltimoPago;
        private Label lblSaldo;
        private Label lblEstado;
    }
}