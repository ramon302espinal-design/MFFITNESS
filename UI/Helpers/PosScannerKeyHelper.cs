using System.Text;
using System.Windows.Forms;

namespace UI.Helpers
{
    /// <summary>
    /// Captura wedge HID en el formulario aunque el foco esté en lista/grilla del POS.
    /// </summary>
    internal static class PosScannerKeyHelper
    {
        private const int ScannerGapMs = 120;

        public static char? KeyToScanChar(KeyEventArgs e)
        {
            if (e.Control || e.Alt)
                return null;

            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                return (char)('0' + (e.KeyCode - Keys.D0));

            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                return (char)('0' + (e.KeyCode - Keys.NumPad0));

            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                char c = (char)('A' + (e.KeyCode - Keys.A));
                return e.Shift ? c : char.ToLowerInvariant(c);
            }

            switch (e.KeyCode)
            {
                case Keys.OemMinus:
                case Keys.Subtract:
                    return '-';
                case Keys.OemPeriod:
                case Keys.Decimal:
                    return '.';
                case Keys.Oemplus:
                    return e.Shift ? '+' : null;
                default:
                    return null;
            }
        }

        public static void AppendChar(StringBuilder buffer, ref DateTime lastKeyUtc, char c)
        {
            var now = DateTime.UtcNow;
            if ((now - lastKeyUtc).TotalMilliseconds > ScannerGapMs)
                buffer.Clear();
            lastKeyUtc = now;
            buffer.Append(c);
        }
    }
}
