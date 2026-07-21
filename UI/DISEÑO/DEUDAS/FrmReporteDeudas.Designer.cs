namespace UI
{
    partial class FrmReporteDeudas
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
            panelTop = new Panel();
            lblResumen = new Label();
            lblTitulo = new Label();
            dgvReporte = new DataGridView();
            panelAcciones = new Panel();
            btnDescargarPdf = new Button();
            btnCerrar = new Button();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReporte).BeginInit();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.White;
            panelTop.Controls.Add(lblResumen);
            panelTop.Controls.Add(lblTitulo);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(20, 16, 20, 12);
            panelTop.Size = new Size(1200, 90);
            panelTop.TabIndex = 0;
            // 
            // lblResumen
            // 
            lblResumen.AutoSize = true;
            lblResumen.Font = new Font("Segoe UI", 10F);
            lblResumen.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumen.Location = new Point(20, 55);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(97, 23);
            lblResumen.TabIndex = 1;
            lblResumen.Text = "Cargando...";
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(20, 16);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(246, 37);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Reporte de deudas";
            // 
            // dgvReporte
            // 
            dgvReporte.AllowUserToAddRows = false;
            dgvReporte.AllowUserToDeleteRows = false;
            dgvReporte.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReporte.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReporte.Dock = DockStyle.Fill;
            dgvReporte.Location = new Point(0, 90);
            dgvReporte.Name = "dgvReporte";
            dgvReporte.ReadOnly = true;
            dgvReporte.RowHeadersWidth = 51;
            dgvReporte.Size = new Size(1200, 520);
            dgvReporte.TabIndex = 1;
            // 
            // panelAcciones
            // 
            panelAcciones.BackColor = Color.White;
            panelAcciones.Controls.Add(btnDescargarPdf);
            panelAcciones.Controls.Add(btnCerrar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 610);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(20, 12, 20, 12);
            panelAcciones.Size = new Size(1200, 70);
            panelAcciones.TabIndex = 2;
            // 
            // btnDescargarPdf
            // 
            btnDescargarPdf.BackColor = Color.FromArgb(229, 57, 53);
            btnDescargarPdf.Cursor = Cursors.Hand;
            btnDescargarPdf.FlatAppearance.BorderSize = 0;
            btnDescargarPdf.FlatStyle = FlatStyle.Flat;
            btnDescargarPdf.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDescargarPdf.ForeColor = Color.White;
            btnDescargarPdf.Location = new Point(20, 16);
            btnDescargarPdf.Name = "btnDescargarPdf";
            btnDescargarPdf.Size = new Size(200, 40);
            btnDescargarPdf.TabIndex = 0;
            btnDescargarPdf.Text = "DESCARGAR PDF";
            btnDescargarPdf.UseVisualStyleBackColor = false;
            btnDescargarPdf.Click += btnDescargarPdf_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrar.Location = new Point(1040, 16);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(140, 40);
            btnCerrar.TabIndex = 1;
            btnCerrar.Text = "CERRAR";
            btnCerrar.UseVisualStyleBackColor = true;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // FrmReporteDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 680);
            Controls.Add(dgvReporte);
            Controls.Add(panelAcciones);
            Controls.Add(panelTop);
            Name = "FrmReporteDeudas";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reporte de deudas";
            WindowState = FormWindowState.Maximized;
            Load += FrmReporteDeudas_Load;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReporte).EndInit();
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTop;
        private Label lblTitulo;
        private Label lblResumen;
        private DataGridView dgvReporte;
        private Panel panelAcciones;
        private Button btnDescargarPdf;
        private Button btnCerrar;
    }
}
