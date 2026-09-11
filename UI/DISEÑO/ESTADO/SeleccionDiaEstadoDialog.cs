using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>Diálogo clásico para elegir un día (KPI personalizado en Estado).</summary>
    internal static class SeleccionDiaEstadoDialog
    {
        internal static DateTime? Mostrar(IWin32Window? owner, DateTime valorInicial)
        {
            using var frm = new Form
            {
                Text = "Día personalizado",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(380, 140),
                ShowInTaskbar = false
            };

            var lbl = new Label
            {
                Text = "Seleccione el día a consultar (cobros/altas):",
                Location = new Point(16, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var dtp = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(16, 48),
                Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10F),
                Value = valorInicial.Date,
                MaxDate = DateTime.Today.AddYears(1),
                MinDate = new DateTime(2000, 1, 1)
            };

            var btnOk = new Button
            {
                Text = "Aplicar",
                DialogResult = DialogResult.OK,
                Location = new Point(196, 92),
                Size = new Size(152, 32),
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(16, 92),
                Size = new Size(120, 32)
            };

            frm.Controls.AddRange(new Control[] { lbl, dtp, btnOk, btnCancel });
            frm.AcceptButton = btnOk;
            frm.CancelButton = btnCancel;

            return frm.ShowDialog(owner) == DialogResult.OK
                ? dtp.Value.Date
                : null;
        }
    }
}
