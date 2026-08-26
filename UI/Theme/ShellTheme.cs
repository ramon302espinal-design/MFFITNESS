using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Tema del shell principal: sidebar, header y dashboard KPI.
    /// </summary>
    public static class ShellTheme
    {
        private static Button? _activeNavButton;
        private static readonly Dictionary<Button, (string Glyph, string Label)> NavButtonData = new();

        private static void EnableUserPaint(Control control)
        {
            try
            {
                typeof(Control).InvokeMember(
                    "SetStyle",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                    null,
                    control,
                    new object[] { ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true });
            }
            catch
            {
                // Si falla, el botón usará texto normal con ícono unicode.
            }
        }

        public static void ApplyToMainShell(Form form, Panel sidebar, Panel contentArea, params Control[] statCards)
        {
            form.BackColor = AppTheme.Background;
            sidebar.BackColor = AppTheme.Sidebar;
            ApplyContentArea(contentArea);

            foreach (var card in statCards.OfType<Panel>())
                card.BackColor = AppTheme.Surface;
        }

        public static void StyleNavButton(Button btn, string? iconPrefix = null)
        {
            if (!string.IsNullOrEmpty(iconPrefix) && !btn.Text.StartsWith(iconPrefix, StringComparison.Ordinal))
                btn.Text = $"{iconPrefix}  {btn.Text.Trim()}";

            ApplyNavButtonBase(btn, isLogout: false);
        }

        public static void StyleNavButtonWithIcon(Button btn, string iconGlyph, string? displayText = null)
        {
            string label = displayText ?? StripLeadingEmoji(btn.Text);
            bool isLogout = btn.Name.Contains("Cerrar", StringComparison.OrdinalIgnoreCase);

            if (!NavIcons.IsAvailable())
            {
                btn.Text = label;
                ApplyNavButtonBase(btn, isLogout);
                return;
            }

            NavButtonData[btn] = (iconGlyph, label);
            btn.Text = string.Empty;
            btn.Tag = "nav-icon";
            btn.BackColor = AppTheme.Sidebar;

            ApplyNavButtonBase(btn, preserveTag: true, isLogout: isLogout);
            EnableUserPaint(btn);

            btn.Paint -= NavButton_Paint;
            btn.Paint += NavButton_Paint;
        }

        private static string StripLeadingEmoji(string text)
        {
            text = text.Trim();
            string[] prefixes = { "💳", "👤", "💰", "📦", "📊", "📋", "📈", "⚠", "⏻" };
            bool changed;
            do
            {
                changed = false;
                foreach (var prefix in prefixes)
                {
                    if (text.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        text = text[prefix.Length..].TrimStart();
                        changed = true;
                    }
                }
            } while (changed);

            return text;
        }

        private static void ApplyNavButtonBase(Button btn, bool preserveTag = false, bool isLogout = false)
        {
            if (!preserveTag) btn.Tag = "nav";

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = AppTheme.Sidebar;
            btn.ForeColor = isLogout ? AppTheme.Error : AppTheme.TextOnDark;
            btn.Font = AppTheme.FontBodyBold;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(40, 0, 8, 0);
            btn.Height = Math.Max(btn.Height, 44);
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter -= NavButton_MouseEnter;
            btn.MouseLeave -= NavButton_MouseLeave;
            btn.MouseEnter += NavButton_MouseEnter;
            btn.MouseLeave += NavButton_MouseLeave;

            if (!isLogout)
            {
                btn.Click -= NavButton_Click;
                btn.Click += NavButton_Click;
            }
        }

        private static void NavButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!NavButtonData.TryGetValue(btn, out var data)) return;

            var bg = btn.BackColor;
            if (bg == Color.Transparent || bg.A == 0)
                bg = AppTheme.Sidebar;

            e.Graphics.Clear(bg);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var iconColor = btn == _activeNavButton ? AppTheme.TextOnPrimary : btn.ForeColor;
            using var iconBrush = new SolidBrush(iconColor);
            using var textBrush = new SolidBrush(btn.ForeColor);

            var iconRect = new RectangleF(14, (btn.Height - 20) / 2f, 22, 22);
            e.Graphics.DrawString(data.Glyph, NavIcons.IconFont, iconBrush, iconRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            var textRect = new RectangleF(40, 0, Math.Max(20, btn.Width - 48), btn.Height);
            ThemeFonts.DrawString(e.Graphics, data.Label, btn.Font, textBrush, textRect,
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });

            // Borde minimalista (no altera fondo ni fuente)
            LuxuryMinimalButtonPaint.DrawBorder(e.Graphics, btn.ClientRectangle);
        }

        public static void SetActiveNavButton(Button active)
        {
            var previous = _activeNavButton;
            if (previous != null && previous != active)
            {
                previous.BackColor = AppTheme.Sidebar;
                previous.ForeColor = AppTheme.TextOnDark;
                previous.Invalidate();
            }

            _activeNavButton = active;
            active.BackColor = AppTheme.SidebarActive;
            active.ForeColor = AppTheme.TextOnPrimary;
            active.Invalidate();
        }

        private static void NavButton_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn != _activeNavButton)
            {
                btn.BackColor = AppTheme.SidebarHover;
                btn.Invalidate();
            }
        }

        private static void NavButton_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn != _activeNavButton)
            {
                btn.BackColor = AppTheme.Sidebar;
                btn.Invalidate();
            }
        }

        private static void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
                SetActiveNavButton(btn);
        }

        public static void ApplyContentArea(Panel content)
        {
            content.BackColor = AppTheme.Background;
            content.Padding = new Padding(AppTheme.SpacingLg);
        }

        public static void ApplyDashboardHeader(Label titleLabel, Label? subtitleLabel = null)
        {
            titleLabel.Tag = "title";
            titleLabel.Font = AppTheme.FontTitle;
            titleLabel.ForeColor = AppTheme.TextPrimary;
            titleLabel.Text = string.IsNullOrWhiteSpace(titleLabel.Text) ? "Panel de control" : titleLabel.Text;

            if (subtitleLabel != null)
            {
                subtitleLabel.Tag = "subtitle";
                subtitleLabel.Font = AppTheme.FontBody;
                subtitleLabel.ForeColor = AppTheme.TextSecondary;
            }
        }

        public static Image? LoadLogo()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var icoPaths = new[]
            {
                Path.Combine(baseDir, "Resources", "IMG_1722.ico"),
                Path.Combine(baseDir, "IMG_1722.ico"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "IMG_1722.ico")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "IMG_1722.ico"))
            };

            foreach (string path in icoPaths)
            {
                if (!File.Exists(path))
                    continue;

                try
                {
                    // new Icon(path) carga 32x32 por defecto → pixelado en Zoom.
                    // Extraemos el frame más grande del .ico (p. ej. 256x256 PNG).
                    Image? best = ExtractLargestIconImage(path);
                    if (best != null)
                        return best;
                }
                catch { /* ignore */ }
            }

            var pngPaths = new[]
            {
                Path.Combine(baseDir, "Resources", "mf_logo.png"),
                Path.Combine(baseDir, "mf_logo.png"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "mf_logo.png"))
            };

            foreach (var path in pngPaths)
            {
                if (File.Exists(path))
                {
                    try { return Image.FromFile(path); }
                    catch { /* ignore */ }
                }
            }

            return null;
        }

        /// <summary>
        /// Lee el directorio del .ico y decodifica la imagen de mayor resolución
        /// (PNG embebido o bitmap). Evita el 32×32 por defecto de System.Drawing.Icon.
        /// </summary>
        private static Image? ExtractLargestIconImage(string icoPath)
        {
            byte[] data = File.ReadAllBytes(icoPath);
            if (data.Length < 6)
                return null;

            ushort type = BitConverter.ToUInt16(data, 2);
            ushort count = BitConverter.ToUInt16(data, 4);
            if (type != 1 || count == 0)
                return null;

            int bestArea = -1;
            int bestOffset = 0;
            int bestSize = 0;
            int bestW = 0;
            int bestH = 0;

            for (int i = 0; i < count; i++)
            {
                int entry = 6 + (i * 16);
                if (entry + 16 > data.Length)
                    break;

                int w = data[entry] == 0 ? 256 : data[entry];
                int h = data[entry + 1] == 0 ? 256 : data[entry + 1];
                int size = BitConverter.ToInt32(data, entry + 8);
                int offset = BitConverter.ToInt32(data, entry + 12);
                int area = w * h;

                if (area <= bestArea || size <= 0 || offset < 0 || offset + size > data.Length)
                    continue;

                bestArea = area;
                bestOffset = offset;
                bestSize = size;
                bestW = w;
                bestH = h;
            }

            if (bestArea <= 0)
                return null;

            // Vista+ ICO: payload PNG (firma 89 50 4E 47).
            if (bestSize >= 8
                && data[bestOffset] == 0x89
                && data[bestOffset + 1] == 0x50
                && data[bestOffset + 2] == 0x4E
                && data[bestOffset + 3] == 0x47)
            {
                using var ms = new MemoryStream(data, bestOffset, bestSize, writable: false);
                using var decoded = Image.FromStream(ms);
                return new Bitmap(decoded);
            }

            // Fallback GDI: pedir el tamaño del mejor entry (puede bajar a 128).
            try
            {
                using var icon = new Icon(icoPath, bestW, bestH);
                return icon.ToBitmap();
            }
            catch
            {
                using var icon = new Icon(icoPath);
                return icon.ToBitmap();
            }
        }

        /// <summary>
        /// Asigna el icono de la app al formulario si el archivo existe (sin lanzar excepciones).
        /// Busca Resources\IMG_1722.ico junto al exe (salida de build) y variantes de desarrollo.
        /// </summary>
        public static void TryApplyFormIcon(Form form)
        {
            if (form == null || form.IsDisposed)
                return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var paths = new[]
            {
                Path.Combine(baseDir, "Resources", "IMG_1722.ico"),
                Path.Combine(baseDir, "IMG_1722.ico"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "IMG_1722.ico")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "IMG_1722.ico"))
            };

            foreach (string path in paths)
            {
                if (!File.Exists(path))
                    continue;

                try
                {
                    // Clonar el icono para no mantener bloqueado el archivo en disco.
                    using var loaded = new Icon(path);
                    form.Icon = (Icon)loaded.Clone();
                    return;
                }
                catch
                {
                    // Fail-soft: sin icono es preferible a tumbar el arranque.
                }
            }
        }

        public static void ShowThemedMessage(IWin32Window owner, string message, string title, MessageBoxIcon icon)
        {
            DialogType type = icon switch
            {
                MessageBoxIcon.Warning => DialogType.Warning,
                MessageBoxIcon.Error => DialogType.Error,
                MessageBoxIcon.Question => DialogType.Question,
                MessageBoxIcon.Information => DialogType.Info,
                _ => DialogType.Info
            };
            AppDialog.Show(owner, message, title, type);
        }
    }
}
