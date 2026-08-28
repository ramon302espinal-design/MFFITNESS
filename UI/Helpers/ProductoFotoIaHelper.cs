using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using CORE;
using CORE.Ollama;

namespace UI.Helpers
{
    /// <summary>
    /// Interpreta una petición en español y aplica mejoras locales
    /// sin regenerar ni recortar el producto (salvo fit_canvas explícito).
    /// </summary>
    internal static class ProductoFotoIaHelper
    {
        public sealed class Plan
        {
            public List<string> Actions { get; } = new();
            public string Fuente { get; set; } = "local";
            public string Resumen => Actions.Count == 0
                ? "mejorar calidad"
                : string.Join(", ", Actions);
        }

        public static async Task<(Bitmap Resultado, Plan Plan)> AplicarPeticionAsync(
            Image fuente,
            string peticion,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(fuente);
            string pedido = (peticion ?? string.Empty).Trim();
            if (pedido.Length == 0)
                pedido = "ponla nítida y mejora la calidad";

            Plan plan = await ResolverPlanAsync(fuente, pedido, ct).ConfigureAwait(false);
            NormalizarPlanSenior(plan, pedido);
            if (plan.Actions.Count == 0)
                plan.Actions.Add("enhance");

            using var clone = new Bitmap(fuente);
            Bitmap resultado = await Task.Run(() =>
            {
                Bitmap current = new Bitmap(clone);
                try
                {
                    foreach (string action in plan.Actions.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        ct.ThrowIfCancellationRequested();
                        Bitmap next = AplicarAccion(current, action);
                        if (!ReferenceEquals(next, current))
                        {
                            current.Dispose();
                            current = next;
                        }
                    }

                    return current;
                }
                catch
                {
                    current.Dispose();
                    throw;
                }
            }, ct).ConfigureAwait(false);

            return (resultado, plan);
        }

        /// <summary>
        /// Reglas senior: quitar fondo no recorta; fit_canvas solo si lo pidieron;
        /// prioriza intención explícita del texto del usuario.
        /// </summary>
        private static void NormalizarPlanSenior(Plan plan, string peticion)
        {
            string t = peticion.ToLowerInvariant();
            bool quiereFondo = Contiene(t, "fondo", "background", "bg ");
            bool quiereLienzo = Contiene(t,
                "lienzo", "canvas", "encuadr", "recorta bord",
                "ajustar al lienzo", "ajusta al lienzo", "ajústala al lienzo",
                "centrar en lienzo", "centra en el lienzo");
            bool quiereNitidez = Contiene(t, "nítid", "nitid", "sharp", "enfoque", "enfoca");
            bool quiereCalidad = Contiene(t, "calidad", "mejor", "arregla", "enhance", "mejora");

            if (quiereFondo && !plan.Actions.Contains("remove_bg", StringComparer.OrdinalIgnoreCase))
                plan.Actions.Insert(0, "remove_bg");

            // Nunca combinar remove_bg + fit_canvas salvo pedirlo explícito:
            // fit_canvas recorta; remove_bg debe preservar el marco original.
            if (quiereFondo && !quiereLienzo)
                plan.Actions.RemoveAll(a => a.Equals("fit_canvas", StringComparison.OrdinalIgnoreCase));

            if (quiereNitidez && !plan.Actions.Contains("sharpen", StringComparer.OrdinalIgnoreCase))
                plan.Actions.Add("sharpen");
            if (quiereCalidad && !plan.Actions.Contains("enhance", StringComparer.OrdinalIgnoreCase)
                && !quiereFondo)
                plan.Actions.Insert(0, "enhance");

            // Orden estable: remove_bg → enhance/sharpen/… → fit_canvas al final.
            var ordered = plan.Actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(PrioridadAccion)
                .Take(3)
                .ToList();
            plan.Actions.Clear();
            plan.Actions.AddRange(ordered);
        }

