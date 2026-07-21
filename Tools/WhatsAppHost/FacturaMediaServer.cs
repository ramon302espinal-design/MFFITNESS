using CORE;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace WhatsAppHost
{
    /// <summary>
    /// Kestrel interno (fallback): sirve PDF locales si Supabase no esta configurado.
    /// GET /media/factura/{pagoId}
    /// Requiere URL publica HTTPS en WhatsAppPublicBaseUrl para que Twilio descargue.
    /// </summary>
    internal static class FacturaMediaServer
    {
        public static IHost? Start(CancellationToken cancellationToken)
        {
            string listenUrl = TwilioSettings.MediaListenUrl;
            if (string.IsNullOrWhiteSpace(listenUrl))
                listenUrl = "http://127.0.0.1:5088";

            Directory.CreateDirectory(FacturaStorage.CarpetaFacturas);
            Directory.CreateDirectory(FacturaStorage.CarpetaWwwrootFacturas);

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls(listenUrl);

            var app = builder.Build();

            app.MapGet("/health", () => Results.Ok(new
            {
                status = "ok",
                facturas = FacturaStorage.CarpetaFacturas,
                publicBase = TwilioSettings.PublicBaseUrl
            }));

            app.MapGet("/media/factura/{pagoId:int}", (int pagoId) =>
            {
                if (pagoId <= 0)
                    return Results.BadRequest("pagoId invalido.");

                string? path = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
                if (path == null)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PDF no encontrado pagoId={pagoId}");
                    return Results.NotFound($"No hay factura PDF para pago {pagoId}.");
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sirviendo PDF pagoId={pagoId} -> {path}");
                return Results.File(
                    path,
                    contentType: "application/pdf",
                    fileDownloadName: FacturaStorage.NombreArchivoPago(pagoId),
                    enableRangeProcessing: true);
            });

            // Alias por si Twilio pide con extensión
            app.MapGet("/media/factura/{pagoId:int}.pdf", (int pagoId) =>
            {
                string? path = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
                if (path == null)
                    return Results.NotFound();

                return Results.File(path, "application/pdf", FacturaStorage.NombreArchivoPago(pagoId));
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    await app.RunAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // apagado normal
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Kestrel ERROR: {ex.Message}");
                }
            }, cancellationToken);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Kestrel media en {listenUrl}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Endpoint: GET /media/factura/{{pagoId}}");
            if (!string.IsNullOrWhiteSpace(TwilioSettings.PublicBaseUrl))
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PublicBaseUrl: {TwilioSettings.PublicBaseUrl}");
            else
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] AVISO: sin WhatsAppPublicBaseUrl. Preferible Supabase; si no, ponga una URL HTTPS publica.");

            return app;
        }
    }
}
