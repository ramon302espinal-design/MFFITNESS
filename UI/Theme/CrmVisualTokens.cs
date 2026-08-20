using System.Drawing;
using System.Windows.Forms;

namespace UI.Theme
{
    /// <summary>
    /// Tokens visuales oficiales del CRM Financiero (FASE 2.3 congelada).
    /// No reemplaza AppTheme del POS; el CRM usa shell claro y Brand 0,122,204.
    /// Fuentes estáticas compartidas: no Dispose.
    /// </summary>
    public static class CrmVisualTokens
    {
        // ——— Colores base ———
        public static readonly Color BgPrimary = Color.FromArgb(247, 249, 252);
        public static readonly Color BgSecondary = Color.FromArgb(248, 250, 252);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceMuted = Color.FromArgb(245, 247, 250);

        public static readonly Color TextPrimary = Color.FromArgb(26, 32, 44);
        public static readonly Color TextSecondary = Color.FromArgb(45, 55, 72);
        public static readonly Color TextMuted = Color.FromArgb(113, 128, 150);

        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color Divider = Color.FromArgb(241, 245, 249);

        // ——— Marca / interacción ———
        public static readonly Color Brand = Color.FromArgb(0, 122, 204);
        public static readonly Color BrandHover = Color.FromArgb(0, 102, 176);
        public static readonly Color BrandPressed = Color.FromArgb(0, 86, 153);
        public static readonly Color OnBrand = Color.White;

        // ——— Semánticos ———
        public static readonly Color Success = Color.FromArgb(22, 163, 74);
        public static readonly Color SuccessBg = Color.FromArgb(240, 253, 244);
        public static readonly Color Warning = Color.FromArgb(217, 119, 6);
        public static readonly Color WarningBg = Color.FromArgb(255, 251, 235);
        public static readonly Color Danger = Color.FromArgb(185, 28, 28);
        public static readonly Color DangerBg = Color.FromArgb(254, 242, 242);
        public static readonly Color Info = Color.FromArgb(59, 130, 246);
        public static readonly Color InfoBg = Color.FromArgb(239, 246, 255);

        // ——— Navegación ———
        public static readonly Color NavIdleBg = SurfaceMuted;
        public static readonly Color NavIdleFg = TextSecondary;
        public static readonly Color NavActiveBg = Surface;
        public static readonly Color NavActiveFg = TextPrimary;
        public static readonly Color Selection = Brand;

        // ——— Tipografía ———
        public static readonly Font FontModule = CreateFont(14F, FontStyle.Bold);
        public static readonly Font FontSection = CreateFont(12F, FontStyle.Bold);
        public static readonly Font FontCard = CreateFont(10F, FontStyle.Bold);
        public static readonly Font FontBody = CreateFont(9.5F, FontStyle.Regular);
        public static readonly Font FontButton = CreateFont(9F, FontStyle.Bold);
        public static readonly Font FontValueLg = CreateFont(18F, FontStyle.Bold);
        public static readonly Font FontValueMd = CreateFont(14F, FontStyle.Bold);
        public static readonly Font FontCaption = CreateFont(8.5F, FontStyle.Regular);
        public static readonly Font FontNav = CreateFont(9.5F, FontStyle.Regular);

        // ——— Espaciado ———
        public const int SpaceXs = 4;
        public const int SpaceSm = 8;
        public const int SpaceMd = 12;
        public const int SpaceLg = 16;
        public const int SpaceXl = 24;
        public const int Space2Xl = 32;

        // ——— Alturas / anchos ———
        public const int HeightHeaderModule = 72;
        public const int HeightHeaderSection = 44;
        public const int HeightButton = 36;
        public const int HeightButtonSm = 28;
        public const int HeightNav = 36;
        public const int HeightKpiMin = 120;
        public const int HeightTableRow = 30;
        public const int HeightTableHeader = 34;
        public const int WidthSidebar = 220;

        /// <summary>Tag para que ThemeApplier no reescriba controles del CRM.</summary>
        public const string ClassicTag = "classic";

        public static void MarkClassic(Control control)
        {
            if (control != null)
                control.Tag = ClassicTag;
        }

        public static Color BorderForState(CrmVisualState state) => state switch
        {
            CrmVisualState.Positive => Success,
            CrmVisualState.Warning => Warning,
            CrmVisualState.Critical => Danger,
            _ => Border
        };

        public static Color HeaderBgForState(CrmVisualState state) => state switch
        {
            CrmVisualState.Positive => SuccessBg,
            CrmVisualState.Warning => WarningBg,
            CrmVisualState.Critical => DangerBg,
            _ => Surface
        };

        public static Color ForeForState(CrmVisualState state) => state switch
        {
            CrmVisualState.Positive => Success,
            CrmVisualState.Warning => Warning,
            CrmVisualState.Critical => Danger,
            CrmVisualState.Info => Info,
            _ => TextPrimary
        };

        private static Font CreateFont(float size, FontStyle style)
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

    /// <summary>Estados visuales compartidos del CRM (sin lógica de negocio).</summary>
    public enum CrmVisualState
    {
        Normal = 0,
        Positive = 1,
        Warning = 2,
        Critical = 3,
        Info = 4
    }
}