        private static int PrioridadAccion(string a) => a.ToLowerInvariant() switch
        {
            "remove_bg" => 0,
            "enhance" => 1,
            "denoise" => 2,
            "sharpen" => 3,
            "brighten" => 4,
            "contrast" => 5,
            "fit_canvas" => 9,
            _ => 5
        };

        private static bool Contiene(string t, params string[] keys) =>
            keys.Any(k => t.Contains(k, StringComparison.Ordinal));

        private static async Task<Plan> ResolverPlanAsync(
            Image fuente,
            string peticion,
            CancellationToken ct)
        {
            Plan local = PlanDesdeTexto(peticion);
            try
            {
                AppConfig.LoadOllamaOptions();
                var client = new OllamaClient();
                if (!await client.IsAvailableAsync(ct).ConfigureAwait(false))
                    return local;

                string model = OllamaOptions.VisionModel;
                if (!await client.HasModelAsync(model, ct).ConfigureAwait(false))
                {
                    model = OllamaOptions.VisionFallbackModel;
                    if (!await client.HasModelAsync(model, ct).ConfigureAwait(false))
                        return local;
                }

                byte[] jpeg = ProductoImagenHelper.ToJpegBytes(
                    fuente,
                    maxSide: Math.Min(OllamaOptions.VisionMaxSide, 768),
                    quality: 82);
                string b64 = Convert.ToBase64String(jpeg);

                string prompt =
                    "Eres un editor SENIOR de fotos de productos (gimnasio/POS).\n" +
                    "Pedido del usuario: \"" + peticion.Replace('"', '\'') + "\"\n\n" +
                    "Responde ÚNICAMENTE JSON:\n" +
                    "{\"actions\":[\"enhance\",\"sharpen\",\"denoise\",\"brighten\",\"contrast\",\"remove_bg\",\"fit_canvas\"]}\n\n" +
                    "Reglas estrictas:\n" +
                    "1) Máximo 3 acciones, solo esas claves.\n" +
                    "2) remove_bg = poner fondo blanco SIN recortar ni cambiar tamaño del lienzo; " +
                    "protege el producto completo (no cortes bordes del producto).\n" +
                    "3) fit_canvas SOLO si pide explícitamente lienzo/canvas/encuadrar/recortar bordes. " +
                    "NUNCA uses fit_canvas junto a remove_bg salvo que lo pidan juntos.\n" +
                    "4) nitidez→sharpen; calidad/arreglar→enhance; brillo→brighten; contraste→contrast.\n" +
                    "5) Si pide quitar fondo, incluye remove_bg y no inventes recortes.\n" +
                    "6) No reemplaces el producto ni generes otra imagen.";

                string raw = await client.GenerateWithImagesAsync(
                        model,
                        prompt,
                        new[] { b64 },
                        jsonFormat: true,
                        numPredict: 160,
                        ct)
                    .ConfigureAwait(false);

                Plan? fromAi = PlanDesdeJson(raw);
                if (fromAi is not { Actions.Count: > 0 })
                    return local;

                // Fusiona: intenciones locales explícitas mandan.
                var merged = new Plan { Fuente = "ollama+local" };
                foreach (string a in local.Actions.Concat(fromAi.Actions))
                {
                    if (!EsAccionValida(a))
                        continue;
                    if (!merged.Actions.Contains(a, StringComparer.OrdinalIgnoreCase))
                        merged.Actions.Add(a);
                }

                return merged.Actions.Count > 0 ? merged : local;
            }
            catch
            {
                return local;
            }
        }

