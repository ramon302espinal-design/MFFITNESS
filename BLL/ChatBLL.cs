using BLL.Models;
using BLL.Facturas;
using CORE;
using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BLL
{
    public class ChatBLL
    {
        private readonly ChatDAL chatDal = new();
        private readonly ClienteDAL clienteDal = new();
        private readonly PagoDAL pagoDal = new();
        private readonly WhatsAppTwilioClient twilio = new();

        public IReadOnlyList<ChatConversacionDto> ListarConversaciones(
            string? filtroNombre = null,
            bool soloNoLeidos = false)
        {
            chatDal.EnsureSchema();
            DataTable dt = chatDal.ListarConversaciones(filtroNombre);
            var lista = new List<ChatConversacionDto>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                string telefonoRaw = row["Telefono"]?.ToString() ?? string.Empty;
                string telefono = MensajeAutomaticoBLL.NormalizarTelefono(telefonoRaw);

                lista.Add(new ChatConversacionDto
                {
                    ClienteId = Convert.ToInt32(row["ClienteId"]),
                    Nombre = row["Nombre"]?.ToString() ?? "Cliente",
                    Telefono = telefonoRaw,
                    UltimoMensaje = row["UltimoMensaje"] == DBNull.Value
                        ? null
                        : row["UltimoMensaje"]?.ToString(),
                    UltimaFecha = row["UltimaFecha"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(row["UltimaFecha"]),
                    NoLeidos = row["NoLeidos"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(row["NoLeidos"]),
                    TelefonoValido = MensajeAutomaticoBLL.EsTelefonoValido(telefono)
                });
            }

            if (soloNoLeidos)
                lista = lista.Where(c => c.NoLeidos > 0).ToList();

            lista.Sort((a, b) =>
            {
                int cmpLeidos = b.NoLeidos.CompareTo(a.NoLeidos);
                if (cmpLeidos != 0)
                    return cmpLeidos;

                DateTime fa = a.UltimaFecha ?? DateTime.MinValue;
                DateTime fb = b.UltimaFecha ?? DateTime.MinValue;
                int cmpFecha = fb.CompareTo(fa);
                return cmpFecha != 0
                    ? cmpFecha
                    : string.Compare(a.Nombre, b.Nombre, StringComparison.OrdinalIgnoreCase);
            });

            return lista;
        }

        public int ContarNoLeidosTotal()
        {
            chatDal.EnsureSchema();
            return chatDal.ContarTotalEntradaNoLeida();
        }

        public bool ClienteTieneTelefonoChat(int clienteId)
        {
            var cliente = clienteDal.ObtenerClientePorId(clienteId);
            if (cliente == null)
                return false;

            string telefono = MensajeAutomaticoBLL.NormalizarTelefono(cliente["Telefono"]?.ToString());
            return MensajeAutomaticoBLL.EsTelefonoValido(telefono);
        }

        public int ContarNoLeidosCliente(int clienteId)
        {
            chatDal.EnsureSchema();
            return chatDal.ContarEntradaNoLeida(clienteId);
        }

        public int ObtenerMaxIdMensajeEntrada()
        {
            chatDal.EnsureSchema();
            return chatDal.ObtenerMaxIdEntrada();
        }

        public IReadOnlyList<ChatNotificacionDto> ListarEntradasDesdeId(int ultimoId)
        {
            chatDal.EnsureSchema();
            DataTable dt = chatDal.ListarEntradasDesdeId(ultimoId);
            var lista = new List<ChatNotificacionDto>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ChatNotificacionDto
                {
                    MensajeId = Convert.ToInt32(row["MensajeId"]),
                    ClienteId = Convert.ToInt32(row["ClienteId"]),
                    ClienteNombre = row["ClienteNombre"]?.ToString() ?? "Cliente",
                    Cuerpo = row["Cuerpo"]?.ToString() ?? string.Empty,
                    Fecha = Convert.ToDateTime(row["Fecha"])
                });
            }

            return lista;
        }

        public IReadOnlyList<ChatMensajeDto> ListarMensajes(int clienteId)
        {
            chatDal.EnsureSchema();
            DataTable dt = chatDal.ListarMensajes(clienteId);
            var lista = new List<ChatMensajeDto>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ChatMensajeDto
                {
                    Id = Convert.ToInt64(row["Id"]),
                    ClienteId = Convert.ToInt32(row["ClienteId"]),
                    Direccion = row["Direccion"]?.ToString() ?? "SALIDA",
                    Cuerpo = row["Cuerpo"]?.ToString() ?? string.Empty,
                    Estado = row["Estado"]?.ToString() ?? string.Empty,
                    Usuario = row["Usuario"] == DBNull.Value ? null : row["Usuario"]?.ToString(),
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    EsAutomatico = row["EsAutomatico"] != DBNull.Value && Convert.ToBoolean(row["EsAutomatico"]),
                    TipoAutomatico = row["TipoAutomatico"] == DBNull.Value
                        ? null
                        : row["TipoAutomatico"]?.ToString()
                });
            }

            return lista;
        }

        public int ContarMensajesCliente(int clienteId)
        {
            chatDal.EnsureSchema();
            return chatDal.ContarMensajesCliente(clienteId);
        }

        public int ObtenerMaxIdMensajeChat(int clienteId)
        {
            chatDal.EnsureSchema();
            return chatDal.ObtenerMaxIdChatCliente(clienteId);
        }

        public ChatEnvioResult EnviarMensajeManual(int clienteId, string texto)
        {
            chatDal.EnsureSchema();

            if (string.IsNullOrWhiteSpace(texto))
                return Fallo("Escriba un mensaje antes de enviar.");

            if (!ValidarWhatsAppConfigurado(out ChatEnvioResult? errorConfig))
                return errorConfig!;

            if (!TryGetClienteParaEnvio(clienteId, out string telefono, out string nombre))
            {
                var cliente = clienteDal.ObtenerClientePorId(clienteId);
                if (cliente == null)
                    return Fallo("Cliente no encontrado.");

                string nom = cliente["Nombre"]?.ToString() ?? "Cliente";
                return Fallo($"Teléfono inválido para {nom}. Actualice el número en Clientes.");
            }

            string cuerpo = texto.Trim();
            string usuario = ObtenerUsuarioActual();

            int mensajeId = chatDal.InsertarMensaje(
                clienteId,
                "SALIDA",
                cuerpo,
                "PENDIENTE",
                usuario);

            WhatsAppEnvioResult envio = twilio.Enviar(telefono, cuerpo);
            return FinalizarEnvio(clienteId, nombre, mensajeId, envio, "Mensaje enviado");
        }

        public ChatEnvioResult EnviarPdfManual(int clienteId, string rutaPdf, string? caption = null)
        {
            chatDal.EnsureSchema();

            if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
                return Fallo("Seleccione un archivo PDF válido.");

            if (!ValidarWhatsAppConfigurado(out ChatEnvioResult? errorConfig))
                return errorConfig!;

            if (!TryGetClienteParaEnvio(clienteId, out string telefono, out string nombre))
            {
                var cliente = clienteDal.ObtenerClientePorId(clienteId);
                if (cliente == null)
                    return Fallo("Cliente no encontrado.");

                string nom = cliente["Nombre"]?.ToString() ?? "Cliente";
                return Fallo($"Teléfono inválido para {nom}. Actualice el número en Clientes.");
            }

            byte[] pdfBytes;
            try
            {
                pdfBytes = File.ReadAllBytes(rutaPdf);
            }
            catch (Exception ex)
            {
                return Fallo("No se pudo leer el PDF: " + ex.Message);
            }

            if (pdfBytes.Length == 0)
                return Fallo("El PDF está vacío.");

            string? mediaUrl = ChatMediaUploader.PublicarPdf(clienteId, pdfBytes);
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                WhatsAppMediaHostLauncher.EnsureRunning();
                string fileName = ChatMediaStorage.GuardarPdf(clienteId, pdfBytes);
                mediaUrl = ChatMediaStorage.ConstruirMediaUrlPublica(fileName);
            }

            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                return Fallo(
                    "No se pudo publicar el PDF para Twilio. "
                    + "Configure Supabase Storage o WhatsAppPublicBaseUrl + WhatsAppHost.");
            }

            string nombreArchivo = Path.GetFileName(rutaPdf);
            string captionEnvio = string.IsNullOrWhiteSpace(caption) ? string.Empty : caption.Trim();
            string cuerpoLog = string.IsNullOrWhiteSpace(captionEnvio)
                ? $"[PDF enviado]\n{nombreArchivo}\n{mediaUrl}"
                : $"{captionEnvio}\n[PDF: {nombreArchivo}]\n{mediaUrl}";

            string usuario = ObtenerUsuarioActual();
            int mensajeId = chatDal.InsertarMensaje(
                clienteId,
                "SALIDA",
                cuerpoLog,
                "PENDIENTE",
                usuario);

            WhatsAppEnvioResult envio = twilio.Enviar(telefono, captionEnvio, mediaUrl: mediaUrl);
            return FinalizarEnvio(clienteId, nombre, mensajeId, envio, "PDF enviado");
        }

        /// <summary>
        /// Busca el último pago del miembro, asegura su PDF y lo envía por WhatsApp.
        /// </summary>
        public ChatEnvioResult EnviarUltimaFacturaMiembro(int clienteId, string? caption = null)
        {
            chatDal.EnsureSchema();

            if (!ValidarWhatsAppConfigurado(out ChatEnvioResult? errorConfig))
                return errorConfig!;

            if (!TryGetClienteParaEnvio(clienteId, out string telefono, out string nombre))
            {
                var cliente = clienteDal.ObtenerClientePorId(clienteId);
                if (cliente == null)
                    return Fallo("Cliente no encontrado.");

                string nom = cliente["Nombre"]?.ToString() ?? "Cliente";
                return Fallo($"Teléfono inválido para {nom}. Actualice el número en Clientes.");
            }

            DataRow? pago = pagoDal.ObtenerUltimoPagoPorCliente(clienteId);
            if (pago == null)
                return Fallo("Este miembro no tiene pagos registrados.");

            int pagoId = Convert.ToInt32(pago["Id"]);
            if (!TryAsegurarFacturaPdf(clienteId, pago, out string? rutaPdf, out string errorPdf))
                return Fallo(errorPdf);

            string? mediaUrl = ResolverMediaUrlFactura(pagoId);
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                if (!string.IsNullOrWhiteSpace(rutaPdf) && File.Exists(rutaPdf))
                    return EnviarPdfManual(clienteId, rutaPdf, caption);

                return Fallo(
                    "No se pudo publicar la factura para Twilio. "
                    + "Configure Supabase Storage o WhatsAppPublicBaseUrl + WhatsAppHost.");
            }

            string concepto = pago["Concepto"]?.ToString()?.Trim() ?? "Membresía";
            decimal monto = pago["Monto"] != DBNull.Value ? Convert.ToDecimal(pago["Monto"]) : 0m;
            DateTime fechaPago = pago["FechaPago"] != DBNull.Value
                ? Convert.ToDateTime(pago["FechaPago"])
                : DateTime.Now;

            string captionEnvio = string.IsNullOrWhiteSpace(caption) ? string.Empty : caption.Trim();
            string cuerpoLog = string.IsNullOrWhiteSpace(captionEnvio)
                ? $"[Factura #{pagoId}] {concepto} · {monto:C2} · {fechaPago:dd/MM/yyyy}\n{mediaUrl}"
                : $"{captionEnvio}\n[Factura #{pagoId}] {concepto} · {monto:C2}\n{mediaUrl}";

            string usuario = ObtenerUsuarioActual();
            int mensajeId = chatDal.InsertarMensaje(
                clienteId,
                "SALIDA",
                cuerpoLog,
                "PENDIENTE",
                usuario);

            WhatsAppEnvioResult envio = twilio.Enviar(telefono, captionEnvio, mediaUrl: mediaUrl);
            return FinalizarEnvio(clienteId, nombre, mensajeId, envio, "Factura enviada");
        }

        private static string? ResolverMediaUrlFactura(int pagoId)
        {
            if (pagoId <= 0)
                return null;

            if (SupabaseSettings.Configurado)
                return FacturaSupabaseUploader.AsegurarPublicada(pagoId);

            WhatsAppMediaHostLauncher.EnsureRunning();
            return FacturaStorage.ConstruirMediaUrlPublica(pagoId);
        }

        private static bool TryAsegurarFacturaPdf(
            int clienteId,
            DataRow pago,
            out string? rutaPdf,
            out string error)
        {
            rutaPdf = null;
            error = string.Empty;

            int pagoId = Convert.ToInt32(pago["Id"]);
            rutaPdf = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
            if (!string.IsNullOrWhiteSpace(rutaPdf))
                return true;

            string concepto = pago["Concepto"]?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(concepto))
                concepto = "Membresía";

            decimal monto = pago["Monto"] != DBNull.Value ? Convert.ToDecimal(pago["Monto"]) : 0m;
            DateTime vencimiento = pago["FechaVencimiento"] != DBNull.Value
                ? Convert.ToDateTime(pago["FechaVencimiento"])
                : DateTime.Today.AddMonths(1);
            string metodo = pago["MetodoPago"]?.ToString()?.Trim() ?? "Efectivo";

            try
            {
                rutaPdf = FacturaMembresiaPdfGenerator.GenerarDesdePago(
                    clienteId,
                    concepto,
                    monto,
                    vencimiento,
                    metodo,
                    pagoId);
            }
            catch (Exception ex)
            {
                error = "Error al generar factura PDF: " + ex.Message;
                return false;
            }

            if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
            {
                error = "No se pudo generar el PDF de la última factura.";
                return false;
            }

            return true;
        }

        private bool ValidarWhatsAppConfigurado(out ChatEnvioResult? error)
        {
            error = null;
            if (!TwilioSettings.WhatsAppHabilitado)
            {
                error = Fallo("WhatsApp deshabilitado en configuración.");
                return false;
            }

            if (!TwilioSettings.CredencialesConfiguradas)
            {
                error = Fallo("Credenciales Twilio no configuradas.");
                return false;
            }

            return true;
        }

        private bool TryGetClienteParaEnvio(int clienteId, out string telefono, out string nombre)
        {
            telefono = string.Empty;
            nombre = "Cliente";

            var cliente = clienteDal.ObtenerClientePorId(clienteId);
            if (cliente == null)
                return false;

            nombre = cliente["Nombre"]?.ToString() ?? "Cliente";
            telefono = MensajeAutomaticoBLL.NormalizarTelefono(cliente["Telefono"]?.ToString());
            if (!MensajeAutomaticoBLL.EsTelefonoValido(telefono))
            {
                return false;
            }

            return true;
        }

        private static string ObtenerUsuarioActual() =>
            string.IsNullOrWhiteSpace(Sesion.Usuario) ? "POS" : Sesion.Usuario;

        private ChatEnvioResult FinalizarEnvio(
            int clienteId,
            string nombre,
            int mensajeId,
            WhatsAppEnvioResult envio,
            string prefijoExito)
        {
            string estado = envio.Exito ? "ENVIADO" : "ERROR";
            chatDal.ActualizarEstadoMensaje(
                mensajeId,
                estado,
                envio.MessageSid,
                envio.Exito ? null : envio.Detalle);

            if (!envio.Exito)
            {
                return new ChatEnvioResult
                {
                    Exito = false,
                    Detalle = envio.Detalle,
                    MensajeId = mensajeId
                };
            }

            return new ChatEnvioResult
            {
                Exito = true,
                Detalle = $"{prefijoExito} a {nombre}.",
                MensajeId = mensajeId
            };
        }

        public void MarcarConversacionLeida(int clienteId)
        {
            chatDal.EnsureSchema();
            chatDal.MarcarEntradaLeida(clienteId);
        }

        public string ObtenerEstadoConexion()
        {
            if (!TwilioSettings.WhatsAppHabilitado)
                return "WhatsApp deshabilitado";

            if (!TwilioSettings.CredencialesConfiguradas)
                return "Twilio sin credenciales";

            string? advertencia = TwilioSettings.ObtenerAdvertenciaConfiguracion();
            if (!string.IsNullOrWhiteSpace(advertencia))
                return advertencia;

            string inbound = TwilioSettings.ObtenerEstadoWebhookInbound();
            if (!string.IsNullOrWhiteSpace(inbound))
                return inbound;

            return TwilioSettings.ModoProduccion
                ? "Conectado (producción)"
                : "Conectado (desarrollo)";
        }

        /// <summary>
        /// Registra un mensaje entrante de Twilio (webhook inbound).
        /// Idempotente por MessageSid.
        /// </summary>
        public ChatInboundResult ProcesarMensajeEntrante(
            string fromWhatsApp,
            string? body,
            string messageSid,
            int numMedia,
            IReadOnlyList<string> mediaUrls,
            string? profileName)
        {
            chatDal.EnsureSchema();

            if (string.IsNullOrWhiteSpace(messageSid))
                return FalloInbound("MessageSid vacío.");

            if (chatDal.ExistePorTwilioSid(messageSid))
            {
                return new ChatInboundResult
                {
                    Exito = true,
                    Duplicado = true,
                    Detalle = "Mensaje ya registrado."
                };
            }

            string telefono = MensajeAutomaticoBLL.NormalizarTelefono(fromWhatsApp);
            var cliente = clienteDal.ObtenerClientePorTelefono(telefono);
            if (cliente == null)
            {
                return new ChatInboundResult
                {
                    Exito = false,
                    Telefono = telefono,
                    Detalle = $"Sin cliente registrado para {telefono}."
                };
            }

            int clienteId = Convert.ToInt32(cliente["Id"]);
            string nombre = cliente["Nombre"]?.ToString() ?? "Cliente";
            string cuerpo = ConstruirCuerpoEntrante(body, numMedia, mediaUrls);

            if (string.IsNullOrWhiteSpace(cuerpo))
                cuerpo = "[Mensaje vacío]";

            string remitente = string.IsNullOrWhiteSpace(profileName) ? "WhatsApp" : profileName.Trim();

            int mensajeId = chatDal.InsertarMensaje(
                clienteId,
                "ENTRADA",
                cuerpo,
                "RECIBIDO",
                remitente,
                messageSid);

            return new ChatInboundResult
            {
                Exito = true,
                MensajeId = mensajeId,
                ClienteId = clienteId,
                Telefono = telefono,
                Detalle = $"Entrada de {nombre}."
            };
        }

        private static string ConstruirCuerpoEntrante(
            string? body,
            int numMedia,
            IReadOnlyList<string> mediaUrls)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(body))
                partes.Add(body.Trim());

            if (numMedia > 0)
            {
                partes.Add($"[Adjunto x{numMedia}]");
                foreach (string url in mediaUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        partes.Add(url.Trim());
                }
            }

            return string.Join("\n", partes);
        }

        private static ChatInboundResult FalloInbound(string detalle) =>
            new() { Exito = false, Detalle = detalle };

        private static ChatEnvioResult Fallo(string detalle) =>
            new() { Exito = false, Detalle = detalle };
    }
}
