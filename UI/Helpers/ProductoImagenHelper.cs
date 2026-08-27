using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

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

        /// <summary>
        /// Carga imagen a JPEG en memoria. Reintenta si el archivo está bloqueado.
        /// JPG/PNG/BMP vía GDI+; WebP y otros vía OpenCv (ImDecode + ImRead).
        /// </summary>
        public static byte[] LoadAsJpegBytes(string path, int maxSide = 1024)
        {
            Exception? last = null;
            for (int attempt = 1; attempt <= 6; attempt++)
            {
                try
                {
                    byte[] raw = ReadAllBytesShared(path);
                    if (raw.Length == 0)
                        throw new InvalidOperationException("Archivo vacío.");

                    return DecodeBytesToJpeg(raw, maxSide, path);
                }
                catch (Exception ex) when (attempt < 6)
                {
                    last = ex;
                    Thread.Sleep(250 * attempt);
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }

            throw new InvalidOperationException(
                "No se pudo abrir la imagen tras varios intentos. " +
                (last?.Message ?? "Formato no soportado o archivo bloqueado."),
                last);
        }

        public static bool TryLoadAsJpegBytes(
            string path,
            int maxSide,
            out byte[]? jpeg,
            out string? error)
        {
            try
            {
                jpeg = LoadAsJpegBytes(path, maxSide);
                error = null;
                return jpeg is { Length: > 0 };
            }
            catch (Exception ex)
            {
                jpeg = null;
                error = ex.GetBaseException().Message;
                return false;
            }
        }

        private static byte[] DecodeBytesToJpeg(byte[] raw, int maxSide, string? pathHint)
        {
            try
            {
                using var ms = new MemoryStream(raw, writable: false);
                using var img = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false);
                return ToJpegBytes(img, maxSide);
            }
            catch
            {
                // GDI+ no abre WebP; seguir con OpenCv.
            }

            try
            {
                using Mat mat = Cv2.ImDecode(raw, ImreadModes.Color);
                if (!mat.Empty())
                    return MatToJpegBytes(mat, maxSide);
            }
            catch
            {
                // continuar
            }

            if (!string.IsNullOrWhiteSpace(pathHint) && File.Exists(pathHint))
            {
                using Mat mat = Cv2.ImRead(pathHint, ImreadModes.Color);
                if (!mat.Empty())
                    return MatToJpegBytes(mat, maxSide);
            }

            throw new InvalidOperationException(
                "Formato no soportado o imagen ilegible (GDI+/OpenCv).");
        }

        private static byte[] MatToJpegBytes(Mat source, int maxSide)
        {
            using Mat resized = ResizeMatIfNeeded(source, maxSide);
            using Bitmap bmp = BitmapConverter.ToBitmap(resized);
            return ToJpegBytes(bmp, maxSide: Math.Max(bmp.Width, bmp.Height));
        }

        private static Mat ResizeMatIfNeeded(Mat source, int maxSide)
        {
            int max = Math.Max(source.Width, source.Height);
            if (max <= maxSide)
                return source.Clone();

            double scale = (double)maxSide / max;
            int nw = Math.Max(1, (int)Math.Round(source.Width * scale));
            int nh = Math.Max(1, (int)Math.Round(source.Height * scale));
            var dst = new Mat();
            Cv2.Resize(source, dst, new OpenCvSharp.Size(nw, nh), 0, 0, InterpolationFlags.Area);
            return dst;
        }

        private static byte[] ReadAllBytesShared(string path)
        {
            using var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return ms.ToArray();
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
