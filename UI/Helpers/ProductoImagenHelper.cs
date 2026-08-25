using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UI.Helpers
{
    internal static class ProductoImagenHelper
    {
        public static byte[] ToJpegBytes(Image image, int maxSide = 1024, long quality = 85)
        {
            using Image resized = ResizeIfNeeded(image, maxSide);
            using var ms = new MemoryStream();
            ImageCodecInfo? encoder = GetJpegEncoder();
            if (encoder != null)
            {
                using var ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                resized.Save(ms, encoder, ep);
            }
            else
            {
                resized.Save(ms, ImageFormat.Jpeg);
            }

            return ms.ToArray();
        }

        public static byte[] LoadAsJpegBytes(string path, int maxSide = 1024)
        {
            using var img = Image.FromFile(path);
            return ToJpegBytes(img, maxSide);
        }

        private static Image ResizeIfNeeded(Image source, int maxSide)
        {
            int w = source.Width;
            int h = source.Height;
            int max = Math.Max(w, h);
            if (max <= maxSide)
                return new Bitmap(source);

            double scale = (double)maxSide / max;
            int nw = Math.Max(1, (int)Math.Round(w * scale));
            int nh = Math.Max(1, (int)Math.Round(h * scale));
            var bmp = new Bitmap(nw, nh);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(source, 0, 0, nw, nh);
            return bmp;
        }

        private static ImageCodecInfo? GetJpegEncoder()
        {
            foreach (var c in ImageCodecInfo.GetImageEncoders())
            {
                if (string.Equals(c.MimeType, "image/jpeg", StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return null;
        }
    }
}
