using System.Drawing;
using System.Windows.Forms;
using BLL.Models;

namespace UI.Helpers
{
    public static class DesactivacionMiembroDialog
    {
        private static readonly Color AzulCorporativo = Color.FromArgb(27, 146, 255);

        public static ModoDesactivacionMiembro? Mostrar(IWin32Window? owner, string nombreCliente)
        {
            ModoDesactivacionMiembro? seleccion = null;

            using Form frm = new Form
            {
                Text = "Desactivar - " + nombreCliente,
                Size = new Size(460, 320),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(20, 16, 20, 16)
            };

            var lblTitulo = new Label
            {
                Text = "¿Cómo desea registrar la baja?",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            var panelOpciones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 170,
                Padding = new Padding(0, 8, 0, 0)
            };

            var btnSinMembresia = CrearBotonOpcion("DESACTIVADO", new Point(0, 0), new Size(195, 36));
            var btnVencido = CrearBotonOpcion("VENCIDO", new Point(210, 0), new Size(195, 36));

            var lblSinMembresia = new Label
            {
                Text = "Cliente se fue por otra razón.\nAparece como CLIENTE DESACTIVADO (no en vencidos).",
                Location = new Point(0, 44),
                Size = new Size(405, 44),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            var lblVencido = new Label
            {
                Text = "Baja por vencimiento.\nAparece como CLIENTE VENCIDO y cuenta en el dashboard.",
                Location = new Point(0, 108),
                Size = new Size(405, 44),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            panelOpciones.Controls.AddRange(new Control[]
            {
                btnSinMembresia, btnVencido, lblSinMembresia, lblVencido
            });

            var panelAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Size = new Size(110, 34),
                Location = new Point(295, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 9.5f),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            panelAcciones.Controls.Add(btnCancelar);

            btnSinMembresia.Click += (_, _) =>
            {
                seleccion = ModoDesactivacionMiembro.SinMembresia;
                frm.DialogResult = DialogResult.OK;
                frm.Close();
            };

            btnVencido.Click += (_, _) =>
            {
                seleccion = ModoDesactivacionMiembro.Vencido;
                frm.DialogResult = DialogResult.OK;
                frm.Close();
            };

            frm.Controls.Add(panelOpciones);
            frm.Controls.Add(lblTitulo);
            frm.Controls.Add(panelAcciones);
            frm.CancelButton = btnCancelar;

            return frm.ShowDialog(owner) == DialogResult.OK ? seleccion : null;
        }

        private static Button CrearBotonOpcion(string texto, Point location, Size size)
        {
            var btn = new Button
            {
                Text = texto,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AzulCorporativo,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = AzulCorporativo;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 245, 255);
            return btn;
        }
    }
}
