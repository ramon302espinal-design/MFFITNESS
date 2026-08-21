namespace UI
{
    partial class FrmCRMFinanciero
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelSidebar = new Panel();
            btnConfiguracion = new Button();
            btnDecisiones = new Button();
            btnTendencias = new Button();
            btnAlertas = new Button();
            btnCapital = new Button();
            btnROI = new Button();
            btnGanancias = new Button();
            btnVentas = new Button();
            btnRanking = new Button();
            btnRentabilidad = new Button();
            btnInversiones = new Button();
            btnDashboard = new Button();
            lblSidebarBrand = new Label();
            lblSidebarModule = new Label();
            panelMain = new Panel();
            panelFooter = new Panel();
            lblFooter = new Label();
            pnlContent = new Panel();
            panelHeader = new Panel();
            btnActualizar = new Button();
            cmbPeriodo = new ComboBox();
            lblSubtitle = new Label();
            lblTitle = new Label();
            panelSidebar.SuspendLayout();
            panelMain.SuspendLayout();
            panelFooter.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.FromArgb(248, 250, 252);
            panelSidebar.BorderStyle = BorderStyle.FixedSingle;
            panelSidebar.Controls.Add(btnConfiguracion);
            panelSidebar.Controls.Add(btnDecisiones);
            panelSidebar.Controls.Add(btnTendencias);
            panelSidebar.Controls.Add(btnAlertas);
            panelSidebar.Controls.Add(btnCapital);
            panelSidebar.Controls.Add(btnROI);
            panelSidebar.Controls.Add(btnGanancias);
            panelSidebar.Controls.Add(btnVentas);
            panelSidebar.Controls.Add(btnRanking);
            panelSidebar.Controls.Add(btnRentabilidad);
            panelSidebar.Controls.Add(btnInversiones);
            panelSidebar.Controls.Add(btnDashboard);
            panelSidebar.Controls.Add(lblSidebarModule);
            panelSidebar.Controls.Add(lblSidebarBrand);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 0);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(220, 720);
            panelSidebar.TabIndex = 0;
            // 
            // lblSidebarBrand
            // 
            lblSidebarBrand.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSidebarBrand.ForeColor = Color.FromArgb(26, 32, 44);
            lblSidebarBrand.Location = new Point(12, 16);
            lblSidebarBrand.Name = "lblSidebarBrand";
            lblSidebarBrand.Size = new Size(192, 28);
            lblSidebarBrand.TabIndex = 0;
            lblSidebarBrand.Text = "MFFITNESS";
            lblSidebarBrand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSidebarModule
            // 
            lblSidebarModule.Font = new Font("Segoe UI", 9F);
            lblSidebarModule.ForeColor = Color.FromArgb(113, 128, 150);
            lblSidebarModule.Location = new Point(12, 44);
            lblSidebarModule.Name = "lblSidebarModule";
            lblSidebarModule.Size = new Size(192, 22);
            lblSidebarModule.TabIndex = 1;
            lblSidebarModule.Text = "CRM FINANCIERO";
            // 
            // btnDashboard
            // 
            btnDashboard.BackColor = Color.FromArgb(245, 247, 250);
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 9.5F);
            btnDashboard.ForeColor = Color.FromArgb(45, 55, 72);
            btnDashboard.Location = new Point(12, 80);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(192, 36);
            btnDashboard.TabIndex = 2;
            btnDashboard.Text = "  Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.UseVisualStyleBackColor = false;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnInversiones
            // 
            btnInversiones.BackColor = Color.FromArgb(245, 247, 250);
            btnInversiones.FlatAppearance.BorderSize = 0;
            btnInversiones.FlatStyle = FlatStyle.Flat;
            btnInversiones.Font = new Font("Segoe UI", 9.5F);
            btnInversiones.ForeColor = Color.FromArgb(45, 55, 72);
            btnInversiones.Location = new Point(12, 120);
            btnInversiones.Name = "btnInversiones";
            btnInversiones.Size = new Size(192, 36);
            btnInversiones.TabIndex = 3;
            btnInversiones.Text = "  Inversiones";
            btnInversiones.TextAlign = ContentAlignment.MiddleLeft;
            btnInversiones.UseVisualStyleBackColor = false;
            btnInversiones.Click += btnInversiones_Click;
            // 
            // btnRentabilidad
            // 
            btnRentabilidad.BackColor = Color.FromArgb(245, 247, 250);
            btnRentabilidad.FlatAppearance.BorderSize = 0;
            btnRentabilidad.FlatStyle = FlatStyle.Flat;
            btnRentabilidad.Font = new Font("Segoe UI", 9.5F);
            btnRentabilidad.ForeColor = Color.FromArgb(45, 55, 72);
            btnRentabilidad.Location = new Point(12, 160);
            btnRentabilidad.Name = "btnRentabilidad";
            btnRentabilidad.Size = new Size(192, 36);
            btnRentabilidad.TabIndex = 4;
            btnRentabilidad.Text = "  Rentabilidad";
            btnRentabilidad.TextAlign = ContentAlignment.MiddleLeft;
            btnRentabilidad.UseVisualStyleBackColor = false;
            btnRentabilidad.Click += btnRentabilidad_Click;
            // 
            // btnRanking
            // 
            btnRanking.BackColor = Color.FromArgb(245, 247, 250);
            btnRanking.FlatAppearance.BorderSize = 0;
            btnRanking.FlatStyle = FlatStyle.Flat;
            btnRanking.Font = new Font("Segoe UI", 9.5F);
            btnRanking.ForeColor = Color.FromArgb(45, 55, 72);
            btnRanking.Location = new Point(12, 200);
            btnRanking.Name = "btnRanking";
            btnRanking.Size = new Size(192, 36);
            btnRanking.TabIndex = 5;
            btnRanking.Text = "  Ranking";
            btnRanking.TextAlign = ContentAlignment.MiddleLeft;
            btnRanking.UseVisualStyleBackColor = false;
            btnRanking.Click += btnRanking_Click;
            // 
            // btnVentas
            // 
            btnVentas.BackColor = Color.FromArgb(245, 247, 250);
            btnVentas.FlatAppearance.BorderSize = 0;
            btnVentas.FlatStyle = FlatStyle.Flat;
            btnVentas.Font = new Font("Segoe UI", 9.5F);
            btnVentas.ForeColor = Color.FromArgb(45, 55, 72);
            btnVentas.Location = new Point(12, 240);
            btnVentas.Name = "btnVentas";
            btnVentas.Size = new Size(192, 36);
            btnVentas.TabIndex = 6;
            btnVentas.Text = "  Ventas";
            btnVentas.TextAlign = ContentAlignment.MiddleLeft;
            btnVentas.UseVisualStyleBackColor = false;
            btnVentas.Click += btnVentas_Click;
            // 
            // btnGanancias
            // 
            btnGanancias.BackColor = Color.FromArgb(245, 247, 250);
            btnGanancias.FlatAppearance.BorderSize = 0;
            btnGanancias.FlatStyle = FlatStyle.Flat;
            btnGanancias.Font = new Font("Segoe UI", 9.5F);
            btnGanancias.ForeColor = Color.FromArgb(45, 55, 72);
            btnGanancias.Location = new Point(12, 280);
            btnGanancias.Name = "btnGanancias";
            btnGanancias.Size = new Size(192, 36);
            btnGanancias.TabIndex = 7;
            btnGanancias.Text = "  Ganancias";
            btnGanancias.TextAlign = ContentAlignment.MiddleLeft;
            btnGanancias.UseVisualStyleBackColor = false;
            btnGanancias.Click += btnGanancias_Click;
            // 
            // btnROI
            // 
            btnROI.BackColor = Color.FromArgb(245, 247, 250);
            btnROI.FlatAppearance.BorderSize = 0;
            btnROI.FlatStyle = FlatStyle.Flat;
            btnROI.Font = new Font("Segoe UI", 9.5F);
            btnROI.ForeColor = Color.FromArgb(45, 55, 72);
            btnROI.Location = new Point(12, 320);
            btnROI.Name = "btnROI";
            btnROI.Size = new Size(192, 36);
            btnROI.TabIndex = 8;
            btnROI.Text = "  ROI";
            btnROI.TextAlign = ContentAlignment.MiddleLeft;
            btnROI.UseVisualStyleBackColor = false;
            btnROI.Click += btnROI_Click;
            // 
            // btnCapital
            // 
            btnCapital.BackColor = Color.FromArgb(245, 247, 250);
            btnCapital.FlatAppearance.BorderSize = 0;
            btnCapital.FlatStyle = FlatStyle.Flat;
            btnCapital.Font = new Font("Segoe UI", 9.5F);
            btnCapital.ForeColor = Color.FromArgb(45, 55, 72);
            btnCapital.Location = new Point(12, 360);
            btnCapital.Name = "btnCapital";
            btnCapital.Size = new Size(192, 36);
            btnCapital.TabIndex = 9;
            btnCapital.Text = "  Capital";
            btnCapital.TextAlign = ContentAlignment.MiddleLeft;
            btnCapital.UseVisualStyleBackColor = false;
            btnCapital.Click += btnCapital_Click;
            // 
            // btnAlertas
            // 
            btnAlertas.BackColor = Color.FromArgb(245, 247, 250);
            btnAlertas.FlatAppearance.BorderSize = 0;
            btnAlertas.FlatStyle = FlatStyle.Flat;
            btnAlertas.Font = new Font("Segoe UI", 9.5F);
            btnAlertas.ForeColor = Color.FromArgb(45, 55, 72);
            btnAlertas.Location = new Point(12, 400);
            btnAlertas.Name = "btnAlertas";
            btnAlertas.Size = new Size(192, 36);
            btnAlertas.TabIndex = 10;
            btnAlertas.Text = "  Alertas";
            btnAlertas.TextAlign = ContentAlignment.MiddleLeft;
            btnAlertas.UseVisualStyleBackColor = false;
            btnAlertas.Click += btnAlertas_Click;
            // 
            // btnTendencias
            // 
            btnTendencias.BackColor = Color.FromArgb(245, 247, 250);
            btnTendencias.FlatAppearance.BorderSize = 0;
            btnTendencias.FlatStyle = FlatStyle.Flat;
            btnTendencias.Font = new Font("Segoe UI", 9.5F);
            btnTendencias.ForeColor = Color.FromArgb(45, 55, 72);
            btnTendencias.Location = new Point(12, 440);
            btnTendencias.Name = "btnTendencias";
            btnTendencias.Size = new Size(192, 36);
            btnTendencias.TabIndex = 11;
            btnTendencias.Text = "  Tendencias";
            btnTendencias.TextAlign = ContentAlignment.MiddleLeft;
            btnTendencias.UseVisualStyleBackColor = false;
            btnTendencias.Click += btnTendencias_Click;
            // 
            // btnDecisiones
            // 
            btnDecisiones.BackColor = Color.FromArgb(245, 247, 250);
            btnDecisiones.FlatAppearance.BorderSize = 0;
            btnDecisiones.FlatStyle = FlatStyle.Flat;
            btnDecisiones.Font = new Font("Segoe UI", 9.5F);
            btnDecisiones.ForeColor = Color.FromArgb(45, 55, 72);
            btnDecisiones.Location = new Point(12, 480);
            btnDecisiones.Name = "btnDecisiones";
            btnDecisiones.Size = new Size(192, 36);
            btnDecisiones.TabIndex = 12;
            btnDecisiones.Text = "  Decisiones";
            btnDecisiones.TextAlign = ContentAlignment.MiddleLeft;
            btnDecisiones.UseVisualStyleBackColor = false;
            btnDecisiones.Click += btnDecisiones_Click;
            // 
            // btnConfiguracion
            // 
            btnConfiguracion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnConfiguracion.BackColor = Color.FromArgb(245, 247, 250);
            btnConfiguracion.Enabled = true;
            btnConfiguracion.FlatAppearance.BorderSize = 0;
            btnConfiguracion.FlatStyle = FlatStyle.Flat;
            btnConfiguracion.Font = new Font("Segoe UI", 9.5F);
            btnConfiguracion.ForeColor = Color.FromArgb(45, 55, 72);
            btnConfiguracion.Location = new Point(12, 668);
            btnConfiguracion.Name = "btnConfiguracion";
            btnConfiguracion.Size = new Size(192, 36);
            btnConfiguracion.TabIndex = 13;
            btnConfiguracion.Text = "  Estrellas";
            btnConfiguracion.TextAlign = ContentAlignment.MiddleLeft;
            btnConfiguracion.UseVisualStyleBackColor = false;
            btnConfiguracion.Click += btnEstrellas_Click;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.White;
            panelMain.Controls.Add(pnlContent);
            panelMain.Controls.Add(panelFooter);
            panelMain.Controls.Add(panelHeader);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(220, 0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1060, 720);
            panelMain.TabIndex = 1;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.White;
            panelHeader.BorderStyle = BorderStyle.FixedSingle;
            panelHeader.Controls.Add(btnActualizar);
            panelHeader.Controls.Add(cmbPeriodo);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1060, 72);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(26, 32, 44);
            lblTitle.Location = new Point(16, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(240, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Dashboard financiero";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblSubtitle.Location = new Point(20, 42);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(280, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Vision general del rendimiento financiero";
            // 
            // cmbPeriodo
            // 
            cmbPeriodo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbPeriodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriodo.Enabled = false;
            cmbPeriodo.Font = new Font("Segoe UI", 9F);
            cmbPeriodo.FormattingEnabled = true;
            cmbPeriodo.Items.AddRange(new object[] { "Este mes", "Ultimos 3 meses", "Ultimos 12 meses", "Anio actual" });
            cmbPeriodo.Location = new Point(780, 22);
            cmbPeriodo.Name = "cmbPeriodo";
            cmbPeriodo.Size = new Size(160, 28);
            cmbPeriodo.TabIndex = 2;
            // 
            // btnActualizar
            // 
            btnActualizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnActualizar.Enabled = false;
            btnActualizar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnActualizar.Location = new Point(952, 20);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(90, 30);
            btnActualizar.TabIndex = 3;
            btnActualizar.Text = "Actualizar";
            btnActualizar.UseVisualStyleBackColor = true;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(247, 249, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 72);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(8);
            pnlContent.Size = new Size(1060, 612);
            pnlContent.TabIndex = 1;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.FromArgb(248, 250, 252);
            panelFooter.BorderStyle = BorderStyle.FixedSingle;
            panelFooter.Controls.Add(lblFooter);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 684);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(1060, 36);
            panelFooter.TabIndex = 2;
            // 
            // lblFooter
            // 
            lblFooter.Dock = DockStyle.Fill;
            lblFooter.Font = new Font("Segoe UI", 8.5F);
            lblFooter.ForeColor = Color.FromArgb(113, 128, 150);
            lblFooter.Location = new Point(0, 0);
            lblFooter.Name = "lblFooter";
            lblFooter.Padding = new Padding(12, 0, 0, 0);
            lblFooter.Size = new Size(1058, 34);
            lblFooter.TabIndex = 0;
            lblFooter.Text = "MFFITNESS  |  CRM Financiero  |  Vista UI (FASE 1 — sin logica)";
            lblFooter.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FrmCRMFinanciero
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1280, 720);
            Controls.Add(panelMain);
            Controls.Add(panelSidebar);
            MinimumSize = new Size(1100, 680);
            Name = "FrmCRMFinanciero";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CRM Financiero — MFFITNESS";
            Load += FrmCRMFinanciero_Load;
            panelSidebar.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSidebar;
        private Label lblSidebarBrand;
        private Label lblSidebarModule;
        private Button btnDashboard;
        private Button btnInversiones;
        private Button btnRentabilidad;
        private Button btnRanking;
        private Button btnVentas;
        private Button btnGanancias;
        private Button btnROI;
        private Button btnCapital;
        private Button btnAlertas;
        private Button btnTendencias;
        private Button btnDecisiones;
        private Button btnConfiguracion;
        private Panel panelMain;
        private Panel panelHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private ComboBox cmbPeriodo;
        private Button btnActualizar;
        private Panel pnlContent;
        private Panel panelFooter;
        private Label lblFooter;
    }
}
