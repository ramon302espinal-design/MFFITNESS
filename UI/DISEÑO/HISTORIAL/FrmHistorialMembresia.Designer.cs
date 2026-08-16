namespace UI.DISEÑO
{
    partial class FrmHistorialMembresia
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvHistorialMembresia;
        private System.Windows.Forms.Button btnActualizar;

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

        private void InitializeComponent()
        {
            this.dgvHistorialMembresia = new System.Windows.Forms.DataGridView();
            this.btnActualizar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorialMembresia)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvHistorialMembresia
            // 
            this.dgvHistorialMembresia.AllowUserToAddRows = false;
            this.dgvHistorialMembresia.AllowUserToDeleteRows = false;
            this.dgvHistorialMembresia.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistorialMembresia.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistorialMembresia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistorialMembresia.Location = new System.Drawing.Point(0, 0);
            this.dgvHistorialMembresia.Name = "dgvHistorialMembresia";
            this.dgvHistorialMembresia.ReadOnly = true;
            this.dgvHistorialMembresia.RowHeadersWidth = 51;
            this.dgvHistorialMembresia.RowTemplate.Height = 29;
            this.dgvHistorialMembresia.Size = new System.Drawing.Size(784, 420);
            this.dgvHistorialMembresia.TabIndex = 0;
            // 
            // btnActualizar
            // 
            this.btnActualizar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnActualizar.Location = new System.Drawing.Point(0, 420);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.Size = new System.Drawing.Size(784, 41);
            this.btnActualizar.TabIndex = 1;
            this.btnActualizar.Text = "Actualizar";
            this.btnActualizar.UseVisualStyleBackColor = true;
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // FrmHistorialMembresia
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.dgvHistorialMembresia);
            this.Controls.Add(this.btnActualizar);
            this.Name = "FrmHistorialMembresia";
            this.Text = "Historial de Membresía";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorialMembresia)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
