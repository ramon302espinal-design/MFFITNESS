namespace UI.DISEÑO.ANALITICA.Controles
{
    partial class UcCrmSectionPanel
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlRoot = new Panel();
            pnlSectionHeader = new Panel();
            flpSectionActions = new FlowLayoutPanel();
            lblSectionTitle = new Label();
            pnlSectionBody = new Panel();
            pnlRoot.SuspendLayout();
            pnlSectionHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlRoot
            // 
            pnlRoot.BackColor = Color.FromArgb(226, 232, 240);
            pnlRoot.Controls.Add(pnlSectionBody);
            pnlRoot.Controls.Add(pnlSectionHeader);
            pnlRoot.Dock = DockStyle.Fill;
            pnlRoot.Location = new Point(0, 0);
            pnlRoot.Name = "pnlRoot";
            pnlRoot.Padding = new Padding(1);
            pnlRoot.Size = new Size(900, 140);
            pnlRoot.TabIndex = 0;
            // 
            // pnlSectionHeader
            // 
            pnlSectionHeader.BackColor = Color.White;
            pnlSectionHeader.Controls.Add(flpSectionActions);
            pnlSectionHeader.Controls.Add(lblSectionTitle);
            pnlSectionHeader.Dock = DockStyle.Top;
            pnlSectionHeader.Location = new Point(1, 1);
            pnlSectionHeader.Name = "pnlSectionHeader";
            pnlSectionHeader.Padding = new Padding(12, 0, 8, 0);
            pnlSectionHeader.Size = new Size(898, 44);
            pnlSectionHeader.TabIndex = 0;
            // 
            // flpSectionActions
            // 
            flpSectionActions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flpSectionActions.FlowDirection = FlowDirection.RightToLeft;
            flpSectionActions.Location = new Point(598, 6);
            flpSectionActions.Name = "flpSectionActions";
            flpSectionActions.Size = new Size(288, 32);
            flpSectionActions.TabIndex = 1;
            flpSectionActions.WrapContents = false;
            flpSectionActions.Visible = false;
            // 
            // lblSectionTitle
            // 
            lblSectionTitle.AutoSize = true;
            lblSectionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSectionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblSectionTitle.Location = new Point(12, 11);
            lblSectionTitle.Name = "lblSectionTitle";
            lblSectionTitle.Size = new Size(70, 23);
            lblSectionTitle.TabIndex = 0;
            lblSectionTitle.Text = "Sección";
            // 
            // pnlSectionBody
            // 
            pnlSectionBody.BackColor = Color.White;
            pnlSectionBody.Dock = DockStyle.Fill;
            pnlSectionBody.Location = new Point(1, 45);
            pnlSectionBody.Name = "pnlSectionBody";
            pnlSectionBody.Padding = new Padding(12, 8, 12, 12);
            pnlSectionBody.Size = new Size(898, 94);
            pnlSectionBody.TabIndex = 1;
            // 
            // UcCrmSectionPanel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(pnlRoot);
            MinimumSize = new Size(200, 80);
            Name = "UcCrmSectionPanel";
            Size = new Size(900, 140);
            pnlRoot.ResumeLayout(false);
            pnlSectionHeader.ResumeLayout(false);
            pnlSectionHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRoot;
        private Panel pnlSectionHeader;
        private Label lblSectionTitle;
        private FlowLayoutPanel flpSectionActions;
        private Panel pnlSectionBody;
    }
}
