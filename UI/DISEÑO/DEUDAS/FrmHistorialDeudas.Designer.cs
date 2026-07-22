namespace UI
{
    partial class FrmHistorialDeudas
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
            dgvHistorial = new DataGridView();
            cmbTipo = new ComboBox();
            dtpDesde = new DateTimePicker();
            dtpHasta = new DateTimePicker();
            txtCliente = new TextBox();
            label1 = new Label();
            lblTipo = new Label();
            lblDesde = new Label();
            lblHasta = new Label();
            lblTotalDeudas = new Label();
            lblTotalPagos = new Label();
            lblBalance = new Label();
            btnActualizar = new Button();
            btnExportar = new Button();
            btnImprimir = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            SuspendLayout();
            // 
            // dgvHistorial
            // 
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.BackgroundColor = Color.White;
            dgvHistorial.BorderStyle = BorderStyle.None;
            dgvHistorial.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistorial.ColumnHeadersHeight = 35;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvHistorial.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvHistorial.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvHistorial.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvHistorial.EnableHeadersVisualStyles = false;
            dgvHistorial.Location = new Point(20, 150);
            dgvHistorial.MultiSelect = false;
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dgvHistorial.RowHeadersVisible = false;
            dgvHistorial.RowHeadersWidth = 51;
            dgvHistorial.RowTemplate.Height = 30;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.Size = new Size(1448, 622);
            dgvHistorial.TabIndex = 0;
            dgvHistorial.CellFormatting += dgvHistorial_CellFormatting;
            // 
            // cmbTipo
            // 
            cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipo.Font = new Font("Segoe UI", 10F);
            cmbTipo.FormattingEnabled = true;
            cmbTipo.Location = new Point(20, 100);
            cmbTipo.Name = "cmbTipo";
            cmbTipo.Size = new Size(180, 31);
            cmbTipo.TabIndex = 2;
            cmbTipo.SelectedIndexChanged += cmbTipo_SelectedIndexChanged;
            // 
            // dtpDesde
            // 
            dtpDesde.Font = new Font("Segoe UI", 10F);
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(540, 100);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(150, 30);
            dtpDesde.TabIndex = 3;
            dtpDesde.ValueChanged += dtpDesde_ValueChanged;
            // 
            // dtpHasta
            // 
            dtpHasta.Font = new Font("Segoe UI", 10F);
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(770, 100);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(150, 30);
            dtpHasta.TabIndex = 4;
            dtpHasta.ValueChanged += dtpHasta_ValueChanged;
            // 
            // txtCliente
            // 
            txtCliente.Font = new Font("Segoe UI", 10F);
            txtCliente.Location = new Point(159, 16);
            txtCliente.Name = "txtCliente";
            txtCliente.PlaceholderText = "Buscar cliente...";
            txtCliente.Size = new Size(350, 30);
            txtCliente.TabIndex = 5;
            txtCliente.TextChanged += txtCliente_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(128, 23);
            label1.TabIndex = 6;
            label1.Text = "Buscar Cliente:";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTipo.Location = new Point(20, 70);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(153, 23);
            lblTipo.TabIndex = 7;
            lblTipo.Text = "Tipo Movimiento:";
            // 
            // lblDesde
            // 
            lblDesde.AutoSize = true;
            lblDesde.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDesde.Location = new Point(540, 70);
            lblDesde.Name = "lblDesde";
            lblDesde.Size = new Size(114, 23);
            lblDesde.TabIndex = 8;
            lblDesde.Text = "Desde Fecha:";
            // 
            // lblHasta
            // 
            lblHasta.AutoSize = true;
            lblHasta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHasta.Location = new Point(770, 70);
            lblHasta.Name = "lblHasta";
            lblHasta.Size = new Size(110, 23);
            lblHasta.TabIndex = 9;
            lblHasta.Text = "Hasta Fecha:";
            // 
            // lblTotalDeudas
            // 
            lblTotalDeudas.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotalDeudas.AutoSize = true;
            lblTotalDeudas.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalDeudas.ForeColor = Color.Red;
            lblTotalDeudas.Location = new Point(20, 792);
            lblTotalDeudas.Name = "lblTotalDeudas";
            lblTotalDeudas.Size = new Size(185, 25);
            lblTotalDeudas.TabIndex = 10;
            lblTotalDeudas.Text = "Total Deudas: $0.00";
            // 
            // lblTotalPagos
            // 
            lblTotalPagos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTotalPagos.AutoSize = true;
            lblTotalPagos.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPagos.ForeColor = Color.Green;
            lblTotalPagos.Location = new Point(250, 792);
            lblTotalPagos.Name = "lblTotalPagos";
            lblTotalPagos.Size = new Size(173, 25);
            lblTotalPagos.TabIndex = 11;
            lblTotalPagos.Text = "Total Pagos: $0.00";
            // 
            // lblBalance
            // 
            lblBalance.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblBalance.AutoSize = true;
            lblBalance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBalance.ForeColor = Color.Black;
            lblBalance.Location = new Point(470, 792);
            lblBalance.Name = "lblBalance";
            lblBalance.Size = new Size(139, 25);
            lblBalance.TabIndex = 12;
            lblBalance.Text = "Balance: $0.00";
            // 
            // btnActualizar
            // 
            btnActualizar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnActualizar.BackColor = Color.FromArgb(0, 122, 204);
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.Location = new Point(980, 784);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(143, 40);
            btnActualizar.TabIndex = 13;
            btnActualizar.Text = "Actualizar";
            btnActualizar.UseVisualStyleBackColor = false;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportar.BackColor = Color.FromArgb(40, 167, 69);
            btnExportar.FlatStyle = FlatStyle.Flat;
            btnExportar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportar.ForeColor = Color.White;
            btnExportar.Location = new Point(1151, 784);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 40);
            btnExportar.TabIndex = 14;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = false;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnImprimir
            // 
            btnImprimir.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImprimir.BackColor = Color.FromArgb(108, 117, 125);
            btnImprimir.FlatStyle = FlatStyle.Flat;
            btnImprimir.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnImprimir.ForeColor = Color.White;
            btnImprimir.Location = new Point(1308, 784);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(130, 40);
            btnImprimir.TabIndex = 15;
            btnImprimir.Text = "Imprimir";
            btnImprimir.UseVisualStyleBackColor = false;
            btnImprimir.Click += btnImprimir_Click;
            // 
            // FrmHistorialDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1488, 842);
            Controls.Add(btnImprimir);
            Controls.Add(btnExportar);
            Controls.Add(btnActualizar);
            Controls.Add(lblBalance);
            Controls.Add(lblTotalPagos);
            Controls.Add(lblTotalDeudas);
            Controls.Add(lblHasta);
            Controls.Add(lblDesde);
            Controls.Add(lblTipo);
            Controls.Add(label1);
            Controls.Add(txtCliente);
            Controls.Add(dtpHasta);
            Controls.Add(dtpDesde);
            Controls.Add(cmbTipo);
            Controls.Add(dgvHistorial);
            Name = "FrmHistorialDeudas";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Historial de Deudas y Pagos";
            WindowState = FormWindowState.Maximized;
            Load += FrmHistorialDeudas_Load;
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvHistorial;
        private ComboBox cmbTipo;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private TextBox txtCliente;
        private Label label1;
        private Label lblTipo;
        private Label lblDesde;
        private Label lblHasta;
        private Label lblTotalDeudas;
        private Label lblTotalPagos;
        private Label lblBalance;
        private Button btnActualizar;
        private Button btnExportar;
        private Button btnImprimir;
    }
}