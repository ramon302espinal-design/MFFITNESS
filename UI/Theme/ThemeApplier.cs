using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI.Theme
{
    public static class ThemeApplier
    {
        private static readonly HashSet<Control> StyledControls = new();
        private static readonly HashSet<DataGridView> ReadOnlyGridsHooked = new();
        private static readonly Dictionary<Panel, Color> StatCardAccents = new();

        private static readonly HashSet<string> SkipCardPanels = new(StringComparer.OrdinalIgnoreCase)
        {
            "panel1", "panelDashboard", "panelHeader",
            "panelFormulario", "panelBotones", "panelCard", "tableLayoutPanel1"
        };

        public static void ApplyToForm(Form form)
        {
            form.BackColor = AppTheme.Background;
            form.Font = AppTheme.FontBody;
            form.ForeColor = AppTheme.TextPrimary;
            ApplyToControlTree(form);
        }

        public static void ApplyToControlTree(Control root)
        {
            if (EsClasico(root))
                return;

            foreach (Control c in root.Controls)
            {
                if (EsClasico(c))
                    continue;

                StyleControl(c);
                if (c.HasChildren)
                    ApplyToControlTree(c);
            }
        }

        /// <summary>
        /// Tag "classic"/"standard": el control (y su subárbol) queda tal cual lo dejó
        /// el diseñador WinForms. Evita que el tema reescriba una pantalla clásica.
        /// </summary>
        private static bool EsClasico(Control control) =>
            control.Tag is string tag
            && (tag.Equals("classic", StringComparison.OrdinalIgnoreCase)
                || tag.Equals("standard", StringComparison.OrdinalIgnoreCase));

        private static void StyleControl(Control control)
        {
            switch (control)
            {
                case Button btn:
                    StyleButton(btn);
                    break;
                case Panel p:
                    StylePanel(p);
                    break;
                case Label lbl:
                    StyleLabel(lbl);
                    break;
                case TextBox tb:
                    StyleTextBox(tb);
                    break;
                case ComboBox cb:
                    StyleComboBox(cb);
                    break;
                case DataGridView dgv:
                    StyleDataGridView(dgv);
                    break;
                case TabControl tc:
                    StyleTabControl(tc);
                    break;
                case GroupBox gb:
                    StyleGroupBox(gb);
                    break;
                case MenuStrip ms:
                    StyleMenuStrip(ms);
                    break;
                case ToolStrip ts:
                    StyleToolStrip(ts);
                    break;
                case CheckBox chk:
                    StyleCheckBox(chk);
                    break;
                case NumericUpDown nud:
                    StyleNumericUpDown(nud);
                    break;
                case DateTimePicker dtp:
                    StyleDateTimePicker(dtp);
                    break;
            }
        }

        public static void StyleButton(Button btn, ButtonVariant? variant = null)
        {
            // Botones clásicos WinForms (diseñador): no aplanar ni redondear.
            if (btn.Tag?.ToString() is "classic" or "standard") return;
            if (btn.Tag?.ToString() is "nav-icon" or "nav" or "nav-back-wired" or "nav-btn-wired") return;
            // Barra superior de módulos: no aplicar región redondeada (rompe el clic, p. ej. Back)
            if (btn.Name is "btnBack" or "btnNavBack" || btn.Name.StartsWith("btnNav", StringComparison.Ordinal))
                return;
            if (IsSidebarButton(btn)) return;
            if (StyledControls.Contains(btn)) return;
            StyledControls.Add(btn);

            var v = variant ?? DetectButtonVariant(btn);
            var (back, fore, hover, pressed) = GetButtonColors(v);

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.Font = v == ButtonVariant.Nav ? AppTheme.FontNav : AppTheme.FontButton;
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(10, 4, 10, 4);
            btn.UseVisualStyleBackColor = false;

            EnsureButtonFitsText(btn);

            if (v != ButtonVariant.Nav)
                ApplyRoundedRegion(btn, AppTheme.RadiusMedium);

            btn.MouseEnter += (_, _) =>
            {
                if (btn.Enabled) btn.BackColor = hover;
            };
            btn.MouseLeave += (_, _) =>
            {
                if (btn.Enabled) btn.BackColor = back;
            };
            btn.MouseDown += (_, _) =>
            {
                if (btn.Enabled) btn.BackColor = pressed;
            };
            btn.MouseUp += (_, _) =>
            {
                if (btn.Enabled) btn.BackColor = btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)) ? hover : back;
            };
            btn.EnabledChanged += (_, _) =>
            {
                btn.ForeColor = btn.Enabled ? fore : AppTheme.TextMuted;
                btn.BackColor = btn.Enabled ? back : AppTheme.Border;
                btn.Cursor = btn.Enabled ? Cursors.Hand : Cursors.Default;
            };
        }

        private static (Color back, Color fore, Color hover, Color pressed) GetButtonColors(ButtonVariant v) => v switch
        {
            ButtonVariant.Primary => (AppTheme.Primary, AppTheme.TextOnPrimary, AppTheme.PrimaryLight, AppTheme.PrimaryDark),
            ButtonVariant.Success => (AppTheme.Success, AppTheme.TextOnPrimary, Color.FromArgb(52, 211, 102), AppTheme.SuccessDark),
            ButtonVariant.Danger => (AppTheme.Error, AppTheme.TextOnPrimary, Color.FromArgb(248, 113, 113), Color.FromArgb(185, 28, 28)),
            ButtonVariant.Warning => (AppTheme.Warning, AppTheme.TextPrimary, Color.FromArgb(251, 191, 36), Color.FromArgb(217, 119, 6)),
            ButtonVariant.Secondary => (AppTheme.Surface, AppTheme.TextPrimary, AppTheme.SurfaceElevated, AppTheme.Border),
            ButtonVariant.Nav => (Color.Transparent, AppTheme.TextOnDark, AppTheme.SidebarHover, AppTheme.SidebarHover),
            _ => (AppTheme.Surface, AppTheme.TextPrimary, AppTheme.SurfaceElevated, AppTheme.Border)
        };

        private static ButtonVariant DetectButtonVariant(Button btn)
        {
            var name = btn.Name.ToLowerInvariant();
            var text = btn.Text.ToLowerInvariant();

            if (IsSidebarButton(btn) || btn.Tag?.ToString() == "nav-icon")
                return ButtonVariant.Nav;

            if (name.Contains("eliminar") || name.Contains("delete") || name.Contains("anular") ||
                text.Contains("eliminar") || text.Contains("anular") || name.Contains("deuda"))
                return ButtonVariant.Danger;

            if (name.Contains("guardar") || name.Contains("pagar") || name.Contains("cobrar") || name.Contains("activar") ||
                name.Contains("confirm") || text.Contains("guardar") || text.Contains("pagar") || text.Contains("cobrar"))
                return ButtonVariant.Success;

            if (name.Contains("nuevo") || name.Contains("agregar") || name.Contains("login") ||
                name.Contains("ingresar") || text.Contains("nuevo"))
                return ButtonVariant.Primary;

            if (name.Contains("cancel") || name.Contains("cerrar") || name.Contains("salir"))
                return ButtonVariant.Secondary;

            return ButtonVariant.Default;
        }

        public static void StylePanel(Panel panel)
        {
            if (SkipCardPanels.Contains(panel.Name)) return;

            var isCard = panel.Tag?.ToString() == "card" ||
                         panel.Name.StartsWith("card", StringComparison.OrdinalIgnoreCase) ||
                         panel.Name.StartsWith("pnl", StringComparison.OrdinalIgnoreCase) ||
                         (panel.Name.StartsWith("panel", StringComparison.OrdinalIgnoreCase) &&
                          panel.Name is not ("panelDashboard" or "panel1"));

            if (isCard && panel.BackColor != Color.Transparent)
            {
                panel.BackColor = AppTheme.Surface;
                panel.Padding = panel.Padding == Padding.Empty ? new Padding(AppTheme.SpacingMd) : panel.Padding;
                panel.Paint -= CardPanel_Paint;
                panel.Paint += CardPanel_Paint;
            }
        }

        private static void CardPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = CreateRoundedPath(rect, AppTheme.RadiusLarge);
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawPath(pen, path);
        }

        public static void StyleLabel(Label lbl)
        {
            if (lbl.Tag?.ToString() == "title")
            {
                lbl.Font = AppTheme.FontTitle;
                lbl.ForeColor = AppTheme.TextPrimary;
            }
            else if (lbl.Tag?.ToString() == "subtitle")
            {
                lbl.Font = AppTheme.FontSubtitle;
                lbl.ForeColor = AppTheme.TextSecondary;
            }
            else if (lbl.Tag?.ToString() == "stat-value")
            {
                lbl.Font = AppTheme.FontStatValue;
                lbl.ForeColor = AppTheme.TextPrimary;
            }
            else if (lbl.Tag?.ToString() == "stat-label")
            {
                lbl.Font = AppTheme.FontStatLabel;
                lbl.ForeColor = AppTheme.TextSecondary;
            }
            else if (lbl.ForeColor == SystemColors.ControlText || lbl.ForeColor == Color.Black)
            {
                lbl.ForeColor = AppTheme.TextPrimary;
            }
        }

        public static void StyleTextBox(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = AppTheme.Surface;
            tb.ForeColor = AppTheme.TextPrimary;
            tb.Font = AppTheme.FontBody;

            tb.Enter += (_, _) => tb.BackColor = AppTheme.SurfaceElevated;
            tb.Leave += (_, _) => tb.BackColor = AppTheme.Surface;
        }

        public static void StyleComboBox(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = AppTheme.Surface;
            cb.ForeColor = AppTheme.TextPrimary;
            cb.Font = AppTheme.FontBody;
        }

        public static void StyleDataGridView(DataGridView dgv, bool attachEmptyState = true)
        {
            dgv.BackgroundColor = AppTheme.Surface;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = AppTheme.Border;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DefaultCellStyle.BackColor = AppTheme.Surface;
            dgv.DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = AppTheme.TextPrimary;
            dgv.DefaultCellStyle.Font = AppTheme.FontBody;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.SurfaceElevated;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Secondary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppTheme.TextOnDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBodyBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
            dgv.ColumnHeadersHeight = 44;
            dgv.RowTemplate.Height = 38;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Formato12Horas.Aplicar(dgv);
            ApplyReadOnlyGridBehavior(dgv);
            if (attachEmptyState)
                EmptyStateHelper.Attach(dgv);
        }

        /// <summary>
        /// Evita edición inline al hacer clic en celdas. Usar Tag = "editable-grid" para excluir un grid.
        /// </summary>
        public static void ApplyReadOnlyGridBehavior(DataGridView dgv)
        {
            if (dgv.Tag?.ToString() == "editable-grid")
                return;

            dgv.ReadOnly = true;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;

            if (dgv.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                dgv.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
            {
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }

            AsegurarColumnasSoloLectura(dgv);

            if (ReadOnlyGridsHooked.Add(dgv))
            {
                dgv.CellBeginEdit += ReadOnlyGrid_CellBeginEdit;
                dgv.DataBindingComplete += ReadOnlyGrid_DataBindingComplete;
            }
        }

        private static void ReadOnlyGrid_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static void ReadOnlyGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is DataGridView dgv)
                AsegurarColumnasSoloLectura(dgv);
        }

        private static void AsegurarColumnasSoloLectura(DataGridView dgv)
        {
            if (dgv.Tag?.ToString() == "editable-grid")
                return;

            dgv.ReadOnly = true;

            foreach (DataGridViewColumn columna in dgv.Columns)
                columna.ReadOnly = true;
        }

        public static void StyleTabControl(TabControl tc)
        {
            tc.Font = AppTheme.FontBodyBold;
            tc.Padding = new Point(AppTheme.SpacingMd, 6);
            foreach (TabPage page in tc.TabPages)
            {
                page.BackColor = AppTheme.Background;
                page.ForeColor = AppTheme.TextPrimary;
            }
        }

        public static void StyleGroupBox(GroupBox gb)
        {
            gb.ForeColor = AppTheme.TextSecondary;
            gb.Font = AppTheme.FontBodyBold;
            gb.BackColor = Color.Transparent;
        }

        public static void StyleMenuStrip(MenuStrip ms)
        {
            ms.BackColor = AppTheme.Surface;
            ms.ForeColor = AppTheme.TextPrimary;
            ms.Font = AppTheme.FontBody;
            ms.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
        }

        public static void StyleToolStrip(ToolStrip ts)
        {
            ts.BackColor = AppTheme.Surface;
            ts.ForeColor = AppTheme.TextPrimary;
            ts.Font = AppTheme.FontBody;
            ts.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
        }

        public static void StyleCheckBox(CheckBox chk)
        {
            chk.ForeColor = AppTheme.TextPrimary;
            chk.Font = AppTheme.FontBody;
            chk.FlatStyle = FlatStyle.Flat;
        }

        public static void StyleNumericUpDown(NumericUpDown nud)
        {
            nud.BorderStyle = BorderStyle.FixedSingle;
            nud.BackColor = AppTheme.Surface;
            nud.ForeColor = AppTheme.TextPrimary;
            nud.Font = AppTheme.FontBody;
        }

        public static void StyleDateTimePicker(DateTimePicker dtp)
        {
            dtp.Font = AppTheme.FontBody;
            dtp.CalendarForeColor = AppTheme.TextPrimary;
            dtp.CalendarMonthBackground = AppTheme.Surface;
        }

        public static void ApplyStatCard(Panel card, Color accent, Label? titleLabel = null, Label? valueLabel = null)
        {
            card.Tag = "card";
            StatCardAccents[card] = accent;
            card.BackColor = AppTheme.Surface;

            card.Paint -= StatCard_Paint;
            card.Paint += StatCard_Paint;

            if (titleLabel != null)
            {
                titleLabel.Tag = "stat-label";
                titleLabel.ForeColor = AppTheme.TextSecondary;
                titleLabel.Font = AppTheme.FontStatLabel;
                titleLabel.BackColor = Color.Transparent;
            }
            if (valueLabel != null)
            {
                valueLabel.Tag = "stat-value";
                valueLabel.ForeColor = accent;
                valueLabel.Font = AppTheme.FontStatValue;
                valueLabel.BackColor = Color.Transparent;
            }
        }

        private static void StatCard_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            var accent = StatCardAccents.TryGetValue(p, out var c) ? c : AppTheme.Primary;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = CreateRoundedPath(rect, AppTheme.RadiusLarge);
            using var pen = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawPath(pen, path);

            using var accentBrush = new SolidBrush(accent);
            e.Graphics.FillRectangle(accentBrush, 0, 0, 4, p.Height);
        }

        private static void EnsureButtonFitsText(Button btn)
        {
            const int minHeight = 40;
            if (btn.Height < minHeight)
                btn.Height = minHeight;

            var textSize = TextRenderer.MeasureText(
                btn.Text,
                btn.Font,
                new Size(int.MaxValue, minHeight),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            var minWidth = textSize.Width + btn.Padding.Horizontal + 12;
            if (btn.Width < minWidth)
                btn.Width = (int)Math.Ceiling((double)minWidth);
        }

        public static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0) return;

            try
            {
                using var path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
                control.Region = new Region(path);
            }
            catch
            {
                control.Region = null;
            }
        }

        public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            int d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static bool IsSidebarButton(Button btn)
        {
            Control? parent = btn.Parent;
            while (parent != null)
            {
                if (parent.Name is "panel1" or "pnlSidebarScroll" or "flowSidebarNav")
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        private class ThemeColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => AppTheme.PrimaryLight;
            public override Color MenuItemSelectedGradientBegin => AppTheme.PrimaryLight;
            public override Color MenuItemSelectedGradientEnd => AppTheme.PrimaryLight;
            public override Color MenuItemBorder => AppTheme.Border;
            public override Color ToolStripDropDownBackground => AppTheme.Surface;
            public override Color ImageMarginGradientBegin => AppTheme.Surface;
            public override Color ImageMarginGradientMiddle => AppTheme.Surface;
            public override Color ImageMarginGradientEnd => AppTheme.Surface;
        }
    }

    public enum ButtonVariant
    {
        Default,
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Nav
    }
}
