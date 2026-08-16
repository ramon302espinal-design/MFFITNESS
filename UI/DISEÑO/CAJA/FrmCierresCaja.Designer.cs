namespace UI.DISEÑO
{
    partial class FrmCierresCaja
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelFiltrosCierre = new Panel();
            tlpCierreRoot = new TableLayoutPanel();
            lblTituloCierre = new Label();
            tlpFilaFiltros = new TableLayoutPanel();
            lblBuscarCierre = new Label();
            txtBuscarCierre = new TextBox();
            lblRangoCierre = new Label();
            cmbRangoCierre = new ComboBox();
            lblDesdeCierre = new Label();
            dtpDesdeCierre = new DateTimePicker();
            lblHastaCierre = new Label();
            dtpHastaCierre = new DateTimePicker();
            btnLimpiarFiltroCierre = new Button();
            panelResumenCierre = new Panel();
            lblResumenCierres = new Label();
            dgvCierres = new DataGridView();
            panelAcciones = new Panel();
            btnEliminarCierre = new Button();
            btnDescargar = new Button();
            btnVolver = new Button();
            panelFiltrosCierre.SuspendLayout();
            tlpCierreRoot.SuspendLayout();
            tlpFilaFiltros.SuspendLayout();
            panelResumenCierre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCierres).BeginInit();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // panelFiltrosCierre
            // 
            panelFiltrosCierre.BackColor = Color.White;
            panelFiltrosCierre.Controls.Add(tlpCierreRoot);
            panelFiltrosCierre.Dock = DockStyle.Top;
            panelFiltrosCierre.Location = new Point(0, 0);
            panelFiltrosCierre.Name = "panelFiltrosCierre";
            panelFiltrosCierre.Padding = new Padding(16, 12, 16, 10);
            panelFiltrosCierre.Size = new Size(1554, 138);
            panelFiltrosCierre.TabIndex = 0;
            // 
            // tlpCierreRoot
            // 
            tlpCierreRoot.ColumnCount = 1;
            tlpCierreRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCierreRoot.Controls.Add(lblTituloCierre, 0, 0);
            tlpCierreRoot.Controls.Add(tlpFilaFiltros, 0, 1);
            tlpCierreRoot.Controls.Add(panelResumenCierre, 0, 2);
            tlpCierreRoot.Dock = DockStyle.Fill;
            tlpCierreRoot.Location = new Point(16, 12);
            tlpCierreRoot.Name = "tlpCierreRoot";
            tlpCierreRoot.RowCount = 3;
            tlpCierreRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tlpCierreRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tlpCierreRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpCierreRoot.Size = new Size(1522, 116);
            tlpCierreRoot.TabIndex = 0;
            // 
            // lblTituloCierre
            // 
            lblTituloCierre.Dock = DockStyle.Fill;
            lblTituloCierre.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblTituloCierre.ForeColor = Color.FromArgb(15, 23, 42);
            lblTituloCierre.Location = new Point(0, 0);
            lblTituloCierre.Margin = new Padding(0, 0, 0, 4);
            lblTituloCierre.Name = "lblTituloCierre";
            lblTituloCierre.Size = new Size(1522, 30);
            lblTituloCierre.TabIndex = 0;
            lblTituloCierre.Text = "CUADRE DE CAJA";
            lblTituloCierre.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpFilaFiltros
            // 
            tlpFilaFiltros.ColumnCount = 9;
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 178F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            tlpFilaFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
            tlpFilaFiltros.Controls.Add(lblBuscarCierre, 0, 0);
            tlpFilaFiltros.Controls.Add(txtBuscarCierre, 1, 0);
            tlpFilaFiltros.Controls.Add(lblRangoCierre, 2, 0);
            tlpFilaFiltros.Controls.Add(cmbRangoCierre, 3, 0);
            tlpFilaFiltros.Controls.Add(lblDesdeCierre, 4, 0);
            tlpFilaFiltros.Controls.Add(dtpDesdeCierre, 5, 0);
            tlpFilaFiltros.Controls.Add(lblHastaCierre, 6, 0);
            tlpFilaFiltros.Controls.Add(dtpHastaCierre, 7, 0);
            tlpFilaFiltros.Controls.Add(btnLimpiarFiltroCierre, 8, 0);
            tlpFilaFiltros.Dock = DockStyle.Fill;
            tlpFilaFiltros.Location = new Point(3, 37);
            tlpFilaFiltros.Name = "tlpFilaFiltros";
            tlpFilaFiltros.RowCount = 1;
            tlpFilaFiltros.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFilaFiltros.Size = new Size(1516, 40);
            tlpFilaFiltros.TabIndex = 1;
            // 
            // lblBuscarCierre
            // 
            lblBuscarCierre.Dock = DockStyle.Fill;
            lblBuscarCierre.Font = new Font("Segoe UI", 9.5F);
            lblBuscarCierre.ForeColor = Color.FromArgb(100, 116, 139);
            lblBuscarCierre.Location = new Point(0, 0);
            lblBuscarCierre.Margin = new Padding(0, 0, 6, 0);
            lblBuscarCierre.Name = "lblBuscarCierre";
            lblBuscarCierre.Size = new Size(56, 40);
            lblBuscarCierre.TabIndex = 0;
            lblBuscarCierre.Text = "Buscar:";
            lblBuscarCierre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtBuscarCierre
            // 
            txtBuscarCierre.Dock = DockStyle.Fill;
            txtBuscarCierre.Font = new Font("Segoe UI", 9.5F);
            txtBuscarCierre.Location = new Point(68, 8);
            txtBuscarCierre.Margin = new Padding(6, 8, 10, 8);
            txtBuscarCierre.Name = "txtBuscarCierre";
            txtBuscarCierre.PlaceholderText = "hoy, ayer, 06/07/2026, julio, turno, usuario...";
            txtBuscarCierre.Size = new Size(710, 29);
            txtBuscarCierre.TabIndex = 1;
            txtBuscarCierre.TextChanged += txtBuscarCierre_TextChanged;
            // 
            // lblRangoCierre
            // 
            lblRangoCierre.Dock = DockStyle.Fill;
            lblRangoCierre.Font = new Font("Segoe UI", 9.5F);
            lblRangoCierre.ForeColor = Color.FromArgb(100, 116, 139);
            lblRangoCierre.Location = new Point(788, 0);
            lblRangoCierre.Margin = new Padding(0, 0, 6, 0);
            lblRangoCierre.Name = "lblRangoCierre";
            lblRangoCierre.Size = new Size(52, 40);
            lblRangoCierre.TabIndex = 2;
            lblRangoCierre.Text = "Rango:";
            lblRangoCierre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbRangoCierre
            // 
            cmbRangoCierre.Dock = DockStyle.Fill;
            cmbRangoCierre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRangoCierre.Font = new Font("Segoe UI", 9.5F);
            cmbRangoCierre.Location = new Point(846, 8);
            cmbRangoCierre.Margin = new Padding(0, 8, 8, 8);
            cmbRangoCierre.Name = "cmbRangoCierre";
            cmbRangoCierre.Size = new Size(170, 29);
            cmbRangoCierre.TabIndex = 3;
            cmbRangoCierre.SelectedIndexChanged += cmbRangoCierre_SelectedIndexChanged;
            // 
            // lblDesdeCierre
            // 
            lblDesdeCierre.Dock = DockStyle.Fill;
            lblDesdeCierre.Font = new Font("Segoe UI", 9.5F);
            lblDesdeCierre.ForeColor = Color.FromArgb(100, 116, 139);
            lblDesdeCierre.Location = new Point(1024, 0);
            lblDesdeCierre.Margin = new Padding(0, 0, 6, 0);
            lblDesdeCierre.Name = "lblDesdeCierre";
            lblDesdeCierre.Size = new Size(52, 40);
            lblDesdeCierre.TabIndex = 4;
            lblDesdeCierre.Text = "Desde:";
            lblDesdeCierre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dtpDesdeCierre
            // 
            dtpDesdeCierre.Dock = DockStyle.Fill;
            dtpDesdeCierre.Enabled = false;
            dtpDesdeCierre.Font = new Font("Segoe UI", 9.5F);
            dtpDesdeCierre.Format = DateTimePickerFormat.Short;
            dtpDesdeCierre.Location = new Point(1082, 8);
            dtpDesdeCierre.Margin = new Padding(0, 8, 8, 8);
            dtpDesdeCierre.Name = "dtpDesdeCierre";
            dtpDesdeCierre.Size = new Size(124, 29);
            dtpDesdeCierre.TabIndex = 5;
            dtpDesdeCierre.ValueChanged += FiltroCierre_Changed;
            // 
            // lblHastaCierre
            // 
            lblHastaCierre.Dock = DockStyle.Fill;
            lblHastaCierre.Font = new Font("Segoe UI", 9.5F);
            lblHastaCierre.ForeColor = Color.FromArgb(100, 116, 139);
            lblHastaCierre.Location = new Point(1214, 0);
            lblHastaCierre.Margin = new Padding(0, 0, 6, 0);
            lblHastaCierre.Name = "lblHastaCierre";
            lblHastaCierre.Size = new Size(46, 40);
            lblHastaCierre.TabIndex = 6;
            lblHastaCierre.Text = "Hasta:";
            lblHastaCierre.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dtpHastaCierre
            // 
            dtpHastaCierre.Dock = DockStyle.Fill;
            dtpHastaCierre.Enabled = false;
            dtpHastaCierre.Font = new Font("Segoe UI", 9.5F);
            dtpHastaCierre.Format = DateTimePickerFormat.Short;
            dtpHastaCierre.Location = new Point(1266, 8);
            dtpHastaCierre.Margin = new Padding(0, 8, 8, 8);
            dtpHastaCierre.Name = "dtpHastaCierre";
            dtpHastaCierre.Size = new Size(124, 29);
            dtpHastaCierre.TabIndex = 7;
            dtpHastaCierre.ValueChanged += FiltroCierre_Changed;
            // 
            // btnLimpiarFiltroCierre
            // 
            btnLimpiarFiltroCierre.BackColor = Color.FromArgb(241, 245, 249);
            btnLimpiarFiltroCierre.Cursor = Cursors.Hand;
            btnLimpiarFiltroCierre.Dock = DockStyle.Fill;
            btnLimpiarFiltroCierre.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnLimpiarFiltroCierre.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
            btnLimpiarFiltroCierre.FlatStyle = FlatStyle.Flat;
            btnLimpiarFiltroCierre.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnLimpiarFiltroCierre.ForeColor = Color.FromArgb(51, 65, 85);
            btnLimpiarFiltroCierre.Location = new Point(1406, 7);
            btnLimpiarFiltroCierre.Margin = new Padding(8, 7, 0, 7);
            btnLimpiarFiltroCierre.MinimumSize = new Size(100, 32);
            btnLimpiarFiltroCierre.Name = "btnLimpiarFiltroCierre";
            btnLimpiarFiltroCierre.Size = new Size(110, 32);
            btnLimpiarFiltroCierre.TabIndex = 8;
            btnLimpiarFiltroCierre.Text = "Limpiar";
            btnLimpiarFiltroCierre.UseVisualStyleBackColor = false;
            btnLimpiarFiltroCierre.Click += btnLimpiarFiltroCierre_Click;
            // 
            // panelResumenCierre
            // 
            panelResumenCierre.BackColor = Color.FromArgb(248, 250, 252);
            panelResumenCierre.Controls.Add(lblResumenCierres);
            panelResumenCierre.Dock = DockStyle.Fill;
            panelResumenCierre.Location = new Point(3, 83);
            panelResumenCierre.Name = "panelResumenCierre";
            panelResumenCierre.Padding = new Padding(10, 6, 10, 6);
            panelResumenCierre.Size = new Size(1516, 30);
            panelResumenCierre.TabIndex = 2;
            // 
            // lblResumenCierres
            // 
            lblResumenCierres.AutoSize = true;
            lblResumenCierres.Dock = DockStyle.Fill;
            lblResumenCierres.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            lblResumenCierres.ForeColor = Color.FromArgb(27, 146, 255);
            lblResumenCierres.Location = new Point(10, 6);
            lblResumenCierres.Name = "lblResumenCierres";
            lblResumenCierres.Size = new Size(216, 21);
            lblResumenCierres.TabIndex = 0;
            lblResumenCierres.Text = "0 cierres · Ingresos RD$ 0.00";
            lblResumenCierres.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvCierres
            // 
            dgvCierres.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCierres.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCierres.Dock = DockStyle.Fill;
            dgvCierres.Location = new Point(0, 138);
            dgvCierres.Name = "dgvCierres";
            dgvCierres.ReadOnly = true;
            dgvCierres.RowHeadersWidth = 51;
            dgvCierres.Size = new Size(1554, 542);
            dgvCierres.TabIndex = 1;
            // 
            // panelAcciones
            // 
            panelAcciones.BackColor = Color.White;
            panelAcciones.Controls.Add(btnEliminarCierre);
            panelAcciones.Controls.Add(btnDescargar);
            panelAcciones.Controls.Add(btnVolver);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 680);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(16, 10, 16, 10);
            panelAcciones.Size = new Size(1554, 60);
            panelAcciones.TabIndex = 2;
            // 
            // btnEliminarCierre
            // 
            btnEliminarCierre.Location = new Point(16, 12);
            btnEliminarCierre.Name = "btnEliminarCierre";
            btnEliminarCierre.Size = new Size(143, 38);
            btnEliminarCierre.TabIndex = 0;
            btnEliminarCierre.Text = "ELIMINAR";
            btnEliminarCierre.UseVisualStyleBackColor = true;
            btnEliminarCierre.Click += btnEliminarCierre_Click;
            // 
            // btnDescargar
            // 
            btnDescargar.Location = new Point(170, 12);
            btnDescargar.Name = "btnDescargar";
            btnDescargar.Size = new Size(143, 38);
            btnDescargar.TabIndex = 1;
            btnDescargar.Text = "DESCARGAR";
            btnDescargar.UseVisualStyleBackColor = true;
            btnDescargar.Click += btnDescargar_Click;
            // 
            // btnVolver
            // 
            btnVolver.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVolver.Location = new Point(1395, 12);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(143, 38);
            btnVolver.TabIndex = 2;
            btnVolver.Text = "VOLVER";
            btnVolver.UseVisualStyleBackColor = true;
            btnVolver.Click += btnVolver_Click;
            // 
            // FrmCierresCaja
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1554, 740);
            Controls.Add(dgvCierres);
            Controls.Add(panelAcciones);
            Controls.Add(panelFiltrosCierre);
            Name = "FrmCierresCaja";
            Text = "Cierres de Caja";
            WindowState = FormWindowState.Maximized;
            Load += FrmCierresCaja_Load;
            panelFiltrosCierre.ResumeLayout(false);
            tlpCierreRoot.ResumeLayout(false);
            tlpFilaFiltros.ResumeLayout(false);
            tlpFilaFiltros.PerformLayout();
            panelResumenCierre.ResumeLayout(false);
            panelResumenCierre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCierres).EndInit();
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelFiltrosCierre;
        private TableLayoutPanel tlpCierreRoot;
        private Label lblTituloCierre;
        private TableLayoutPanel tlpFilaFiltros;
        private Label lblBuscarCierre;
        private TextBox txtBuscarCierre;
        private Label lblRangoCierre;
        private ComboBox cmbRangoCierre;
        private Label lblDesdeCierre;
        private DateTimePicker dtpDesdeCierre;
        private Label lblHastaCierre;
        private DateTimePicker dtpHastaCierre;
        private Button btnLimpiarFiltroCierre;
        private Panel panelResumenCierre;
        private Label lblResumenCierres;
        private DataGridView dgvCierres;
        private Panel panelAcciones;
        private Button btnEliminarCierre;
        private Button btnDescargar;
        private Button btnVolver;
    }
}
