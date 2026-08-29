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

        public static void SetDisplayIndexSafe(DataGridViewColumn column, int displayIndex)
        {
            DataGridView? grid = column.DataGridView;
            if (grid == null || grid.IsDisposed || grid.Columns.Count == 0)
                return;

            if (displayIndex < 0 || displayIndex >= grid.Columns.Count)
                return;

            column.DisplayIndex = displayIndex;
        }

        /// <summary>Envuelve formateo de columnas (suspend layout + quitar Fill antes de Width).</summary>
        public static void RunColumnLayout(DataGridView grid, Action layout)
        {
            if (grid == null || grid.IsDisposed)
                return;

            grid.SuspendLayout();
            try
            {
                if (grid.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None)
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                layout();
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
