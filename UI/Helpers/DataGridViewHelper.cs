using System;
using System.Windows.Forms;

namespace UI.Helpers
{
    public static class DataGridViewHelper
    {
        public static bool TryGetColumn(DataGridView grid, string columnName, out DataGridViewColumn? column)
        {
            column = null;
            if (grid == null || string.IsNullOrWhiteSpace(columnName) || grid.Columns.Count == 0)
                return false;

            if (grid.Columns.Contains(columnName))
            {
                column = grid.Columns[columnName];
                return column != null;
            }

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (string.Equals(col.Name, columnName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(col.DataPropertyName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    column = col;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// WinForms: con <see cref="DataGridViewAutoSizeColumnsMode.Fill"/>, asignar
        /// <see cref="DataGridViewColumn.Width"/> lanza NullReferenceException en DataGridViewBand.
        /// </summary>
        public static void SetColumnWidth(DataGridViewColumn column, int width)
        {
            DataGridView? grid = column.DataGridView;
            if (grid == null || grid.IsDisposed)
                return;

            if (grid.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None)
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.Width = Math.Max(width, 0);
        }

        /// <summary>
        /// Proporción Fill sin romper <see cref="DataGridViewAutoSizeColumnsMode.Fill"/>
        /// (usar en grids anclados Left|Right que deben redistribuir al redimensionar).
        /// </summary>
        public static void SetColumnFill(DataGridViewColumn column, float fillWeight, int minimumWidth = 40)
        {
            if (column == null)
                return;

            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = Math.Max(fillWeight, 1f);
            column.MinimumWidth = Math.Max(minimumWidth, 1);
        }

        public static void SetDisplayIndexSafe(DataGridViewColumn column, int displayIndex)
        {
            DataGridView? grid = column.DataGridView;
            if (grid == null || grid.IsDisposed || grid.Columns.Count == 0)
                return;

            if (displayIndex < 0 || displayIndex >= grid.Columns.Count)
                return;

            column.DisplayIndex = displayIndex;
        }

        /// <summary>Envuelve formateo de columnas (suspend layout).</summary>
        /// <param name="restoreFill">
        /// True: deja el grid en Fill al terminar (columnas con FillWeight se redistribuyen al anclar Left|Right).
        /// False: deja AutoSize en None (útil si el layout usa anchos fijos via SetColumnWidth).
        /// </param>
        public static void RunColumnLayout(DataGridView grid, Action layout, bool restoreFill = false)
        {
            if (grid == null || grid.IsDisposed)
                return;

            grid.SuspendLayout();
            try
            {
                if (!restoreFill && grid.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None)
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                layout();

                if (restoreFill)
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            finally
            {
                grid.ResumeLayout(true);
            }
        }

        public static void ConfigureColumn(DataGridView grid, string columnName, Action<DataGridViewColumn> configure)
        {
            if (!TryGetColumn(grid, columnName, out DataGridViewColumn? column) || column == null)
                return;

            if (column.DataGridView == null || column.DataGridView.IsDisposed)
                return;

            configure(column);
        }

        public static void HideColumn(DataGridView grid, string columnName)
        {
            ConfigureColumn(grid, columnName, column => column.Visible = false);
        }

        public static void SetDisplayIndex(DataGridView grid, string columnName, int displayIndex)
        {
            ConfigureColumn(grid, columnName, column => SetDisplayIndexSafe(column, displayIndex));
        }
    }
}
