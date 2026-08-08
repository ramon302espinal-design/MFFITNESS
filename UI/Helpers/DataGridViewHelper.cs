using System;
using System.Windows.Forms;

namespace UI.Helpers
{
    public static class DataGridViewHelper
    {
        public static void ConfigureColumn(DataGridView grid, string columnName, Action<DataGridViewColumn> configure)
        {
            if (grid.Columns[columnName] is DataGridViewColumn column)
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
