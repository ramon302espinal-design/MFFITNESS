namespace UI
{
    partial class FrmDeudas
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
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            btnPagar = new Button();
            dgvDeudas = new DataGridView();
            btnNuevaDeuda = new Button();
            cmbFiltro = new ComboBox();
            lblFiltro = new Label();
            btnEnviarWhatsApp = new Button();
            btnActualizar = new Button();
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvDeudas).BeginInit();
            SuspendLayout();
            // 
            // btnPagar
            // 
            btnPagar.BackColor = SystemColors.MenuHighlight;
            btnPagar.FlatStyle = FlatStyle.Flat;
            btnPagar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(235, 487);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(129, 59);
            btnPagar.TabIndex = 0;
            btnPagar.Tag = "classic";
            btnPagar.Text = "ABONAR";
            btnPagar.UseVisualStyleBackColor = false;
            btnPagar.Click += btnPagar_Click;
            // 
            // dgvDeudas
            // 
            dgvDeudas.AllowUserToAddRows = false;
            dgvDeudas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDeudas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvDeudas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(27, 146, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvDeudas.DefaultCellStyle = dataGridViewCellStyle2;
            dgvDeudas.Dock = DockStyle.Top;
            dgvDeudas.Location = new Point(0, 0);
            dgvDeudas.Name = "dgvDeudas";
            dgvDeudas.RowHeadersWidth = 51;
            dgvDeudas.Size = new Size(1688, 402);
            dgvDeudas.TabIndex = 1;
            dgvDeudas.CellFormatting += dgvDeudas_CellFormatting;
            // 
            // btnNuevaDeuda
            // 
            btnNuevaDeuda.BackColor = Color.Red;
            btnNuevaDeuda.FlatStyle = FlatStyle.Flat;
            btnNuevaDeuda.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnNuevaDeuda.ForeColor = Color.White;
            btnNuevaDeuda.Location = new Point(63, 487);
            btnNuevaDeuda.Name = "btnNuevaDeuda";
            btnNuevaDeuda.Size = new Size(165, 59);
            btnNuevaDeuda.TabIndex = 3;
            btnNuevaDeuda.Tag = "classic";
            btnNuevaDeuda.Text = "NUEVA DEUDA";
            btnNuevaDeuda.UseVisualStyleBackColor = false;
            btnNuevaDeuda.Click += btnNuevaDeuda_Click;
            // 
            // cmbFiltro
            // 
            cmbFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltro.Font = new Font("Segoe UI", 10F);
            cmbFiltro.FormattingEnabled = true;
            cmbFiltro.Location = new Point(112, 437);
            cmbFiltro.Name = "cmbFiltro";
            cmbFiltro.Size = new Size(200, 31);
            cmbFiltro.TabIndex = 4;
            cmbFiltro.SelectedIndexChanged += cmbFiltro_SelectedIndexChanged;
            // 
            // lblFiltro
            // 
            lblFiltro.AutoSize = true;
            lblFiltro.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFiltro.Location = new Point(28, 440);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Size = new Size(81, 23);
            lblFiltro.TabIndex = 5;
            lblFiltro.Text = "FILTRAR:";
            // 
            // btnEnviarWhatsApp
            // 
            btnEnviarWhatsApp.BackColor = Color.Lime;
            btnEnviarWhatsApp.FlatStyle = FlatStyle.Flat;
            btnEnviarWhatsApp.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEnviarWhatsApp.ForeColor = SystemColors.WindowText;
            btnEnviarWhatsApp.Location = new Point(373, 487);
            btnEnviarWhatsApp.Name = "btnEnviarWhatsApp";
            btnEnviarWhatsApp.Size = new Size(180, 59);
            btnEnviarWhatsApp.TabIndex = 7;
            btnEnviarWhatsApp.Tag = "classic";
            btnEnviarWhatsApp.Text = "ENVIAR WHATSAPP";
            btnEnviarWhatsApp.UseVisualStyleBackColor = false;
            btnEnviarWhatsApp.Click += btnEnviarWhatsApp_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.BackColor = SystemColors.Window;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActualizar.Location = new Point(563, 487);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(140, 59);
            btnActualizar.TabIndex = 8;
            btnActualizar.Tag = "classic";
            btnActualizar.Text = "ACTUALIZAR";
            btnActualizar.UseVisualStyleBackColor = false;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBuscar.Location = new Point(330, 440);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(82, 23);
            lblBuscar.TabIndex = 9;
            lblBuscar.Text = "BUSCAR:";
            // 
            // txtBuscar
            // 
            txtBuscar.Font = new Font("Segoe UI", 10F);
            txtBuscar.Location = new Point(420, 437);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Cliente, concepto, plan, saldo...";
            txtBuscar.Size = new Size(380, 30);
            txtBuscar.TabIndex = 10;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // FrmDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1688, 727);
            Controls.Add(btnActualizar);
            Controls.Add(btnEnviarWhatsApp);
            Controls.Add(txtBuscar);
            Controls.Add(lblBuscar);
            Controls.Add(lblFiltro);
            Controls.Add(cmbFiltro);
            Controls.Add(btnNuevaDeuda);
            Controls.Add(dgvDeudas);
            Controls.Add(btnPagar);
            Name = "FrmDeudas";
            Text = "FrmDeudas";
            WindowState = FormWindowState.Maximized;
            Load += FrmDeudas_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDeudas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnPagar;
        private DataGridView dgvDeudas;
        private Button btnNuevaDeuda;
        private ComboBox cmbFiltro;
        private Label lblFiltro;
        private Button btnEnviarWhatsApp;
        private Button btnActualizar;
        private Label lblBuscar;
        private TextBox txtBuscar;
    }
}