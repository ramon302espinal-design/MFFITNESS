namespace UI.DISEÑO.ESTADO
{
    partial class FrmAñadirMiembro
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
            cbmTipoPlanAñadir = new ComboBox();
            dtFechaInicio = new DateTimePicker();
            dtFechaVence = new DateTimePicker();
            cmbMiembro = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            lblNotaVence = new Label();
            tbnGuardar = new Button();
            SuspendLayout();
            // 
            // cbmTipoPlanAñadir
            // 
            cbmTipoPlanAñadir.DropDownStyle = ComboBoxStyle.DropDownList;
            cbmTipoPlanAñadir.FormattingEnabled = true;
            cbmTipoPlanAñadir.Location = new Point(26, 77);
            cbmTipoPlanAñadir.Name = "cbmTipoPlanAñadir";
            cbmTipoPlanAñadir.Size = new Size(200, 28);
            cbmTipoPlanAñadir.TabIndex = 0;
            // 
            // dtFechaInicio
            // 
            dtFechaInicio.Format = DateTimePickerFormat.Short;
            dtFechaInicio.Location = new Point(26, 172);
            dtFechaInicio.MaxDate = new DateTime(2100, 12, 31, 0, 0, 0, 0);
            dtFechaInicio.MinDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
            dtFechaInicio.Name = "dtFechaInicio";
            dtFechaInicio.Size = new Size(200, 27);
            dtFechaInicio.TabIndex = 2;
            dtFechaInicio.ValueChanged += dtFechaInicio_ValueChanged;
            // 
            // dtFechaVence
            // 
            dtFechaVence.CalendarForeColor = Color.FromArgb(22, 101, 52);
            dtFechaVence.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dtFechaVence.ForeColor = Color.FromArgb(22, 101, 52);
            dtFechaVence.Format = DateTimePickerFormat.Short;
            dtFechaVence.Location = new Point(280, 172);
            dtFechaVence.MaxDate = new DateTime(2100, 12, 31, 0, 0, 0, 0);
            dtFechaVence.MinDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
            dtFechaVence.Name = "dtFechaVence";
            dtFechaVence.Size = new Size(200, 27);
            dtFechaVence.TabIndex = 3;
            dtFechaVence.Enabled = false;
            dtFechaVence.ShowUpDown = false;
            // 
            // cmbMiembro
            // 
            cmbMiembro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMiembro.FormattingEnabled = true;
            cmbMiembro.Location = new Point(280, 77);
            cmbMiembro.Name = "cmbMiembro";
            cmbMiembro.Size = new Size(327, 28);
            cmbMiembro.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(26, 54);
            label1.Name = "label1";
            label1.Size = new Size(49, 20);
            label1.TabIndex = 4;
            label1.Text = "PLAN";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(280, 54);
            label2.Name = "label2";
            label2.Size = new Size(81, 20);
            label2.TabIndex = 5;
            label2.Text = "MIEMBRO";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(26, 149);
            label3.Name = "label3";
            label3.Size = new Size(148, 20);
            label3.TabIndex = 6;
            label3.Text = "FECHA DE INGRESO";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.ForeColor = Color.FromArgb(22, 101, 52);
            label4.Location = new Point(280, 149);
            label4.Name = "label4";
            label4.Size = new Size(178, 20);
            label4.TabIndex = 7;
            label4.Text = "VENCE PRÓXIMO PAGO";
            // 
            // lblNotaVence
            // 
            lblNotaVence.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
            lblNotaVence.ForeColor = Color.FromArgb(100, 116, 139);
            lblNotaVence.Location = new Point(26, 208);
            lblNotaVence.Name = "lblNotaVence";
            lblNotaVence.Size = new Size(581, 40);
            lblNotaVence.TabIndex = 8;
            lblNotaVence.Text = "El calendario de vencimiento muestra el mes siguiente según la regla del gimnasio (7–19 → día 15; 20–6 → fin de mes).";
            // 
            // tbnGuardar
            // 
            tbnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tbnGuardar.Location = new Point(235, 262);
            tbnGuardar.Name = "tbnGuardar";
            tbnGuardar.Size = new Size(167, 53);
            tbnGuardar.TabIndex = 9;
            tbnGuardar.Text = "GUARDAR";
            tbnGuardar.UseVisualStyleBackColor = true;
            tbnGuardar.Click += tbnGuardar_Click;
            // 
            // FrmAñadirMiembro
            // 
            AcceptButton = tbnGuardar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(634, 338);
            Controls.Add(lblNotaVence);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cmbMiembro);
            Controls.Add(dtFechaVence);
            Controls.Add(dtFechaInicio);
            Controls.Add(cbmTipoPlanAñadir);
            Controls.Add(tbnGuardar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmAñadirMiembro";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Añadir miembro (ya pagado)";
            Load += FrmAñadirMiembro_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cbmTipoPlanAñadir;
        private DateTimePicker dtFechaInicio;
        private DateTimePicker dtFechaVence;
        private ComboBox cmbMiembro;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label lblNotaVence;
        private Button tbnGuardar;
    }
}
