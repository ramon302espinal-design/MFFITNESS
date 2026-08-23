using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLL;
using CORE;

namespace UI.Helpers
{
    /// <summary>
    /// Avisos de inventario: stock agotado (0) o en/bajo mínimo.
    /// Al abrir el programa + cada 15 min. Mute opcional ("No mostrar más")
    /// hasta que cambie el conjunto de productos críticos.
    /// </summary>
    public static class StockInventoryAlert
    {
        public const int IntervaloMinutos = 15;

        private static readonly object Sync = new();
        private static bool _dialogoVisible;

        private static string PrefsPath =>
            Path.Combine(FacturaStorage.CarpetaRaizMffitness, "stock-alert.prefs");

        /// <summary>
        /// Muestra el aviso si hay productos críticos y no está silenciado para esa huella.
        /// </summary>
        public static void TryShow(IWin32Window? owner)
        {
            lock (Sync)
            {
                if (_dialogoVisible)
                    return;
            }

            if (!TryBuildCriticalSummary(out string mensaje, out string fingerprint))
                return;

            if (IsMuted(fingerprint))
                return;

            lock (Sync)
            {
                if (_dialogoVisible)
                    return;
                _dialogoVisible = true;
            }

            try
            {
                using var dlg = new FrmAvisoStockInventario(mensaje);
                dlg.ShowDialog(owner);
                if (dlg.NoMostrarMas)
                    SaveMute(fingerprint);
            }
            finally
            {
                lock (Sync)
                    _dialogoVisible = false;
            }
        }

        /// <summary>
        /// Aviso inmediato de un producto (no respeta mute periódico).
        /// </summary>
        public static void AvisarProductoSiCritico(IWin32Window? owner, int productoId)
            => AvisarProductosSiCriticos(owner, new[] { productoId });

        /// <summary>
        /// Aviso inmediato agrupado (venta con varios ítems). No respeta mute periódico.
        /// </summary>
        public static void AvisarProductosSiCriticos(IWin32Window? owner, IEnumerable<int> productoIds)
        {
            var ids = productoIds.Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0)
                return;

            DataTable productos;
            try
            {
                productos = new ProductoBLL().ObtenerProductos();
            }
            catch
            {
                return;
            }

            var agotados = new List<string>();
            var enMinimo = new List<string>();

            foreach (DataRow row in productos.Rows)
            {
                int id = Convert.ToInt32(row["Id"]);
                if (!ids.Contains(id))
                    continue;

                if (!TryLeerStock(row, out string nombre, out int stock, out int minimo))
                    continue;

                if (stock <= 0)
                    agotados.Add(nombre);
                else if (stock <= minimo)
                    enMinimo.Add($"{nombre} (stock {stock} / mín. {minimo})");
            }

            if (agotados.Count == 0 && enMinimo.Count == 0)
                return;

            var sb = new StringBuilder();
            if (agotados.Count > 0)
            {
                sb.AppendLine("Producto(s) agotado(s):");
                foreach (string n in agotados)
                    sb.AppendLine("• " + n);
            }

            if (enMinimo.Count > 0)
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine("Producto(s) en o bajo el stock mínimo:");
                foreach (string n in enMinimo)
                    sb.AppendLine("• " + n);
            }

