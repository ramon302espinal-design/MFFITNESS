using System;
using System.Drawing;
using System.Linq;

namespace UI.Theme
{
    /// <summary>
    /// Paleta de marca MFFITNESS extraída del logo (azul eléctrico + negro).
    /// IMPORTANTE: Las fuentes son recursos estáticos compartidos — nunca usar "using" ni Dispose sobre ellas.
    /// </summary>
    public static class AppTheme
    {
        // Marca
        public static readonly Color Primary = Color.FromArgb(30, 144, 255);
        public static readonly Color PrimaryDark = Color.FromArgb(21, 112, 199);
        public static readonly Color PrimaryLight = Color.FromArgb(77, 166, 255);
        public static readonly Color Secondary = Color.FromArgb(10, 10, 10);
        public static readonly Color Accent = Color.FromArgb(30, 144, 255);

        // Semánticos
        public static readonly Color Success = Color.FromArgb(34, 197, 94);
        public static readonly Color SuccessDark = Color.FromArgb(22, 163, 74);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);
        public static readonly Color Error = Color.FromArgb(239, 68, 68);
        public static readonly Color Info = Color.FromArgb(59, 130, 246);

        // Texto
        public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        public static readonly Color TextOnDark = Color.White;
        public static readonly Color TextOnPrimary = Color.White;

        // Fondos
        public static readonly Color Background = Color.FromArgb(241, 245, 249);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceElevated = Color.FromArgb(248, 250, 252);
        public static readonly Color Sidebar = Color.FromArgb(12, 12, 14);
        public static readonly Color SidebarHover = Color.FromArgb(30, 30, 35);
        public static readonly Color SidebarActive = Color.FromArgb(30, 144, 255);

        // Bordes
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color BorderFocus = Primary;
        public static readonly Color Divider = Color.FromArgb(241, 245, 249);

        // Tipografía GDI+ segura (Segoe UI — compatible con DrawString en controles custom)
        public static readonly Font FontTitle = CreateSafeFont(22F, FontStyle.Bold);
        public static readonly Font FontSubtitle = CreateSafeFont(13F, FontStyle.Bold);
        public static readonly Font FontBody = CreateSafeFont(10F, FontStyle.Regular);
        public static readonly Font FontBodyBold = CreateSafeFont(10F, FontStyle.Bold);
        public static readonly Font FontCaption = CreateSafeFont(9F, FontStyle.Regular);
        public static readonly Font FontButton = CreateSafeFont(10F, FontStyle.Bold);
        public static readonly Font FontNav = CreateSafeFont(10.5F, FontStyle.Bold);
        public static readonly Font FontStatValue = CreateSafeFont(26F, FontStyle.Bold);
        public static readonly Font FontStatLabel = CreateSafeFont(9F, FontStyle.Bold);

        // Geometría
        public const int RadiusSmall = 6;
        public const int RadiusMedium = 10;
        public const int RadiusLarge = 14;
        public const int SpacingSm = 8;
        public const int SpacingMd = 16;
        public const int SpacingLg = 24;

        private static Font CreateSafeFont(float size, FontStyle style)
        {
            try
            {
                var font = new Font("Segoe UI", size, style, GraphicsUnit.Point);
                ThemeFonts.ValidateFont(font);
                return font;
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily, size, style, GraphicsUnit.Point);
            }
        }
    }
}
