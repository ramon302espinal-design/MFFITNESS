namespace UI
{
    partial class FrmReportes
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
            panelHeader = new Panel();
            txtBusca = new TextBox();
            label4 = new Label();
            btnGenerarExcel = new Button();
            btnGenerarPDF = new Button();
            cmbReporte = new ComboBox();
            panelPie = new Panel();
            lblEstadoSync = new Label();
            lblTotal = new Label();
            dgvMostrarDatos = new DataGridView();
            panelHeader.SuspendLayout();
            panelPie.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMostrarDatos).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.Controls.Add(txtBusca);
            panelHeader.Controls.Add(label4);
            panelHeader.Controls.Add(btnGenerarExcel);
            panelHeader.Controls.Add(btnGenerarPDF);
            panelHeader.Controls.Add(cmbReporte);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(12, 8, 12, 8);
            panelHeader.Size = new Size(1100, 229);
            panelHeader.TabIndex = 0;
            panelHeader.Tag = "classic";
            // 
            // txtBusca
            // 
            txtBusca.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBusca.Location = new Point(89, 184);
            txtBusca.Name = "txtBusca";
            txtBusca.PlaceholderText = "Producto, miembro, método, usuario, fecha o monto...";
            txtBusca.Size = new Size(990, 27);
            txtBusca.TabIndex = 4;
            txtBusca.Tag = "classic";
            txtBusca.TextChanged += txtBusca_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(11, 188);
            label4.Name = "label4";
            label4.Size = new Size(68, 20);
            label4.TabIndex = 3;
            label4.Tag = "classic";
            label4.Text = "BUSCAR";
            // 
            // btnGenerarExcel
            // 
            btnGenerarExcel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGenerarExcel.Location = new Point(269, 138);
            btnGenerarExcel.Name = "btnGenerarExcel";
            btnGenerarExcel.Size = new Size(175, 32);
            btnGenerarExcel.TabIndex = 1;
            btnGenerarExcel.Tag = "classic";
            btnGenerarExcel.Text = "EXPORTAR A EXCEL";
            btnGenerarExcel.UseVisualStyleBackColor = true;
            btnGenerarExcel.Click += btnGenerarExcel_Click;
            // 
            // btnGenerarPDF
            // 
            btnGenerarPDF.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGenerarPDF.Location = new Point(459, 138);
            btnGenerarPDF.Name = "btnGenerarPDF";
            btnGenerarPDF.Size = new Size(175, 32);
            btnGenerarPDF.TabIndex = 2;
            btnGenerarPDF.Tag = "classic";
            btnGenerarPDF.Text = "EXPORTAR A PDF";
            btnGenerarPDF.UseVisualStyleBackColor = true;
            btnGenerarPDF.Click += btnGenerarPDF_Click;
            // 
            // cmbReporte
            // 
            cmbReporte.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReporte.FormattingEnabled = true;
            cmbReporte.Location = new Point(11, 140);
            cmbReporte.Name = "cmbReporte";
            cmbReporte.Size = new Size(240, 28);
            cmbReporte.TabIndex = 0;
            cmbReporte.Tag = "classic";
            cmbReporte.SelectedIndexChanged += cmbReporte_SelectedIndexChanged;
            // 
            // panelPie
            // 
            panelPie.Controls.Add(lblEstadoSync);
            panelPie.Controls.Add(lblTotal);
            panelPie.Dock = DockStyle.Bottom;
            panelPie.Location = new Point(0, 644);
            panelPie.Name = "panelPie";
            panelPie.Padding = new Padding(12, 8, 12, 8);
            panelPie.Size = new Size(1100, 72);
            panelPie.TabIndex = 2;
            panelPie.Tag = "classic";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotal.Location = new Point(12, 8);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(35, 41);
            lblTotal.TabIndex = 0;
            lblTotal.Tag = "classic";
            lblTotal.Text = "0";
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEstadoSync
            // 
            lblEstadoSync.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblEstadoSync.Font = new Font("Segoe UI", 8F);
            lblEstadoSync.ForeColor = Color.FromArgb(64, 64, 64);
            lblEstadoSync.Location = new Point(12, 44);
            lblEstadoSync.Name = "lblEstadoSync";
            lblEstadoSync.Size = new Size(1076, 20);
            lblEstadoSync.TabIndex = 1;
            lblEstadoSync.Text = "Reportes POS = movimientos del período.";
            // 
            // dgvMostrarDatos
            // 
            dgvMostrarDatos.AllowUserToAddRows = false;
            dgvMostrarDatos.AllowUserToDeleteRows = false;
            dgvMostrarDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMostrarDatos.ColumnHeadersHeight = 36;
            dgvMostrarDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMostrarDatos.Location = new Point(0, 306);
            dgvMostrarDatos.Name = "dgvMostrarDatos";
            dgvMostrarDatos.ReadOnly = true;
            dgvMostrarDatos.RowHeadersVisible = false;
            dgvMostrarDatos.RowHeadersWidth = 51;
            dgvMostrarDatos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMostrarDatos.Size = new Size(1100, 338);
            dgvMostrarDatos.TabIndex = 1;
            dgvMostrarDatos.Tag = "classic";
            // 
            // FrmReportes
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(dgvMostrarDatos);
            Controls.Add(panelPie);
            Controls.Add(panelHeader);
            Name = "FrmReportes";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "classic";
            Text = "Reportes";
            Load += FrmReportes_Load;
            Shown += FrmReportes_Shown;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelPie.ResumeLayout(false);
            panelPie.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMostrarDatos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private Panel panelPie;
        private ComboBox cmbReporte;
        private Button btnGenerarPDF;
        private Button btnGenerarExcel;
        private DataGridView dgvMostrarDatos;
        private Label lblTotal;
        private Label lblEstadoSync;
        private Label label4;
        private TextBox txtBusca;
    }
}
