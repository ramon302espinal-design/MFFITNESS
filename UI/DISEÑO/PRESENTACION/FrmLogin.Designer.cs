namespace UI.DISEÑO
{
    partial class FrmLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            panelCard = new Panel();
            panelHeader = new Panel();
            picLoginLogo = new PictureBox();
            lblMF = new Label();
            lblFitness = new Label();
            panelFormulario = new Panel();
            lblVersion = new Label();
            lblUsuario = new Label();
            comboUsuarios = new ComboBox();
            lblContraseña = new Label();
            txtContraseña = new TextBox();
            chkMostrarContraseña = new CheckBox();
            panelBotones = new Panel();
            btnCancelar = new Button();
            btnIniciar = new Button();
            tableLayoutPanel1.SuspendLayout();
            panelCard.SuspendLayout();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLoginLogo).BeginInit();
            panelFormulario.SuspendLayout();
            panelBotones.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.FromArgb(20, 20, 20);
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panelCard, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1371, 933);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panelCard
            // 
            panelCard.Anchor = AnchorStyles.None;
            panelCard.BackColor = Color.White;
            panelCard.Controls.Add(panelHeader);
            panelCard.Controls.Add(panelFormulario);
            panelCard.Controls.Add(panelBotones);
            panelCard.Location = new Point(371, 100);
            panelCard.Margin = new Padding(3, 4, 3, 4);
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(629, 733);
            panelCard.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(0, 123, 255);
            panelHeader.Controls.Add(picLoginLogo);
            panelHeader.Controls.Add(lblMF);
            panelHeader.Controls.Add(lblFitness);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Margin = new Padding(3, 4, 3, 4);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(629, 187);
            panelHeader.TabIndex = 0;
            // 
            // picLoginLogo
            // 
            picLoginLogo.BackColor = Color.Transparent;
            picLoginLogo.Location = new Point(200, 27);
            picLoginLogo.Margin = new Padding(3, 4, 3, 4);
            picLoginLogo.Name = "picLoginLogo";
            picLoginLogo.Size = new Size(229, 120);
            picLoginLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLoginLogo.TabIndex = 3;
            picLoginLogo.TabStop = false;
            // 
            // lblMF
            // 
            lblMF.Font = new Font("Segoe UI", 42F, FontStyle.Bold);
            lblMF.ForeColor = Color.White;
            lblMF.Location = new Point(0, 20);
            lblMF.Name = "lblMF";
            lblMF.Size = new Size(629, 80);
            lblMF.TabIndex = 0;
            lblMF.Text = "MF";
            lblMF.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFitness
            // 
            lblFitness.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblFitness.ForeColor = Color.White;
            lblFitness.Location = new Point(0, 100);
            lblFitness.Name = "lblFitness";
            lblFitness.Size = new Size(629, 47);
            lblFitness.TabIndex = 1;
            lblFitness.Text = "FITNESS";
            lblFitness.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelFormulario
            // 
            panelFormulario.BackColor = Color.White;
            panelFormulario.Controls.Add(lblVersion);
            panelFormulario.Controls.Add(lblUsuario);
            panelFormulario.Controls.Add(comboUsuarios);
            panelFormulario.Controls.Add(lblContraseña);
            panelFormulario.Controls.Add(txtContraseña);
            panelFormulario.Controls.Add(chkMostrarContraseña);
            panelFormulario.Location = new Point(0, 187);
            panelFormulario.Margin = new Padding(3, 4, 3, 4);
            panelFormulario.Name = "panelFormulario";
            panelFormulario.Size = new Size(629, 413);
            panelFormulario.TabIndex = 1;
            // 
            // lblVersion
            // 
            lblVersion.Anchor = AnchorStyles.None;
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 8.5F);
            lblVersion.ForeColor = Color.FromArgb(130, 130, 130);
            lblVersion.Location = new Point(267, 372);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(114, 20);
            lblVersion.TabIndex = 6;
            lblVersion.Text = "VERSION 1.1.12";
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.FromArgb(60, 60, 60);
            lblUsuario.Location = new Point(86, 47);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(96, 25);
            lblUsuario.TabIndex = 0;
            lblUsuario.Text = "USUARIO";
            // 
            // comboUsuarios
            // 
            comboUsuarios.BackColor = SystemColors.MenuHighlight;
            comboUsuarios.DropDownStyle = ComboBoxStyle.DropDownList;
            comboUsuarios.Font = new Font("Segoe UI", 12F);
            comboUsuarios.FormattingEnabled = true;
            comboUsuarios.Location = new Point(86, 87);
            comboUsuarios.Margin = new Padding(3, 4, 3, 4);
            comboUsuarios.Name = "comboUsuarios";
            comboUsuarios.Size = new Size(457, 36);
            comboUsuarios.TabIndex = 1;
            // 
            // lblContraseña
            // 
            lblContraseña.AutoSize = true;
            lblContraseña.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblContraseña.ForeColor = Color.FromArgb(60, 60, 60);
            lblContraseña.Location = new Point(86, 167);
            lblContraseña.Name = "lblContraseña";
            lblContraseña.Size = new Size(138, 25);
            lblContraseña.TabIndex = 2;
            lblContraseña.Text = "CONTRASEÑA";
            // 
            // txtContraseña
            // 
            txtContraseña.Font = new Font("Segoe UI", 12F);
            txtContraseña.Location = new Point(86, 207);
            txtContraseña.Margin = new Padding(3, 4, 3, 4);
            txtContraseña.Name = "txtContraseña";
            txtContraseña.PasswordChar = '●';
            txtContraseña.Size = new Size(457, 34);
            txtContraseña.TabIndex = 2;
            // 
            // chkMostrarContraseña
            // 
            chkMostrarContraseña.AutoSize = true;
            chkMostrarContraseña.Font = new Font("Segoe UI", 9.5F);
            chkMostrarContraseña.ForeColor = Color.FromArgb(100, 100, 100);
            chkMostrarContraseña.Location = new Point(86, 267);
            chkMostrarContraseña.Margin = new Padding(3, 4, 3, 4);
            chkMostrarContraseña.Name = "chkMostrarContraseña";
            chkMostrarContraseña.Size = new Size(167, 25);
            chkMostrarContraseña.TabIndex = 3;
            chkMostrarContraseña.Text = "Mostrar contraseña";
            chkMostrarContraseña.UseVisualStyleBackColor = true;
            chkMostrarContraseña.CheckedChanged += chkMostrarContraseña_CheckedChanged;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.White;
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnIniciar);
            panelBotones.Location = new Point(0, 600);
            panelBotones.Margin = new Padding(3, 4, 3, 4);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(629, 133);
            panelBotones.TabIndex = 2;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = Color.FromArgb(230, 230, 230);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.FromArgb(80, 80, 80);
            btnCancelar.Location = new Point(131, 33);
            btnCancelar.Margin = new Padding(3, 4, 3, 4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(171, 67);
            btnCancelar.TabIndex = 4;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Click += btnCerrar_Click;
            // 
            // btnIniciar
            // 
            btnIniciar.BackColor = Color.FromArgb(0, 123, 255);
            btnIniciar.Cursor = Cursors.Hand;
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.FlatStyle = FlatStyle.Flat;
            btnIniciar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnIniciar.ForeColor = Color.White;
            btnIniciar.Location = new Point(326, 33);
            btnIniciar.Margin = new Padding(3, 4, 3, 4);
            btnIniciar.Name = "btnIniciar";
            btnIniciar.Size = new Size(171, 67);
            btnIniciar.TabIndex = 5;
            btnIniciar.Text = "INICIAR";
            btnIniciar.UseVisualStyleBackColor = false;
            btnIniciar.Click += btnEntrar_Click;
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 20);
            ClientSize = new Size(1371, 933);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "FrmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MF FITNESS - Login";
            WindowState = FormWindowState.Maximized;
            Load += FrmLogin_Load;
            VisibleChanged += FrmLogin_VisibleChanged;
            tableLayoutPanel1.ResumeLayout(false);
            panelCard.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLoginLogo).EndInit();
            panelFormulario.ResumeLayout(false);
            panelFormulario.PerformLayout();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panelCard;
        private Panel panelHeader;
        private PictureBox picLoginLogo;
        private Label lblMF;
        private Label lblFitness;
        private Panel panelFormulario;
        private Label lblUsuario;
        private ComboBox comboUsuarios;
        private Label lblContraseña;
        private TextBox txtContraseña;
        private CheckBox chkMostrarContraseña;
        private Panel panelBotones;
        private Label lblVersion;
        private Button btnCancelar;
        private Button btnIniciar;
    }
}
