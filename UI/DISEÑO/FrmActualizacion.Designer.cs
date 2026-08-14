namespace UI.DISEÑO
{
    partial class FrmActualizacion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSub = new Label();
            panelInstalled = new Panel();
            lblInstalledTitle = new Label();
            lblInstalledValue = new Label();
            panelStatus = new Panel();
            panelBadgeAccent = new Panel();
            lblStatusTitle = new Label();
            lblStatusHint = new Label();
            panelReqs = new Panel();
            lblReqTitle = new Label();
            lblReqCaja = new Label();
            lblReqManager = new Label();
            lblSession = new Label();
            lblDetail = new Label();
            progressBar = new ProgressBar();
            panelBotones = new Panel();
            btnCheck = new Button();
            btnInstall = new Button();
            btnClose = new Button();
            panelHeader.SuspendLayout();
            panelInstalled.SuspendLayout();
            panelStatus.SuspendLayout();
            panelReqs.SuspendLayout();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 144, 255);
            panelHeader.Controls.Add(lblHeaderTitle);
            panelHeader.Controls.Add(lblHeaderSub);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(640, 72);
            panelHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Location = new Point(24, 14);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(592, 28);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Centro de actualizaciones";
            // 
            // lblHeaderSub
            // 
            lblHeaderSub.Font = new Font("Segoe UI", 9F);
            lblHeaderSub.ForeColor = Color.FromArgb(220, 235, 255);
            lblHeaderSub.Location = new Point(24, 42);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(592, 22);
            lblHeaderSub.TabIndex = 1;
            lblHeaderSub.Text = "Descarga verificada (SHA256) · instalación segura con backup y recovery";
            // 
            // panelInstalled
            // 
            panelInstalled.BackColor = Color.White;
            panelInstalled.BorderStyle = BorderStyle.FixedSingle;
            panelInstalled.Controls.Add(lblInstalledTitle);
            panelInstalled.Controls.Add(lblInstalledValue);
            panelInstalled.Location = new Point(24, 88);
            panelInstalled.Name = "panelInstalled";
            panelInstalled.Size = new Size(592, 86);
            panelInstalled.TabIndex = 1;
            // 
            // lblInstalledTitle
            // 
            lblInstalledTitle.Font = new Font("Segoe UI", 9F);
            lblInstalledTitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblInstalledTitle.Location = new Point(16, 12);
            lblInstalledTitle.Name = "lblInstalledTitle";
            lblInstalledTitle.Size = new Size(560, 20);
            lblInstalledTitle.TabIndex = 0;
            lblInstalledTitle.Text = "INSTALACIÓN ACTUAL";
            // 
            // lblInstalledValue
            // 
            lblInstalledValue.Font = new Font("Segoe UI", 10F);
            lblInstalledValue.ForeColor = Color.FromArgb(15, 23, 42);
            lblInstalledValue.Location = new Point(16, 38);
            lblInstalledValue.Name = "lblInstalledValue";
            lblInstalledValue.Size = new Size(560, 36);
            lblInstalledValue.TabIndex = 1;
            lblInstalledValue.Text = "App — · Build — · DB —";
            // 
            // panelStatus
            // 
            panelStatus.BackColor = Color.White;
            panelStatus.BorderStyle = BorderStyle.FixedSingle;
            panelStatus.Controls.Add(panelBadgeAccent);
            panelStatus.Controls.Add(lblStatusTitle);
            panelStatus.Controls.Add(lblStatusHint);
            panelStatus.Location = new Point(24, 186);
            panelStatus.Name = "panelStatus";
            panelStatus.Size = new Size(592, 100);
            panelStatus.TabIndex = 2;
            // 
            // panelBadgeAccent
            // 
            panelBadgeAccent.BackColor = Color.FromArgb(59, 130, 246);
            panelBadgeAccent.Dock = DockStyle.Left;
            panelBadgeAccent.Location = new Point(0, 0);
            panelBadgeAccent.Name = "panelBadgeAccent";
            panelBadgeAccent.Size = new Size(6, 98);
            panelBadgeAccent.TabIndex = 0;
            // 
            // lblStatusTitle
            // 
            lblStatusTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblStatusTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblStatusTitle.Location = new Point(20, 14);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Size = new Size(556, 28);
            lblStatusTitle.TabIndex = 1;
            lblStatusTitle.Text = "Sin consultar";
            // 
            // lblStatusHint
            // 
            lblStatusHint.Font = new Font("Segoe UI", 9F);
            lblStatusHint.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatusHint.Location = new Point(20, 46);
            lblStatusHint.Name = "lblStatusHint";
            lblStatusHint.Size = new Size(556, 42);
            lblStatusHint.TabIndex = 2;
            lblStatusHint.Text = "Pulse «Buscar» para consultar el último release en GitHub.";
            // 
            // panelReqs
            // 
            panelReqs.BackColor = Color.White;
            panelReqs.BorderStyle = BorderStyle.FixedSingle;
            panelReqs.Controls.Add(lblReqTitle);
            panelReqs.Controls.Add(lblReqCaja);
            panelReqs.Controls.Add(lblReqManager);
            panelReqs.Controls.Add(lblSession);
            panelReqs.Location = new Point(24, 298);
            panelReqs.Name = "panelReqs";
            panelReqs.Size = new Size(592, 110);
            panelReqs.TabIndex = 3;
            // 
            // lblReqTitle
            // 
            lblReqTitle.Font = new Font("Segoe UI", 9F);
            lblReqTitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblReqTitle.Location = new Point(16, 10);
            lblReqTitle.Name = "lblReqTitle";
            lblReqTitle.Size = new Size(560, 20);
            lblReqTitle.TabIndex = 0;
            lblReqTitle.Text = "REQUISITOS";
            // 
            // lblReqCaja
            // 
            lblReqCaja.Font = new Font("Segoe UI", 9F);
            lblReqCaja.ForeColor = Color.FromArgb(100, 116, 139);
            lblReqCaja.Location = new Point(16, 36);
            lblReqCaja.Name = "lblReqCaja";
            lblReqCaja.Size = new Size(560, 20);
            lblReqCaja.TabIndex = 1;
            lblReqCaja.Text = "Caja: —";
            // 
            // lblReqManager
            // 
            lblReqManager.Font = new Font("Segoe UI", 9F);
            lblReqManager.ForeColor = Color.FromArgb(100, 116, 139);
            lblReqManager.Location = new Point(16, 58);
            lblReqManager.Name = "lblReqManager";
            lblReqManager.Size = new Size(560, 20);
            lblReqManager.TabIndex = 2;
            lblReqManager.Text = "UpdateManager: —";
            // 
            // lblSession
            // 
            lblSession.Font = new Font("Segoe UI", 9F);
            lblSession.ForeColor = Color.FromArgb(148, 163, 184);
            lblSession.Location = new Point(16, 82);
            lblSession.Name = "lblSession";
            lblSession.Size = new Size(560, 20);
            lblSession.TabIndex = 3;
            lblSession.Text = "Sin sesiones locales recientes.";
            // 
            // lblDetail
            // 
            lblDetail.Font = new Font("Segoe UI", 9F);
            lblDetail.ForeColor = Color.FromArgb(100, 116, 139);
            lblDetail.Location = new Point(24, 420);
            lblDetail.Name = "lblDetail";
            lblDetail.Size = new Size(592, 36);
            lblDetail.TabIndex = 4;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(24, 458);
            progressBar.MarqueeAnimationSpeed = 28;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(592, 12);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 5;
            progressBar.Visible = false;
            // 
            // panelBotones
            // 
            panelBotones.Controls.Add(btnCheck);
            panelBotones.Controls.Add(btnInstall);
            panelBotones.Controls.Add(btnClose);
            panelBotones.Location = new Point(0, 484);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(640, 56);
            panelBotones.TabIndex = 6;
            // 
            // btnCheck
            // 
            btnCheck.FlatStyle = FlatStyle.System;
            btnCheck.Location = new Point(24, 6);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(140, 36);
            btnCheck.TabIndex = 0;
            btnCheck.Tag = "classic";
            btnCheck.Text = "Buscar";
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            // 
            // btnInstall
            // 
            btnInstall.Enabled = false;
            btnInstall.FlatStyle = FlatStyle.System;
            btnInstall.Location = new Point(176, 6);
            btnInstall.Name = "btnInstall";
            btnInstall.Size = new Size(220, 36);
            btnInstall.TabIndex = 1;
            btnInstall.Tag = "classic";
            btnInstall.Text = "Instalar actualización";
            btnInstall.UseVisualStyleBackColor = true;
            btnInstall.Click += btnInstall_Click;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.FlatStyle = FlatStyle.System;
            btnClose.Location = new Point(500, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(116, 36);
            btnClose.TabIndex = 2;
            btnClose.Tag = "classic";
            btnClose.Text = "Cerrar";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // FrmActualizacion
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            CancelButton = btnClose;
            ClientSize = new Size(640, 560);
            Controls.Add(panelBotones);
            Controls.Add(progressBar);
            Controls.Add(lblDetail);
            Controls.Add(panelReqs);
            Controls.Add(panelStatus);
            Controls.Add(panelInstalled);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmActualizacion";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Actualización del sistema";
            Shown += FrmActualizacion_Shown;
            panelHeader.ResumeLayout(false);
            panelInstalled.ResumeLayout(false);
            panelStatus.ResumeLayout(false);
            panelReqs.ResumeLayout(false);
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Panel panelInstalled;
        private Label lblInstalledTitle;
        private Label lblInstalledValue;
        private Panel panelStatus;
        private Panel panelBadgeAccent;
        private Label lblStatusTitle;
        private Label lblStatusHint;
        private Panel panelReqs;
        private Label lblReqTitle;
        private Label lblReqCaja;
        private Label lblReqManager;
        private Label lblSession;
        private Label lblDetail;
        private ProgressBar progressBar;
        private Panel panelBotones;
        private Button btnCheck;
        private Button btnInstall;
        private Button btnClose;
    }
}