        private static Plan PlanDesdeTexto(string texto)
        {
            var plan = new Plan { Fuente = "local" };
            string t = texto.ToLowerInvariant();

            if (Contiene(t, "fondo", "background", "quitar fondo", "eliminar fondo", "sin fondo"))
                plan.Actions.Add("remove_bg");
            if (Contiene(t,
                    "lienzo", "canvas", "encuadr", "recorta bord",
                    "ajustar al lienzo", "ajusta al lienzo", "ajústala al lienzo"))
                plan.Actions.Add("fit_canvas");
            if (Contiene(t, "nítid", "nitid", "sharp", "enfoque", "enfoca"))
                plan.Actions.Add("sharpen");
            if (Contiene(t, "ruido", "noise", "suaviz"))
                plan.Actions.Add("denoise");
            if (Contiene(t, "brill", "ilumina", "bright", "más clara", "mas clara"))
                plan.Actions.Add("brighten");
            if (Contiene(t, "contraste", "contrast", "vibran"))
                plan.Actions.Add("contrast");
            if (Contiene(t, "calidad", "mejor", "arregla", "arreglo", "enhance", "mejora"))
                plan.Actions.Add("enhance");

            if (plan.Actions.Count == 0)
            {
                plan.Actions.Add("enhance");
                plan.Actions.Add("sharpen");
            }

            return plan;
        }

        private static Plan? PlanDesdeJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(ExtractJsonObject(raw));
                if (!doc.RootElement.TryGetProperty("actions", out JsonElement arr)
                    || arr.ValueKind != JsonValueKind.Array)
                    return null;

                var plan = new Plan();
                foreach (JsonElement el in arr.EnumerateArray())
                {
                    string? a = el.GetString()?.Trim().ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(a))
                        continue;
                    a = a switch
                    {
                        "nitidez" or "nitida" or "nítida" => "sharpen",
                        "calidad" or "mejorar" => "enhance",
                        "fondo" or "quitar_fondo" or "background" or "remove-bg" => "remove_bg",
                        "lienzo" or "canvas" or "ajustar" => "fit_canvas",
                        "brillo" => "brighten",
                        _ => a
                    };
                    if (EsAccionValida(a) && !plan.Actions.Contains(a, StringComparer.OrdinalIgnoreCase))
                        plan.Actions.Add(a);
                    if (plan.Actions.Count >= 3)
                        break;
                }

                return plan.Actions.Count > 0 ? plan : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool EsAccionValida(string a) =>
            a is "enhance" or "sharpen" or "denoise" or "brighten"
                or "contrast" or "remove_bg" or "fit_canvas";

        private static string ExtractJsonObject(string raw)
        {
            int i = raw.IndexOf('{');
            int j = raw.LastIndexOf('}');
            if (i >= 0 && j > i)
                return raw[i..(j + 1)];
            return raw;
        }

        private static Bitmap AplicarAccion(Bitmap source, string action) =>
            action.ToLowerInvariant() switch
            {
                "enhance" => ProductoImagenHelper.MejorarCalidadPreservandoContenido(source),
                "sharpen" => AplicarNitidez(source, 1.55f),
                "denoise" => AplicarDenoise(source),
                "brighten" => AplicarBrilloContraste(source, beta: 18, alpha: 1.05),
                "contrast" => AplicarBrilloContraste(source, beta: 0, alpha: 1.22),
                "remove_bg" => QuitarFondoConRembgOFallback(source),
                "fit_canvas" => AjustarAlLienzo(source),
                _ => new Bitmap(source)
            };

        private static Bitmap AplicarNitidez(Bitmap source, float amount)
        {
            using Mat bgr = ToBgr(source);
            using Mat blur = new Mat();
            Cv2.GaussianBlur(bgr, blur, new OpenCvSharp.Size(0, 0), 1.1);
            using Mat sharp = new Mat();
            Cv2.AddWeighted(bgr, amount, blur, 1.0 - amount, 0, sharp);
            return BitmapConverter.ToBitmap(sharp);
        }

        private static Bitmap AplicarDenoise(Bitmap source)
        {
            using Mat bgr = ToBgr(source);
            using Mat dst = new Mat();
            Cv2.BilateralFilter(bgr, dst, 7, 50, 50);
            return BitmapConverter.ToBitmap(dst);
        }

