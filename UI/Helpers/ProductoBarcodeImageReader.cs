using System.Drawing;
using BLL;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace UI.Helpers
{
    /// <summary>Lectura local de EAN/UPC desde foto (sin IA — instantáneo).</summary>
    public static class ProductoBarcodeImageReader
    {
        private static readonly BarcodeReader Reader = CreateReader();

        public static string? TryReadFromImage(Image image)
        {
            if (image == null)
                return null;

            using var bitmap = new Bitmap(image);
            return Decode(bitmap);
        }

        public static string? TryReadFromJpegBytes(byte[] jpeg)
        {
            if (jpeg == null || jpeg.Length == 0)
                return null;

            using var ms = new MemoryStream(jpeg);
            using var img = Image.FromStream(ms);
            using var bitmap = new Bitmap(img);
            return Decode(bitmap);
        }

        private static BarcodeReader CreateReader()
        {
            return new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats =
                    [
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39
                    ]
                }
            };
        }

        private static string? Decode(Bitmap bitmap)
        {
            try
            {
                Result? result = Reader.Decode(bitmap);
                if (string.IsNullOrWhiteSpace(result?.Text))
                    return null;

                return ProductoBarcodeNormalizer.TryNormalizeBarcode(result.Text, out string? code)
                    ? code
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
