namespace UI
{
    partial class FrmAnaTendencias
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
            pnlVentas = new Panel();
            lblVentasTitle = new Label();
            lblVentasValue = new Label();
            lblVentasDesc = new Label();
            pnlGanancias = new Panel();
            lblGananciasTitle = new Label();
            lblGananciasValue = new Label();
            lblGananciasDesc = new Label();
            pnlROI = new Panel();
            lblROITitle = new Label();
            lblROIValue = new Label();
            lblROIDesc = new Label();
            pnlInventario = new Panel();
            lblInventarioTitle = new Label();
            lblInventarioValue = new Label();
            lblInventarioDesc = new Label();
            pnlCapital = new Panel();
            lblCapitalTitle = new Label();
            lblCapitalValue = new Label();
            lblCapitalDesc = new Label();
            pnlProductos = new Panel();
            lblProductosTitle = new Label();
            lblProductosValue = new Label();
            lblProductosDesc = new Label();
            pnlPeriodo = new Panel();
            lblPeriodoTitle = new Label();
            lblPeriodoValue = new Label();
            lblPeriodoDesc = new Label();
            pnlGrafico = new Panel();
            lblGraficoTitle = new Label();
            lblGraficoValue = new Label();
            lblGraficoDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlVentas.SuspendLayout();
            pnlGanancias.SuspendLayout();
            pnlROI.SuspendLayout();
            pnlInventario.SuspendLayout();
            pnlCapital.SuspendLayout();
            pnlProductos.SuspendLayout();
            pnlPeriodo.SuspendLayout();
            pnlGrafico.SuspendLayout();
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
            lblHeaderLocal.Text = "Tendencias";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlVentas);
            panelScroll.Controls.Add(pnlGanancias);
            panelScroll.Controls.Add(pnlROI);
            panelScroll.Controls.Add(pnlInventario);
            panelScroll.Controls.Add(pnlCapital);
            panelScroll.Controls.Add(pnlProductos);
            panelScroll.Controls.Add(pnlPeriodo);
            panelScroll.Controls.Add(pnlGrafico);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlVentas
            // 
            pnlVentas.BackColor = Color.White;
            pnlVentas.BorderStyle = BorderStyle.FixedSingle;
            pnlVentas.Controls.Add(lblVentasDesc);
            pnlVentas.Controls.Add(lblVentasValue);
            pnlVentas.Controls.Add(lblVentasTitle);
            pnlVentas.Location = new Point(16, 16);
            pnlVentas.Name = "pnlVentas";
            pnlVentas.Size = new Size(900, 110);
            pnlVentas.TabIndex = 0;
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
            // pnlGanancias
            // 
            pnlGanancias.BackColor = Color.White;
            pnlGanancias.BorderStyle = BorderStyle.FixedSingle;
            pnlGanancias.Controls.Add(lblGananciasDesc);
            pnlGanancias.Controls.Add(lblGananciasValue);
            pnlGanancias.Controls.Add(lblGananciasTitle);
            pnlGanancias.Location = new Point(16, 142);
            pnlGanancias.Name = "pnlGanancias";
            pnlGanancias.Size = new Size(900, 110);
            pnlGanancias.TabIndex = 1;
            pnlGanancias.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciasTitle
            // 
            lblGananciasTitle.AutoSize = true;
            lblGananciasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciasTitle.Location = new Point(14, 12);
            lblGananciasTitle.Name = "lblGananciasTitle";
            lblGananciasTitle.Size = new Size(120, 23);
            lblGananciasTitle.TabIndex = 0;
            lblGananciasTitle.Text = "Ganancias";
            // 
            // lblGananciasValue
            // 
            lblGananciasValue.AutoSize = true;
            lblGananciasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciasValue.Location = new Point(14, 42);
            lblGananciasValue.Name = "lblGananciasValue";
            lblGananciasValue.Size = new Size(120, 41);
            lblGananciasValue.TabIndex = 1;
            lblGananciasValue.Text = "—";
            // 
            // lblGananciasDesc
            // 
            lblGananciasDesc.AutoSize = true;
            lblGananciasDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciasDesc.Location = new Point(16, 84);
            lblGananciasDesc.Name = "lblGananciasDesc";
            lblGananciasDesc.Size = new Size(180, 19);
            lblGananciasDesc.TabIndex = 2;
            lblGananciasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROI
            // 
            pnlROI.BackColor = Color.White;
            pnlROI.BorderStyle = BorderStyle.FixedSingle;
            pnlROI.Controls.Add(lblROIDesc);
            pnlROI.Controls.Add(lblROIValue);
            pnlROI.Controls.Add(lblROITitle);
            pnlROI.Location = new Point(16, 268);
            pnlROI.Name = "pnlROI";
            pnlROI.Size = new Size(900, 110);
            pnlROI.TabIndex = 2;
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
            // pnlInventario
            // 
            pnlInventario.BackColor = Color.White;
            pnlInventario.BorderStyle = BorderStyle.FixedSingle;
            pnlInventario.Controls.Add(lblInventarioDesc);
            pnlInventario.Controls.Add(lblInventarioValue);
            pnlInventario.Controls.Add(lblInventarioTitle);
            pnlInventario.Location = new Point(16, 394);
            pnlInventario.Name = "pnlInventario";
            pnlInventario.Size = new Size(900, 110);
            pnlInventario.TabIndex = 3;
            pnlInventario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblInventarioTitle
            // 
            lblInventarioTitle.AutoSize = true;
            lblInventarioTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInventarioTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblInventarioTitle.Location = new Point(14, 12);
            lblInventarioTitle.Name = "lblInventarioTitle";
            lblInventarioTitle.Size = new Size(120, 23);
            lblInventarioTitle.TabIndex = 0;
            lblInventarioTitle.Text = "Inventario";
            // 
            // lblInventarioValue
            // 
            lblInventarioValue.AutoSize = true;
            lblInventarioValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblInventarioValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblInventarioValue.Location = new Point(14, 42);
            lblInventarioValue.Name = "lblInventarioValue";
            lblInventarioValue.Size = new Size(120, 41);
            lblInventarioValue.TabIndex = 1;
            lblInventarioValue.Text = "RD$ 0.00";
            // 
            // lblInventarioDesc
            // 
            lblInventarioDesc.AutoSize = true;
            lblInventarioDesc.Font = new Font("Segoe UI", 8.5F);
            lblInventarioDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblInventarioDesc.Location = new Point(16, 84);
            lblInventarioDesc.Name = "lblInventarioDesc";
            lblInventarioDesc.Size = new Size(180, 19);
            lblInventarioDesc.TabIndex = 2;
            lblInventarioDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlCapital
            // 
            pnlCapital.BackColor = Color.White;
            pnlCapital.BorderStyle = BorderStyle.FixedSingle;
            pnlCapital.Controls.Add(lblCapitalDesc);
            pnlCapital.Controls.Add(lblCapitalValue);
            pnlCapital.Controls.Add(lblCapitalTitle);
            pnlCapital.Location = new Point(16, 520);
            pnlCapital.Name = "pnlCapital";
            pnlCapital.Size = new Size(900, 110);
            pnlCapital.TabIndex = 4;
            pnlCapital.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCapitalTitle
            // 
            lblCapitalTitle.AutoSize = true;
            lblCapitalTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCapitalTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCapitalTitle.Location = new Point(14, 12);
            lblCapitalTitle.Name = "lblCapitalTitle";
            lblCapitalTitle.Size = new Size(120, 23);
            lblCapitalTitle.TabIndex = 0;
            lblCapitalTitle.Text = "Capital";
            // 
            // lblCapitalValue
            // 
            lblCapitalValue.AutoSize = true;
            lblCapitalValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCapitalValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCapitalValue.Location = new Point(14, 42);
            lblCapitalValue.Name = "lblCapitalValue";
            lblCapitalValue.Size = new Size(120, 41);
            lblCapitalValue.TabIndex = 1;
            lblCapitalValue.Text = "—";
            // 
            // lblCapitalDesc
            // 
            lblCapitalDesc.AutoSize = true;
            lblCapitalDesc.Font = new Font("Segoe UI", 8.5F);
            lblCapitalDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCapitalDesc.Location = new Point(16, 84);
            lblCapitalDesc.Name = "lblCapitalDesc";
            lblCapitalDesc.Size = new Size(180, 19);
            lblCapitalDesc.TabIndex = 2;
            lblCapitalDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlProductos
            // 
            pnlProductos.BackColor = Color.White;
            pnlProductos.BorderStyle = BorderStyle.FixedSingle;
            pnlProductos.Controls.Add(lblProductosDesc);
            pnlProductos.Controls.Add(lblProductosValue);
            pnlProductos.Controls.Add(lblProductosTitle);
            pnlProductos.Location = new Point(16, 646);
            pnlProductos.Name = "pnlProductos";
            pnlProductos.Size = new Size(900, 110);
            pnlProductos.TabIndex = 5;
            pnlProductos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductosTitle
            // 
            lblProductosTitle.AutoSize = true;
            lblProductosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductosTitle.Location = new Point(14, 12);
            lblProductosTitle.Name = "lblProductosTitle";
            lblProductosTitle.Size = new Size(120, 23);
            lblProductosTitle.TabIndex = 0;
            lblProductosTitle.Text = "Productos";
            // 
            // lblProductosValue
            // 
            lblProductosValue.AutoSize = true;
            lblProductosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductosValue.Location = new Point(14, 42);
            lblProductosValue.Name = "lblProductosValue";
            lblProductosValue.Size = new Size(120, 41);
            lblProductosValue.TabIndex = 1;
            lblProductosValue.Text = "0 %";
            // 
            // lblProductosDesc
            // 
            lblProductosDesc.AutoSize = true;
            lblProductosDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductosDesc.Location = new Point(16, 84);
            lblProductosDesc.Name = "lblProductosDesc";
            lblProductosDesc.Size = new Size(180, 19);
            lblProductosDesc.TabIndex = 2;
            lblProductosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlPeriodo
            // 
            pnlPeriodo.BackColor = Color.White;
            pnlPeriodo.BorderStyle = BorderStyle.FixedSingle;
            pnlPeriodo.Controls.Add(lblPeriodoDesc);
            pnlPeriodo.Controls.Add(lblPeriodoValue);
            pnlPeriodo.Controls.Add(lblPeriodoTitle);
            pnlPeriodo.Location = new Point(16, 772);
            pnlPeriodo.Name = "pnlPeriodo";
            pnlPeriodo.Size = new Size(900, 110);
            pnlPeriodo.TabIndex = 6;
            pnlPeriodo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblPeriodoTitle
            // 
            lblPeriodoTitle.AutoSize = true;
            lblPeriodoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPeriodoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblPeriodoTitle.Location = new Point(14, 12);
            lblPeriodoTitle.Name = "lblPeriodoTitle";
            lblPeriodoTitle.Size = new Size(120, 23);
            lblPeriodoTitle.TabIndex = 0;
            lblPeriodoTitle.Text = "Periodo";
            // 
            // lblPeriodoValue
            // 
            lblPeriodoValue.AutoSize = true;
            lblPeriodoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblPeriodoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblPeriodoValue.Location = new Point(14, 42);
            lblPeriodoValue.Name = "lblPeriodoValue";
            lblPeriodoValue.Size = new Size(120, 41);
            lblPeriodoValue.TabIndex = 1;
            lblPeriodoValue.Text = "RD$ 0.00";
            // 
            // lblPeriodoDesc
            // 
            lblPeriodoDesc.AutoSize = true;
            lblPeriodoDesc.Font = new Font("Segoe UI", 8.5F);
            lblPeriodoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblPeriodoDesc.Location = new Point(16, 84);
            lblPeriodoDesc.Name = "lblPeriodoDesc";
            lblPeriodoDesc.Size = new Size(180, 19);
            lblPeriodoDesc.TabIndex = 2;
            lblPeriodoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGrafico
            // 
            pnlGrafico.BackColor = Color.White;
            pnlGrafico.BorderStyle = BorderStyle.FixedSingle;
            pnlGrafico.Controls.Add(lblGraficoDesc);
            pnlGrafico.Controls.Add(lblGraficoValue);
            pnlGrafico.Controls.Add(lblGraficoTitle);
            pnlGrafico.Location = new Point(16, 898);
            pnlGrafico.Name = "pnlGrafico";
            pnlGrafico.Size = new Size(900, 110);
            pnlGrafico.TabIndex = 7;
            pnlGrafico.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGraficoTitle
            // 
            lblGraficoTitle.AutoSize = true;
            lblGraficoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGraficoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGraficoTitle.Location = new Point(14, 12);
            lblGraficoTitle.Name = "lblGraficoTitle";
            lblGraficoTitle.Size = new Size(120, 23);
            lblGraficoTitle.TabIndex = 0;
            lblGraficoTitle.Text = "Grafico";
            // 
            // lblGraficoValue
            // 
            lblGraficoValue.AutoSize = true;
            lblGraficoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGraficoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGraficoValue.Location = new Point(14, 42);
            lblGraficoValue.Name = "lblGraficoValue";
            lblGraficoValue.Size = new Size(120, 41);
            lblGraficoValue.TabIndex = 1;
            lblGraficoValue.Text = "—";
            // 
            // lblGraficoDesc
            // 
            lblGraficoDesc.AutoSize = true;
            lblGraficoDesc.Font = new Font("Segoe UI", 8.5F);
            lblGraficoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGraficoDesc.Location = new Point(16, 84);
            lblGraficoDesc.Name = "lblGraficoDesc";
            lblGraficoDesc.Size = new Size(180, 19);
            lblGraficoDesc.TabIndex = 2;
            lblGraficoDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaTendencias
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaTendencias";
            Text = "Tendencias";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlVentas.ResumeLayout(false);
            pnlVentas.PerformLayout();
            pnlGanancias.ResumeLayout(false);
            pnlGanancias.PerformLayout();
            pnlROI.ResumeLayout(false);
            pnlROI.PerformLayout();
            pnlInventario.ResumeLayout(false);
            pnlInventario.PerformLayout();
            pnlCapital.ResumeLayout(false);
            pnlCapital.PerformLayout();
            pnlProductos.ResumeLayout(false);
            pnlProductos.PerformLayout();
            pnlPeriodo.ResumeLayout(false);
            pnlPeriodo.PerformLayout();
            pnlGrafico.ResumeLayout(false);
            pnlGrafico.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlVentas;
        private Label lblVentasTitle;
        private Label lblVentasValue;
        private Label lblVentasDesc;
        private Panel pnlGanancias;
        private Label lblGananciasTitle;
        private Label lblGananciasValue;
        private Label lblGananciasDesc;
        private Panel pnlROI;
        private Label lblROITitle;
        private Label lblROIValue;
        private Label lblROIDesc;
        private Panel pnlInventario;
        private Label lblInventarioTitle;
        private Label lblInventarioValue;
        private Label lblInventarioDesc;
        private Panel pnlCapital;
        private Label lblCapitalTitle;
        private Label lblCapitalValue;
        private Label lblCapitalDesc;
        private Panel pnlProductos;
        private Label lblProductosTitle;
        private Label lblProductosValue;
        private Label lblProductosDesc;
        private Panel pnlPeriodo;
        private Label lblPeriodoTitle;
        private Label lblPeriodoValue;
        private Label lblPeriodoDesc;
        private Panel pnlGrafico;
        private Label lblGraficoTitle;
        private Label lblGraficoValue;
        private Label lblGraficoDesc;
    }
}