        private static Bitmap AplicarBrilloContraste(Bitmap source, double beta, double alpha)
        {
            using Mat bgr = ToBgr(source);
            using Mat dst = new Mat();
            bgr.ConvertTo(dst, MatType.CV_8UC3, alpha, beta);
            return BitmapConverter.ToBitmap(dst);
        }

        /// <summary>
        /// Preferir rembg (bria-rmbg) local; si no está, OpenCV flood-fill.
        /// Siempre preserva el tamaño del original (fondo blanco, sin recorte de marco).
        /// </summary>
        private static Bitmap QuitarFondoConRembgOFallback(Bitmap source)
        {
            if (ProductoRembgHelper.TryQuitarFondoSobreBlanco(source, out Bitmap? rembg, out _))
            {
                if (rembg != null)
                    return rembg;
            }

            return QuitarFondoPreservandoMarco(source);
        }

        /// <summary>
        /// Quita fondo a blanco SIN cambiar tamaño.
        /// Flood-fill desde el borde (Lab) + limpieza de residuos + feather.
        /// </summary>
        private static Bitmap QuitarFondoPreservandoMarco(Bitmap source)
        {
            using Mat bgr = ToBgr(source);
            int w = bgr.Width;
            int h = bgr.Height;

            using Mat lab = new Mat();
            Cv2.CvtColor(bgr, lab, ColorConversionCodes.BGR2Lab);

            // Máscara de flood: 0 = no visitado / producto, 255 = fondo.
            using Mat bgMask = new Mat(h, w, MatType.CV_8UC1, Scalar.All(0));

            // Tolerancia Lab: L más laxo, a/b más estricto (color).
            var lo = new Scalar(18, 12, 12);
            var up = new Scalar(18, 12, 12);

            // Flood desde TODO el perímetro (elimina residuos de fondo).
            using Mat floodMask = new Mat(h + 2, w + 2, MatType.CV_8UC1, Scalar.All(0));
            void Seed(int x, int y)
            {
                if (x < 0 || y < 0 || x >= w || y >= h)
                    return;
                if (bgMask.At<byte>(y, x) != 0)
                    return;
                // MaskOnly: pinta 255 en floodMask; FixedRange respecto al seed.
                Cv2.FloodFill(
                    lab,
                    floodMask,
                    new OpenCvSharp.Point(x, y),
                    new Scalar(0, 0, 0),
                    out _,
                    lo,
                    up,
                    FloodFillFlags.Link4 | FloodFillFlags.MaskOnly | (FloodFillFlags)(255 << 8));
            }

            for (int x = 0; x < w; x++)
            {
                Seed(x, 0);
                Seed(x, h - 1);
            }
            for (int y = 0; y < h; y++)
            {
                Seed(0, y);
                Seed(w - 1, y);
            }

            // Copiar floodMask (offset +1,+1) → bgMask
            using (Mat roi = new Mat(floodMask, new OpenCvSharp.Rect(1, 1, w, h)))
                roi.CopyTo(bgMask);

            // Segunda pasada más agresiva en Lab con umbral a color de borde.
            Scalar bgLab = EstimarFondoBordeLab(lab);
            using Mat labDiff = DistanciaLab(lab, bgLab);
            using Mat nearBg = new Mat();
            Cv2.Threshold(labDiff, nearBg, 22, 255, ThresholdTypes.BinaryInv);
            // Solo marcar como fondo lo conectado al borde (evita agujeros en producto).
            using Mat nearBgBorder = new Mat();
            Cv2.BitwiseAnd(nearBg, bgMask, nearBgBorder);
            // Expandir fondo hacia píxeles muy similares todavía no marcados.
            using Mat cand = new Mat();
            using Mat notBg = new Mat();
            Cv2.BitwiseNot(bgMask, notBg);
            Cv2.BitwiseAnd(nearBg, notBg, cand);
            using Mat dil = new Mat();
            using Mat ker3 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.Dilate(bgMask, dil, ker3, iterations: 2);
            using Mat grow = new Mat();
            Cv2.BitwiseAnd(cand, dil, grow);
            Cv2.BitwiseOr(bgMask, grow, bgMask);

            // Producto = inverso del fondo.
            using Mat fg = new Mat();
            Cv2.BitwiseNot(bgMask, fg);

            // Quedarse con componentes grandes (quita motas residuales).
            LimpiarComponentesPequenos(fg, minAreaRatio: 0.004);

            // Cerrar huecos internos del producto + dilatar un poco (no comer bordes).
            int k = Math.Max(3, Math.Min(w, h) / 70) | 1;
            using Mat ker = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(k, k));
            Cv2.MorphologyEx(fg, fg, MorphTypes.Close, ker, iterations: 2);
            Cv2.Dilate(fg, fg, ker, iterations: 1);

