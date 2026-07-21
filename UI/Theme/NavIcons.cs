using System;
using System.Drawing;
using System.Drawing.Text;

namespace UI.Theme
{
    /// <summary>
    /// Iconos vectoriales Segoe MDL2 Assets para navegación.
    /// </summary>
    public static class NavIcons
    {
        public const string FontFamily = "Segoe MDL2 Assets";

        public const string Pos = "\uE8A7";
        public const string Client = "\uE716";
        public const string Cash = "\uE825";
        public const string Inventory = "\uE7BF";
        public const string Status = "\uE9D2";
        public const string History = "\uE81C";
        public const string Reports = "\uE9F9";
        public const string Debts = "\uE7BA";
        public const string Logout = "\uE8AC";
        public const string Dashboard = "\uE80F";

        public static Font IconFont { get; } = CreateIconFont(14F);
        public static Font IconFontLarge { get; } = CreateIconFont(18F);

        public static bool IsAvailable()
        {
            try
            {
                ValidateIconFont(IconFont);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Font CreateIconFont(float size)
        {
            try
            {
                if (!IsFontFamilyInstalled(FontFamily))
                    return AppTheme.FontBody;

                var font = new Font(FontFamily, size, FontStyle.Regular, GraphicsUnit.Point);
                ValidateIconFont(font);
                return font;
            }
            catch
            {
                return AppTheme.FontBody;
            }
        }

        private static void ValidateIconFont(Font font)
        {
            using var bmp = new Bitmap(4, 4);
            using var g = Graphics.FromImage(bmp);
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.DrawString("\uE8A7", font, Brushes.White, 0, 0);
        }

        private static bool IsFontFamilyInstalled(string family)
        {
            using var fonts = new InstalledFontCollection();
            foreach (var f in fonts.Families)
            {
                if (string.Equals(f.Name, family, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
