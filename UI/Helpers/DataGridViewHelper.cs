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

        public static void ConfigureColumn(DataGridView grid, string columnName, Action<DataGridViewColumn> configure)
        {
            if (!TryGetColumn(grid, columnName, out DataGridViewColumn? column) || column == null)
                return;

            configure(column);
        }

        public static void HideColumn(DataGridView grid, string columnName)
        {
            ConfigureColumn(grid, columnName, column => column.Visible = false);
        }

        public static void SetDisplayIndex(DataGridView grid, string columnName, int displayIndex)
        {
            ConfigureColumn(grid, columnName, column => column.DisplayIndex = displayIndex);
        }
    }
}
