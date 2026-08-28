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

        /// <summary>
        /// Mejora nitidez/contraste/ruido sin alterar el contenido (no genera ni reemplaza el producto).
        /// Pipeline: denoise suave → CLAHE → unsharp mask.
        /// </summary>
        public static Bitmap MejorarCalidadPreservandoContenido(Image source)
        {
            ArgumentNullException.ThrowIfNull(source);

            using var srcBmp = new Bitmap(source);
            using Mat bgrIn = BitmapConverter.ToMat(srcBmp);
            using Mat bgr = new Mat();
            if (bgrIn.Channels() == 4)
                Cv2.CvtColor(bgrIn, bgr, ColorConversionCodes.BGRA2BGR);
            else if (bgrIn.Channels() == 1)
                Cv2.CvtColor(bgrIn, bgr, ColorConversionCodes.GRAY2BGR);
            else
                bgrIn.CopyTo(bgr);

            using Mat denoised = new Mat();
            Cv2.BilateralFilter(bgr, denoised, d: 5, sigmaColor: 35, sigmaSpace: 35);

            using Mat lab = new Mat();
            Cv2.CvtColor(denoised, lab, ColorConversionCodes.BGR2Lab);
            Mat[] planes = Cv2.Split(lab);
            try
            {
                using var clahe = Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new OpenCvSharp.Size(8, 8));
                clahe.Apply(planes[0], planes[0]);
                Cv2.Merge(planes, lab);
            }
            finally
            {
                foreach (Mat p in planes)
                    p.Dispose();
            }

            using Mat contrast = new Mat();
            Cv2.CvtColor(lab, contrast, ColorConversionCodes.Lab2BGR);

            using Mat blur = new Mat();
            Cv2.GaussianBlur(contrast, blur, new OpenCvSharp.Size(0, 0), 1.15);
            using Mat sharp = new Mat();
            Cv2.AddWeighted(contrast, 1.42, blur, -0.42, 0, sharp);

            return BitmapConverter.ToBitmap(sharp);
        }

        /// <summary>Rota alrededor del centro ampliando el lienzo para no cortar esquinas.</summary>
        public static Bitmap RotarGrados(Image source, float grados)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (Math.Abs(grados) < 0.01f)
                return new Bitmap(source);

            using var srcBmp = new Bitmap(source);
            using Mat src = BitmapConverter.ToMat(srcBmp);
            using Mat bgr = new Mat();
            if (src.Channels() == 4)
                Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
            else if (src.Channels() == 1)
                Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
            else
                src.CopyTo(bgr);

            var center = new Point2f(bgr.Width / 2f, bgr.Height / 2f);
            using Mat rot = Cv2.GetRotationMatrix2D(center, grados, 1.0);

            double cos = Math.Abs(rot.Get<double>(0, 0));
            double sin = Math.Abs(rot.Get<double>(0, 1));
            int nw = Math.Max(1, (int)Math.Round(bgr.Height * sin + bgr.Width * cos));
            int nh = Math.Max(1, (int)Math.Round(bgr.Height * cos + bgr.Width * sin));
            rot.Set(0, 2, rot.Get<double>(0, 2) + (nw / 2.0) - center.X);
            rot.Set(1, 2, rot.Get<double>(1, 2) + (nh / 2.0) - center.Y);

            using Mat dst = new Mat();
            Cv2.WarpAffine(
                bgr,
                dst,
                rot,
                new OpenCvSharp.Size(nw, nh),
                InterpolationFlags.Linear,
                BorderTypes.Constant,
                Scalar.White);

            return BitmapConverter.ToBitmap(dst);
        }

        public static Bitmap Recortar(Image source, Rectangle area)
        {
            ArgumentNullException.ThrowIfNull(source);
            var bounds = new Rectangle(0, 0, source.Width, source.Height);
            area = Rectangle.Intersect(bounds, area);
            if (area.Width < 2 || area.Height < 2)
                throw new ArgumentException("Área de recorte inválida.", nameof(area));

            var bmp = new Bitmap(area.Width, area.Height);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(
                source,
                new Rectangle(0, 0, area.Width, area.Height),
                area,
                GraphicsUnit.Pixel);
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
