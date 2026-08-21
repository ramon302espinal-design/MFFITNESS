namespace UI
{
    partial class FrmAnaRoi
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
            lblCrmHint = new Label();
            panelScroll = new Panel();
            pnlROIgeneral = new Panel();
            lblROIgeneralTitle = new Label();
            lblROIgeneralValue = new Label();
            lblROIgeneralDesc = new Label();
            pnlROIrealizado = new Panel();
            lblROIrealizadoTitle = new Label();
            lblROIrealizadoValue = new Label();
            lblROIrealizadoDesc = new Label();
            pnlROIproyectado = new Panel();
            lblROIproyectadoTitle = new Label();
            lblROIproyectadoValue = new Label();
            lblROIproyectadoDesc = new Label();
            pnlROIporproducto = new Panel();
            lblROIporproductoTitle = new Label();
            lblROIporproductoValue = new Label();
            lblROIporproductoDesc = new Label();
            pnlROIporinversion = new Panel();
            lblROIporinversionTitle = new Label();
            lblROIporinversionValue = new Label();
            lblROIporinversionDesc = new Label();
            pnlGrafico = new Panel();
            lblGraficoTitle = new Label();
            lblGraficoValue = new Label();
            lblGraficoDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlROIgeneral.SuspendLayout();
            pnlROIrealizado.SuspendLayout();
            pnlROIproyectado.SuspendLayout();
            pnlROIporproducto.SuspendLayout();
            pnlROIporinversion.SuspendLayout();
            pnlGrafico.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeaderLocal
            // 
            panelHeaderLocal.BackColor = Color.White;
            panelHeaderLocal.BorderStyle = BorderStyle.FixedSingle;
            panelHeaderLocal.Controls.Add(lblCrmHint);
            panelHeaderLocal.Controls.Add(lblHeaderLocal);
            panelHeaderLocal.Dock = DockStyle.Top;
            panelHeaderLocal.Location = new Point(0, 0);
            panelHeaderLocal.Name = "panelHeaderLocal";
            panelHeaderLocal.Size = new Size(940, 64);
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

            // 
            // lblCrmHint
            // 
            lblCrmHint.AutoSize = true;
            lblCrmHint.Font = new Font("Segoe UI", 8F);
            lblCrmHint.ForeColor = Color.FromArgb(113, 128, 150);
            lblCrmHint.Location = new Point(14, 40);
            lblCrmHint.Name = "lblCrmHint";
            lblCrmHint.Size = new Size(800, 19);
            lblCrmHint.TabIndex = 1;
            lblCrmHint.Text = "";
            lblHeaderLocal.Text = "ROI";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlROIgeneral);
            panelScroll.Controls.Add(pnlROIrealizado);
            panelScroll.Controls.Add(pnlROIproyectado);
            panelScroll.Controls.Add(pnlROIporproducto);
            panelScroll.Controls.Add(pnlROIporinversion);
            panelScroll.Controls.Add(pnlGrafico);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 64);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlROIgeneral
            // 
            pnlROIgeneral.BackColor = Color.White;
            pnlROIgeneral.BorderStyle = BorderStyle.FixedSingle;
            pnlROIgeneral.Controls.Add(lblROIgeneralDesc);
            pnlROIgeneral.Controls.Add(lblROIgeneralValue);
            pnlROIgeneral.Controls.Add(lblROIgeneralTitle);
            pnlROIgeneral.Location = new Point(16, 16);
            pnlROIgeneral.Name = "pnlROIgeneral";
            pnlROIgeneral.Size = new Size(900, 110);
            pnlROIgeneral.TabIndex = 0;
            pnlROIgeneral.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROIgeneralTitle
            // 
            lblROIgeneralTitle.AutoSize = true;
            lblROIgeneralTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROIgeneralTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROIgeneralTitle.Location = new Point(14, 12);
            lblROIgeneralTitle.Name = "lblROIgeneralTitle";
            lblROIgeneralTitle.Size = new Size(120, 23);
            lblROIgeneralTitle.TabIndex = 0;
            lblROIgeneralTitle.Text = "ROI general";
            // 
            // lblROIgeneralValue
            // 
            lblROIgeneralValue.AutoSize = true;
            lblROIgeneralValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIgeneralValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIgeneralValue.Location = new Point(14, 42);
            lblROIgeneralValue.Name = "lblROIgeneralValue";
            lblROIgeneralValue.Size = new Size(120, 41);
            lblROIgeneralValue.TabIndex = 1;
            lblROIgeneralValue.Text = "RD$ 0.00";
            // 
            // lblROIgeneralDesc
            // 
            lblROIgeneralDesc.AutoSize = true;
            lblROIgeneralDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIgeneralDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIgeneralDesc.Location = new Point(16, 84);
            lblROIgeneralDesc.Name = "lblROIgeneralDesc";
            lblROIgeneralDesc.Size = new Size(180, 19);
            lblROIgeneralDesc.TabIndex = 2;
            lblROIgeneralDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROIrealizado
            // 
            pnlROIrealizado.BackColor = Color.White;
            pnlROIrealizado.BorderStyle = BorderStyle.FixedSingle;
            pnlROIrealizado.Controls.Add(lblROIrealizadoDesc);
            pnlROIrealizado.Controls.Add(lblROIrealizadoValue);
            pnlROIrealizado.Controls.Add(lblROIrealizadoTitle);
            pnlROIrealizado.Location = new Point(16, 142);
            pnlROIrealizado.Name = "pnlROIrealizado";
            pnlROIrealizado.Size = new Size(900, 110);
            pnlROIrealizado.TabIndex = 1;
            pnlROIrealizado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROIrealizadoTitle
            // 
            lblROIrealizadoTitle.AutoSize = true;
            lblROIrealizadoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROIrealizadoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROIrealizadoTitle.Location = new Point(14, 12);
            lblROIrealizadoTitle.Name = "lblROIrealizadoTitle";
            lblROIrealizadoTitle.Size = new Size(120, 23);
            lblROIrealizadoTitle.TabIndex = 0;
            lblROIrealizadoTitle.Text = "ROI realizado";
            // 
            // lblROIrealizadoValue
            // 
            lblROIrealizadoValue.AutoSize = true;
            lblROIrealizadoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIrealizadoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIrealizadoValue.Location = new Point(14, 42);
            lblROIrealizadoValue.Name = "lblROIrealizadoValue";
            lblROIrealizadoValue.Size = new Size(120, 41);
            lblROIrealizadoValue.TabIndex = 1;
            lblROIrealizadoValue.Text = "—";
            // 
            // lblROIrealizadoDesc
            // 
            lblROIrealizadoDesc.AutoSize = true;
            lblROIrealizadoDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIrealizadoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIrealizadoDesc.Location = new Point(16, 84);
            lblROIrealizadoDesc.Name = "lblROIrealizadoDesc";
            lblROIrealizadoDesc.Size = new Size(180, 19);
            lblROIrealizadoDesc.TabIndex = 2;
            lblROIrealizadoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROIproyectado
            // 
            pnlROIproyectado.BackColor = Color.White;
            pnlROIproyectado.BorderStyle = BorderStyle.FixedSingle;
            pnlROIproyectado.Controls.Add(lblROIproyectadoDesc);
            pnlROIproyectado.Controls.Add(lblROIproyectadoValue);
            pnlROIproyectado.Controls.Add(lblROIproyectadoTitle);
            pnlROIproyectado.Location = new Point(16, 268);
            pnlROIproyectado.Name = "pnlROIproyectado";
            pnlROIproyectado.Size = new Size(900, 110);
            pnlROIproyectado.TabIndex = 2;
            pnlROIproyectado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROIproyectadoTitle
            // 
            lblROIproyectadoTitle.AutoSize = true;
            lblROIproyectadoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROIproyectadoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROIproyectadoTitle.Location = new Point(14, 12);
            lblROIproyectadoTitle.Name = "lblROIproyectadoTitle";
            lblROIproyectadoTitle.Size = new Size(120, 23);
            lblROIproyectadoTitle.TabIndex = 0;
            lblROIproyectadoTitle.Text = "ROI proyectado";
            // 
            // lblROIproyectadoValue
            // 
            lblROIproyectadoValue.AutoSize = true;
            lblROIproyectadoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIproyectadoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIproyectadoValue.Location = new Point(14, 42);
            lblROIproyectadoValue.Name = "lblROIproyectadoValue";
            lblROIproyectadoValue.Size = new Size(120, 41);
            lblROIproyectadoValue.TabIndex = 1;
            lblROIproyectadoValue.Text = "0 %";
            // 
            // lblROIproyectadoDesc
            // 
            lblROIproyectadoDesc.AutoSize = true;
            lblROIproyectadoDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIproyectadoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIproyectadoDesc.Location = new Point(16, 84);
            lblROIproyectadoDesc.Name = "lblROIproyectadoDesc";
            lblROIproyectadoDesc.Size = new Size(180, 19);
            lblROIproyectadoDesc.TabIndex = 2;
            lblROIproyectadoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROIporproducto
            // 
            pnlROIporproducto.BackColor = Color.White;
            pnlROIporproducto.BorderStyle = BorderStyle.FixedSingle;
            pnlROIporproducto.Controls.Add(lblROIporproductoDesc);
            pnlROIporproducto.Controls.Add(lblROIporproductoValue);
            pnlROIporproducto.Controls.Add(lblROIporproductoTitle);
            pnlROIporproducto.Location = new Point(16, 394);
            pnlROIporproducto.Name = "pnlROIporproducto";
            pnlROIporproducto.Size = new Size(900, 110);
            pnlROIporproducto.TabIndex = 3;
            pnlROIporproducto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROIporproductoTitle
            // 
            lblROIporproductoTitle.AutoSize = true;
            lblROIporproductoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROIporproductoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROIporproductoTitle.Location = new Point(14, 12);
            lblROIporproductoTitle.Name = "lblROIporproductoTitle";
            lblROIporproductoTitle.Size = new Size(120, 23);
            lblROIporproductoTitle.TabIndex = 0;
            lblROIporproductoTitle.Text = "ROI por producto";
            // 
            // lblROIporproductoValue
            // 
            lblROIporproductoValue.AutoSize = true;
            lblROIporproductoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIporproductoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIporproductoValue.Location = new Point(14, 42);
            lblROIporproductoValue.Name = "lblROIporproductoValue";
            lblROIporproductoValue.Size = new Size(120, 41);
            lblROIporproductoValue.TabIndex = 1;
            lblROIporproductoValue.Text = "RD$ 0.00";
            // 
            // lblROIporproductoDesc
            // 
            lblROIporproductoDesc.AutoSize = true;
            lblROIporproductoDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIporproductoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIporproductoDesc.Location = new Point(16, 84);
            lblROIporproductoDesc.Name = "lblROIporproductoDesc";
            lblROIporproductoDesc.Size = new Size(180, 19);
            lblROIporproductoDesc.TabIndex = 2;
            lblROIporproductoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROIporinversion
            // 
            pnlROIporinversion.BackColor = Color.White;
            pnlROIporinversion.BorderStyle = BorderStyle.FixedSingle;
            pnlROIporinversion.Controls.Add(lblROIporinversionDesc);
            pnlROIporinversion.Controls.Add(lblROIporinversionValue);
            pnlROIporinversion.Controls.Add(lblROIporinversionTitle);
            pnlROIporinversion.Location = new Point(16, 520);
            pnlROIporinversion.Name = "pnlROIporinversion";
            pnlROIporinversion.Size = new Size(900, 110);
            pnlROIporinversion.TabIndex = 4;
            pnlROIporinversion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROIporinversionTitle
            // 
            lblROIporinversionTitle.AutoSize = true;
            lblROIporinversionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROIporinversionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROIporinversionTitle.Location = new Point(14, 12);
            lblROIporinversionTitle.Name = "lblROIporinversionTitle";
            lblROIporinversionTitle.Size = new Size(120, 23);
            lblROIporinversionTitle.TabIndex = 0;
            lblROIporinversionTitle.Text = "ROI por inversion";
            // 
            // lblROIporinversionValue
            // 
            lblROIporinversionValue.AutoSize = true;
            lblROIporinversionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIporinversionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIporinversionValue.Location = new Point(14, 42);
            lblROIporinversionValue.Name = "lblROIporinversionValue";
            lblROIporinversionValue.Size = new Size(120, 41);
            lblROIporinversionValue.TabIndex = 1;
            lblROIporinversionValue.Text = "—";
            // 
            // lblROIporinversionDesc
            // 
            lblROIporinversionDesc.AutoSize = true;
            lblROIporinversionDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIporinversionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIporinversionDesc.Location = new Point(16, 84);
            lblROIporinversionDesc.Name = "lblROIporinversionDesc";
            lblROIporinversionDesc.Size = new Size(180, 19);
            lblROIporinversionDesc.TabIndex = 2;
            lblROIporinversionDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGrafico
            // 
            pnlGrafico.BackColor = Color.White;
            pnlGrafico.BorderStyle = BorderStyle.FixedSingle;
            pnlGrafico.Controls.Add(lblGraficoDesc);
            pnlGrafico.Controls.Add(lblGraficoValue);
            pnlGrafico.Controls.Add(lblGraficoTitle);
            pnlGrafico.Location = new Point(16, 646);
            pnlGrafico.Name = "pnlGrafico";
            pnlGrafico.Size = new Size(900, 110);
            pnlGrafico.TabIndex = 5;
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
            lblGraficoValue.Text = "0 %";
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
            // FrmAnaRoi
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaRoi";
            Text = "ROI";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlROIgeneral.ResumeLayout(false);
            pnlROIgeneral.PerformLayout();
            pnlROIrealizado.ResumeLayout(false);
            pnlROIrealizado.PerformLayout();
            pnlROIproyectado.ResumeLayout(false);
            pnlROIproyectado.PerformLayout();
            pnlROIporproducto.ResumeLayout(false);
            pnlROIporproducto.PerformLayout();
            pnlROIporinversion.ResumeLayout(false);
            pnlROIporinversion.PerformLayout();
            pnlGrafico.ResumeLayout(false);
            pnlGrafico.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Label lblCrmHint;
        private Panel panelScroll;
        private Panel pnlROIgeneral;
        private Label lblROIgeneralTitle;
        private Label lblROIgeneralValue;
        private Label lblROIgeneralDesc;
        private Panel pnlROIrealizado;
        private Label lblROIrealizadoTitle;
        private Label lblROIrealizadoValue;
        private Label lblROIrealizadoDesc;
        private Panel pnlROIproyectado;
        private Label lblROIproyectadoTitle;
        private Label lblROIproyectadoValue;
        private Label lblROIproyectadoDesc;
        private Panel pnlROIporproducto;
        private Label lblROIporproductoTitle;
        private Label lblROIporproductoValue;
        private Label lblROIporproductoDesc;
        private Panel pnlROIporinversion;
        private Label lblROIporinversionTitle;
        private Label lblROIporinversionValue;
        private Label lblROIporinversionDesc;
        private Panel pnlGrafico;
        private Label lblGraficoTitle;
        private Label lblGraficoValue;
        private Label lblGraficoDesc;
    }
}
