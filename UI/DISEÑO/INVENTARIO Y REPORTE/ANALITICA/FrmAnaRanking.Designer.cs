namespace UI
{
    partial class FrmAnaRanking
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
            panelHeaderLocal = new Panel();
            lblHeaderLocal = new Label();
            panelScroll = new Panel();
            pnlFiltros = new Panel();
            lblFiltrosTitle = new Label();
            lblFiltrosValue = new Label();
            lblFiltrosDesc = new Label();
            pnlRanking = new Panel();
            lblRankingTitle = new Label();
            lblRankingValue = new Label();
            lblRankingDesc = new Label();
            pnlScore = new Panel();
            lblScoreTitle = new Label();
            lblScoreValue = new Label();
            lblScoreDesc = new Label();
            pnlVentas = new Panel();
            lblVentasTitle = new Label();
            lblVentasValue = new Label();
            lblVentasDesc = new Label();
            pnlGanancia = new Panel();
            lblGananciaTitle = new Label();
            lblGananciaValue = new Label();
            lblGananciaDesc = new Label();
            pnlROI = new Panel();
            lblROITitle = new Label();
            lblROIValue = new Label();
            lblROIDesc = new Label();
            pnlRotacion = new Panel();
            lblRotacionTitle = new Label();
            lblRotacionValue = new Label();
            lblRotacionDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlFiltros.SuspendLayout();
            pnlRanking.SuspendLayout();
            pnlScore.SuspendLayout();
            pnlVentas.SuspendLayout();
            pnlGanancia.SuspendLayout();
            pnlROI.SuspendLayout();
            pnlRotacion.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeaderLocal
            // 
            panelHeaderLocal.BackColor = Color.White;
            panelHeaderLocal.BorderStyle = BorderStyle.FixedSingle;
            panelHeaderLocal.Controls.Add(lblHeaderLocal);
            panelHeaderLocal.Dock = DockStyle.Top;
            panelHeaderLocal.Location = new Point(0, 0);
            panelHeaderLocal.Name = "panelHeaderLocal";
            panelHeaderLocal.Size = new Size(940, 48);
            panelHeaderLocal.TabIndex = 0;
            // 
            // lblHeaderLocal
            // 
            lblHeaderLocal.AutoSize = true;
            lblHeaderLocal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblHeaderLocal.ForeColor = Color.FromArgb(26, 32, 44);
            lblHeaderLocal.Location = new Point(14, 12);
            lblHeaderLocal.Name = "lblHeaderLocal";
            lblHeaderLocal.Size = new Size(100, 28);
            lblHeaderLocal.TabIndex = 0;
            lblHeaderLocal.Text = "Ranking";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlFiltros);
            panelScroll.Controls.Add(pnlRanking);
            panelScroll.Controls.Add(pnlScore);
            panelScroll.Controls.Add(pnlVentas);
            panelScroll.Controls.Add(pnlGanancia);
            panelScroll.Controls.Add(pnlROI);
            panelScroll.Controls.Add(pnlRotacion);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlFiltros
            // 
            pnlFiltros.BackColor = Color.White;
            pnlFiltros.BorderStyle = BorderStyle.FixedSingle;
            pnlFiltros.Controls.Add(lblFiltrosDesc);
            pnlFiltros.Controls.Add(lblFiltrosValue);
            pnlFiltros.Controls.Add(lblFiltrosTitle);
            pnlFiltros.Location = new Point(16, 16);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(900, 110);
            pnlFiltros.TabIndex = 0;
            pnlFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblFiltrosTitle
            // 
            lblFiltrosTitle.AutoSize = true;
            lblFiltrosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFiltrosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblFiltrosTitle.Location = new Point(14, 12);
            lblFiltrosTitle.Name = "lblFiltrosTitle";
            lblFiltrosTitle.Size = new Size(120, 23);
            lblFiltrosTitle.TabIndex = 0;
            lblFiltrosTitle.Text = "Filtros";
            // 
            // lblFiltrosValue
            // 
            lblFiltrosValue.AutoSize = true;
            lblFiltrosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblFiltrosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblFiltrosValue.Location = new Point(14, 42);
            lblFiltrosValue.Name = "lblFiltrosValue";
            lblFiltrosValue.Size = new Size(120, 41);
            lblFiltrosValue.TabIndex = 1;
            lblFiltrosValue.Text = "RD$ 0.00";
            // 
            // lblFiltrosDesc
            // 
            lblFiltrosDesc.AutoSize = true;
            lblFiltrosDesc.Font = new Font("Segoe UI", 8.5F);
            lblFiltrosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblFiltrosDesc.Location = new Point(16, 84);
            lblFiltrosDesc.Name = "lblFiltrosDesc";
            lblFiltrosDesc.Size = new Size(180, 19);
            lblFiltrosDesc.TabIndex = 2;
            lblFiltrosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlRanking
            // 
            pnlRanking.BackColor = Color.White;
            pnlRanking.BorderStyle = BorderStyle.FixedSingle;
            pnlRanking.Controls.Add(lblRankingDesc);
            pnlRanking.Controls.Add(lblRankingValue);
            pnlRanking.Controls.Add(lblRankingTitle);
            pnlRanking.Location = new Point(16, 142);
            pnlRanking.Name = "pnlRanking";
            pnlRanking.Size = new Size(900, 110);
            pnlRanking.TabIndex = 1;
            pnlRanking.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblRankingTitle
            // 
            lblRankingTitle.AutoSize = true;
            lblRankingTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRankingTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblRankingTitle.Location = new Point(14, 12);
            lblRankingTitle.Name = "lblRankingTitle";
            lblRankingTitle.Size = new Size(120, 23);
            lblRankingTitle.TabIndex = 0;
            lblRankingTitle.Text = "Ranking";
            // 
            // lblRankingValue
            // 
            lblRankingValue.AutoSize = true;
            lblRankingValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblRankingValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblRankingValue.Location = new Point(14, 42);
            lblRankingValue.Name = "lblRankingValue";
            lblRankingValue.Size = new Size(120, 41);
            lblRankingValue.TabIndex = 1;
            lblRankingValue.Text = "—";
            // 
            // lblRankingDesc
            // 
            lblRankingDesc.AutoSize = true;
            lblRankingDesc.Font = new Font("Segoe UI", 8.5F);
            lblRankingDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblRankingDesc.Location = new Point(16, 84);
            lblRankingDesc.Name = "lblRankingDesc";
            lblRankingDesc.Size = new Size(180, 19);
            lblRankingDesc.TabIndex = 2;
            lblRankingDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlScore
            // 
            pnlScore.BackColor = Color.White;
            pnlScore.BorderStyle = BorderStyle.FixedSingle;
            pnlScore.Controls.Add(lblScoreDesc);
            pnlScore.Controls.Add(lblScoreValue);
            pnlScore.Controls.Add(lblScoreTitle);
            pnlScore.Location = new Point(16, 268);
            pnlScore.Name = "pnlScore";
            pnlScore.Size = new Size(900, 110);
            pnlScore.TabIndex = 2;
            pnlScore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblScoreTitle
            // 
            lblScoreTitle.AutoSize = true;
            lblScoreTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblScoreTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblScoreTitle.Location = new Point(14, 12);
            lblScoreTitle.Name = "lblScoreTitle";
            lblScoreTitle.Size = new Size(120, 23);
            lblScoreTitle.TabIndex = 0;
            lblScoreTitle.Text = "Score";
            // 
            // lblScoreValue
            // 
            lblScoreValue.AutoSize = true;
            lblScoreValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblScoreValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblScoreValue.Location = new Point(14, 42);
            lblScoreValue.Name = "lblScoreValue";
            lblScoreValue.Size = new Size(120, 41);
            lblScoreValue.TabIndex = 1;
            lblScoreValue.Text = "0 %";
            // 
            // lblScoreDesc
            // 
            lblScoreDesc.AutoSize = true;
            lblScoreDesc.Font = new Font("Segoe UI", 8.5F);
            lblScoreDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblScoreDesc.Location = new Point(16, 84);
            lblScoreDesc.Name = "lblScoreDesc";
            lblScoreDesc.Size = new Size(180, 19);
            lblScoreDesc.TabIndex = 2;
            lblScoreDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlVentas
            // 
            pnlVentas.BackColor = Color.White;
            pnlVentas.BorderStyle = BorderStyle.FixedSingle;
            pnlVentas.Controls.Add(lblVentasDesc);
            pnlVentas.Controls.Add(lblVentasValue);
            pnlVentas.Controls.Add(lblVentasTitle);
            pnlVentas.Location = new Point(16, 394);
            pnlVentas.Name = "pnlVentas";
            pnlVentas.Size = new Size(900, 110);
            pnlVentas.TabIndex = 3;
            pnlVentas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblVentasTitle
            // 
            lblVentasTitle.AutoSize = true;
            lblVentasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblVentasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblVentasTitle.Location = new Point(14, 12);
            lblVentasTitle.Name = "lblVentasTitle";
            lblVentasTitle.Size = new Size(120, 23);
            lblVentasTitle.TabIndex = 0;
            lblVentasTitle.Text = "Ventas";
            // 
            // lblVentasValue
            // 
            lblVentasValue.AutoSize = true;
            lblVentasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblVentasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblVentasValue.Location = new Point(14, 42);
            lblVentasValue.Name = "lblVentasValue";
            lblVentasValue.Size = new Size(120, 41);
            lblVentasValue.TabIndex = 1;
            lblVentasValue.Text = "RD$ 0.00";
            // 
            // lblVentasDesc
            // 
            lblVentasDesc.AutoSize = true;
            lblVentasDesc.Font = new Font("Segoe UI", 8.5F);
            lblVentasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblVentasDesc.Location = new Point(16, 84);
            lblVentasDesc.Name = "lblVentasDesc";
            lblVentasDesc.Size = new Size(180, 19);
            lblVentasDesc.TabIndex = 2;
            lblVentasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGanancia
            // 
            pnlGanancia.BackColor = Color.White;
            pnlGanancia.BorderStyle = BorderStyle.FixedSingle;
            pnlGanancia.Controls.Add(lblGananciaDesc);
            pnlGanancia.Controls.Add(lblGananciaValue);
            pnlGanancia.Controls.Add(lblGananciaTitle);
            pnlGanancia.Location = new Point(16, 520);
            pnlGanancia.Name = "pnlGanancia";
            pnlGanancia.Size = new Size(900, 110);
            pnlGanancia.TabIndex = 4;
            pnlGanancia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciaTitle
            // 
            lblGananciaTitle.AutoSize = true;
            lblGananciaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciaTitle.Location = new Point(14, 12);
            lblGananciaTitle.Name = "lblGananciaTitle";
            lblGananciaTitle.Size = new Size(120, 23);
            lblGananciaTitle.TabIndex = 0;
            lblGananciaTitle.Text = "Ganancia";
            // 
            // lblGananciaValue
            // 
            lblGananciaValue.AutoSize = true;
            lblGananciaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciaValue.Location = new Point(14, 42);
            lblGananciaValue.Name = "lblGananciaValue";
            lblGananciaValue.Size = new Size(120, 41);
            lblGananciaValue.TabIndex = 1;
            lblGananciaValue.Text = "—";
            // 
            // lblGananciaDesc
            // 
            lblGananciaDesc.AutoSize = true;
            lblGananciaDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciaDesc.Location = new Point(16, 84);
            lblGananciaDesc.Name = "lblGananciaDesc";
            lblGananciaDesc.Size = new Size(180, 19);
            lblGananciaDesc.TabIndex = 2;
            lblGananciaDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROI
            // 
            pnlROI.BackColor = Color.White;
            pnlROI.BorderStyle = BorderStyle.FixedSingle;
            pnlROI.Controls.Add(lblROIDesc);
            pnlROI.Controls.Add(lblROIValue);
            pnlROI.Controls.Add(lblROITitle);
            pnlROI.Location = new Point(16, 646);
            pnlROI.Name = "pnlROI";
            pnlROI.Size = new Size(900, 110);
            pnlROI.TabIndex = 5;
            pnlROI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROITitle
            // 
            lblROITitle.AutoSize = true;
            lblROITitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROITitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROITitle.Location = new Point(14, 12);
            lblROITitle.Name = "lblROITitle";
            lblROITitle.Size = new Size(120, 23);
            lblROITitle.TabIndex = 0;
            lblROITitle.Text = "ROI";
            // 
            // lblROIValue
            // 
            lblROIValue.AutoSize = true;
            lblROIValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIValue.Location = new Point(14, 42);
            lblROIValue.Name = "lblROIValue";
            lblROIValue.Size = new Size(120, 41);
            lblROIValue.TabIndex = 1;
            lblROIValue.Text = "0 %";
            // 
            // lblROIDesc
            // 
            lblROIDesc.AutoSize = true;
            lblROIDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIDesc.Location = new Point(16, 84);
            lblROIDesc.Name = "lblROIDesc";
            lblROIDesc.Size = new Size(180, 19);
            lblROIDesc.TabIndex = 2;
            lblROIDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlRotacion
            // 
            pnlRotacion.BackColor = Color.White;
            pnlRotacion.BorderStyle = BorderStyle.FixedSingle;
            pnlRotacion.Controls.Add(lblRotacionDesc);
            pnlRotacion.Controls.Add(lblRotacionValue);
            pnlRotacion.Controls.Add(lblRotacionTitle);
            pnlRotacion.Location = new Point(16, 772);
            pnlRotacion.Name = "pnlRotacion";
            pnlRotacion.Size = new Size(900, 110);
            pnlRotacion.TabIndex = 6;
            pnlRotacion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblRotacionTitle
            // 
            lblRotacionTitle.AutoSize = true;
            lblRotacionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRotacionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblRotacionTitle.Location = new Point(14, 12);
            lblRotacionTitle.Name = "lblRotacionTitle";
            lblRotacionTitle.Size = new Size(120, 23);
            lblRotacionTitle.TabIndex = 0;
            lblRotacionTitle.Text = "Rotacion";
            // 
            // lblRotacionValue
            // 
            lblRotacionValue.AutoSize = true;
            lblRotacionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblRotacionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblRotacionValue.Location = new Point(14, 42);
            lblRotacionValue.Name = "lblRotacionValue";
            lblRotacionValue.Size = new Size(120, 41);
            lblRotacionValue.TabIndex = 1;
            lblRotacionValue.Text = "RD$ 0.00";
            // 
            // lblRotacionDesc
            // 
            lblRotacionDesc.AutoSize = true;
            lblRotacionDesc.Font = new Font("Segoe UI", 8.5F);
            lblRotacionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblRotacionDesc.Location = new Point(16, 84);
            lblRotacionDesc.Name = "lblRotacionDesc";
            lblRotacionDesc.Size = new Size(180, 19);
            lblRotacionDesc.TabIndex = 2;
            lblRotacionDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaRanking
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaRanking";
            Text = "Ranking";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlRanking.ResumeLayout(false);
            pnlRanking.PerformLayout();
            pnlScore.ResumeLayout(false);
            pnlScore.PerformLayout();
            pnlVentas.ResumeLayout(false);
            pnlVentas.PerformLayout();
            pnlGanancia.ResumeLayout(false);
            pnlGanancia.PerformLayout();
            pnlROI.ResumeLayout(false);
            pnlROI.PerformLayout();
            pnlRotacion.ResumeLayout(false);
            pnlRotacion.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlFiltros;
        private Label lblFiltrosTitle;
        private Label lblFiltrosValue;
        private Label lblFiltrosDesc;
        private Panel pnlRanking;
        private Label lblRankingTitle;
        private Label lblRankingValue;
        private Label lblRankingDesc;
        private Panel pnlScore;
        private Label lblScoreTitle;
        private Label lblScoreValue;
        private Label lblScoreDesc;
        private Panel pnlVentas;
        private Label lblVentasTitle;
        private Label lblVentasValue;
        private Label lblVentasDesc;
        private Panel pnlGanancia;
        private Label lblGananciaTitle;
        private Label lblGananciaValue;
        private Label lblGananciaDesc;
        private Panel pnlROI;
        private Label lblROITitle;
        private Label lblROIValue;
        private Label lblROIDesc;
        private Panel pnlRotacion;
        private Label lblRotacionTitle;
        private Label lblRotacionValue;
        private Label lblRotacionDesc;
    }
}
