using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace BLL.Services
{
    /// <summary>Extrae texto de PDF de factura (best-effort) para la orquesta de texto.</summary>
    internal static class FacturaPdfTextExtractor
    {
        public static string? TryExtract(string filePath, int maxChars = 8000)
        {
            try
            {
                using var reader = new PdfReader(filePath);
                using var pdf = new PdfDocument(reader);
                var sb = new StringBuilder();
                int pages = Math.Min(pdf.GetNumberOfPages(), 4);
                for (int i = 1; i <= pages; i++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(
                        pdf.GetPage(i),
                        new LocationTextExtractionStrategy());
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        if (sb.Length > 0)
                            sb.AppendLine();
                        sb.Append(pageText.Trim());
                    }

                    if (sb.Length >= maxChars)
                        break;
                }

                if (sb.Length == 0)
                    return null;

                string text = sb.ToString();
                if (text.Length > maxChars)
                    text = text[..maxChars];
                return text;
            }
            catch
            {
                return null;
            }
        }
    }
}
