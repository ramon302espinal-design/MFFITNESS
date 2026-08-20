namespace UI
{
    partial class FrmAnaDecisiones
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
            pnlOportunidades = new Panel();
            lblOportunidadesTitle = new Label();
            lblOportunidadesValue = new Label();
            lblOportunidadesDesc = new Label();
            pnlReposicion = new Panel();
            lblReposicionTitle = new Label();
            lblReposicionValue = new Label();
            lblReposicionDesc = new Label();
            pnlInversiones = new Panel();
            lblInversionesTitle = new Label();
            lblInversionesValue = new Label();
            lblInversionesDesc = new Label();
            pnlPrecios = new Panel();
            lblPreciosTitle = new Label();
            lblPreciosValue = new Label();
            lblPreciosDesc = new Label();
            pnlCapitalcongelado = new Panel();
            lblCapitalcongeladoTitle = new Label();
            lblCapitalcongeladoValue = new Label();
            lblCapitalcongeladoDesc = new Label();
            pnlRiesgos = new Panel();
            lblRiesgosTitle = new Label();
            lblRiesgosValue = new Label();
            lblRiesgosDesc = new Label();
            pnlDecisiones = new Panel();
            lblDecisionesTitle = new Label();
            lblDecisionesValue = new Label();
            lblDecisionesDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlOportunidades.SuspendLayout();
            pnlReposicion.SuspendLayout();
            pnlInversiones.SuspendLayout();
            pnlPrecios.SuspendLayout();
            pnlCapitalcongelado.SuspendLayout();
            pnlRiesgos.SuspendLayout();
            pnlDecisiones.SuspendLayout();
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
            lblHeaderLocal.Text = "Centro de decisiones";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlOportunidades);
            panelScroll.Controls.Add(pnlReposicion);
            panelScroll.Controls.Add(pnlInversiones);
            panelScroll.Controls.Add(pnlPrecios);
            panelScroll.Controls.Add(pnlCapitalcongelado);
            panelScroll.Controls.Add(pnlRiesgos);
            panelScroll.Controls.Add(pnlDecisiones);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlOportunidades
            // 
            pnlOportunidades.BackColor = Color.White;
            pnlOportunidades.BorderStyle = BorderStyle.FixedSingle;
            pnlOportunidades.Controls.Add(lblOportunidadesDesc);
            pnlOportunidades.Controls.Add(lblOportunidadesValue);
            pnlOportunidades.Controls.Add(lblOportunidadesTitle);
            pnlOportunidades.Location = new Point(16, 16);
            pnlOportunidades.Name = "pnlOportunidades";
            pnlOportunidades.Size = new Size(900, 110);
            pnlOportunidades.TabIndex = 0;
            pnlOportunidades.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblOportunidadesTitle
            // 
            lblOportunidadesTitle.AutoSize = true;
            lblOportunidadesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblOportunidadesTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblOportunidadesTitle.Location = new Point(14, 12);
            lblOportunidadesTitle.Name = "lblOportunidadesTitle";
            lblOportunidadesTitle.Size = new Size(120, 23);
            lblOportunidadesTitle.TabIndex = 0;
            lblOportunidadesTitle.Text = "Oportunidades";
            // 
            // lblOportunidadesValue
            // 
            lblOportunidadesValue.AutoSize = true;
            lblOportunidadesValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblOportunidadesValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblOportunidadesValue.Location = new Point(14, 42);
            lblOportunidadesValue.Name = "lblOportunidadesValue";
            lblOportunidadesValue.Size = new Size(120, 41);
            lblOportunidadesValue.TabIndex = 1;
            lblOportunidadesValue.Text = "RD$ 0.00";
            // 
            // lblOportunidadesDesc
            // 
            lblOportunidadesDesc.AutoSize = true;
            lblOportunidadesDesc.Font = new Font("Segoe UI", 8.5F);
            lblOportunidadesDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblOportunidadesDesc.Location = new Point(16, 84);
            lblOportunidadesDesc.Name = "lblOportunidadesDesc";
            lblOportunidadesDesc.Size = new Size(180, 19);
            lblOportunidadesDesc.TabIndex = 2;
            lblOportunidadesDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlReposicion
            // 
            pnlReposicion.BackColor = Color.White;
            pnlReposicion.BorderStyle = BorderStyle.FixedSingle;
            pnlReposicion.Controls.Add(lblReposicionDesc);
            pnlReposicion.Controls.Add(lblReposicionValue);
            pnlReposicion.Controls.Add(lblReposicionTitle);
            pnlReposicion.Location = new Point(16, 142);
            pnlReposicion.Name = "pnlReposicion";
            pnlReposicion.Size = new Size(900, 110);
            pnlReposicion.TabIndex = 1;
            pnlReposicion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblReposicionTitle
            // 
            lblReposicionTitle.AutoSize = true;
            lblReposicionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblReposicionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblReposicionTitle.Location = new Point(14, 12);
            lblReposicionTitle.Name = "lblReposicionTitle";
            lblReposicionTitle.Size = new Size(120, 23);
            lblReposicionTitle.TabIndex = 0;
            lblReposicionTitle.Text = "Reposicion";
            // 
            // lblReposicionValue
            // 
            lblReposicionValue.AutoSize = true;
            lblReposicionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblReposicionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblReposicionValue.Location = new Point(14, 42);
            lblReposicionValue.Name = "lblReposicionValue";
            lblReposicionValue.Size = new Size(120, 41);
            lblReposicionValue.TabIndex = 1;
            lblReposicionValue.Text = "—";
            // 
            // lblReposicionDesc
            // 
            lblReposicionDesc.AutoSize = true;
            lblReposicionDesc.Font = new Font("Segoe UI", 8.5F);
            lblReposicionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblReposicionDesc.Location = new Point(16, 84);
            lblReposicionDesc.Name = "lblReposicionDesc";
            lblReposicionDesc.Size = new Size(180, 19);
            lblReposicionDesc.TabIndex = 2;
            lblReposicionDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlInversiones
            // 
            pnlInversiones.BackColor = Color.White;
            pnlInversiones.BorderStyle = BorderStyle.FixedSingle;
            pnlInversiones.Controls.Add(lblInversionesDesc);
            pnlInversiones.Controls.Add(lblInversionesValue);
            pnlInversiones.Controls.Add(lblInversionesTitle);
            pnlInversiones.Location = new Point(16, 268);
            pnlInversiones.Name = "pnlInversiones";
            pnlInversiones.Size = new Size(900, 110);
            pnlInversiones.TabIndex = 2;
            pnlInversiones.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblInversionesTitle
            // 
            lblInversionesTitle.AutoSize = true;
            lblInversionesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInversionesTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblInversionesTitle.Location = new Point(14, 12);
            lblInversionesTitle.Name = "lblInversionesTitle";
            lblInversionesTitle.Size = new Size(120, 23);
            lblInversionesTitle.TabIndex = 0;
            lblInversionesTitle.Text = "Inversiones";
            // 
            // lblInversionesValue
            // 
            lblInversionesValue.AutoSize = true;
            lblInversionesValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblInversionesValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblInversionesValue.Location = new Point(14, 42);
            lblInversionesValue.Name = "lblInversionesValue";
            lblInversionesValue.Size = new Size(120, 41);
            lblInversionesValue.TabIndex = 1;
            lblInversionesValue.Text = "0 %";
            // 
            // lblInversionesDesc
            // 
            lblInversionesDesc.AutoSize = true;
            lblInversionesDesc.Font = new Font("Segoe UI", 8.5F);
            lblInversionesDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblInversionesDesc.Location = new Point(16, 84);
            lblInversionesDesc.Name = "lblInversionesDesc";
            lblInversionesDesc.Size = new Size(180, 19);
            lblInversionesDesc.TabIndex = 2;
            lblInversionesDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlPrecios
            // 
            pnlPrecios.BackColor = Color.White;
            pnlPrecios.BorderStyle = BorderStyle.FixedSingle;
            pnlPrecios.Controls.Add(lblPreciosDesc);
            pnlPrecios.Controls.Add(lblPreciosValue);
            pnlPrecios.Controls.Add(lblPreciosTitle);
            pnlPrecios.Location = new Point(16, 394);
            pnlPrecios.Name = "pnlPrecios";
            pnlPrecios.Size = new Size(900, 110);
            pnlPrecios.TabIndex = 3;
            pnlPrecios.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblPreciosTitle
            // 
            lblPreciosTitle.AutoSize = true;
            lblPreciosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPreciosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblPreciosTitle.Location = new Point(14, 12);
            lblPreciosTitle.Name = "lblPreciosTitle";
            lblPreciosTitle.Size = new Size(120, 23);
            lblPreciosTitle.TabIndex = 0;
            lblPreciosTitle.Text = "Precios";
            // 
            // lblPreciosValue
            // 
            lblPreciosValue.AutoSize = true;
            lblPreciosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblPreciosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblPreciosValue.Location = new Point(14, 42);
            lblPreciosValue.Name = "lblPreciosValue";
            lblPreciosValue.Size = new Size(120, 41);
            lblPreciosValue.TabIndex = 1;
            lblPreciosValue.Text = "RD$ 0.00";
            // 
            // lblPreciosDesc
            // 
            lblPreciosDesc.AutoSize = true;
            lblPreciosDesc.Font = new Font("Segoe UI", 8.5F);
            lblPreciosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblPreciosDesc.Location = new Point(16, 84);
            lblPreciosDesc.Name = "lblPreciosDesc";
            lblPreciosDesc.Size = new Size(180, 19);
            lblPreciosDesc.TabIndex = 2;
            lblPreciosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlCapitalcongelado
            // 
            pnlCapitalcongelado.BackColor = Color.White;
            pnlCapitalcongelado.BorderStyle = BorderStyle.FixedSingle;
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoDesc);
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoValue);
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoTitle);
            pnlCapitalcongelado.Location = new Point(16, 520);
            pnlCapitalcongelado.Name = "pnlCapitalcongelado";
            pnlCapitalcongelado.Size = new Size(900, 110);
            pnlCapitalcongelado.TabIndex = 4;
            pnlCapitalcongelado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCapitalcongeladoTitle
            // 
            lblCapitalcongeladoTitle.AutoSize = true;
            lblCapitalcongeladoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCapitalcongeladoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCapitalcongeladoTitle.Location = new Point(14, 12);
            lblCapitalcongeladoTitle.Name = "lblCapitalcongeladoTitle";
            lblCapitalcongeladoTitle.Size = new Size(120, 23);
            lblCapitalcongeladoTitle.TabIndex = 0;
            lblCapitalcongeladoTitle.Text = "Capital congelado";
            // 
            // lblCapitalcongeladoValue
            // 
            lblCapitalcongeladoValue.AutoSize = true;
            lblCapitalcongeladoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCapitalcongeladoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCapitalcongeladoValue.Location = new Point(14, 42);
            lblCapitalcongeladoValue.Name = "lblCapitalcongeladoValue";
            lblCapitalcongeladoValue.Size = new Size(120, 41);
            lblCapitalcongeladoValue.TabIndex = 1;
            lblCapitalcongeladoValue.Text = "—";
            // 
            // lblCapitalcongeladoDesc
            // 
            lblCapitalcongeladoDesc.AutoSize = true;
            lblCapitalcongeladoDesc.Font = new Font("Segoe UI", 8.5F);
            lblCapitalcongeladoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCapitalcongeladoDesc.Location = new Point(16, 84);
            lblCapitalcongeladoDesc.Name = "lblCapitalcongeladoDesc";
            lblCapitalcongeladoDesc.Size = new Size(180, 19);
            lblCapitalcongeladoDesc.TabIndex = 2;
            lblCapitalcongeladoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlRiesgos
            // 
            pnlRiesgos.BackColor = Color.White;
            pnlRiesgos.BorderStyle = BorderStyle.FixedSingle;
            pnlRiesgos.Controls.Add(lblRiesgosDesc);
            pnlRiesgos.Controls.Add(lblRiesgosValue);
            pnlRiesgos.Controls.Add(lblRiesgosTitle);
            pnlRiesgos.Location = new Point(16, 646);
            pnlRiesgos.Name = "pnlRiesgos";
            pnlRiesgos.Size = new Size(900, 110);
            pnlRiesgos.TabIndex = 5;
            pnlRiesgos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblRiesgosTitle
            // 
            lblRiesgosTitle.AutoSize = true;
            lblRiesgosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRiesgosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblRiesgosTitle.Location = new Point(14, 12);
            lblRiesgosTitle.Name = "lblRiesgosTitle";
            lblRiesgosTitle.Size = new Size(120, 23);
            lblRiesgosTitle.TabIndex = 0;
            lblRiesgosTitle.Text = "Riesgos";
            // 
            // lblRiesgosValue
            // 
            lblRiesgosValue.AutoSize = true;
            lblRiesgosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblRiesgosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblRiesgosValue.Location = new Point(14, 42);
            lblRiesgosValue.Name = "lblRiesgosValue";
            lblRiesgosValue.Size = new Size(120, 41);
            lblRiesgosValue.TabIndex = 1;
            lblRiesgosValue.Text = "0 %";
            // 
            // lblRiesgosDesc
            // 
            lblRiesgosDesc.AutoSize = true;
            lblRiesgosDesc.Font = new Font("Segoe UI", 8.5F);
            lblRiesgosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblRiesgosDesc.Location = new Point(16, 84);
            lblRiesgosDesc.Name = "lblRiesgosDesc";
            lblRiesgosDesc.Size = new Size(180, 19);
            lblRiesgosDesc.TabIndex = 2;
            lblRiesgosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlDecisiones
            // 
            pnlDecisiones.BackColor = Color.White;
            pnlDecisiones.BorderStyle = BorderStyle.FixedSingle;
            pnlDecisiones.Controls.Add(lblDecisionesDesc);
            pnlDecisiones.Controls.Add(lblDecisionesValue);
            pnlDecisiones.Controls.Add(lblDecisionesTitle);
            pnlDecisiones.Location = new Point(16, 772);
            pnlDecisiones.Name = "pnlDecisiones";
            pnlDecisiones.Size = new Size(900, 110);
            pnlDecisiones.TabIndex = 6;
            pnlDecisiones.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblDecisionesTitle
            // 
            lblDecisionesTitle.AutoSize = true;
            lblDecisionesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDecisionesTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblDecisionesTitle.Location = new Point(14, 12);
            lblDecisionesTitle.Name = "lblDecisionesTitle";
            lblDecisionesTitle.Size = new Size(120, 23);
            lblDecisionesTitle.TabIndex = 0;
            lblDecisionesTitle.Text = "Decisiones";
            // 
            // lblDecisionesValue
            // 
            lblDecisionesValue.AutoSize = true;
            lblDecisionesValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDecisionesValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblDecisionesValue.Location = new Point(14, 42);
            lblDecisionesValue.Name = "lblDecisionesValue";
            lblDecisionesValue.Size = new Size(120, 41);
            lblDecisionesValue.TabIndex = 1;
            lblDecisionesValue.Text = "RD$ 0.00";
            // 
            // lblDecisionesDesc
            // 
            lblDecisionesDesc.AutoSize = true;
            lblDecisionesDesc.Font = new Font("Segoe UI", 8.5F);
            lblDecisionesDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblDecisionesDesc.Location = new Point(16, 84);
            lblDecisionesDesc.Name = "lblDecisionesDesc";
            lblDecisionesDesc.Size = new Size(180, 19);
            lblDecisionesDesc.TabIndex = 2;
            lblDecisionesDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaDecisiones
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaDecisiones";
            Text = "Centro de decisiones";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlOportunidades.ResumeLayout(false);
            pnlOportunidades.PerformLayout();
            pnlReposicion.ResumeLayout(false);
            pnlReposicion.PerformLayout();
            pnlInversiones.ResumeLayout(false);
            pnlInversiones.PerformLayout();
            pnlPrecios.ResumeLayout(false);
            pnlPrecios.PerformLayout();
            pnlCapitalcongelado.ResumeLayout(false);
            pnlCapitalcongelado.PerformLayout();
            pnlRiesgos.ResumeLayout(false);
            pnlRiesgos.PerformLayout();
            pnlDecisiones.ResumeLayout(false);
            pnlDecisiones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlOportunidades;
        private Label lblOportunidadesTitle;
        private Label lblOportunidadesValue;
        private Label lblOportunidadesDesc;
        private Panel pnlReposicion;
        private Label lblReposicionTitle;
        private Label lblReposicionValue;
        private Label lblReposicionDesc;
        private Panel pnlInversiones;
        private Label lblInversionesTitle;
        private Label lblInversionesValue;
        private Label lblInversionesDesc;
        private Panel pnlPrecios;
        private Label lblPreciosTitle;
        private Label lblPreciosValue;
        private Label lblPreciosDesc;
        private Panel pnlCapitalcongelado;
        private Label lblCapitalcongeladoTitle;
        private Label lblCapitalcongeladoValue;
        private Label lblCapitalcongeladoDesc;
        private Panel pnlRiesgos;
        private Label lblRiesgosTitle;
        private Label lblRiesgosValue;
        private Label lblRiesgosDesc;
        private Panel pnlDecisiones;
        private Label lblDecisionesTitle;
        private Label lblDecisionesValue;
        private Label lblDecisionesDesc;
    }
}