            // Residuos de fondo: lo que sigue siendo fondo y está lejos del producto → forzar blanco limpio.
            // Feather solo en el borde del sujeto.
            using Mat alpha8 = new Mat();
            int feather = Math.Max(5, Math.Min(w, h) / 55) | 1;
            Cv2.GaussianBlur(fg, alpha8, new OpenCvSharp.Size(feather, feather), 0);

            // Descontaminación de color en bordes (mata halo/residuo).
            return ComponerSobreBlancoLimpio(bgr, alpha8, bgLab);
        }

        private static Scalar EstimarFondoBordeLab(Mat lab)
        {
            int w = lab.Width;
            int h = lab.Height;
            int band = Math.Max(4, Math.Min(w, h) / 28);

            Scalar MeanRoi(int x, int y, int rw, int rh)
            {
                rw = Math.Max(1, Math.Min(rw, w - x));
                rh = Math.Max(1, Math.Min(rh, h - y));
                using Mat roi = new Mat(lab, new OpenCvSharp.Rect(x, y, rw, rh));
                return Cv2.Mean(roi);
            }

            Scalar t = MeanRoi(0, 0, w, band);
            Scalar b = MeanRoi(0, h - band, w, band);
            Scalar l = MeanRoi(0, 0, band, h);
            Scalar r = MeanRoi(w - band, 0, band, h);
            return new Scalar(
                (t.Val0 + b.Val0 + l.Val0 + r.Val0) / 4.0,
                (t.Val1 + b.Val1 + l.Val1 + r.Val1) / 4.0,
                (t.Val2 + b.Val2 + l.Val2 + r.Val2) / 4.0);
        }

        private static Mat DistanciaLab(Mat lab, Scalar bg)
        {
            // |dL| + |da| + |db| aproximado en 8U.
            Mat[] ch = Cv2.Split(lab);
            try
            {
                using Mat dL = new Mat();
                using Mat dA = new Mat();
                using Mat dB = new Mat();
                Cv2.Absdiff(ch[0], new Scalar(bg.Val0), dL);
                Cv2.Absdiff(ch[1], new Scalar(bg.Val1), dA);
                Cv2.Absdiff(ch[2], new Scalar(bg.Val2), dB);
                using Mat sum = new Mat();
                Cv2.Add(dL, dA, sum);
                Cv2.Add(sum, dB, sum);
                return sum.Clone();
            }
            finally
            {
                foreach (Mat c in ch)
                    c.Dispose();
            }
        }

        private static void LimpiarComponentesPequenos(Mat fg255, double minAreaRatio)
        {
            int minArea = Math.Max(40, (int)(fg255.Width * fg255.Height * minAreaRatio));
            Cv2.FindContours(
                fg255.Clone(),
                out OpenCvSharp.Point[][] contours,
                out HierarchyIndex[] _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            fg255.SetTo(Scalar.All(0));
            foreach (var c in contours)
            {
                if (Cv2.ContourArea(c) >= minArea)
                    Cv2.DrawContours(fg255, new[] { c }, -1, Scalar.All(255), thickness: -1);
            }
        }

        private static Bitmap ComponerSobreBlancoLimpio(Mat bgr, Mat alpha8, Scalar bgLab)
        {
            int w = bgr.Width;
            int h = bgr.Height;
            using Mat lab = new Mat();
            Cv2.CvtColor(bgr, lab, ColorConversionCodes.BGR2Lab);

            var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, w, h);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                int stride = data.Stride;
                byte[] row = new byte[stride];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float a = alpha8.At<byte>(y, x) / 255f;
                        Vec3b p = bgr.At<Vec3b>(y, x);

                        // En semitransparencia: reduce spill del fondo hacia blanco.
                        if (a > 0.05f && a < 0.95f)
                        {
                            Vec3b labP = lab.At<Vec3b>(y, x);
                            double d =
                                Math.Abs(labP.Item0 - bgLab.Val0) +
                                Math.Abs(labP.Item1 - bgLab.Val1) +
                                Math.Abs(labP.Item2 - bgLab.Val2);
                            if (d < 28)
                                a *= 0.15f; // residuo → casi blanco
                        }

                        if (a < 0.04f)
                        {
                            row[x * 3] = 255;
                            row[x * 3 + 1] = 255;
                            row[x * 3 + 2] = 255;
                            continue;
                        }

                        byte r = (byte)Math.Clamp(Math.Round(p.Item2 * a + 255 * (1 - a)), 0, 255);
                        byte g = (byte)Math.Clamp(Math.Round(p.Item1 * a + 255 * (1 - a)), 0, 255);
                        byte b = (byte)Math.Clamp(Math.Round(p.Item0 * a + 255 * (1 - a)), 0, 255);
                        row[x * 3] = b;
                        row[x * 3 + 1] = g;
                        row[x * 3 + 2] = r;
                    }

                    Marshal.Copy(row, 0, data.Scan0 + y * stride, Math.Min(stride, row.Length));
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }

        /// <summary>Recorta márgenes claros y centra (solo si el usuario lo pide).</summary>
        private static Bitmap AjustarAlLienzo(Bitmap source)
        {
            using Mat bgr = ToBgr(source);
            using Mat gray = new Mat();
            Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);
            using Mat mask = new Mat();
            Cv2.Threshold(gray, mask, 245, 255, ThresholdTypes.BinaryInv);
            using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);

            Cv2.FindContours(
                mask,
                out OpenCvSharp.Point[][] contours,
                out HierarchyIndex[] _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            OpenCvSharp.Rect bound;
            if (contours.Length == 0)
            {
                bound = new OpenCvSharp.Rect(0, 0, bgr.Width, bgr.Height);
            }
            else
            {
                bound = Cv2.BoundingRect(contours.OrderByDescending(c => Cv2.ContourArea(c)).First());
                int pad = Math.Max(12, Math.Min(bound.Width, bound.Height) / 18);
                int x = Math.Max(0, bound.X - pad);
                int y = Math.Max(0, bound.Y - pad);
                int r = Math.Min(bgr.Width, bound.X + bound.Width + pad);
                int btm = Math.Min(bgr.Height, bound.Y + bound.Height + pad);
                bound = new OpenCvSharp.Rect(x, y, Math.Max(1, r - x), Math.Max(1, btm - y));
            }

            using Mat cropped = new Mat(bgr, bound).Clone();
            int side = Math.Max(cropped.Width, cropped.Height);
            side = Math.Max(side, 64);
            using Mat canvas = new Mat(side, side, MatType.CV_8UC3, Scalar.White);
            int ox = (side - cropped.Width) / 2;
            int oy = (side - cropped.Height) / 2;
            using Mat roi = new Mat(canvas, new OpenCvSharp.Rect(ox, oy, cropped.Width, cropped.Height));
            cropped.CopyTo(roi);
            return BitmapConverter.ToBitmap(canvas);
        }

        private static Mat ToBgr(Bitmap source)
        {
            using Mat src = BitmapConverter.ToMat(source);
            var bgr = new Mat();
            if (src.Channels() == 4)
                Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
            else if (src.Channels() == 1)
                Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
            else
                src.CopyTo(bgr);
            return bgr;
        }
    }
}
