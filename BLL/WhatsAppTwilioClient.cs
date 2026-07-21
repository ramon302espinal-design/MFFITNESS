using CORE;
using BLL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BLL
{
    public sealed class WhatsAppTwilioClient
    {
        public WhatsAppEnvioResult Enviar(
            string numeroDestino,
            string mensaje,
            string? contentSidEspecifico = null,
            string? mediaUrl = null,
            IReadOnlyDictionary<string, string>? variablesFactura = null)
        {
            if (string.IsNullOrWhiteSpace(numeroDestino))
            {
                return Fallo("Telefono destino invalido.");
            }

            if (!TwilioSettings.CredencialesConfiguradas)
            {
                return Fallo("Credenciales Twilio no configuradas.");
            }

            string? contentSid = ResolverContentSid(contentSidEspecifico);
            bool usarPlantilla = !string.IsNullOrWhiteSpace(contentSid);
            string? mediaUrlNormalizada = NormalizarMediaUrl(mediaUrl);
            bool tieneMedia = !string.IsNullOrWhiteSpace(mediaUrlNormalizada);

            if (!string.IsNullOrWhiteSpace(mediaUrl) && !tieneMedia)
            {
                Trace.WriteLine($"[WhatsApp] MediaUrl rechazada (no es https absoluta): '{mediaUrl}'");
            }

            // Factura PDF: un solo aviso = Body corto + MediaUrl (COMPROBANTE...).
            // No cascada a plantilla media ni a plantilla+link: eso duplicaba 2-3 mensajes
            // cuando Twilio ya habia aceptado el adjunto (status queued/accepted).
            bool adjuntarLibre = tieneMedia
                && (TwilioSettings.AdjuntarPdfLibre
                    || !TwilioSettings.ModoProduccion
                    || TwilioSettings.PermitirBodyEnProduccion);

            if (tieneMedia && adjuntarLibre && MediaListoParaTwilio(mediaUrlNormalizada))
            {
                var media = EnviarInterno(
                    numeroDestino,
                    string.IsNullOrWhiteSpace(mensaje) ? "Factura MFFITNESS" : TruncarParaMedia(mensaje),
                    contentSid: null,
                    mediaUrl: mediaUrlNormalizada,
                    forzarBodyConMedia: true);

                // Si Twilio acepto el mensaje (hay SID y no fallo duro), NO reenviar.
                if (FueAceptadoPorTwilio(media))
                {
                    return new WhatsAppEnvioResult
                    {
                        Exito = true,
                        Entregado = media.Entregado
                                     || EsEstadoEntregaMediaOk(media.StatusFinal)
                                     || EsEstadoEnviado(media.StatusFinal ?? string.Empty),
                        Detalle = "PDF adjunto enviado." + DetalleMediaUrl(mediaUrlNormalizada),
                        MessageSid = media.MessageSid,
                        StatusFinal = media.StatusFinal
                    };
                }

                Trace.WriteLine(
                    "[WhatsApp] Adjunto libre no aceptado; respaldo plantilla/link. " + media.Detalle);
            }
            else if (tieneMedia && TwilioSettings.UsaPlantillaFacturaMedia)
            {
                // Solo si no hay adjunto libre: plantilla twilio/media.
                var facturaMedia = EnviarFacturaMediaTemplate(
                    numeroDestino,
                    mensaje,
                    mediaUrlNormalizada!,
                    variablesFactura);
                if (FueAceptadoPorTwilio(facturaMedia))
                    return facturaMedia;

                Trace.WriteLine(
                    "[WhatsApp] Plantilla factura media fallo; se intenta respaldo. " + facturaMedia.Detalle);
            }

            // Ultimo recurso: plantilla generica (con link si hay mediaUrl).
            // Solo si el adjunto PDF no se pudo aceptar.
            if (usarPlantilla)
            {
                string mensajeFinal = tieneMedia
                    ? AsegurarLinkEnTexto(mensaje, mediaUrlNormalizada!)
                    : mensaje;
                var soloPlantilla = EnviarInterno(numeroDestino, mensajeFinal, contentSid, mediaUrl: null);
                if (!soloPlantilla.Exito)
                    return soloPlantilla;

                string extra = tieneMedia
                    ? " | PDF como link (adjunto no disponible)."
                      + DetalleMediaUrl(mediaUrlNormalizada)
                    : string.Empty;

                return new WhatsAppEnvioResult
                {
                    Exito = soloPlantilla.Exito,
                    Entregado = soloPlantilla.Entregado
                                 || FueAceptadoPorTwilio(soloPlantilla),
                    Detalle = (soloPlantilla.Detalle ?? string.Empty) + extra,
                    MessageSid = soloPlantilla.MessageSid,
                    StatusFinal = soloPlantilla.StatusFinal
                };
            }

            return EnviarInterno(numeroDestino, mensaje, contentSid, adjuntarLibre ? mediaUrlNormalizada : null);
        }

        private WhatsAppEnvioResult EnviarFacturaMediaTemplate(
            string numeroDestino,
            string mensaje,
            string mediaUrl,
            IReadOnlyDictionary<string, string>? variablesFactura)
        {
            try
            {
                if (!Uri.TryCreate(mediaUrl, UriKind.Absolute, out Uri? uri))
                    return Fallo("MediaUrl invalida para plantilla factura.");

                // Plantilla Meta-compliant:
                // media = https://dominio/{{1}}
                // body  = ... Plan: {{2}}. Monto: {{3}}. ... recibo: {{6}}. texto fijo final.
                string pathSuffix = ExtraerPathVariablePlantillaMedia(uri);

                string plan = LeerVar(variablesFactura, "PLAN", "Membresia");
                string monto = LeerVar(variablesFactura, "MONTO", "Ver PDF");
                string fechaPago = LeerVar(variablesFactura, "FECHA_PAGO", DateTime.Now.ToString("dd/MM/yyyy"));
                string fechaVence = LeerVar(variablesFactura, "FECHA_VENCE", DateTime.Now.ToString("dd/MM/yyyy"));
                string recibo = LeerVar(variablesFactura, "NUMERO_RECIBO", "MF-0");

                // Si no vienen vars estructuradas, no volcar el cuerpo libre entero en {{2}}
                // (eso era lo que Meta rechazo: cuerpo casi vacio + variable al final).
                if (variablesFactura == null || variablesFactura.Count == 0)
                {
                    string cuerpo = WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(mensaje, null);
                    cuerpo = System.Text.RegularExpressions.Regex.Replace(
                        cuerpo,
                        @"https?://\S+",
                        string.Empty,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                    if (!string.IsNullOrWhiteSpace(cuerpo))
                        plan = cuerpo.Length > 80 ? cuerpo[..80].TrimEnd() + "." : cuerpo;
                }

                string contentVariables = WhatsAppContentVariableHelper.SerializarFacturaMedia(
                    pathSuffix,
                    plan,
                    monto,
                    fechaPago,
                    fechaVence,
                    recibo);

                string numeroOrigen = NormalizarE164(TwilioSettings.PhoneNumber);
                string numeroTo = NormalizarE164(numeroDestino);

                InicializarTwilioClient();

                Trace.WriteLine(
                    $"[WhatsApp] Factura MEDIA ContentSid={TwilioSettings.ContentSidFactura} vars={contentVariables}");

                var message = MessageResource.Create(
                    to: new PhoneNumber($"whatsapp:{numeroTo}"),
                    from: new PhoneNumber($"whatsapp:{numeroOrigen}"),
                    contentSid: TwilioSettings.ContentSidFactura,
                    contentVariables: contentVariables);

                string? messageSid = message.Sid;
                if (string.IsNullOrWhiteSpace(messageSid))
                {
                    return new WhatsAppEnvioResult
                    {
                        Exito = true,
                        Entregado = true,
                        Detalle = "PDF adjunto (plantilla media) aceptado por Twilio."
                            + DetalleMediaUrl(mediaUrl),
                        StatusFinal = "accepted"
                    };
                }

                var estado = EsperarEstadoFinal(messageSid, esMedia: true);
                bool ok = !EsEstadoFallido(estado.Status)
                          && (EsEstadoEntregado(estado.Status) || EsEstadoEntregaMediaOk(estado.Status));

                return new WhatsAppEnvioResult
                {
                    Exito = true,
                    Entregado = ok,
                    Detalle = ok
                        ? "PDF adjunto enviado (plantilla twilio/media)." + DetalleMediaUrl(mediaUrl)
                        : FormatearErrorEstado(estado) + DetalleMediaUrl(mediaUrl),
                    MessageSid = messageSid,
                    StatusFinal = estado.Status
                };
            }
            catch (ApiException apiEx)
            {
                return Fallo(FormatearApiException(apiEx) + DetalleMediaUrl(mediaUrl));
            }
            catch (Exception ex)
            {
                return Fallo(ex.Message + DetalleMediaUrl(mediaUrl));
            }
        }

        private static string AsegurarLinkEnTexto(string mensaje, string mediaUrl)
        {
            if (mensaje.Contains(mediaUrl, StringComparison.OrdinalIgnoreCase))
                return mensaje;
            return $"{mensaje} Factura PDF: {mediaUrl}";
        }

        private static string TruncarParaMedia(string mensaje)
        {
            string t = WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(mensaje, null);
            t = System.Text.RegularExpressions.Regex.Replace(
                t,
                @"https?://\S+",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            if (t.Length > 300)
                t = t[..300].TrimEnd() + ".";
            return string.IsNullOrWhiteSpace(t) ? "Factura MFFITNESS" : t;
        }

        private WhatsAppEnvioResult EnviarInterno(
            string numeroDestino,
            string mensaje,
            string? contentSid,
            string? mediaUrl,
            bool forzarBodyConMedia = false)
        {
            bool usarPlantilla = !string.IsNullOrWhiteSpace(contentSid) && string.IsNullOrWhiteSpace(mediaUrl);

            if (TwilioSettings.ModoProduccion
                && !usarPlantilla
                && !TwilioSettings.PermitirBodyEnProduccion
                && string.IsNullOrWhiteSpace(mediaUrl))
            {
                return Fallo(
                    "WhatsApp produccion requiere plantilla ContentSid aprobada. " +
                    "Configure TwilioContentSidGenerico en App.config.");
            }

            // En produccion, MediaUrl + Body puede fallar fuera de ventana 24h;
            // se intenta igual (host/Supabase ya sirvieron el PDF).
            _ = forzarBodyConMedia;

            try
            {
                string numeroOrigen = NormalizarE164(TwilioSettings.PhoneNumber);
                string numeroTo = NormalizarE164(numeroDestino);

                InicializarTwilioClient();

                MessageResource message;

                if (usarPlantilla)
                {
                    string claveVariable = TwilioContentTemplateResolver.ObtenerClaveVariable(contentSid!);
                    string cuerpo = WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(mensaje, null);
                    string contentVariables = WhatsAppContentVariableHelper.Serializar(claveVariable, cuerpo);

                    Trace.WriteLine($"WhatsApp ContentSid={contentSid} Variables={contentVariables}");

                    message = MessageResource.Create(
                        to: new PhoneNumber($"whatsapp:{numeroTo}"),
                        from: new PhoneNumber($"whatsapp:{numeroOrigen}"),
                        contentSid: contentSid,
                        contentVariables: contentVariables);
                }
                else if (!string.IsNullOrWhiteSpace(mediaUrl))
                {
                    // MessageResource.Create: mediaUrl = List<Uri> con URL publica (Supabase o Host).
                    var mediaUri = new Uri(mediaUrl, UriKind.Absolute);
                    var mediaUrls = new List<Uri> { mediaUri };

                    if (!MediaListoParaTwilio(mediaUrl))
                    {
                        return Fallo(
                            "No hay media publica lista. Configure Supabase (bucket FACTURAS) " +
                            "o WhatsAppPublicBaseUrl + WhatsAppHost (fallback)." +
                            DetalleMediaUrl(mediaUrl));
                    }

                    Trace.WriteLine($"WhatsApp MediaUrl={mediaUri.AbsoluteUri}");

                    message = MessageResource.Create(
                        to: new PhoneNumber($"whatsapp:{numeroTo}"),
                        from: new PhoneNumber($"whatsapp:{numeroOrigen}"),
                        body: string.IsNullOrWhiteSpace(mensaje) ? "Factura MFFITNESS" : mensaje,
                        mediaUrl: mediaUrls);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(mensaje))
                        return Fallo("Mensaje WhatsApp vacio (sin Body ni MediaUrl).");

                    message = MessageResource.Create(
                        to: new PhoneNumber($"whatsapp:{numeroTo}"),
                        from: new PhoneNumber($"whatsapp:{numeroOrigen}"),
                        body: mensaje);
                }

                string? messageSid = message.Sid;
                if (string.IsNullOrWhiteSpace(messageSid))
                {
                    return new WhatsAppEnvioResult
                    {
                        Exito = true,
                        Entregado = true,
                        Detalle = "Mensaje aceptado por Twilio." + DetalleMediaUrl(mediaUrl),
                        MessageSid = null,
                        StatusFinal = "accepted"
                    };
                }

                var estadoFinal = EsperarEstadoFinal(messageSid, esMedia: !string.IsNullOrWhiteSpace(mediaUrl));

                if (EsEstadoFallido(estadoFinal.Status))
                {
                    return new WhatsAppEnvioResult
                    {
                        Exito = true,
                        Entregado = false,
                        Detalle = FormatearErrorEstado(estadoFinal) + DetalleMediaUrl(mediaUrl),
                        MessageSid = messageSid,
                        StatusFinal = estadoFinal.Status
                    };
                }

                if (TwilioSettings.ExigirEntregaConfirmada)
                {
                    if (EsEstadoEntregado(estadoFinal.Status) || EsEstadoEnviado(estadoFinal.Status))
                    {
                        return new WhatsAppEnvioResult
                        {
                            Exito = true,
                            Entregado = true,
                            Detalle = (EsEstadoEntregado(estadoFinal.Status)
                                ? $"Entregado (status={estadoFinal.Status})."
                                : $"Aceptado por Twilio (status={estadoFinal.Status}).")
                                + DetalleMediaUrl(mediaUrl),
                            MessageSid = messageSid,
                            StatusFinal = estadoFinal.Status
                        };
                    }

                    return new WhatsAppEnvioResult
                    {
                        Exito = true,
                        Entregado = false,
                        Detalle =
                            $"Twilio no confirmo el envio (status={estadoFinal.Status}). " +
                            (usarPlantilla
                                ? "Verifique plantilla ContentSid y numero destino en WhatsApp Business."
                                : "En produccion use TwilioContentSidGenerico con plantilla aprobada.")
                            + DetalleMediaUrl(mediaUrl),
                        MessageSid = messageSid,
                        StatusFinal = estadoFinal.Status
                    };
                }

                bool entregado = EsEstadoEntregado(estadoFinal.Status) || EsEstadoEnviado(estadoFinal.Status);
                return new WhatsAppEnvioResult
                {
                    Exito = true,
                    Entregado = entregado,
                    Detalle = $"status={estadoFinal.Status}" + DetalleMediaUrl(mediaUrl),
                    MessageSid = messageSid,
                    StatusFinal = estadoFinal.Status
                };
            }
            catch (ApiException apiEx)
            {
                return Fallo(FormatearApiException(apiEx) + DetalleMediaUrl(mediaUrl));
            }
            catch (Exception ex)
            {
                return Fallo(ex.Message + DetalleMediaUrl(mediaUrl));
            }
        }

        private static string LeerVar(
            IReadOnlyDictionary<string, string>? vars,
            string key,
            string fallback)
        {
            if (vars != null
                && vars.TryGetValue(key, out string? value)
                && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
            return fallback;
        }

        /// <summary>
        /// Sufijo para {{1}} de la plantilla twilio/media (dominio fijo en Twilio + path variable).
        /// </summary>
        private static string ExtraerPathVariablePlantillaMedia(Uri uri)
        {
            string absolute = uri.AbsolutePath.TrimStart('/');

            if (SupabaseSettings.Configurado)
            {
                string marker = "object/public/" + SupabaseSettings.BucketFacturas.Trim('/') + "/";
                int idx = absolute.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                    return absolute[(idx + marker.Length)..].TrimStart('/');

                return Path.GetFileName(absolute);
            }

            string baseUrl = TwilioSettings.PublicBaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(baseUrl)
                && Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri)
                && string.Equals(uri.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase))
            {
                string basePath = baseUri.AbsolutePath.Trim('/');
                if (!string.IsNullOrEmpty(basePath)
                    && absolute.StartsWith(basePath + "/", StringComparison.OrdinalIgnoreCase))
                    return absolute[(basePath.Length + 1)..];
            }

            return absolute;
        }

        /// <summary>
        /// Supabase Storage publico no necesita WhatsAppHost/Ngrok.
        /// </summary>
        private static bool MediaListoParaTwilio(string? mediaUrl)
        {
            if (!string.IsNullOrWhiteSpace(mediaUrl)
                && mediaUrl.IndexOf("supabase.co", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (SupabaseSettings.Configurado)
                return true;

            return MediaHostDisponible();
        }

        /// <summary>
        /// Health del host local (fallback sin Supabase).
        /// </summary>
        private static bool MediaHostDisponible()
        {
            try
            {
                string baseUrl = TwilioSettings.MediaListenUrl?.TrimEnd('/') ?? "http://127.0.0.1:5088";
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var resp = client.GetAsync(baseUrl + "/health").GetAwaiter().GetResult();
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static string? NormalizarMediaUrl(string? mediaUrl)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl))
                return null;

            string trimmed = mediaUrl.Trim();
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri))
                return null;

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                return null;

            return uri.AbsoluteUri.TrimEnd('/');
        }

        private static string DetalleMediaUrl(string? mediaUrl) =>
            string.IsNullOrWhiteSpace(mediaUrl) ? string.Empty : $" MediaUrl={mediaUrl}";

        private static string NormalizarE164(string numero)
        {
            numero = (numero ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(numero))
                return numero;

            if (!numero.StartsWith("+"))
                numero = "+" + numero.TrimStart('+');

            return numero;
        }

        private static string? ResolverContentSid(string? contentSidEspecifico)
        {
            // Produccion: siempre la plantilla Approved de App.config (ignora SID viejos en BD).
            if (TwilioSettings.ModoProduccion && !string.IsNullOrWhiteSpace(TwilioSettings.ContentSidGenerico))
                return TwilioSettings.ContentSidGenerico.Trim();

            if (!string.IsNullOrWhiteSpace(contentSidEspecifico))
                return contentSidEspecifico.Trim();

            return string.IsNullOrWhiteSpace(TwilioSettings.ContentSidGenerico)
                ? null
                : TwilioSettings.ContentSidGenerico.Trim();
        }

        private static (string Status, string? ErrorCode, string? ErrorMessage) EsperarEstadoFinal(
            string messageSid,
            bool esMedia = false)
        {
            string ultimoStatus = "queued";
            string? errorCode = null;
            string? errorMessage = null;

            // No congelar la UI: pocas consultas cortas. Twilio sigue procesando en segundo plano.
            int intentosConfig = TwilioSettings.IntentosConsultaEstado;
            int intentos = esMedia
                ? Math.Clamp(Math.Min(intentosConfig, 4), 1, 4)
                : Math.Clamp(Math.Min(intentosConfig, 3), 1, 3);
            int esperaBase = Math.Clamp(TwilioSettings.MilisegundosEntreConsultasEstado, 200, 800);

            for (int intento = 0; intento < intentos; intento++)
            {
                int esperaMs = intento == 0 ? Math.Min(400, esperaBase) : esperaBase;
                System.Threading.Thread.Sleep(esperaMs);

                var fetched = MessageResource.Fetch(pathSid: messageSid);
                ultimoStatus = fetched.Status?.ToString() ?? ultimoStatus;
                errorCode = fetched.ErrorCode?.ToString();
                errorMessage = fetched.ErrorMessage;

                if (EsEstadoFallido(ultimoStatus) || EsEstadoEntregado(ultimoStatus))
                    break;

                if (esMedia)
                {
                    if (EsEstadoEntregaMediaOk(ultimoStatus)
                        || EsEstadoEnviadoReal(ultimoStatus)
                        || ultimoStatus.Equals("accepted", StringComparison.OrdinalIgnoreCase)
                        || ultimoStatus.Equals("queued", StringComparison.OrdinalIgnoreCase))
                        break;
                    continue;
                }

                if (EsEstadoEnviado(ultimoStatus)
                    || ultimoStatus.Equals("queued", StringComparison.OrdinalIgnoreCase)
                    || ultimoStatus.Equals("accepted", StringComparison.OrdinalIgnoreCase))
                    break;
            }

            return (ultimoStatus, errorCode, errorMessage);
        }

        private static bool EsEstadoEntregado(string status) =>
            status.Equals("delivered", StringComparison.OrdinalIgnoreCase)
            || status.Equals("read", StringComparison.OrdinalIgnoreCase);

        private static bool EsEstadoEnviadoReal(string status) =>
            status.Equals("sent", StringComparison.OrdinalIgnoreCase)
            || status.Equals("accepted", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Para adjuntos: "sending"/"queued" NO cuentan — ahi Twilio aun puede fallar (63016, media).
        /// </summary>
        private static bool EsEstadoEntregaMediaOk(string? status) =>
            !string.IsNullOrWhiteSpace(status)
            && (EsEstadoEntregado(status) || status.Equals("sent", StringComparison.OrdinalIgnoreCase));

        private static bool EsEstadoEnviado(string status) =>
            EsEstadoEnviadoReal(status)
            || status.Equals("sending", StringComparison.OrdinalIgnoreCase)
            || status.Equals("queued", StringComparison.OrdinalIgnoreCase);

        private static bool EsEstadoFallido(string status) =>
            status.Equals("failed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("undelivered", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Twilio ya acepto el mensaje (hay SID y no esta en failed/undelivered).
        /// Evita reenviar respaldos que duplican avisos en el chat del cliente.
        /// </summary>
        private static bool FueAceptadoPorTwilio(WhatsAppEnvioResult r)
        {
            if (r == null || !r.Exito)
                return false;

            if (!string.IsNullOrWhiteSpace(r.StatusFinal) && EsEstadoFallido(r.StatusFinal))
                return false;

            return !string.IsNullOrWhiteSpace(r.MessageSid)
                   || r.Entregado
                   || EsEstadoEntregaMediaOk(r.StatusFinal)
                   || (!string.IsNullOrWhiteSpace(r.StatusFinal)
                       && EsEstadoEnviado(r.StatusFinal));
        }

        private static void InicializarTwilioClient()
        {
            if (TwilioSettings.UsaApiKey)
                TwilioClient.Init(TwilioSettings.ApiKeySid, TwilioSettings.ApiKeySecret, TwilioSettings.AccountSid);
            else
                TwilioClient.Init(TwilioSettings.AccountSid, TwilioSettings.AuthToken);
        }

        private static WhatsAppEnvioResult Fallo(string detalle) =>
            new() { Exito = false, Entregado = false, Detalle = detalle };

        private static string FormatearErrorEstado((string Status, string? ErrorCode, string? ErrorMessage) estado)
        {
            if (!string.IsNullOrWhiteSpace(estado.ErrorCode) || !string.IsNullOrWhiteSpace(estado.ErrorMessage))
            {
                return FormatearErrorTwilio(estado.ErrorCode, estado.ErrorMessage);
            }

            return $"Twilio status={estado.Status}";
        }

        private static string FormatearApiException(ApiException apiEx) =>
            FormatearErrorTwilio(apiEx.Code.ToString(), apiEx.Message);

        private static string FormatearErrorTwilio(string? code, string? message)
        {
            if (code == "63007")
            {
                return TwilioSettings.ModoProduccion
                    ? "ERROR 63007: Numero destino no autorizado. El cliente debe escribir primero al +14247284594 o usar plantilla ContentSid aprobada."
                    : "ERROR 63007: Numero no esta en sandbox. Envie 'join <codigo>' al +14155238886.";
            }

            if (code == "63016" || (message?.Contains("24 hour", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                if (!string.IsNullOrWhiteSpace(TwilioSettings.ContentSidGenerico))
                {
                    return "ERROR 63016: Mensaje fuera de ventana 24h o plantilla no elegible. " +
                           "Verifique en Twilio que la plantilla " +
                           $"(ContentSid {TwilioSettings.ContentSidGenerico}) este Approved con " +
                           "'WhatsApp business initiated' activo. Si el numero ya fue verificado por Meta, " +
                           "reintente el envio; el cliente tambien puede escribir primero al " +
                           $"{TwilioSettings.PhoneNumber} para abrir ventana 24h.";
                }

                return "ERROR 63016: Fuera de ventana 24h. Configure TwilioContentSidGenerico con plantilla aprobada en Twilio/Meta.";
            }

            if (code == "21656")
            {
                return "ERROR 21656: Variables de plantilla invalidas para WhatsApp. " +
                       $"Plantilla activa: {TwilioSettings.ContentSidGenerico}. " +
                       "Revise que la plantilla tenga {{1}} Approved en Twilio/Meta y que el texto no viole reglas de variables.";
            }

            if (code == "63015" || (message?.Contains("Channel", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return $"ERROR canal WhatsApp: {message}. Verifique que +14247284594 este activo como WhatsApp Sender en Twilio.";
            }

            return $"Twilio error {code}: {message}";
        }
    }
}
