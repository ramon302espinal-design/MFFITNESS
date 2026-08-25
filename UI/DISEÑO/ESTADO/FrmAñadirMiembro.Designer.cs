namespace UI.DISEÑO.ESTADO
{
    partial class FrmAñadirMiembro
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
            cbmTipoPlanAñadir = new ComboBox();
            dtFechaInicio = new DateTimePicker();
            cmbMiembro = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            tbnGuardar = new Button();
            SuspendLayout();
            // 
            // cbmTipoPlanAñadir
            // 
            cbmTipoPlanAñadir.FormattingEnabled = true;
            cbmTipoPlanAñadir.Location = new Point(26, 77);
            cbmTipoPlanAñadir.Name = "cbmTipoPlanAñadir";
            cbmTipoPlanAñadir.Size = new Size(151, 28);
            cbmTipoPlanAñadir.TabIndex = 0;
            // 
            // dtFechaInicio
            // 
            dtFechaInicio.Format = DateTimePickerFormat.Short;
            dtFechaInicio.Location = new Point(172, 172);
            dtFechaInicio.MinDate = new DateTime(2000, 1, 1);
            dtFechaInicio.MaxDate = new DateTime(2100, 12, 31);
            dtFechaInicio.Name = "dtFechaInicio";
            dtFechaInicio.Size = new Size(307, 27);
            dtFechaInicio.TabIndex = 1;
            // 
            // cmbMiembro
            // 
            cmbMiembro.FormattingEnabled = true;
            cmbMiembro.Location = new Point(456, 77);
            cmbMiembro.Name = "cmbMiembro";
            cmbMiembro.Size = new Size(151, 28);
            cmbMiembro.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(26, 54);
            label1.Name = "label1";
            label1.Size = new Size(49, 20);
            label1.TabIndex = 3;
            label1.Text = "PLAN";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(456, 54);
            label2.Name = "label2";
            label2.Size = new Size(81, 20);
            label2.TabIndex = 4;
            label2.Text = "MIEMBRO";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(172, 149);
            label3.Name = "label3";
            label3.Size = new Size(148, 20);
            label3.TabIndex = 5;
            label3.Text = "FECHA DE INGRESO";
            // 
            // tbnGuardar
            // 
            tbnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tbnGuardar.Location = new Point(235, 223);
            tbnGuardar.Name = "tbnGuardar";
            tbnGuardar.Size = new Size(167, 53);
            tbnGuardar.TabIndex = 6;
            tbnGuardar.Text = "GUARDAR";
            tbnGuardar.UseVisualStyleBackColor = true;
            // 
            // FrmAñadirMiembro
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(682, 450);
            Controls.Add(tbnGuardar);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cmbMiembro);
            Controls.Add(dtFechaInicio);
            Controls.Add(cbmTipoPlanAñadir);
            Name = "FrmAñadirMiembro";
            Text = "FrmAñadirMiembro";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cbmTipoPlanAñadir;
        private DateTimePicker dtFechaInicio;
        private ComboBox cmbMiembro;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button tbnGuardar;
    }
}