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
            label2 = new Label();
            txtConcepto = new TextBox();
            label3 = new Label();
            txtMonto = new TextBox();
            dtpVencimiento = new DateTimePicker();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(56, 86);
            label1.Name = "label1";
            label1.Size = new Size(90, 28);
            label1.TabIndex = 0;
            label1.Text = "CLIENTE";
            // 
            // cbClientes
            // 
            cbClientes.FormattingEnabled = true;
            cbClientes.Location = new Point(152, 88);
            cbClientes.Name = "cbClientes";
            cbClientes.Size = new Size(171, 28);
            cbClientes.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(55, 159);
            label2.Name = "label2";
            label2.Size = new Size(116, 28);
            label2.TabIndex = 2;
            label2.Text = "CONCEPTO";
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(177, 163);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(193, 27);
            txtConcepto.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label3.Location = new Point(58, 236);
            label3.Name = "label3";
            label3.Size = new Size(88, 28);
            label3.TabIndex = 4;
            label3.Text = "MONTO";
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(152, 240);
            txtMonto.Name = "txtMonto";
            txtMonto.Size = new Size(118, 27);
            txtMonto.TabIndex = 5;
            // 
            // dtpVencimiento
            // 
            dtpVencimiento.Location = new Point(55, 298);
            dtpVencimiento.Name = "dtpVencimiento";
            dtpVencimiento.Size = new Size(297, 27);
            dtpVencimiento.TabIndex = 6;
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.Location = new Point(62, 387);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(94, 29);
            btnGuardar.TabIndex = 7;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancelar.Location = new Point(200, 387);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(103, 29);
            btnCancelar.TabIndex = 8;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmCrearDeuda
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(425, 451);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(dtpVencimiento);
            Controls.Add(txtMonto);
            Controls.Add(label3);
            Controls.Add(txtConcepto);
            Controls.Add(label2);
            Controls.Add(cbClientes);
            Controls.Add(label1);
            Name = "FrmCrearDeuda";
            Text = "CREAR DEUDA";
            Load += FrmCrearDeuda_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox cbClientes;
        private Label label2;
        private TextBox txtConcepto;
        private Label label3;
        private TextBox txtMonto;
        private DateTimePicker dtpVencimiento;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}