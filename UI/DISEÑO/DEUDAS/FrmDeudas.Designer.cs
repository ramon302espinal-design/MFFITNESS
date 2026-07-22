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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            btnPagar = new Button();
            dgvDeudas = new DataGridView();
            btnNuevaDeuda = new Button();
            cmbFiltro = new ComboBox();
            lblFiltro = new Label();
            btnVerHistorial = new Button();
            btnEnviarWhatsApp = new Button();
            btnActualizar = new Button();
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvDeudas).BeginInit();
            SuspendLayout();
            // 
            // btnPagar
            // 
            btnPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnPagar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(246, 487);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(129, 40);
            btnPagar.TabIndex = 0;
            btnPagar.Text = "COBRAR";
            btnPagar.UseVisualStyleBackColor = false;
            btnPagar.Click += btnPagar_Click;
            // 
            // dgvDeudas
            // 
            dgvDeudas.AllowUserToAddRows = false;
            dgvDeudas.AllowUserToDeleteRows = false;
            dgvDeudas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvDeudas.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 146, 255);
            dgvDeudas.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
            dgvDeudas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDeudas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvDeudas.BackgroundColor = Color.White;
            dgvDeudas.BorderStyle = BorderStyle.None;
            dgvDeudas.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDeudas.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            dgvDeudas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDeudas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(27, 146, 255);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvDeudas.DefaultCellStyle = dataGridViewCellStyle1;
            dgvDeudas.Dock = DockStyle.Top;
            dgvDeudas.EnableHeadersVisualStyles = false;
            dgvDeudas.Location = new Point(0, 0);
            dgvDeudas.MultiSelect = false;
            dgvDeudas.Name = "dgvDeudas";
            dgvDeudas.ReadOnly = true;
            dgvDeudas.RowHeadersVisible = false;
            dgvDeudas.RowHeadersWidth = 51;
            dgvDeudas.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 146, 255);
            dgvDeudas.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dgvDeudas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDeudas.Size = new Size(1688, 402);
            dgvDeudas.TabIndex = 1;
            dgvDeudas.CellFormatting += dgvDeudas_CellFormatting;
            // 
            // btnNuevaDeuda
            // 
            btnNuevaDeuda.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnNuevaDeuda.Location = new Point(63, 487);
            btnNuevaDeuda.Name = "btnNuevaDeuda";
            btnNuevaDeuda.Size = new Size(165, 40);
            btnNuevaDeuda.TabIndex = 3;
            btnNuevaDeuda.Text = "NUEVA DEUDA";
            btnNuevaDeuda.UseVisualStyleBackColor = true;
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
            // btnVerHistorial
            // 
            btnVerHistorial.Font = new Font("Segoe UI", 10F);
            btnVerHistorial.Location = new Point(400, 487);
            btnVerHistorial.Name = "btnVerHistorial";
            btnVerHistorial.Size = new Size(140, 40);
            btnVerHistorial.TabIndex = 6;
            btnVerHistorial.Text = "VER HISTORIAL";
            btnVerHistorial.UseVisualStyleBackColor = true;
            btnVerHistorial.Click += btnVerHistorial_Click;
            // 
            // btnEnviarWhatsApp
            // 
            btnEnviarWhatsApp.Font = new Font("Segoe UI", 10F);
            btnEnviarWhatsApp.Location = new Point(560, 487);
            btnEnviarWhatsApp.Name = "btnEnviarWhatsApp";
            btnEnviarWhatsApp.Size = new Size(180, 40);
            btnEnviarWhatsApp.TabIndex = 7;
            btnEnviarWhatsApp.Text = "ENVIAR WHATSAPP";
            btnEnviarWhatsApp.UseVisualStyleBackColor = true;
            btnEnviarWhatsApp.Click += btnEnviarWhatsApp_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.Font = new Font("Segoe UI", 10F);
            btnActualizar.Location = new Point(760, 487);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(140, 40);
            btnActualizar.TabIndex = 8;
            btnActualizar.Text = "ACTUALIZAR";
            btnActualizar.UseVisualStyleBackColor = true;
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
            Controls.Add(btnVerHistorial);
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
        private Button btnVerHistorial;
        private Button btnEnviarWhatsApp;
        private Button btnActualizar;
        private Label lblBuscar;
        private TextBox txtBuscar;
    }
}