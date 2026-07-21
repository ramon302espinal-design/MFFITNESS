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
            lblSubtitulo = new Label();
            panelFormulario = new Panel();
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
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1200, 700);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panelCard
            // 
            panelCard.Anchor = AnchorStyles.None;
            panelCard.BackColor = Color.White;
            panelCard.Controls.Add(panelHeader);
            panelCard.Controls.Add(panelFormulario);
            panelCard.Controls.Add(panelBotones);
            panelCard.Location = new Point(325, 75);
            panelCard.Name = "panelCard";
            panelCard.Size = new Size(550, 550);
            panelCard.TabIndex = 0;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(0, 123, 255);
            panelHeader.Controls.Add(picLoginLogo);
            panelHeader.Controls.Add(lblMF);
            panelHeader.Controls.Add(lblFitness);
            panelHeader.Controls.Add(lblSubtitulo);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(550, 140);
            panelHeader.TabIndex = 0;
            // 
            // picLoginLogo
            // 
            picLoginLogo.BackColor = Color.Transparent;
            picLoginLogo.Location = new Point(175, 20);
            picLoginLogo.Name = "picLoginLogo";
            picLoginLogo.Size = new Size(200, 90);
            picLoginLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLoginLogo.TabIndex = 3;
            picLoginLogo.TabStop = false;
            // 
            // lblMF
            // 
            lblMF.Font = new Font("Segoe UI", 42F, FontStyle.Bold);
            lblMF.ForeColor = Color.White;
            lblMF.Location = new Point(0, 15);
            lblMF.Name = "lblMF";
            lblMF.Size = new Size(550, 60);
            lblMF.TabIndex = 0;
            lblMF.Text = "MF";
            lblMF.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFitness
            // 
            lblFitness.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblFitness.ForeColor = Color.White;
            lblFitness.Location = new Point(0, 75);
            lblFitness.Name = "lblFitness";
            lblFitness.Size = new Size(550, 35);
            lblFitness.TabIndex = 1;
            lblFitness.Text = "FITNESS";
            lblFitness.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.Font = new Font("Segoe UI", 11F);
            lblSubtitulo.ForeColor = Color.FromArgb(240, 240, 240);
            lblSubtitulo.Location = new Point(0, 110);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(550, 25);
            lblSubtitulo.TabIndex = 2;
            lblSubtitulo.Text = "Sistema de Gestion";
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelFormulario
            // 
            panelFormulario.BackColor = Color.White;
            panelFormulario.Controls.Add(lblUsuario);
            panelFormulario.Controls.Add(comboUsuarios);
            panelFormulario.Controls.Add(lblContraseña);
            panelFormulario.Controls.Add(txtContraseña);
            panelFormulario.Controls.Add(chkMostrarContraseña);
            panelFormulario.Location = new Point(0, 140);
            panelFormulario.Name = "panelFormulario";
            panelFormulario.Size = new Size(550, 310);
            panelFormulario.TabIndex = 1;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.FromArgb(60, 60, 60);
            lblUsuario.Location = new Point(75, 35);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(75, 20);
            lblUsuario.TabIndex = 0;
            lblUsuario.Text = "USUARIO";
            // 
            // comboUsuarios
            // 
            comboUsuarios.DropDownStyle = ComboBoxStyle.DropDownList;
            comboUsuarios.Font = new Font("Segoe UI", 12F);
            comboUsuarios.FormattingEnabled = true;
            comboUsuarios.Location = new Point(75, 65);
            comboUsuarios.Name = "comboUsuarios";
            comboUsuarios.Size = new Size(400, 29);
            comboUsuarios.TabIndex = 1;
            // 
            // lblContraseña
            // 
            lblContraseña.AutoSize = true;
            lblContraseña.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblContraseña.ForeColor = Color.FromArgb(60, 60, 60);
            lblContraseña.Location = new Point(75, 125);
            lblContraseña.Name = "lblContraseña";
            lblContraseña.Size = new Size(111, 20);
            lblContraseña.TabIndex = 2;
            lblContraseña.Text = "CONTRASEÑA";
            // 
            // txtContraseña
            // 
            txtContraseña.Font = new Font("Segoe UI", 12F);
            txtContraseña.Location = new Point(75, 155);
            txtContraseña.Name = "txtContraseña";
            txtContraseña.PasswordChar = '●';
            txtContraseña.Size = new Size(400, 29);
            txtContraseña.TabIndex = 2;
            // 
            // chkMostrarContraseña
            // 
            chkMostrarContraseña.AutoSize = true;
            chkMostrarContraseña.Font = new Font("Segoe UI", 9.5F);
            chkMostrarContraseña.ForeColor = Color.FromArgb(100, 100, 100);
            chkMostrarContraseña.Location = new Point(75, 200);
            chkMostrarContraseña.Name = "chkMostrarContraseña";
            chkMostrarContraseña.Size = new Size(147, 21);
            chkMostrarContraseña.TabIndex = 3;
            chkMostrarContraseña.Text = "Mostrar contraseña";
            chkMostrarContraseña.UseVisualStyleBackColor = true;
            chkMostrarContraseña.Click += chkMostrarContraseña_CheckedChanged;
            // 
            // panelBotones
            // 
            panelBotones.BackColor = Color.White;
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnIniciar);
            panelBotones.Location = new Point(0, 450);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(550, 100);
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
            btnCancelar.Location = new Point(115, 25);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(150, 50);
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
            btnIniciar.Location = new Point(285, 25);
            btnIniciar.Name = "btnIniciar";
            btnIniciar.Size = new Size(150, 50);
            btnIniciar.TabIndex = 5;
            btnIniciar.Text = "INICIAR";
            btnIniciar.UseVisualStyleBackColor = false;
            btnIniciar.Click += btnEntrar_Click;
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 20);
            ClientSize = new Size(1200, 700);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.None;
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
        private Label lblSubtitulo;
        private Panel panelFormulario;
        private Label lblUsuario;
        private ComboBox comboUsuarios;
        private Label lblContraseña;
        private TextBox txtContraseña;
        private CheckBox chkMostrarContraseña;
        private Panel panelBotones;
        private Button btnCancelar;
        private Button btnIniciar;
    }
}
