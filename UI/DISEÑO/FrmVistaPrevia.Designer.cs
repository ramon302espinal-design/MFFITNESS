namespace UI
{
    partial class FrmVistaPrevia
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
            txtVistaPrevia = new TextBox();
            btnImprimir = new Button();
            btnCerrar = new Button();
            SuspendLayout();
            // 
            // txtVistaPrevia
            // 
            txtVistaPrevia.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtVistaPrevia.Font = new System.Drawing.Font("Courier New", 9F);
            txtVistaPrevia.Location = new System.Drawing.Point(12, 12);
            txtVistaPrevia.Multiline = true;
            txtVistaPrevia.Name = "txtVistaPrevia";
            txtVistaPrevia.ReadOnly = true;
            txtVistaPrevia.ScrollBars = ScrollBars.Both;
            txtVistaPrevia.Size = new System.Drawing.Size(776, 487);
            txtVistaPrevia.TabIndex = 0;
            txtVistaPrevia.WordWrap = false;
            // 
            // btnImprimir
            // 
            btnImprimir.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImprimir.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnImprimir.FlatStyle = FlatStyle.Flat;
            btnImprimir.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnImprimir.ForeColor = System.Drawing.Color.White;
            btnImprimir.Location = new System.Drawing.Point(566, 515);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new System.Drawing.Size(110, 40);
            btnImprimir.TabIndex = 1;
            btnImprimir.Text = "Imprimir";
            btnImprimir.UseVisualStyleBackColor = false;
            btnImprimir.Click += btnImprimir_Click;
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnCerrar.Location = new System.Drawing.Point(682, 515);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new System.Drawing.Size(106, 40);
            btnCerrar.TabIndex = 2;
            btnCerrar.Text = "Cerrar";
            btnCerrar.UseVisualStyleBackColor = false;
            btnCerrar.Click += btnCerrar_Click;
            // 
            // FrmVistaPrevia
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 567);
            Controls.Add(btnCerrar);
            Controls.Add(btnImprimir);
            Controls.Add(txtVistaPrevia);
            Name = "FrmVistaPrevia";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Vista Previa de Impresión";
            Load += FrmVistaPrevia_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtVistaPrevia;
        private Button btnImprimir;
        private Button btnCerrar;
    }
}
