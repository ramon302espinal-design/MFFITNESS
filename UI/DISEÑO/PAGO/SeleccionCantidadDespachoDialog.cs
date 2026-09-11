using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>Mini diálogo: cantidad manual a despachar de una línea de saldo a favor.</summary>
    internal static class SeleccionCantidadDespachoDialog
    {
        /// <returns>Cantidad elegida, o null si cancela.</returns>
        internal static int? Mostrar(
            IWin32Window? owner,
            string producto,
            int cantidadDisponible,
            int valorInicial = 1)
        {
            if (cantidadDisponible <= 0)
                return null;

            int inicial = Math.Clamp(valorInicial, 1, cantidadDisponible);

            using var frm = new Form
            {
                Text = "Cantidad a despachar",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(400, 168),
                ShowInTaskbar = false
            };

            var lblInfo = new Label
            {
                Text = $"{producto}\nDisponible en reserva: {cantidadDisponible}",
                Location = new Point(16, 12),
                Size = new Size(368, 48),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var lblCant = new Label
            {
                Text = "Cantidad:",
                Location = new Point(16, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            var txt = new TextBox
            {
                Location = new Point(110, 68),
                Size = new Size(100, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = inicial.ToString(),
                TextAlign = HorizontalAlignment.Center
            };

            var btnOk = new Button
            {
                Text = "Despachar",
                DialogResult = DialogResult.None,
                Location = new Point(220, 116),
                Size = new Size(164, 36),
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(16, 116),
                Size = new Size(120, 36)
            };

            int? cantidadElegida = null;

            void IntentarAceptar()
            {
                if (!int.TryParse(txt.Text.Trim(), out int n) || n <= 0)
                {
                    MessageBox.Show(
                        frm,
                        "Ingrese una cantidad entera mayor a cero.",
                        "Cantidad",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    txt.Focus();
                    txt.SelectAll();
                    return;
                }

                if (n > cantidadDisponible)
                {
                    MessageBox.Show(
                        frm,
                        $"Solo hay {cantidadDisponible} unidad(es) de {producto} en la reserva.",
                        "Cantidad",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txt.Focus();
                    txt.SelectAll();
                    return;
                }

                cantidadElegida = n;
                frm.DialogResult = DialogResult.OK;
            }

            btnOk.Click += (_, _) => IntentarAceptar();
            txt.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    IntentarAceptar();
                }
            };

            frm.Controls.AddRange(new Control[] { lblInfo, lblCant, txt, btnOk, btnCancel });
            frm.AcceptButton = btnOk;
            frm.CancelButton = btnCancel;

            frm.Shown += (_, _) =>
            {
                txt.Focus();
                txt.SelectAll();
            };

            return frm.ShowDialog(owner) == DialogResult.OK ? cantidadElegida : null;
        }
    }
}
