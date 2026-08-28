using System;
using System.Globalization;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    internal static class SeleccionPeriodoEstadoDialog
    {
        internal sealed class OpcionPeriodo
        {
            public bool EsHoy { get; init; }
            public int Mes { get; init; }
            public int Anio { get; init; }
            public string Etiqueta { get; init; } = string.Empty;

            public override string ToString() => Etiqueta;
        }

        internal static OpcionPeriodo? Mostrar(IWin32Window? owner)
        {
            var cultura = CultureInfo.GetCultureInfo("es-DO");
            int anio = DateTime.Today.Year;

            using var frm = new Form
            {
                Text = "Período del reporte PDF",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new System.Drawing.Size(420, 150),
                ShowInTaskbar = false
            };

            var lbl = new Label
            {
                Text = "Seleccione el mes a descargar:",
                Location = new System.Drawing.Point(16, 16),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            var cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(16, 48),
                Size = new System.Drawing.Size(388, 28),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };

            cmb.Items.Add(new OpcionPeriodo { EsHoy = true, Anio = anio, Etiqueta = "HOY (activos en vivo)" });
            for (int mes = 1; mes <= 12; mes++)
            {
                string nombre = cultura.DateTimeFormat.GetMonthName(mes);
                if (!string.IsNullOrEmpty(nombre))
                    nombre = char.ToUpper(nombre[0], cultura) + nombre[1..];

                cmb.Items.Add(new OpcionPeriodo
                {
                    EsHoy = false,
                    Mes = mes,
                    Anio = anio,
                    Etiqueta = $"{nombre} {anio}"
                });
            }

            cmb.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "Descargar PDF",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(212, 98),
                Size = new System.Drawing.Size(192, 36),
                Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold)
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(16, 98),
                Size = new System.Drawing.Size(120, 36)
            };

            frm.Controls.AddRange(new Control[] { lbl, cmb, btnOk, btnCancel });
            frm.AcceptButton = btnOk;
            frm.CancelButton = btnCancel;

            return frm.ShowDialog(owner) == DialogResult.OK && cmb.SelectedItem is OpcionPeriodo op
                ? op
                : null;
        }
    }
}