            MessageBox.Show(
                owner,
                sb.ToString().TrimEnd(),
                "Aviso de inventario",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        public static bool TryBuildCriticalSummary(out string mensaje, out string fingerprint)
        {
            mensaje = string.Empty;
            fingerprint = string.Empty;

            DataTable productos;
            try
            {
                productos = new ProductoBLL().ObtenerProductos();
            }
            catch
            {
                return false;
            }

            if (productos.Rows.Count == 0)
                return false;

            var agotados = new List<(int Id, string Nombre)>();
            var enMinimo = new List<(int Id, string Nombre, int Stock, int Minimo)>();

            foreach (DataRow row in productos.Rows)
            {
                if (!TryLeerStock(row, out string nombre, out int stock, out int minimo))
                    continue;

                int id = Convert.ToInt32(row["Id"]);
                if (stock <= 0)
                    agotados.Add((id, nombre));
                else if (stock <= minimo)
                    enMinimo.Add((id, nombre, stock, minimo));
            }

            if (agotados.Count == 0 && enMinimo.Count == 0)
                return false;

            fingerprint = string.Join(",",
                agotados.Select(a => a.Id)
                    .Concat(enMinimo.Select(m => m.Id))
                    .Distinct()
                    .OrderBy(id => id));

            var sb = new StringBuilder();
            if (agotados.Count > 0)
            {
                sb.AppendLine("Productos agotados (sin stock):");
                foreach (var a in agotados.OrderBy(x => x.Nombre))
                    sb.AppendLine("• " + a.Nombre);
            }

            if (enMinimo.Count > 0)
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine("Productos en o bajo el stock mínimo:");
                foreach (var m in enMinimo.OrderBy(x => x.Nombre))
                    sb.AppendLine($"• {m.Nombre} (stock {m.Stock} / mín. {m.Minimo})");
            }

            mensaje = sb.ToString().TrimEnd();
            return true;
        }

        private static bool TryLeerStock(
            DataRow row,
            out string nombre,
            out int stock,
            out int minimo)
        {
            nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;
            stock = 0;
            minimo = 0;

            if (string.IsNullOrEmpty(nombre))
                return false;

            object? stockObj = row["StockActual"];
            object? minObj = row["StockMinimo"];
            if (stockObj == null || stockObj == DBNull.Value
                || minObj == null || minObj == DBNull.Value)
                return false;

            stock = Convert.ToInt32(stockObj);
            minimo = Convert.ToInt32(minObj);
            return true;
        }

        private static bool IsMuted(string fingerprint)
        {
            try
            {
                if (!File.Exists(PrefsPath))
                    return false;

                string mutedFp = string.Empty;
                bool muted = false;
                foreach (string line in File.ReadAllLines(PrefsPath))
                {
                    if (line.StartsWith("Muted=", StringComparison.OrdinalIgnoreCase))
                        muted = string.Equals(line.Substring(6).Trim(), "1", StringComparison.Ordinal);
                    else if (line.StartsWith("Fingerprint=", StringComparison.OrdinalIgnoreCase))
                        mutedFp = line.Substring(12).Trim();
                }

                return muted && string.Equals(mutedFp, fingerprint, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static void SaveMute(string fingerprint)
        {
            try
            {
                Directory.CreateDirectory(FacturaStorage.CarpetaRaizMffitness);
                File.WriteAllText(
                    PrefsPath,
                    "Muted=1" + Environment.NewLine + "Fingerprint=" + fingerprint + Environment.NewLine);
            }
            catch
            {
                // Preferencia best-effort; no bloquear UI.
            }
        }
    }

    /// <summary>Diálogo de aviso de stock con opción "No mostrar más".</summary>
    internal sealed class FrmAvisoStockInventario : Form
    {
        private readonly CheckBox _chkNoMostrar;

        public bool NoMostrarMas => _chkNoMostrar.Checked;

        public FrmAvisoStockInventario(string mensaje)
        {
            Text = "Aviso de inventario";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(460, 320);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            var lblTitle = new Label
            {
                Text = "Stock crítico",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 32, 44),
                Location = new Point(16, 12),
                AutoSize = true
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(16, 48),
                Size = new Size(428, 190),
                Text = mensaje,
                BackColor = Color.FromArgb(247, 249, 252)
            };

            _chkNoMostrar = new CheckBox
            {
                Text = "No mostrar más",
                Location = new Point(16, 250),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 55, 72)
            };

            var btnOk = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Size = new Size(110, 32),
                Location = new Point(334, 272),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Controls.Add(lblTitle);
            Controls.Add(txt);
            Controls.Add(_chkNoMostrar);
            Controls.Add(btnOk);
            AcceptButton = btnOk;
        }
    }
}
