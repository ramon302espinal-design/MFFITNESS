using CORE;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace WhatsAppHost
{
    /// <summary>
    /// Kestrel interno: webhook inbound Twilio + PDF locales (fallback sin Supabase).
    /// POST /webhook/twilio/whatsapp — mensajes entrantes WhatsApp
    /// GET  /media/factura/{pagoId} — PDF para Twilio (si no hay Supabase)
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

            app.MapGet("/health", () =>
            {
                WhatsAppStackSecrets.InvalidateCache();
                return Results.Ok(new
                {
                    status = "ok",
                    facturas = FacturaStorage.CarpetaFacturas,
                    publicBase = TwilioSettings.PublicBaseUrl,
                    webhook = TwilioSettings.WebhookPublicUrl ?? "(configurar WhatsAppPublicBaseUrl)"
                });
            });

            app.MapPost(TwilioSettings.WebhookInboundPath, (Delegate)TwilioInboundWebhook.HandleAsync);

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

            app.MapGet("/media/factura/{pagoId:int}.pdf", (int pagoId) =>
            {
                string? path = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
                if (path == null)
                    return Results.NotFound();

                return Results.File(path, "application/pdf", FacturaStorage.NombreArchivoPago(pagoId));
            });

            app.MapGet("/media/chat/{fileName}", (string fileName) =>
            {
                if (!ChatMediaStorage.EsNombreSeguro(fileName))
                    return Results.BadRequest("nombre invalido.");

                string? path = ChatMediaStorage.ResolverRutaExistente(fileName);
                if (path == null)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Chat PDF no encontrado: {fileName}");
                    return Results.NotFound();
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sirviendo chat PDF -> {path}");
                return Results.File(path, "application/pdf", fileName, enableRangeProcessing: true);
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

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Kestrel en {listenUrl}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Webhook inbound: POST {TwilioSettings.WebhookInboundPath}");
            if (!string.IsNullOrWhiteSpace(TwilioSettings.WebhookPublicUrl))
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Twilio Console URL: {TwilioSettings.WebhookPublicUrl}");
            else
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] AVISO: configure WhatsAppPublicBaseUrl (HTTPS) para que Twilio entregue mensajes entrantes.");

            if (!SupabaseSettings.Configurado)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Media fallback: GET /media/factura/{{pagoId}}");
                if (!string.IsNullOrWhiteSpace(TwilioSettings.PublicBaseUrl))
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PublicBaseUrl: {TwilioSettings.PublicBaseUrl}");
            }

            return app;
        }
    }
}
