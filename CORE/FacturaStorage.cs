using System;
using System.IO;

namespace CORE
{
    /// <summary>
    /// Carpeta compartida de facturas PDF (UI + WhatsAppHost / Twilio).
    /// </summary>
    public static class FacturaStorage
    {
        public static string CarpetaRaizMffitness =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS");

        /// <summary>
        /// %LocalAppData%\MFFITNESS\Facturas — fuente de verdad para Twilio.
        /// </summary>
        public static string CarpetaFacturas
        {
            get
            {
                string dir = Path.Combine(CarpetaRaizMffitness, "Facturas");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>
        /// Copia espejo junto al ejecutable: wwwroot\facturas (proyecto / host).
        /// </summary>
        public static string CarpetaWwwrootFacturas
        {
            get
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "wwwroot", "facturas");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string NombreArchivoPago(int pagoId) =>
            $"factura_{Math.Max(1, pagoId)}.pdf";

        public static string RutaFacturaPago(int pagoId) =>
            Path.Combine(CarpetaFacturas, NombreArchivoPago(pagoId));

        public static string? ResolverRutaFacturaExistente(int pagoId)
        {
            string primaria = RutaFacturaPago(pagoId);
            if (File.Exists(primaria))
                return primaria;

            string espejo = Path.Combine(CarpetaWwwrootFacturas, NombreArchivoPago(pagoId));
            if (File.Exists(espejo))
                return espejo;

            return null;
        }

        /// <summary>
        /// True si no hay PDF o el archivo es anterior al pago (reutilización de factura_{pagoId} vieja).
        /// </summary>
        public static bool FacturaPdfDesactualizada(int pagoId, DateTime fechaPagoReferencia)
        {
            if (pagoId <= 0)
                return true;

            string? ruta = ResolverRutaFacturaExistente(pagoId);
            if (string.IsNullOrWhiteSpace(ruta))
                return true;

            try
            {
                var info = new FileInfo(ruta);
                if (!info.Exists)
                    return true;

                // Margen por reloj del sistema / redondeo SQL.
                DateTime umbral = fechaPagoReferencia.ToLocalTime().AddSeconds(-5);
                return info.LastWriteTime < umbral;
            }
            catch
            {
                return true;
            }
        }

        public static void GuardarFactura(int pagoId, byte[] pdfBytes)
        {
            if (pagoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pagoId));

            string primaria = RutaFacturaPago(pagoId);
            File.WriteAllBytes(primaria, pdfBytes);

            try
            {
                string espejo = Path.Combine(CarpetaWwwrootFacturas, NombreArchivoPago(pagoId));
                File.Copy(primaria, espejo, overwrite: true);
            }
            catch
            {
                // El espejo es opcional; la ruta LocalAppData basta para el host.
            }
        }

        public static string? ResolverLogoPath()
        {
            string local = Path.Combine(CarpetaRaizMffitness, "Resources", "mf_logo.png");
            if (File.Exists(local))
                return local;

            string baseRes = Path.Combine(AppContext.BaseDirectory, "Resources", "mf_logo.png");
            if (File.Exists(baseRes))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(local)!);
                    File.Copy(baseRes, local, overwrite: true);
                }
                catch
                {
                    // ignore
                }
                return baseRes;
            }

            return File.Exists(local) ? local : null;
        }

        public static string? ConstruirMediaUrlPublica(int pagoId)
        {
            if (pagoId <= 0)
            {
                System.Diagnostics.Trace.WriteLine("[Factura] MediaUrl no construida: pagoId invalido.");
                return null;
            }

            // Preferir Supabase Storage (publico).
            if (SupabaseSettings.Configurado)
            {
                return SupabaseSettings.ConstruirUrlPublicaObjeto(NombreArchivoPago(pagoId));
            }

            string baseUrl = TwilioSettings.PublicBaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                System.Diagnostics.Trace.WriteLine(
                    "[Factura] MediaUrl vacia: configure Supabase (recomendado) o WhatsAppPublicBaseUrl.");
                return null;
            }

            return $"{baseUrl}/media/factura/{pagoId}.pdf";
        }
    }
}
