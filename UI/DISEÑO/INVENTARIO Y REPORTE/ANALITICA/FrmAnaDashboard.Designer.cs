namespace UI
{
    partial class FrmAnaDashboard
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
            panelScroll = new Panel();
            pnlActions = new GroupBox();
            btnAccionesVer = new Button();
            lblAccImpacto = new Label();
            lblAccExitosas = new Label();
            lblAccCompletadas = new Label();
            lblAccEnProceso = new Label();
            lblAccPendientes = new Label();
            pnlDecisions = new GroupBox();
            btnDecisionVer = new Button();
            lblDecision3 = new Label();
            lblDecision2 = new Label();
            lblDecision1 = new Label();
            pnlTrends = new GroupBox();
            pnlChartTrends = new Panel();
            cmbTrendPeriod = new ComboBox();
            cmbTrendMetric = new ComboBox();
            pnlProducts = new GroupBox();
            lstWatch = new ListBox();
            lstTop = new ListBox();
            lblWatchTitle = new Label();
            lblTopTitle = new Label();
            pnlCapital = new GroupBox();
            pnlChartCapital = new Panel();
            lblCapCaja = new Label();
            lblCapInventario = new Label();
            lblCapPendiente = new Label();
            lblCapRecuperado = new Label();
            lblCapInvertido = new Label();
            pnlProfitability = new GroupBox();
            lblGanPotencial = new Label();
            lblGanRealizada = new Label();
            lblProfitRoi = new Label();
            lblProfitMargen = new Label();
            lblProfitGanancia = new Label();
            pnlFrozenCapital = new GroupBox();
            btnVerDetalleCapital = new Button();
            progressFrozen = new ProgressBar();
            lblFrozenDias = new Label();
            lblFrozenProductos = new Label();
            lblFrozenPct = new Label();
            lblFrozenValor = new Label();
            pnlInventoryHealth = new GroupBox();
            lblHealthScore = new Label();
            lblHealthTitle = new Label();
            pnlCriticos = new Panel();
            lblCriticosVal = new Label();
            lblCriticosTitle = new Label();
            pnlLentos = new Panel();
            lblLentosVal = new Label();
            lblLentosTitle = new Label();
            pnlBuenos = new Panel();
            lblBuenosVal = new Label();
            lblBuenosTitle = new Label();
            pnlEstrella = new Panel();
            lblEstrellaVal = new Label();
            lblEstrellaTitle = new Label();
            pnlKPIs = new GroupBox();
            pnlKpiRoi = new Panel();
            lblKpiRoiDelta = new Label();
            lblKpiRoiVal = new Label();
            lblKpiRoiTitle = new Label();
            pnlKpiGanancia = new Panel();
            lblKpiGanDelta = new Label();
            lblKpiGanVal = new Label();
            lblKpiGanTitle = new Label();
            pnlKpiVentas = new Panel();
            lblKpiVenDelta = new Label();
            lblKpiVenVal = new Label();
            lblKpiVenTitle = new Label();
            pnlKpiInventario = new Panel();
            lblKpiInvDelta = new Label();
            lblKpiInvVal = new Label();
            lblKpiInvTitle = new Label();
            pnlKpiCapital = new Panel();
            lblKpiCapDelta = new Label();
            lblKpiCapVal = new Label();
            lblKpiCapTitle = new Label();
            panelScroll.SuspendLayout();
            pnlActions.SuspendLayout();
            pnlDecisions.SuspendLayout();
            pnlTrends.SuspendLayout();
            pnlProducts.SuspendLayout();
            pnlCapital.SuspendLayout();
            pnlProfitability.SuspendLayout();
            pnlFrozenCapital.SuspendLayout();
            pnlInventoryHealth.SuspendLayout();
            pnlCriticos.SuspendLayout();
            pnlLentos.SuspendLayout();
            pnlBuenos.SuspendLayout();
            pnlEstrella.SuspendLayout();
            pnlKPIs.SuspendLayout();
            pnlKpiRoi.SuspendLayout();
            pnlKpiGanancia.SuspendLayout();
            pnlKpiVentas.SuspendLayout();
            pnlKpiInventario.SuspendLayout();
            pnlKpiCapital.SuspendLayout();
            SuspendLayout();
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlActions);
            panelScroll.Controls.Add(pnlDecisions);
            panelScroll.Controls.Add(pnlTrends);
            panelScroll.Controls.Add(pnlProducts);
            panelScroll.Controls.Add(pnlCapital);
            panelScroll.Controls.Add(pnlProfitability);
            panelScroll.Controls.Add(pnlFrozenCapital);
            panelScroll.Controls.Add(pnlInventoryHealth);
            panelScroll.Controls.Add(pnlKPIs);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 0);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(12);
            panelScroll.Size = new Size(1800, 720);
            panelScroll.TabIndex = 0;
            // 
            // pnlActions
            // 
            pnlActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlActions.Controls.Add(btnAccionesVer);
            pnlActions.Controls.Add(lblAccImpacto);
            pnlActions.Controls.Add(lblAccExitosas);
            pnlActions.Controls.Add(lblAccCompletadas);
            pnlActions.Controls.Add(lblAccEnProceso);
            pnlActions.Controls.Add(lblAccPendientes);
            pnlActions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlActions.Location = new Point(16, 1060);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(1718, 130);
            pnlActions.TabIndex = 8;
            pnlActions.TabStop = false;
            pnlActions.Text = "Acciones de negocio";
            // 
            // btnAccionesVer
            // 
            btnAccionesVer.Location = new Point(760, 48);
            btnAccionesVer.Name = "btnAccionesVer";
            btnAccionesVer.Size = new Size(150, 32);
            btnAccionesVer.TabIndex = 5;
            btnAccionesVer.Text = "Ver acciones";
            btnAccionesVer.UseVisualStyleBackColor = true;
            btnAccionesVer.Click += btnAccionesVer_Click;
            // 
            // lblAccImpacto
            // 
            lblAccImpacto.Font = new Font("Segoe UI", 8.5F);
            lblAccImpacto.ForeColor = Color.FromArgb(113, 128, 150);
            lblAccImpacto.Location = new Point(20, 92);
            lblAccImpacto.Name = "lblAccImpacto";
            lblAccImpacto.Size = new Size(700, 24);
            lblAccImpacto.TabIndex = 4;
            lblAccImpacto.Text = "Impacto observado: —";
            // 
            // lblAccExitosas
            // 
            lblAccExitosas.Font = new Font("Segoe UI", 9F);
            lblAccExitosas.Location = new Point(320, 60);
            lblAccExitosas.Name = "lblAccExitosas";
            lblAccExitosas.Size = new Size(280, 24);
            lblAccExitosas.TabIndex = 3;
            lblAccExitosas.Text = "Exitosas (histórico): —";
            // 
            // lblAccCompletadas
            // 
            lblAccCompletadas.Font = new Font("Segoe UI", 9F);
            lblAccCompletadas.Location = new Point(20, 60);
            lblAccCompletadas.Name = "lblAccCompletadas";
            lblAccCompletadas.Size = new Size(280, 24);
            lblAccCompletadas.TabIndex = 2;
            lblAccCompletadas.Text = "Completadas: —";
            // 
            // lblAccEnProceso
            // 
            lblAccEnProceso.Font = new Font("Segoe UI", 9F);
            lblAccEnProceso.Location = new Point(320, 32);
            lblAccEnProceso.Name = "lblAccEnProceso";
            lblAccEnProceso.Size = new Size(280, 24);
            lblAccEnProceso.TabIndex = 1;
            lblAccEnProceso.Text = "En proceso: —";
            // 
            // lblAccPendientes
            // 
            lblAccPendientes.Font = new Font("Segoe UI", 9F);
            lblAccPendientes.Location = new Point(20, 32);
            lblAccPendientes.Name = "lblAccPendientes";
            lblAccPendientes.Size = new Size(280, 24);
            lblAccPendientes.TabIndex = 0;
            lblAccPendientes.Text = "Pendientes: —";
            // 
            // pnlDecisions
            // 
            pnlDecisions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlDecisions.Controls.Add(btnDecisionVer);
            pnlDecisions.Controls.Add(lblDecision3);
            pnlDecisions.Controls.Add(lblDecision2);
            pnlDecisions.Controls.Add(lblDecision1);
            pnlDecisions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlDecisions.Location = new Point(16, 900);
            pnlDecisions.Name = "pnlDecisions";
            pnlDecisions.Size = new Size(1718, 150);
            pnlDecisions.TabIndex = 7;
            pnlDecisions.TabStop = false;
            pnlDecisions.Text = "Centro de decisiones";
            // 
            // btnDecisionVer
            // 
            btnDecisionVer.Location = new Point(760, 56);
            btnDecisionVer.Name = "btnDecisionVer";
            btnDecisionVer.Size = new Size(150, 32);
            btnDecisionVer.TabIndex = 3;
            btnDecisionVer.Text = "Ver / Analizar";
            btnDecisionVer.UseVisualStyleBackColor = true;
            btnDecisionVer.Click += btnDecisionVer_Click;
            // 
            // lblDecision3
            // 
            lblDecision3.Font = new Font("Segoe UI", 9F);
            lblDecision3.Location = new Point(20, 88);
            lblDecision3.Name = "lblDecision3";
            lblDecision3.Size = new Size(700, 24);
            lblDecision3.TabIndex = 2;
            // 
            // lblDecision2
            // 
            lblDecision2.Font = new Font("Segoe UI", 9F);
            lblDecision2.Location = new Point(20, 60);
            lblDecision2.Name = "lblDecision2";
            lblDecision2.Size = new Size(700, 24);
            lblDecision2.TabIndex = 1;
            // 
            // lblDecision1
            // 
            lblDecision1.Font = new Font("Segoe UI", 9F);
            lblDecision1.Location = new Point(20, 32);
            lblDecision1.Name = "lblDecision1";
            lblDecision1.Size = new Size(700, 24);
            lblDecision1.TabIndex = 0;
            // 
            // pnlTrends
            // 
            pnlTrends.Controls.Add(pnlChartTrends);
            pnlTrends.Controls.Add(cmbTrendPeriod);
            pnlTrends.Controls.Add(cmbTrendMetric);
            pnlTrends.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlTrends.Location = new Point(779, 680);
            pnlTrends.Name = "pnlTrends";
            pnlTrends.Size = new Size(469, 200);
            pnlTrends.TabIndex = 6;
            pnlTrends.TabStop = false;
            pnlTrends.Text = "Tendencias";
            pnlTrends.DoubleClick += pnlTrends_DoubleClick;
            // 
            // pnlChartTrends
            // 
            pnlChartTrends.BackColor = Color.FromArgb(237, 242, 247);
            pnlChartTrends.BorderStyle = BorderStyle.FixedSingle;
            pnlChartTrends.Location = new Point(16, 72);
            pnlChartTrends.Name = "pnlChartTrends";
            pnlChartTrends.Size = new Size(424, 110);
            pnlChartTrends.TabIndex = 2;
            pnlChartTrends.Paint += pnlChartTrends_Paint;
            // 
            // cmbTrendPeriod
            // 
            cmbTrendPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrendPeriod.Font = new Font("Segoe UI", 9F);
            cmbTrendPeriod.FormattingEnabled = true;
            cmbTrendPeriod.Items.AddRange(new object[] { "Ultimos 12 meses", "Ultimos 6 meses", "Este anio" });
            cmbTrendPeriod.Location = new Point(170, 32);
            cmbTrendPeriod.Name = "cmbTrendPeriod";
            cmbTrendPeriod.Size = new Size(160, 28);
            cmbTrendPeriod.TabIndex = 1;
            cmbTrendPeriod.SelectedIndexChanged += cmbTrendPeriod_SelectedIndexChanged;
            // 
            // cmbTrendMetric
            // 
            cmbTrendMetric.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrendMetric.Font = new Font("Segoe UI", 9F);
            cmbTrendMetric.FormattingEnabled = true;
            cmbTrendMetric.Items.AddRange(new object[] { "Ventas", "Ganancia", "ROI", "Margen", "Capital" });
            cmbTrendMetric.Location = new Point(16, 32);
            cmbTrendMetric.Name = "cmbTrendMetric";
            cmbTrendMetric.Size = new Size(140, 28);
            cmbTrendMetric.TabIndex = 0;
            cmbTrendMetric.SelectedIndexChanged += cmbTrendMetric_SelectedIndexChanged;
            // 
            // pnlProducts
            // 
            pnlProducts.Controls.Add(lstWatch);
            pnlProducts.Controls.Add(lstTop);
            pnlProducts.Controls.Add(lblWatchTitle);
            pnlProducts.Controls.Add(lblTopTitle);
            pnlProducts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlProducts.Location = new Point(16, 680);
            pnlProducts.Name = "pnlProducts";
            pnlProducts.Size = new Size(757, 200);
            pnlProducts.TabIndex = 5;
            pnlProducts.TabStop = false;
            pnlProducts.Text = "Productos";
            // 
            // lstWatch
            // 
            lstWatch.Font = new Font("Segoe UI", 9F);
            lstWatch.FormattingEnabled = true;
            lstWatch.Location = new Point(396, 52);
            lstWatch.Name = "lstWatch";
            lstWatch.Size = new Size(355, 124);
            lstWatch.TabIndex = 3;
            lstWatch.DoubleClick += lstWatch_DoubleClick;
            // 
            // lstTop
            // 
            lstTop.Font = new Font("Segoe UI", 9F);
            lstTop.FormattingEnabled = true;
            lstTop.Location = new Point(16, 52);
            lstTop.Name = "lstTop";
            lstTop.Size = new Size(374, 124);
            lstTop.TabIndex = 1;
            lstTop.DoubleClick += lstTop_DoubleClick;
            // 
            // lblWatchTitle
            // 
            lblWatchTitle.AutoSize = true;
            lblWatchTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWatchTitle.Location = new Point(412, 28);
            lblWatchTitle.Name = "lblWatchTitle";
            lblWatchTitle.Size = new Size(176, 20);
            lblWatchTitle.TabIndex = 2;
            lblWatchTitle.Text = "PRODUCTOS A VIGILAR";
            // 
            // lblTopTitle
            // 
            lblTopTitle.AutoSize = true;
            lblTopTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTopTitle.Location = new Point(16, 28);
            lblTopTitle.Name = "lblTopTitle";
            lblTopTitle.Size = new Size(129, 20);
            lblTopTitle.TabIndex = 0;
            lblTopTitle.Text = "TOP PRODUCTOS";
            // 
            // pnlCapital
            // 
            pnlCapital.Controls.Add(pnlChartCapital);
            pnlCapital.Controls.Add(lblCapCaja);
            pnlCapital.Controls.Add(lblCapInventario);
            pnlCapital.Controls.Add(lblCapPendiente);
            pnlCapital.Controls.Add(lblCapRecuperado);
            pnlCapital.Controls.Add(lblCapInvertido);
            pnlCapital.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlCapital.Location = new Point(496, 504);
            pnlCapital.Name = "pnlCapital";
            pnlCapital.Size = new Size(555, 160);
            pnlCapital.TabIndex = 4;
            pnlCapital.TabStop = false;
            pnlCapital.Text = "Capital";
            // 
            // pnlChartCapital
            // 
            pnlChartCapital.BackColor = Color.FromArgb(237, 242, 247);
            pnlChartCapital.BorderStyle = BorderStyle.FixedSingle;
            pnlChartCapital.Location = new Point(303, 32);
            pnlChartCapital.Name = "pnlChartCapital";
            pnlChartCapital.Size = new Size(246, 110);
            pnlChartCapital.TabIndex = 5;
            pnlChartCapital.Paint += pnlChartCapital_Paint;
            // 
            // lblCapCaja
            // 
            lblCapCaja.AutoSize = true;
            lblCapCaja.Font = new Font("Segoe UI", 8.5F);
            lblCapCaja.Location = new Point(8, 120);
            lblCapCaja.Name = "lblCapCaja";
            lblCapCaja.Size = new Size(134, 20);
            lblCapCaja.TabIndex = 4;
            lblCapCaja.Text = "En caja: RD$42,000";
            // 
            // lblCapInventario
            // 
            lblCapInventario.AutoSize = true;
            lblCapInventario.Font = new Font("Segoe UI", 8.5F);
            lblCapInventario.Location = new Point(8, 98);
            lblCapInventario.Name = "lblCapInventario";
            lblCapInventario.Size = new Size(173, 20);
            lblCapInventario.TabIndex = 3;
            lblCapInventario.Text = "En inventario: RD$95,000";
            // 
            // lblCapPendiente
            // 
            lblCapPendiente.AutoSize = true;
            lblCapPendiente.Font = new Font("Segoe UI", 8.5F);
            lblCapPendiente.Location = new Point(8, 76);
            lblCapPendiente.Name = "lblCapPendiente";
            lblCapPendiente.Size = new Size(152, 20);
            lblCapPendiente.TabIndex = 2;
            lblCapPendiente.Text = "Pendiente: RD$31,500";
            // 
            // lblCapRecuperado
            // 
            lblCapRecuperado.AutoSize = true;
            lblCapRecuperado.Font = new Font("Segoe UI", 8.5F);
            lblCapRecuperado.Location = new Point(8, 54);
            lblCapRecuperado.Name = "lblCapRecuperado";
            lblCapRecuperado.Size = new Size(167, 20);
            lblCapRecuperado.TabIndex = 1;
            lblCapRecuperado.Text = "Recuperado: RD$88,500";
            // 
            // lblCapInvertido
            // 
            lblCapInvertido.AutoSize = true;
            lblCapInvertido.Font = new Font("Segoe UI", 8.5F);
            lblCapInvertido.Location = new Point(8, 32);
            lblCapInvertido.Name = "lblCapInvertido";
            lblCapInvertido.Size = new Size(154, 20);
            lblCapInvertido.TabIndex = 0;
            lblCapInvertido.Text = "Invertido: RD$120,000";
            // 
            // pnlProfitability
            // 
            pnlProfitability.Controls.Add(lblGanPotencial);
            pnlProfitability.Controls.Add(lblGanRealizada);
            pnlProfitability.Controls.Add(lblProfitRoi);
            pnlProfitability.Controls.Add(lblProfitMargen);
            pnlProfitability.Controls.Add(lblProfitGanancia);
            pnlProfitability.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlProfitability.Location = new Point(16, 504);
            pnlProfitability.Name = "pnlProfitability";
            pnlProfitability.Size = new Size(460, 160);
            pnlProfitability.TabIndex = 3;
            pnlProfitability.TabStop = false;
            pnlProfitability.Text = "Rentabilidad";
            // 
            // lblGanPotencial
            // 
            lblGanPotencial.AutoSize = true;
            lblGanPotencial.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGanPotencial.ForeColor = Color.FromArgb(49, 130, 206);
            lblGanPotencial.Location = new Point(258, 111);
            lblGanPotencial.Name = "lblGanPotencial";
            lblGanPotencial.Size = new Size(176, 20);
            lblGanPotencial.TabIndex = 4;
            lblGanPotencial.Text = "GANANCIA POTENCIAL";
            // 
            // lblGanRealizada
            // 
            lblGanRealizada.AutoSize = true;
            lblGanRealizada.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGanRealizada.ForeColor = Color.FromArgb(56, 161, 105);
            lblGanRealizada.Location = new Point(258, 48);
            lblGanRealizada.Name = "lblGanRealizada";
            lblGanRealizada.Size = new Size(179, 20);
            lblGanRealizada.TabIndex = 3;
            lblGanRealizada.Text = "GANANCIA REALIZADA";
            // 
            // lblProfitRoi
            // 
            lblProfitRoi.AutoSize = true;
            lblProfitRoi.Font = new Font("Segoe UI", 9F);
            lblProfitRoi.Location = new Point(20, 88);
            lblProfitRoi.Name = "lblProfitRoi";
            lblProfitRoi.Size = new Size(79, 20);
            lblProfitRoi.TabIndex = 2;
            lblProfitRoi.Text = "ROI: 43.7%";
            // 
            // lblProfitMargen
            // 
            lblProfitMargen.AutoSize = true;
            lblProfitMargen.Font = new Font("Segoe UI", 9F);
            lblProfitMargen.Location = new Point(20, 62);
            lblProfitMargen.Name = "lblProfitMargen";
            lblProfitMargen.Size = new Size(106, 20);
            lblProfitMargen.TabIndex = 1;
            lblProfitMargen.Text = "Margen: 28.9%";
            // 
            // lblProfitGanancia
            // 
            lblProfitGanancia.AutoSize = true;
            lblProfitGanancia.Font = new Font("Segoe UI", 9F);
            lblProfitGanancia.Location = new Point(20, 36);
            lblProfitGanancia.Name = "lblProfitGanancia";
            lblProfitGanancia.Size = new Size(148, 20);
            lblProfitGanancia.TabIndex = 0;
            lblProfitGanancia.Text = "Ganancia: RD$52,000";
            // 
            // pnlFrozenCapital
            // 
            pnlFrozenCapital.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlFrozenCapital.Controls.Add(btnVerDetalleCapital);
            pnlFrozenCapital.Controls.Add(progressFrozen);
            pnlFrozenCapital.Controls.Add(lblFrozenDias);
            pnlFrozenCapital.Controls.Add(lblFrozenProductos);
            pnlFrozenCapital.Controls.Add(lblFrozenPct);
            pnlFrozenCapital.Controls.Add(lblFrozenValor);
            pnlFrozenCapital.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlFrozenCapital.Location = new Point(16, 360);
            pnlFrozenCapital.Name = "pnlFrozenCapital";
            pnlFrozenCapital.Size = new Size(1718, 130);
            pnlFrozenCapital.TabIndex = 2;
            pnlFrozenCapital.TabStop = false;
            pnlFrozenCapital.Text = "Capital congelado";
            // 
            // btnVerDetalleCapital
            // 
            btnVerDetalleCapital.Location = new Point(760, 44);
            btnVerDetalleCapital.Name = "btnVerDetalleCapital";
            btnVerDetalleCapital.Size = new Size(150, 32);
            btnVerDetalleCapital.TabIndex = 5;
            btnVerDetalleCapital.Text = "VER DETALLE";
            btnVerDetalleCapital.UseVisualStyleBackColor = true;
            btnVerDetalleCapital.Click += btnVerDetalleCapital_Click;
            // 
            // progressFrozen
            // 
            progressFrozen.Location = new Point(450, 48);
            progressFrozen.Name = "progressFrozen";
            progressFrozen.Size = new Size(280, 24);
            progressFrozen.TabIndex = 4;
            // 
            // lblFrozenDias
            // 
            lblFrozenDias.AutoSize = true;
            lblFrozenDias.Font = new Font("Segoe UI", 9F);
            lblFrozenDias.Location = new Point(280, 68);
            lblFrozenDias.Name = "lblFrozenDias";
            lblFrozenDias.Size = new Size(24, 20);
            lblFrozenDias.TabIndex = 3;
            lblFrozenDias.Text = "—";
            // 
            // lblFrozenProductos
            // 
            lblFrozenProductos.AutoSize = true;
            lblFrozenProductos.Font = new Font("Segoe UI", 9F);
            lblFrozenProductos.Location = new Point(280, 40);
            lblFrozenProductos.Name = "lblFrozenProductos";
            lblFrozenProductos.Size = new Size(88, 20);
            lblFrozenProductos.TabIndex = 2;
            lblFrozenProductos.Text = "0 productos";
            // 
            // lblFrozenPct
            // 
            lblFrozenPct.AutoSize = true;
            lblFrozenPct.Font = new Font("Segoe UI", 9F);
            lblFrozenPct.ForeColor = Color.FromArgb(113, 128, 150);
            lblFrozenPct.Location = new Point(24, 76);
            lblFrozenPct.Name = "lblFrozenPct";
            lblFrozenPct.Size = new Size(24, 20);
            lblFrozenPct.TabIndex = 1;
            lblFrozenPct.Text = "—";
            // 
            // lblFrozenValor
            // 
            lblFrozenValor.AutoSize = true;
            lblFrozenValor.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblFrozenValor.Location = new Point(20, 32);
            lblFrozenValor.Name = "lblFrozenValor";
            lblFrozenValor.Size = new Size(144, 41);
            lblFrozenValor.TabIndex = 0;
            lblFrozenValor.Text = "RD$ 0.00";
            // 
            // pnlInventoryHealth
            // 
            pnlInventoryHealth.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlInventoryHealth.Controls.Add(lblHealthScore);
            pnlInventoryHealth.Controls.Add(lblHealthTitle);
            pnlInventoryHealth.Controls.Add(pnlCriticos);
            pnlInventoryHealth.Controls.Add(pnlLentos);
            pnlInventoryHealth.Controls.Add(pnlBuenos);
            pnlInventoryHealth.Controls.Add(pnlEstrella);
            pnlInventoryHealth.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlInventoryHealth.Location = new Point(16, 185);
            pnlInventoryHealth.Name = "pnlInventoryHealth";
            pnlInventoryHealth.Size = new Size(1718, 170);
            pnlInventoryHealth.TabIndex = 1;
            pnlInventoryHealth.TabStop = false;
            pnlInventoryHealth.Text = "Estado del inventario";
            // 
            // lblHealthScore
            // 
            lblHealthScore.AutoSize = true;
            lblHealthScore.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblHealthScore.ForeColor = Color.FromArgb(56, 161, 105);
            lblHealthScore.Location = new Point(660, 72);
            lblHealthScore.Name = "lblHealthScore";
            lblHealthScore.Size = new Size(142, 50);
            lblHealthScore.TabIndex = 5;
            lblHealthScore.Text = "0 / 100";
            // 
            // lblHealthTitle
            // 
            lblHealthTitle.AutoSize = true;
            lblHealthTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblHealthTitle.Location = new Point(650, 40);
            lblHealthTitle.Name = "lblHealthTitle";
            lblHealthTitle.Size = new Size(185, 20);
            lblHealthTitle.TabIndex = 4;
            lblHealthTitle.Text = "SALUD DEL INVENTARIO";
            // 
            // pnlCriticos
            // 
            pnlCriticos.BackColor = Color.White;
            pnlCriticos.BorderStyle = BorderStyle.FixedSingle;
            pnlCriticos.Controls.Add(lblCriticosVal);
            pnlCriticos.Controls.Add(lblCriticosTitle);
            pnlCriticos.Location = new Point(478, 28);
            pnlCriticos.Name = "pnlCriticos";
            pnlCriticos.Size = new Size(140, 110);
            pnlCriticos.TabIndex = 3;
            // 
            // lblCriticosVal
            // 
            lblCriticosVal.AutoSize = true;
            lblCriticosVal.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblCriticosVal.Location = new Point(10, 48);
            lblCriticosVal.Name = "lblCriticosVal";
            lblCriticosVal.Size = new Size(43, 50);
            lblCriticosVal.TabIndex = 1;
            lblCriticosVal.Text = "0";
            // 
            // lblCriticosTitle
            // 
            lblCriticosTitle.AutoSize = true;
            lblCriticosTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblCriticosTitle.Location = new Point(10, 12);
            lblCriticosTitle.Name = "lblCriticosTitle";
            lblCriticosTitle.Size = new Size(75, 20);
            lblCriticosTitle.TabIndex = 0;
            lblCriticosTitle.Text = "CRITICOS";
            // 
            // pnlLentos
            // 
            pnlLentos.BackColor = Color.White;
            pnlLentos.BorderStyle = BorderStyle.FixedSingle;
            pnlLentos.Controls.Add(lblLentosVal);
            pnlLentos.Controls.Add(lblLentosTitle);
            pnlLentos.Location = new Point(324, 28);
            pnlLentos.Name = "pnlLentos";
            pnlLentos.Size = new Size(140, 110);
            pnlLentos.TabIndex = 2;
            // 
            // lblLentosVal
            // 
            lblLentosVal.AutoSize = true;
            lblLentosVal.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblLentosVal.Location = new Point(10, 48);
            lblLentosVal.Name = "lblLentosVal";
            lblLentosVal.Size = new Size(43, 50);
            lblLentosVal.TabIndex = 1;
            lblLentosVal.Text = "0";
            // 
            // lblLentosTitle
            // 
            lblLentosTitle.AutoSize = true;
            lblLentosTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblLentosTitle.Location = new Point(10, 12);
            lblLentosTitle.Name = "lblLentosTitle";
            lblLentosTitle.Size = new Size(64, 20);
            lblLentosTitle.TabIndex = 0;
            lblLentosTitle.Text = "LENTOS";
            // 
            // pnlBuenos
            // 
            pnlBuenos.BackColor = Color.White;
            pnlBuenos.BorderStyle = BorderStyle.FixedSingle;
            pnlBuenos.Controls.Add(lblBuenosVal);
            pnlBuenos.Controls.Add(lblBuenosTitle);
            pnlBuenos.Location = new Point(170, 28);
            pnlBuenos.Name = "pnlBuenos";
            pnlBuenos.Size = new Size(140, 110);
            pnlBuenos.TabIndex = 1;
            // 
            // lblBuenosVal
            // 
            lblBuenosVal.AutoSize = true;
            lblBuenosVal.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblBuenosVal.Location = new Point(10, 48);
            lblBuenosVal.Name = "lblBuenosVal";
            lblBuenosVal.Size = new Size(43, 50);
            lblBuenosVal.TabIndex = 1;
            lblBuenosVal.Text = "0";
            // 
            // lblBuenosTitle
            // 
            lblBuenosTitle.AutoSize = true;
            lblBuenosTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblBuenosTitle.Location = new Point(10, 12);
            lblBuenosTitle.Name = "lblBuenosTitle";
            lblBuenosTitle.Size = new Size(95, 20);
            lblBuenosTitle.TabIndex = 0;
            lblBuenosTitle.Text = "SALUDABLE";
            // 
            // pnlEstrella
            // 
            pnlEstrella.BackColor = Color.White;
            pnlEstrella.BorderStyle = BorderStyle.FixedSingle;
            pnlEstrella.Controls.Add(lblEstrellaVal);
            pnlEstrella.Controls.Add(lblEstrellaTitle);
            pnlEstrella.Location = new Point(16, 28);
            pnlEstrella.Name = "pnlEstrella";
            pnlEstrella.Size = new Size(140, 110);
            pnlEstrella.TabIndex = 0;
            // 
            // lblEstrellaVal
            // 
            lblEstrellaVal.AutoSize = true;
            lblEstrellaVal.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblEstrellaVal.Location = new Point(10, 48);
            lblEstrellaVal.Name = "lblEstrellaVal";
            lblEstrellaVal.Size = new Size(43, 50);
            lblEstrellaVal.TabIndex = 1;
            lblEstrellaVal.Text = "0";
            // 
            // lblEstrellaTitle
            // 
            lblEstrellaTitle.AutoSize = true;
            lblEstrellaTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblEstrellaTitle.Location = new Point(10, 12);
            lblEstrellaTitle.Name = "lblEstrellaTitle";
            lblEstrellaTitle.Size = new Size(79, 20);
            lblEstrellaTitle.TabIndex = 0;
            lblEstrellaTitle.Text = "ESTRELLA";
            // 
            // pnlKPIs
            // 
            pnlKPIs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlKPIs.Controls.Add(pnlKpiRoi);
            pnlKPIs.Controls.Add(pnlKpiGanancia);
            pnlKPIs.Controls.Add(pnlKpiVentas);
            pnlKPIs.Controls.Add(pnlKpiInventario);
            pnlKPIs.Controls.Add(pnlKpiCapital);
            pnlKPIs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnlKPIs.Location = new Point(17, 48);
            pnlKPIs.Name = "pnlKPIs";
            pnlKPIs.Size = new Size(1718, 134);
            pnlKPIs.TabIndex = 0;
            pnlKPIs.TabStop = false;
            pnlKPIs.Text = "KPIs";
            // 
            // pnlKpiRoi
            // 
            pnlKpiRoi.BackColor = Color.White;
            pnlKpiRoi.BorderStyle = BorderStyle.FixedSingle;
            pnlKpiRoi.Controls.Add(lblKpiRoiDelta);
            pnlKpiRoi.Controls.Add(lblKpiRoiVal);
            pnlKpiRoi.Controls.Add(lblKpiRoiTitle);
            pnlKpiRoi.Location = new Point(847, 28);
            pnlKpiRoi.Name = "pnlKpiRoi";
            pnlKpiRoi.Size = new Size(201, 100);
            pnlKpiRoi.TabIndex = 4;
            // 
            // lblKpiRoiDelta
            // 
            lblKpiRoiDelta.AutoSize = true;
            lblKpiRoiDelta.Font = new Font("Segoe UI", 8.5F);
            lblKpiRoiDelta.ForeColor = Color.FromArgb(56, 161, 105);
            lblKpiRoiDelta.Location = new Point(10, 72);
            lblKpiRoiDelta.Name = "lblKpiRoiDelta";
            lblKpiRoiDelta.Size = new Size(122, 20);
            lblKpiRoiDelta.TabIndex = 2;
            lblKpiRoiDelta.Text = "Ganancia / COGS";
            // 
            // lblKpiRoiVal
            // 
            lblKpiRoiVal.AutoSize = true;
            lblKpiRoiVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblKpiRoiVal.ForeColor = Color.FromArgb(26, 32, 44);
            lblKpiRoiVal.Location = new Point(10, 36);
            lblKpiRoiVal.Name = "lblKpiRoiVal";
            lblKpiRoiVal.Size = new Size(62, 32);
            lblKpiRoiVal.TabIndex = 1;
            lblKpiRoiVal.Text = "N/D";
            // 
            // lblKpiRoiTitle
            // 
            lblKpiRoiTitle.AutoSize = true;
            lblKpiRoiTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblKpiRoiTitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblKpiRoiTitle.Location = new Point(10, 10);
            lblKpiRoiTitle.Name = "lblKpiRoiTitle";
            lblKpiRoiTitle.Size = new Size(35, 20);
            lblKpiRoiTitle.TabIndex = 0;
            lblKpiRoiTitle.Text = "ROI";
            // 
            // pnlKpiGanancia
            // 
            pnlKpiGanancia.BackColor = Color.White;
            pnlKpiGanancia.BorderStyle = BorderStyle.FixedSingle;
            pnlKpiGanancia.Controls.Add(lblKpiGanDelta);
            pnlKpiGanancia.Controls.Add(lblKpiGanVal);
            pnlKpiGanancia.Controls.Add(lblKpiGanTitle);
            pnlKpiGanancia.Location = new Point(641, 28);
            pnlKpiGanancia.Name = "pnlKpiGanancia";
            pnlKpiGanancia.Size = new Size(201, 100);
            pnlKpiGanancia.TabIndex = 3;
            // 
            // lblKpiGanDelta
            // 
            lblKpiGanDelta.AutoSize = true;
            lblKpiGanDelta.Font = new Font("Segoe UI", 8.5F);
            lblKpiGanDelta.ForeColor = Color.FromArgb(56, 161, 105);
            lblKpiGanDelta.Location = new Point(10, 72);
            lblKpiGanDelta.Name = "lblKpiGanDelta";
            lblKpiGanDelta.Size = new Size(115, 20);
            lblKpiGanDelta.TabIndex = 2;
            lblKpiGanDelta.Text = "Realizada (mes)";
            // 
            // lblKpiGanVal
            // 
            lblKpiGanVal.AutoSize = true;
            lblKpiGanVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblKpiGanVal.ForeColor = Color.FromArgb(26, 32, 44);
            lblKpiGanVal.Location = new Point(10, 36);
            lblKpiGanVal.Name = "lblKpiGanVal";
            lblKpiGanVal.Size = new Size(118, 32);
            lblKpiGanVal.TabIndex = 1;
            lblKpiGanVal.Text = "RD$ 0.00";
            // 
            // lblKpiGanTitle
            // 
            lblKpiGanTitle.AutoSize = true;
            lblKpiGanTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblKpiGanTitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblKpiGanTitle.Location = new Point(10, 10);
            lblKpiGanTitle.Name = "lblKpiGanTitle";
            lblKpiGanTitle.Size = new Size(91, 20);
            lblKpiGanTitle.TabIndex = 0;
            lblKpiGanTitle.Text = "GANANCIA";
            // 
            // pnlKpiVentas
            // 
            pnlKpiVentas.BackColor = Color.White;
            pnlKpiVentas.BorderStyle = BorderStyle.FixedSingle;
            pnlKpiVentas.Controls.Add(lblKpiVenDelta);
            pnlKpiVentas.Controls.Add(lblKpiVenVal);
            pnlKpiVentas.Controls.Add(lblKpiVenTitle);
            pnlKpiVentas.Location = new Point(435, 28);
            pnlKpiVentas.Name = "pnlKpiVentas";
            pnlKpiVentas.Size = new Size(201, 100);
            pnlKpiVentas.TabIndex = 2;
            // 
            // lblKpiVenDelta
            // 
            lblKpiVenDelta.AutoSize = true;
            lblKpiVenDelta.Font = new Font("Segoe UI", 8.5F);
            lblKpiVenDelta.ForeColor = Color.FromArgb(56, 161, 105);
            lblKpiVenDelta.Location = new Point(10, 72);
            lblKpiVenDelta.Name = "lblKpiVenDelta";
            lblKpiVenDelta.Size = new Size(64, 20);
            lblKpiVenDelta.TabIndex = 2;
            lblKpiVenDelta.Text = "Ingresos";
            // 
            // lblKpiVenVal
            // 
            lblKpiVenVal.AutoSize = true;
            lblKpiVenVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblKpiVenVal.ForeColor = Color.FromArgb(26, 32, 44);
            lblKpiVenVal.Location = new Point(10, 36);
            lblKpiVenVal.Name = "lblKpiVenVal";
            lblKpiVenVal.Size = new Size(118, 32);
            lblKpiVenVal.TabIndex = 1;
            lblKpiVenVal.Text = "RD$ 0.00";
            // 
            // lblKpiVenTitle
            // 
            lblKpiVenTitle.AutoSize = true;
            lblKpiVenTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblKpiVenTitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblKpiVenTitle.Location = new Point(10, 10);
            lblKpiVenTitle.Name = "lblKpiVenTitle";
            lblKpiVenTitle.Size = new Size(66, 20);
            lblKpiVenTitle.TabIndex = 0;
            lblKpiVenTitle.Text = "VENTAS";
            // 
            // pnlKpiInventario
            // 
            pnlKpiInventario.BackColor = Color.White;
            pnlKpiInventario.BorderStyle = BorderStyle.FixedSingle;
            pnlKpiInventario.Controls.Add(lblKpiInvDelta);
            pnlKpiInventario.Controls.Add(lblKpiInvVal);
            pnlKpiInventario.Controls.Add(lblKpiInvTitle);
            pnlKpiInventario.Location = new Point(229, 28);
            pnlKpiInventario.Name = "pnlKpiInventario";
            pnlKpiInventario.Size = new Size(201, 100);
            pnlKpiInventario.TabIndex = 1;
            // 
            // lblKpiInvDelta
            // 
            lblKpiInvDelta.AutoSize = true;
            lblKpiInvDelta.Font = new Font("Segoe UI", 8.5F);
            lblKpiInvDelta.ForeColor = Color.FromArgb(56, 161, 105);
            lblKpiInvDelta.Location = new Point(10, 72);
            lblKpiInvDelta.Name = "lblKpiInvDelta";
            lblKpiInvDelta.Size = new Size(84, 20);
            lblKpiInvDelta.TabIndex = 2;
            lblKpiInvDelta.Text = "Valor a PVP";
            // 
            // lblKpiInvVal
            // 
            lblKpiInvVal.AutoSize = true;
            lblKpiInvVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblKpiInvVal.ForeColor = Color.FromArgb(26, 32, 44);
            lblKpiInvVal.Location = new Point(10, 36);
            lblKpiInvVal.Name = "lblKpiInvVal";
            lblKpiInvVal.Size = new Size(118, 32);
            lblKpiInvVal.TabIndex = 1;
            lblKpiInvVal.Text = "RD$ 0.00";
            // 
            // lblKpiInvTitle
            // 
            lblKpiInvTitle.AutoSize = true;
            lblKpiInvTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblKpiInvTitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblKpiInvTitle.Location = new Point(10, 10);
            lblKpiInvTitle.Name = "lblKpiInvTitle";
            lblKpiInvTitle.Size = new Size(101, 20);
            lblKpiInvTitle.TabIndex = 0;
            lblKpiInvTitle.Text = "INVENTARIO";
            // 
            // pnlKpiCapital
            // 
            pnlKpiCapital.BackColor = Color.White;
            pnlKpiCapital.BorderStyle = BorderStyle.FixedSingle;
            pnlKpiCapital.Controls.Add(lblKpiCapDelta);
            pnlKpiCapital.Controls.Add(lblKpiCapVal);
            pnlKpiCapital.Controls.Add(lblKpiCapTitle);
            pnlKpiCapital.Location = new Point(23, 28);
            pnlKpiCapital.Name = "pnlKpiCapital";
            pnlKpiCapital.Size = new Size(201, 100);
            pnlKpiCapital.TabIndex = 0;
            // 
            // lblKpiCapDelta
            // 
            lblKpiCapDelta.AutoSize = true;
            lblKpiCapDelta.Font = new Font("Segoe UI", 8.5F);
            lblKpiCapDelta.ForeColor = Color.FromArgb(56, 161, 105);
            lblKpiCapDelta.Location = new Point(10, 72);
            lblKpiCapDelta.Name = "lblKpiCapDelta";
            lblKpiCapDelta.Size = new Size(108, 20);
            lblKpiCapDelta.TabIndex = 2;
            lblKpiCapDelta.Text = "Capital a costo";
            // 
            // lblKpiCapVal
            // 
            lblKpiCapVal.AutoSize = true;
            lblKpiCapVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblKpiCapVal.ForeColor = Color.FromArgb(26, 32, 44);
            lblKpiCapVal.Location = new Point(10, 36);
            lblKpiCapVal.Name = "lblKpiCapVal";
            lblKpiCapVal.Size = new Size(118, 32);
            lblKpiCapVal.TabIndex = 1;
            lblKpiCapVal.Text = "RD$ 0.00";
            // 
            // lblKpiCapTitle
            // 
            lblKpiCapTitle.AutoSize = true;
            lblKpiCapTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblKpiCapTitle.ForeColor = Color.FromArgb(113, 128, 150);
            lblKpiCapTitle.Location = new Point(10, 10);
            lblKpiCapTitle.Name = "lblKpiCapTitle";
            lblKpiCapTitle.Size = new Size(70, 20);
            lblKpiCapTitle.TabIndex = 0;
            lblKpiCapTitle.Text = "CAPITAL";
            // 
            // FrmAnaDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1800, 720);
            Controls.Add(panelScroll);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaDashboard";
            Tag = "classic";
            Text = "Dashboard";
            Load += FrmAnaDashboard_Load;
            panelScroll.ResumeLayout(false);
            pnlActions.ResumeLayout(false);
            pnlDecisions.ResumeLayout(false);
            pnlTrends.ResumeLayout(false);
            pnlProducts.ResumeLayout(false);
            pnlProducts.PerformLayout();
            pnlCapital.ResumeLayout(false);
            pnlCapital.PerformLayout();
            pnlProfitability.ResumeLayout(false);
            pnlProfitability.PerformLayout();
            pnlFrozenCapital.ResumeLayout(false);
            pnlFrozenCapital.PerformLayout();
            pnlInventoryHealth.ResumeLayout(false);
            pnlInventoryHealth.PerformLayout();
            pnlCriticos.ResumeLayout(false);
            pnlCriticos.PerformLayout();
            pnlLentos.ResumeLayout(false);
            pnlLentos.PerformLayout();
            pnlBuenos.ResumeLayout(false);
            pnlBuenos.PerformLayout();
            pnlEstrella.ResumeLayout(false);
            pnlEstrella.PerformLayout();
            pnlKPIs.ResumeLayout(false);
            pnlKpiRoi.ResumeLayout(false);
            pnlKpiRoi.PerformLayout();
            pnlKpiGanancia.ResumeLayout(false);
            pnlKpiGanancia.PerformLayout();
            pnlKpiVentas.ResumeLayout(false);
            pnlKpiVentas.PerformLayout();
            pnlKpiInventario.ResumeLayout(false);
            pnlKpiInventario.PerformLayout();
            pnlKpiCapital.ResumeLayout(false);
            pnlKpiCapital.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelScroll;
        private GroupBox pnlKPIs;
        private Panel pnlKpiCapital;
        private Label lblKpiCapTitle;
        private Label lblKpiCapVal;
        private Label lblKpiCapDelta;
        private Panel pnlKpiInventario;
        private Label lblKpiInvTitle;
        private Label lblKpiInvVal;
        private Label lblKpiInvDelta;
        private Panel pnlKpiVentas;
        private Label lblKpiVenTitle;
        private Label lblKpiVenVal;
        private Label lblKpiVenDelta;
        private Panel pnlKpiGanancia;
        private Label lblKpiGanTitle;
        private Label lblKpiGanVal;
        private Label lblKpiGanDelta;
        private Panel pnlKpiRoi;
        private Label lblKpiRoiTitle;
        private Label lblKpiRoiVal;
        private Label lblKpiRoiDelta;
        private GroupBox pnlInventoryHealth;
        private Panel pnlEstrella;
        private Label lblEstrellaTitle;
        private Label lblEstrellaVal;
        private Panel pnlBuenos;
        private Label lblBuenosTitle;
        private Label lblBuenosVal;
        private Panel pnlLentos;
        private Label lblLentosTitle;
        private Label lblLentosVal;
        private Panel pnlCriticos;
        private Label lblCriticosTitle;
        private Label lblCriticosVal;
        private Label lblHealthTitle;
        private Label lblHealthScore;
        private GroupBox pnlFrozenCapital;
        private Label lblFrozenValor;
        private Label lblFrozenPct;
        private Label lblFrozenProductos;
        private Label lblFrozenDias;
        private ProgressBar progressFrozen;
        private Button btnVerDetalleCapital;
        private GroupBox pnlProfitability;
        private Label lblProfitGanancia;
        private Label lblProfitMargen;
        private Label lblProfitRoi;
        private Label lblGanRealizada;
        private Label lblGanPotencial;
        private GroupBox pnlCapital;
        private Label lblCapInvertido;
        private Label lblCapRecuperado;
        private Label lblCapPendiente;
        private Label lblCapInventario;
        private Label lblCapCaja;
        private Panel pnlChartCapital;
        private GroupBox pnlProducts;
        private Label lblTopTitle;
        private ListBox lstTop;
        private Label lblWatchTitle;
        private ListBox lstWatch;
        private GroupBox pnlTrends;
        private ComboBox cmbTrendMetric;
        private ComboBox cmbTrendPeriod;
        private Panel pnlChartTrends;
        private GroupBox pnlDecisions;
        private Label lblDecision1;
        private Label lblDecision2;
        private Label lblDecision3;
        private Button btnDecisionVer;
        private GroupBox pnlActions;
        private Label lblAccPendientes;
        private Label lblAccEnProceso;
        private Label lblAccCompletadas;
        private Label lblAccExitosas;
        private Label lblAccImpacto;
        private Button btnAccionesVer;
    }
}
