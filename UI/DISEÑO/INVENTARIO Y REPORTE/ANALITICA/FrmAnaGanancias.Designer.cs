namespace UI
{
    partial class FrmAnaGanancias
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
            pnlGananciarealizada = new Panel();
            lblGananciarealizadaTitle = new Label();
            lblGananciarealizadaValue = new Label();
            lblGananciarealizadaDesc = new Label();
            pnlGananciapotencial = new Panel();
            lblGananciapotencialTitle = new Label();
            lblGananciapotencialValue = new Label();
            lblGananciapotencialDesc = new Label();
            pnlGananciaporproducto = new Panel();
            lblGananciaporproductoTitle = new Label();
            lblGananciaporproductoValue = new Label();
            lblGananciaporproductoDesc = new Label();
            pnlGananciaporinversion = new Panel();
            lblGananciaporinversionTitle = new Label();
            lblGananciaporinversionValue = new Label();
            lblGananciaporinversionDesc = new Label();
            pnlGananciaporcategoria = new Panel();
            lblGananciaporcategoriaTitle = new Label();
            lblGananciaporcategoriaValue = new Label();
            lblGananciaporcategoriaDesc = new Label();
            pnlGrafico = new Panel();
            lblGraficoTitle = new Label();
            lblGraficoValue = new Label();
            lblGraficoDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlGananciarealizada.SuspendLayout();
            pnlGananciapotencial.SuspendLayout();
            pnlGananciaporproducto.SuspendLayout();
            pnlGananciaporinversion.SuspendLayout();
            pnlGananciaporcategoria.SuspendLayout();
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
            lblHeaderLocal.Text = "Ganancias";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlGananciarealizada);
            panelScroll.Controls.Add(pnlGananciapotencial);
            panelScroll.Controls.Add(pnlGananciaporproducto);
            panelScroll.Controls.Add(pnlGananciaporinversion);
            panelScroll.Controls.Add(pnlGananciaporcategoria);
            panelScroll.Controls.Add(pnlGrafico);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 64);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlGananciarealizada
            // 
            pnlGananciarealizada.BackColor = Color.White;
            pnlGananciarealizada.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciarealizada.Controls.Add(lblGananciarealizadaDesc);
            pnlGananciarealizada.Controls.Add(lblGananciarealizadaValue);
            pnlGananciarealizada.Controls.Add(lblGananciarealizadaTitle);
            pnlGananciarealizada.Location = new Point(16, 16);
            pnlGananciarealizada.Name = "pnlGananciarealizada";
            pnlGananciarealizada.Size = new Size(900, 110);
            pnlGananciarealizada.TabIndex = 0;
            pnlGananciarealizada.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciarealizadaTitle
            // 
            lblGananciarealizadaTitle.AutoSize = true;
            lblGananciarealizadaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciarealizadaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciarealizadaTitle.Location = new Point(14, 12);
            lblGananciarealizadaTitle.Name = "lblGananciarealizadaTitle";
            lblGananciarealizadaTitle.Size = new Size(120, 23);
            lblGananciarealizadaTitle.TabIndex = 0;
            lblGananciarealizadaTitle.Text = "Ganancia realizada";
            // 
            // lblGananciarealizadaValue
            // 
            lblGananciarealizadaValue.AutoSize = true;
            lblGananciarealizadaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciarealizadaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciarealizadaValue.Location = new Point(14, 42);
            lblGananciarealizadaValue.Name = "lblGananciarealizadaValue";
            lblGananciarealizadaValue.Size = new Size(120, 41);
            lblGananciarealizadaValue.TabIndex = 1;
            lblGananciarealizadaValue.Text = "RD$ 0.00";
            // 
            // lblGananciarealizadaDesc
            // 
            lblGananciarealizadaDesc.AutoSize = true;
            lblGananciarealizadaDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciarealizadaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciarealizadaDesc.Location = new Point(16, 84);
            lblGananciarealizadaDesc.Name = "lblGananciarealizadaDesc";
            lblGananciarealizadaDesc.Size = new Size(180, 19);
            lblGananciarealizadaDesc.TabIndex = 2;
            lblGananciarealizadaDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGananciapotencial
            // 
            pnlGananciapotencial.BackColor = Color.White;
            pnlGananciapotencial.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciapotencial.Controls.Add(lblGananciapotencialDesc);
            pnlGananciapotencial.Controls.Add(lblGananciapotencialValue);
            pnlGananciapotencial.Controls.Add(lblGananciapotencialTitle);
            pnlGananciapotencial.Location = new Point(16, 142);
            pnlGananciapotencial.Name = "pnlGananciapotencial";
            pnlGananciapotencial.Size = new Size(900, 110);
            pnlGananciapotencial.TabIndex = 1;
            pnlGananciapotencial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciapotencialTitle
            // 
            lblGananciapotencialTitle.AutoSize = true;
            lblGananciapotencialTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciapotencialTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciapotencialTitle.Location = new Point(14, 12);
            lblGananciapotencialTitle.Name = "lblGananciapotencialTitle";
            lblGananciapotencialTitle.Size = new Size(120, 23);
            lblGananciapotencialTitle.TabIndex = 0;
            lblGananciapotencialTitle.Text = "Ganancia potencial";
            // 
            // lblGananciapotencialValue
            // 
            lblGananciapotencialValue.AutoSize = true;
            lblGananciapotencialValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciapotencialValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciapotencialValue.Location = new Point(14, 42);
            lblGananciapotencialValue.Name = "lblGananciapotencialValue";
            lblGananciapotencialValue.Size = new Size(120, 41);
            lblGananciapotencialValue.TabIndex = 1;
            lblGananciapotencialValue.Text = "—";
            // 
            // lblGananciapotencialDesc
            // 
            lblGananciapotencialDesc.AutoSize = true;
            lblGananciapotencialDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciapotencialDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciapotencialDesc.Location = new Point(16, 84);
            lblGananciapotencialDesc.Name = "lblGananciapotencialDesc";
            lblGananciapotencialDesc.Size = new Size(180, 19);
            lblGananciapotencialDesc.TabIndex = 2;
            lblGananciapotencialDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGananciaporproducto
            // 
            pnlGananciaporproducto.BackColor = Color.White;
            pnlGananciaporproducto.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciaporproducto.Controls.Add(lblGananciaporproductoDesc);
            pnlGananciaporproducto.Controls.Add(lblGananciaporproductoValue);
            pnlGananciaporproducto.Controls.Add(lblGananciaporproductoTitle);
            pnlGananciaporproducto.Location = new Point(16, 268);
            pnlGananciaporproducto.Name = "pnlGananciaporproducto";
            pnlGananciaporproducto.Size = new Size(900, 110);
            pnlGananciaporproducto.TabIndex = 2;
            pnlGananciaporproducto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciaporproductoTitle
            // 
            lblGananciaporproductoTitle.AutoSize = true;
            lblGananciaporproductoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciaporproductoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciaporproductoTitle.Location = new Point(14, 12);
            lblGananciaporproductoTitle.Name = "lblGananciaporproductoTitle";
            lblGananciaporproductoTitle.Size = new Size(120, 23);
            lblGananciaporproductoTitle.TabIndex = 0;
            lblGananciaporproductoTitle.Text = "Ganancia por producto";
            // 
            // lblGananciaporproductoValue
            // 
            lblGananciaporproductoValue.AutoSize = true;
            lblGananciaporproductoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciaporproductoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciaporproductoValue.Location = new Point(14, 42);
            lblGananciaporproductoValue.Name = "lblGananciaporproductoValue";
            lblGananciaporproductoValue.Size = new Size(120, 41);
            lblGananciaporproductoValue.TabIndex = 1;
            lblGananciaporproductoValue.Text = "0 %";
            // 
            // lblGananciaporproductoDesc
            // 
            lblGananciaporproductoDesc.AutoSize = true;
            lblGananciaporproductoDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciaporproductoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciaporproductoDesc.Location = new Point(16, 84);
            lblGananciaporproductoDesc.Name = "lblGananciaporproductoDesc";
            lblGananciaporproductoDesc.Size = new Size(180, 19);
            lblGananciaporproductoDesc.TabIndex = 2;
            lblGananciaporproductoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGananciaporinversion
            // 
            pnlGananciaporinversion.BackColor = Color.White;
            pnlGananciaporinversion.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciaporinversion.Controls.Add(lblGananciaporinversionDesc);
            pnlGananciaporinversion.Controls.Add(lblGananciaporinversionValue);
            pnlGananciaporinversion.Controls.Add(lblGananciaporinversionTitle);
            pnlGananciaporinversion.Location = new Point(16, 394);
            pnlGananciaporinversion.Name = "pnlGananciaporinversion";
            pnlGananciaporinversion.Size = new Size(900, 110);
            pnlGananciaporinversion.TabIndex = 3;
            pnlGananciaporinversion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciaporinversionTitle
            // 
            lblGananciaporinversionTitle.AutoSize = true;
            lblGananciaporinversionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciaporinversionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciaporinversionTitle.Location = new Point(14, 12);
            lblGananciaporinversionTitle.Name = "lblGananciaporinversionTitle";
            lblGananciaporinversionTitle.Size = new Size(120, 23);
            lblGananciaporinversionTitle.TabIndex = 0;
            lblGananciaporinversionTitle.Text = "Ganancia por inversion";
            // 
            // lblGananciaporinversionValue
            // 
            lblGananciaporinversionValue.AutoSize = true;
            lblGananciaporinversionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciaporinversionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciaporinversionValue.Location = new Point(14, 42);
            lblGananciaporinversionValue.Name = "lblGananciaporinversionValue";
            lblGananciaporinversionValue.Size = new Size(120, 41);
            lblGananciaporinversionValue.TabIndex = 1;
            lblGananciaporinversionValue.Text = "RD$ 0.00";
            // 
            // lblGananciaporinversionDesc
            // 
            lblGananciaporinversionDesc.AutoSize = true;
            lblGananciaporinversionDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciaporinversionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciaporinversionDesc.Location = new Point(16, 84);
            lblGananciaporinversionDesc.Name = "lblGananciaporinversionDesc";
            lblGananciaporinversionDesc.Size = new Size(180, 19);
            lblGananciaporinversionDesc.TabIndex = 2;
            lblGananciaporinversionDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGananciaporcategoria
            // 
            pnlGananciaporcategoria.BackColor = Color.White;
            pnlGananciaporcategoria.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciaporcategoria.Controls.Add(lblGananciaporcategoriaDesc);
            pnlGananciaporcategoria.Controls.Add(lblGananciaporcategoriaValue);
            pnlGananciaporcategoria.Controls.Add(lblGananciaporcategoriaTitle);
            pnlGananciaporcategoria.Location = new Point(16, 520);
            pnlGananciaporcategoria.Name = "pnlGananciaporcategoria";
            pnlGananciaporcategoria.Size = new Size(900, 110);
            pnlGananciaporcategoria.TabIndex = 4;
            pnlGananciaporcategoria.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciaporcategoriaTitle
            // 
            lblGananciaporcategoriaTitle.AutoSize = true;
            lblGananciaporcategoriaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciaporcategoriaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciaporcategoriaTitle.Location = new Point(14, 12);
            lblGananciaporcategoriaTitle.Name = "lblGananciaporcategoriaTitle";
            lblGananciaporcategoriaTitle.Size = new Size(120, 23);
            lblGananciaporcategoriaTitle.TabIndex = 0;
            lblGananciaporcategoriaTitle.Text = "Ganancia por categoria";
            // 
            // lblGananciaporcategoriaValue
            // 
            lblGananciaporcategoriaValue.AutoSize = true;
            lblGananciaporcategoriaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciaporcategoriaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciaporcategoriaValue.Location = new Point(14, 42);
            lblGananciaporcategoriaValue.Name = "lblGananciaporcategoriaValue";
            lblGananciaporcategoriaValue.Size = new Size(120, 41);
            lblGananciaporcategoriaValue.TabIndex = 1;
            lblGananciaporcategoriaValue.Text = "—";
            // 
            // lblGananciaporcategoriaDesc
            // 
            lblGananciaporcategoriaDesc.AutoSize = true;
            lblGananciaporcategoriaDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciaporcategoriaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciaporcategoriaDesc.Location = new Point(16, 84);
            lblGananciaporcategoriaDesc.Name = "lblGananciaporcategoriaDesc";
            lblGananciaporcategoriaDesc.Size = new Size(180, 19);
            lblGananciaporcategoriaDesc.TabIndex = 2;
            lblGananciaporcategoriaDesc.Text = "Dato visual mock — sin logica";
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
            // FrmAnaGanancias
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaGanancias";
            Text = "Ganancias";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlGananciarealizada.ResumeLayout(false);
            pnlGananciarealizada.PerformLayout();
            pnlGananciapotencial.ResumeLayout(false);
            pnlGananciapotencial.PerformLayout();
            pnlGananciaporproducto.ResumeLayout(false);
            pnlGananciaporproducto.PerformLayout();
            pnlGananciaporinversion.ResumeLayout(false);
            pnlGananciaporinversion.PerformLayout();
            pnlGananciaporcategoria.ResumeLayout(false);
            pnlGananciaporcategoria.PerformLayout();
            pnlGrafico.ResumeLayout(false);
            pnlGrafico.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Label lblCrmHint;
        private Panel panelScroll;
        private Panel pnlGananciarealizada;
        private Label lblGananciarealizadaTitle;
        private Label lblGananciarealizadaValue;
        private Label lblGananciarealizadaDesc;
        private Panel pnlGananciapotencial;
        private Label lblGananciapotencialTitle;
        private Label lblGananciapotencialValue;
        private Label lblGananciapotencialDesc;
        private Panel pnlGananciaporproducto;
        private Label lblGananciaporproductoTitle;
        private Label lblGananciaporproductoValue;
        private Label lblGananciaporproductoDesc;
        private Panel pnlGananciaporinversion;
        private Label lblGananciaporinversionTitle;
        private Label lblGananciaporinversionValue;
        private Label lblGananciaporinversionDesc;
        private Panel pnlGananciaporcategoria;
        private Label lblGananciaporcategoriaTitle;
        private Label lblGananciaporcategoriaValue;
        private Label lblGananciaporcategoriaDesc;
        private Panel pnlGrafico;
        private Label lblGraficoTitle;
        private Label lblGraficoValue;
        private Label lblGraficoDesc;
    }
}
