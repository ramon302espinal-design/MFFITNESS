namespace UI
{
    partial class FrmReportes
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
            panelNav = new Panel();
            btnNavClientes = new Button();
            btnNavReportes = new Button();
            btnNavInventario = new Button();
            btnNavHistorial = new Button();
            btnNavCaja = new Button();
            btnNavEstado = new Button();
            btnNavDeudas = new Button();
            btnNavPagar = new Button();
            btnBack = new Button();
            dtDesde = new DateTimePicker();
            dtHasta = new DateTimePicker();
            label1 = new Label();
            label2 = new Label();
            lbltiempo = new Label();
            cmbReporte = new ComboBox();
            btnGenerarReporte = new Button();
            btnGenerarPDF = new Button();
            btnGenerarExcel = new Button();
            dgvMostrarDatos = new DataGridView();
            label3 = new Label();
            lblTotal = new Label();
            panelNav.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMostrarDatos).BeginInit();
            SuspendLayout();
            // 
            // panelNav
            // 
            panelNav.BackColor = Color.White;
            panelNav.Controls.Add(btnNavClientes);
            panelNav.Controls.Add(btnNavReportes);
            panelNav.Controls.Add(btnNavInventario);
            panelNav.Controls.Add(btnNavHistorial);
            panelNav.Controls.Add(btnNavCaja);
            panelNav.Controls.Add(btnNavEstado);
            panelNav.Controls.Add(btnNavDeudas);
            panelNav.Controls.Add(btnNavPagar);
            panelNav.Controls.Add(btnBack);
            panelNav.Dock = DockStyle.Top;
            panelNav.Location = new Point(0, 0);
            panelNav.Name = "panelNav";
            panelNav.Size = new Size(1150, 52);
            panelNav.TabIndex = 0;
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(940, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 8;
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = true;
            // 
            // btnNavReportes
            // 
            btnNavReportes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavReportes.Location = new Point(810, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new Size(120, 32);
            btnNavReportes.TabIndex = 7;
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = true;
            // 
            // btnNavInventario
            // 
            btnNavInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavInventario.Location = new Point(670, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new Size(130, 32);
            btnNavInventario.TabIndex = 6;
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = true;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavHistorial.Location = new Point(540, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new Size(120, 32);
            btnNavHistorial.TabIndex = 5;
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = true;
            // 
            // btnNavCaja
            // 
            btnNavCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavCaja.Location = new Point(420, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new Size(110, 32);
            btnNavCaja.TabIndex = 4;
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = true;
            // 
            // btnNavEstado
            // 
            btnNavEstado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavEstado.Location = new Point(300, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new Size(110, 32);
            btnNavEstado.TabIndex = 3;
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = true;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavDeudas.Location = new Point(180, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new Size(110, 32);
            btnNavDeudas.TabIndex = 2;
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // btnNavPagar
            // 
            btnNavPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnNavPagar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavPagar.ForeColor = Color.White;
            btnNavPagar.Location = new Point(60, 10);
            btnNavPagar.Name = "btnNavPagar";
            btnNavPagar.Size = new Size(110, 32);
            btnNavPagar.TabIndex = 1;
            btnNavPagar.Text = "COBRAR";
            btnNavPagar.UseVisualStyleBackColor = false;
            // 
            // btnBack
            // 
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Segoe UI", 11F);
            btnBack.Location = new Point(8, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(43, 35);
            btnBack.TabIndex = 0;
            btnBack.UseVisualStyleBackColor = true;
            // 
            // dtDesde
            // 
            dtDesde.Format = DateTimePickerFormat.Short;
            dtDesde.Location = new Point(144, 96);
            dtDesde.Name = "dtDesde";
            dtDesde.Size = new Size(278, 27);
            dtDesde.TabIndex = 0;
            dtDesde.ValueChanged += RangoFechas_ValueChanged;
            // 
            // dtHasta
            // 
            dtHasta.Format = DateTimePickerFormat.Short;
            dtHasta.Location = new Point(428, 96);
            dtHasta.Name = "dtHasta";
            dtHasta.Size = new Size(272, 27);
            dtHasta.TabIndex = 1;
            dtHasta.ValueChanged += RangoFechas_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(241, 64);
            label1.Name = "label1";
            label1.Size = new Size(55, 20);
            label1.TabIndex = 2;
            label1.Text = "DESDE";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(533, 64);
            label2.Name = "label2";
            label2.Size = new Size(58, 20);
            label2.TabIndex = 3;
            label2.Text = "HASTA";
            // 
            // lbltiempo
            // 
            lbltiempo.AutoSize = true;
            lbltiempo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lbltiempo.ForeColor = Color.FromArgb(27, 146, 255);
            lbltiempo.Location = new Point(720, 86);
            lbltiempo.Name = "lbltiempo";
            lbltiempo.Size = new Size(98, 46);
            lbltiempo.TabIndex = 11;
            lbltiempo.Text = "1 día";
            // 
            // cmbReporte
            // 
            cmbReporte.FormattingEnabled = true;
            cmbReporte.Location = new Point(317, 147);
            cmbReporte.Name = "cmbReporte";
            cmbReporte.Size = new Size(223, 28);
            cmbReporte.TabIndex = 4;
            // 
            // btnGenerarReporte
            // 
            btnGenerarReporte.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGenerarReporte.Location = new Point(36, 309);
            btnGenerarReporte.Name = "btnGenerarReporte";
            btnGenerarReporte.Size = new Size(175, 29);
            btnGenerarReporte.TabIndex = 5;
            btnGenerarReporte.Text = "GENERAR REPORTE";
            btnGenerarReporte.UseVisualStyleBackColor = true;
            btnGenerarReporte.Click += btnGenerarReporte_Click;
            // 
            // btnGenerarPDF
            // 
            btnGenerarPDF.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGenerarPDF.Location = new Point(470, 309);
            btnGenerarPDF.Name = "btnGenerarPDF";
            btnGenerarPDF.Size = new Size(175, 29);
            btnGenerarPDF.TabIndex = 6;
            btnGenerarPDF.Text = "EXPORTAR A PDF";
            btnGenerarPDF.UseVisualStyleBackColor = true;
            btnGenerarPDF.Click += btnGenerarPDF_Click;
            // 
            // btnGenerarExcel
            // 
            btnGenerarExcel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGenerarExcel.Location = new Point(271, 309);
            btnGenerarExcel.Name = "btnGenerarExcel";
            btnGenerarExcel.Size = new Size(175, 29);
            btnGenerarExcel.TabIndex = 7;
            btnGenerarExcel.Text = "EXPORTAR A EXCEL";
            btnGenerarExcel.UseVisualStyleBackColor = true;
            btnGenerarExcel.Click += btnGenerarExcel_Click;
            // 
            // dgvMostrarDatos
            // 
            dgvMostrarDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMostrarDatos.Location = new Point(36, 378);
            dgvMostrarDatos.Name = "dgvMostrarDatos";
            dgvMostrarDatos.RowHeadersWidth = 51;
            dgvMostrarDatos.Size = new Size(639, 340);
            dgvMostrarDatos.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(36, 734);
            label3.Name = "label3";
            label3.Size = new Size(126, 20);
            label3.TabIndex = 9;
            label3.Text = "TOTAL GENERAL";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotal.Location = new Point(170, 734);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(18, 20);
            lblTotal.TabIndex = 10;
            lblTotal.Text = "0";
            // 
            // FrmReportes
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1150, 797);
            Controls.Add(lblTotal);
            Controls.Add(label3);
            Controls.Add(dgvMostrarDatos);
            Controls.Add(btnGenerarExcel);
            Controls.Add(btnGenerarPDF);
            Controls.Add(btnGenerarReporte);
            Controls.Add(cmbReporte);
            Controls.Add(lbltiempo);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dtHasta);
            Controls.Add(dtDesde);
            Controls.Add(panelNav);
            Name = "FrmReportes";
            Text = "FrmReportes";
            WindowState = FormWindowState.Maximized;
            Load += FrmReportes_Load;
            panelNav.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMostrarDatos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion

        private Panel panelNav;
        private Button btnBack;
        private Button btnNavPagar;
        private Button btnNavDeudas;
        private Button btnNavEstado;
        private Button btnNavCaja;
        private Button btnNavHistorial;
        private Button btnNavInventario;
        private Button btnNavReportes;
        private Button btnNavClientes;
        private DateTimePicker dtDesde;
        private DateTimePicker dtHasta;
        private Label label1;
        private Label label2;
        private Label lbltiempo;
        private ComboBox cmbReporte;
        private Button btnGenerarReporte;
        private Button btnGenerarPDF;
        private Button btnGenerarExcel;
        private DataGridView dgvMostrarDatos;
        private Label label3;
        private Label lblTotal;
    }
}