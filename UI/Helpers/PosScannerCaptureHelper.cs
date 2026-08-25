using System;
using System.Text;
using System.Windows.Forms;

namespace UI.Helpers
{
    internal static class PosScannerCaptureHelper
    {
        public static bool HandleKeyDown(
            KeyEventArgs e,
            StringBuilder buffer,
            ref DateTime lastKeyUtc,
            Func<bool> shouldCapture,
            Action<string> onScanComplete)
        {
            if (!shouldCapture())
                return false;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (buffer.Length == 0)
                    return false;

                string raw = buffer.ToString();
                buffer.Clear();
                onScanComplete(raw);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }

            char? c = PosScannerKeyHelper.KeyToScanChar(e);
            if (c == null)
                return false;

            PosScannerKeyHelper.AppendChar(buffer, ref lastKeyUtc, c.Value);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return true;
        }
    }
}
