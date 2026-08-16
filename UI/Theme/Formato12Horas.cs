using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Red de seguridad de presentación: si un grid declara un formato de 24 horas,
    /// la celda se muestra igual en 12 horas (AM/PM). No altera el valor almacenado.
    /// </summary>
    public static class Formato12Horas
    {
        private static readonly HashSet<DataGridView> Enlazados = new();

        public static void AplicarAFormulario(Form form)
        {
            if (form == null)
                return;

            foreach (DataGridView grid in BuscarGrids(form))
                Aplicar(grid);
        }

        public static void Aplicar(DataGridView grid)
        {
            if (grid == null || !Enlazados.Add(grid))
                return;

            grid.CellFormatting += Grid_CellFormatting;
            grid.Disposed += Grid_Disposed;
        }

        private static void Grid_Disposed(object? sender, EventArgs e)
        {
            if (sender is DataGridView grid)
                Enlazados.Remove(grid);
        }

        private static IEnumerable<DataGridView> BuscarGrids(Control root)
        {
            foreach (Control hijo in root.Controls)
            {
                if (hijo is DataGridView grid)
                    yield return grid;

                if (hijo.HasChildren)
                {
                    foreach (DataGridView anidado in BuscarGrids(hijo))
                        yield return anidado;
                }
            }
        }

        private static void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is not DateTime fecha || e.CellStyle == null)
                return;

            string formato = e.CellStyle.Format;
            if (string.IsNullOrEmpty(formato) || !formato.Contains("HH", StringComparison.Ordinal))
                return;

            string formato12 = formato.Replace("HH", "hh", StringComparison.Ordinal);
            if (!formato12.Contains("tt", StringComparison.Ordinal))
                formato12 += " tt";

            e.Value = fecha.ToString(
                formato12,
                e.CellStyle.FormatProvider ?? CultureInfo.CurrentCulture);
            e.FormattingApplied = true;
        }
    }
}
