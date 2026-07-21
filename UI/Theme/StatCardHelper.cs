using System.Drawing;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Layout consistente para tarjetas KPI del dashboard.
    /// </summary>
    public static class StatCardHelper
    {
        public static void Configure(Panel card, Label titleLabel, Label valueLabel, Color accent, string titleText)
        {
            card.SuspendLayout();
            card.Controls.Clear();

            card.BackColor = AppTheme.Surface;
            card.Margin = Padding.Empty;
            card.Padding = new Padding(18, 16, 14, 14);
            card.Size = new Size(PresentacionDashboardStyle.CardWidth, PresentacionDashboardStyle.CardHeight);
            card.MinimumSize = card.Size;
            card.MaximumSize = card.Size;

            titleLabel.Text = titleText;
            titleLabel.AutoSize = false;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.ForeColor = AppTheme.TextSecondary;
            titleLabel.Font = AppTheme.FontBodyBold;
            titleLabel.TextAlign = ContentAlignment.TopLeft;
            titleLabel.Padding = new Padding(8, 0, 4, 0);

            valueLabel.AutoSize = false;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.BackColor = Color.Transparent;
            valueLabel.ForeColor = accent;
            valueLabel.Font = AppTheme.FontStatValue;
            valueLabel.TextAlign = ContentAlignment.BottomLeft;
            valueLabel.Padding = new Padding(8, 0, 4, 4);
            if (string.IsNullOrWhiteSpace(valueLabel.Text))
                valueLabel.Text = "0";

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(valueLabel, 0, 1);

            card.Controls.Add(layout);
            ThemeApplier.ApplyStatCard(card, accent);

            card.ResumeLayout(true);
        }
    }
}
