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
            btnActualizar = new Button();
            btnExportar = new Button();
            btnImprimir = new Button();
            lblResumenExport = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            SuspendLayout();
            // 
            // dgvHistorial
            // 
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorial.Location = new Point(20, 150);
            dgvHistorial.MultiSelect = false;
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dgvHistorial.RowHeadersWidth = 51;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.Size = new Size(1065, 622);
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
            // btnActualizar
            // 
            btnActualizar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnActualizar.FlatStyle = FlatStyle.System;
            btnActualizar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnActualizar.Location = new Point(980, 784);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(143, 40);
            btnActualizar.TabIndex = 13;
            btnActualizar.Tag = "classic";
            btnActualizar.Text = "Actualizar";
            btnActualizar.UseVisualStyleBackColor = true;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportar.FlatStyle = FlatStyle.System;
            btnExportar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportar.Location = new Point(1151, 784);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(130, 40);
            btnExportar.TabIndex = 14;
            btnExportar.Tag = "classic";
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click;
            // 
            // btnImprimir
            // 
            btnImprimir.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImprimir.FlatStyle = FlatStyle.System;
            btnImprimir.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnImprimir.Location = new Point(1308, 784);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(130, 40);
            btnImprimir.TabIndex = 15;
            btnImprimir.Tag = "classic";
            btnImprimir.Text = "Imprimir";
            btnImprimir.UseVisualStyleBackColor = true;
            btnImprimir.Click += btnImprimir_Click;
            // 
            // lblResumenExport
            // 
            lblResumenExport.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblResumenExport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblResumenExport.ForeColor = Color.FromArgb(45, 45, 45);
            lblResumenExport.Location = new Point(20, 784);
            lblResumenExport.Name = "lblResumenExport";
            lblResumenExport.Size = new Size(940, 40);
            lblResumenExport.TabIndex = 16;
            lblResumenExport.Text = "Resumen financiero (filas visibles)";
            lblResumenExport.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FrmHistorialDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1488, 842);
            Controls.Add(lblResumenExport);
            Controls.Add(btnImprimir);
            Controls.Add(btnExportar);
            Controls.Add(btnActualizar);
            Controls.Add(lblHasta);
            Controls.Add(lblDesde);
            Controls.Add(lblTipo);
            Controls.Add(label1);
            Controls.Add(txtCliente);
            Controls.Add(dtpHasta);
            Controls.Add(dtpDesde);
            Controls.Add(cmbTipo);
            Controls.Add(dgvHistorial);
            Font = new Font("Segoe UI", 9F);
            ForeColor = SystemColors.ControlText;
            Name = "FrmHistorialDeudas";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "classic";
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
        private Button btnActualizar;
        private Button btnExportar;
        private Button btnImprimir;
        private Label lblResumenExport;
    }
}