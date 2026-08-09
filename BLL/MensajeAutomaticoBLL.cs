using CORE;
using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using BLL.Models;

namespace BLL
{
    public class MensajeAutomaticoBLL
    {
        private readonly MensajeAutomaticoDAL dal = new MensajeAutomaticoDAL();
        private readonly ClienteDAL clienteDAL = new ClienteDAL();
        private readonly DeudaDAL deudaDAL = new DeudaDAL();
        private readonly WhatsAppTwilioClient twilioClient = new WhatsAppTwilioClient();

        /// <summary>Detalle del ultimo intento Twilio (para mostrar en UI tras cobrar).</summary>
        public string? UltimoDetalleEnvio { get; private set; }

        // ===============================
        // ENVÍO POR PLANTILLA (NÚCLEO)
        // ===============================
        public bool EnviarMensajeTemplado(
            int clienteId,
            string tipoPlantilla,
            Dictionary<string, string> variables,
            int? referenciaId = null,
            string? mediaUrl = null,
            bool adjuntarArchivo = true,
            string? cuerpoTwilioOverride = null)
        {
            if (!TwilioSettings.WhatsAppHabilitado)
            {
                System.Diagnostics.Debug.WriteLine($"WhatsApp deshabilitado. No se envia {tipoPlantilla}.");
                UltimoDetalleEnvio = "WhatsApp deshabilitado.";
                return false;
            }

            int mensajeId = 0;
            try
            {
                if (!TryObtenerTelefonoCliente(clienteId, out string numeroTelefono, out string nombreCliente))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"WhatsApp: cliente {clienteId} sin telefono valido para {tipoPlantilla}.");
                    UltimoDetalleEnvio = "Cliente sin telefono valido.";
                    return false;
                }

                variables["CLIENTE"] = nombreCliente;

                string plantilla = dal.ObtenerPlantilla(tipoPlantilla);
                // cuerpoTwilioOverride == "" es valido (factura solo PDF, sin texto).
                if (string.IsNullOrWhiteSpace(plantilla) && cuerpoTwilioOverride is null)
                {
                    System.Diagnostics.Debug.WriteLine($"Plantilla no encontrada: {tipoPlantilla}");
                    UltimoDetalleEnvio = $"Plantilla no encontrada: {tipoPlantilla}";
                    return false;
                }

                string mensajeTwilio;
                if (cuerpoTwilioOverride != null)
                {
                    // Incluye "" para enviar solo MediaUrl sin caption.
                    mensajeTwilio = string.IsNullOrWhiteSpace(cuerpoTwilioOverride)
                        ? string.Empty
                        : WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(
                            cuerpoTwilioOverride, nombreCliente);
                }
                else
                {
                    string mensaje = AplicarVariables(plantilla!, variables);
                    mensajeTwilio = WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(mensaje, nombreCliente);
                }

                mensajeId = dal.RegistrarMensaje(
                    clienteId,
                    tipoPlantilla,
                    numeroTelefono,
                    mensajeTwilio,
                    "PENDIENTE",
                    referenciaId);

                var resultado = twilioClient.Enviar(
                    numeroTelefono,
                    mensajeTwilio,
                    null,
                    mediaUrl,
                    variables);

                string detalleRespuesta = FormatearDetalleRespuesta(resultado);
                // (ya no se sugiere abrir link en UI; el PDF debe ir como adjunto)

                UltimoDetalleEnvio = detalleRespuesta;

                if (!resultado.Exito)
                {
                    dal.ActualizarEstadoMensaje(mensajeId, "ERROR", detalleRespuesta);
                    dal.IncrementarIntentos(mensajeId);
                    return false;
                }

                // Aceptado por Twilio (SID) cuenta como enviado: no disparar segundo aviso de respaldo.
                bool ok = resultado.Entregado
                          || !string.IsNullOrWhiteSpace(resultado.MessageSid);

                dal.ActualizarEstadoMensaje(
                    mensajeId,
                    ok ? "ENVIADO" : "ERROR",
                    detalleRespuesta);

                if (!ok)
                    dal.IncrementarIntentos(mensajeId);

                return ok;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enviando {tipoPlantilla}: {ex.Message}");
                UltimoDetalleEnvio = ex.Message;
                if (mensajeId > 0)
                    dal.ActualizarEstadoMensaje(mensajeId, "ERROR", ex.Message);
                return false;
            }
        }

        // ===============================
        // MEMBRESÍA - PAGO / FACTURA
        // ===============================
        public void EnviarMensajePagoMembresia(int clienteId, DateTime fechaVencimiento)
        {
            EnviarMensajeTemplado(clienteId, "PAGO_MEMBRESIA", new Dictionary<string, string>
            {
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["FECHA_VENCE_COMPLETA"] = fechaVencimiento.ToString(
                    "dddd, dd MMMM yyyy",
                    new CultureInfo("es-DO"))
            });
        }

        public string EnviarFacturaMembresia(
            int clienteId,
            string nombrePlan,
            decimal monto,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            string numeroRecibo,
            string metodoPago = "Efectivo",
            int? pagoId = null)
        {
            UltimoDetalleEnvio = null;

            // Cuerpo corto profesional para {{1}} (incluye link si hay PublicBaseUrl).
            string? mediaUrl = pagoId.HasValue
                ? FacturaStorage.ConstruirMediaUrlPublica(pagoId.Value)
                : null;

            if (pagoId.HasValue && pagoId.Value > 0)
            {
                try
                {
                    if (FacturaStorage.ResolverRutaFacturaExistente(pagoId.Value) == null)
                    {
                        Facturas.FacturaMembresiaPdfGenerator.GenerarDesdePago(
                            clienteId,
                            nombrePlan,
                            monto,
                            fechaVencimiento,
                            metodoPago,
                            pagoId.Value);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"FACTURA_MEMBRESIA PDF (se intenta WhatsApp igual): {ex.Message}");
                }
            }

            // Reconstruir mediaUrl despues de generar PDF (PublicBaseUrl ya validado).
            mediaUrl = pagoId.HasValue
                ? FacturaStorage.ConstruirMediaUrlPublica(pagoId.Value)
                : null;

            // Solo arrancar media local si NO usamos Supabase Storage.
            if (!SupabaseSettings.Configurado)
            {
                bool hostOk = WhatsAppMediaHostLauncher.EnsureRunning();
                if (!hostOk)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "FACTURA_MEMBRESIA: WhatsAppHost no disponible en 5088 tras autoarranque.");
                }
            }

            // Sin cuerpo de texto: el PDF va solo como adjunto (plantilla media o MediaUrl).
            string cuerpoCorto = string.Empty;

            bool enviado = EnviarMensajeTemplado(
                clienteId,
                "FACTURA_MEMBRESIA",
                new Dictionary<string, string>
                {
                    ["PLAN"] = nombrePlan,
                    ["MONTO"] = FormatearMonto(monto),
                    ["FECHA_PAGO"] = fechaPago.ToString("dd/MM/yyyy HH:mm"),
                    ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                    ["NUMERO_RECIBO"] = numeroRecibo,
                    ["METODO_PAGO"] = metodoPago
                },
                referenciaId: pagoId,
                mediaUrl: mediaUrl,
                adjuntarArchivo: true,
                cuerpoTwilioOverride: cuerpoCorto);

            // No enviar segundo mensaje de texto de respaldo: ensucia el chat del miembro.
            if (!enviado)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"FACTURA_MEMBRESIA no enviada: {UltimoDetalleEnvio}");
            }

            return UltimoDetalleEnvio
                   ?? (enviado ? "WhatsApp: PDF enviado." : "WhatsApp: PDF no entregado.");
        }

        public void EnviarMensajeVencimientoProximo(int clienteId, DateTime fechaVencimiento, int? membresiaId = null)
        {
            if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_PROXIMO", membresiaId))
                return;

            EnviarMensajeTemplado(clienteId, "VENCIMIENTO_PROXIMO", new Dictionary<string, string>
            {
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["FECHA_VENCE_COMPLETA"] = fechaVencimiento.ToString(
                    "dddd, dd MMMM yyyy",
                    new CultureInfo("es-DO"))
            }, membresiaId);
        }

        public bool EnviarMensajeVencimientoHoy(int clienteId, DateTime fechaVencimiento, string nombrePlan, int? membresiaId = null)
        {
            if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_HOY", membresiaId))
                return true;

            return EnviarMensajeTemplado(clienteId, "VENCIMIENTO_HOY", new Dictionary<string, string>
            {
                ["PLAN"] = nombrePlan,
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["FECHA_VENCE_COMPLETA"] = fechaVencimiento.ToString(
                    "dddd, dd MMMM yyyy",
                    new CultureInfo("es-DO"))
            }, membresiaId);
        }

        public bool EnviarMensajeMembresiaVencida(int clienteId, DateTime fechaVencimiento, string nombrePlan, int? membresiaId = null)
        {
            if (dal.NotificacionYaEnviada(clienteId, "MEMBRESIA_VENCIDA", membresiaId))
                return true;

            return EnviarMensajeTemplado(clienteId, "MEMBRESIA_VENCIDA", new Dictionary<string, string>
            {
                ["PLAN"] = nombrePlan,
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["MOTIVO"] = "Membresia vencida por fecha"
            }, membresiaId);
        }

        public bool EnviarMensajeDesactivacion(int clienteId, string motivo)
        {
            return EnviarMensajeTemplado(clienteId, "DESACTIVACION_MEMBRESIA", new Dictionary<string, string>
            {
                ["MOTIVO"] = string.IsNullOrWhiteSpace(motivo) ? "Sin especificar" : motivo
            });
        }

        public void EnviarMensajeFinanciamiento(
            int clienteId,
            string nombrePlan,
            decimal precioTotal,
            decimal pagoInicial,
            decimal saldoPendiente,
            DateTime fechaVencimiento)
        {
            EnviarMensajeTemplado(clienteId, "FINANCIAMIENTO", new Dictionary<string, string>
            {
                ["PLAN"] = nombrePlan,
                ["PRECIO_TOTAL"] = FormatearMonto(precioTotal),
                ["PAGO_INICIAL"] = FormatearMonto(pagoInicial),
                ["SALDO"] = FormatearMonto(saldoPendiente),
                ["FECHA_VENCIMIENTO"] = fechaVencimiento.ToString("dd/MM/yyyy")
            });
        }

        // ===============================
        // DEUDAS
        // ===============================
        public bool EnviarNotificacionDeudaCreada(
            int clienteId,
            string concepto,
            decimal monto,
            DateTime fechaVencimiento,
            int? deudaId = null)
        {
            return EnviarMensajeTemplado(clienteId, "DEUDA_CREADA", new Dictionary<string, string>
            {
                ["CONCEPTO"] = concepto,
                ["MONTO"] = FormatearMonto(monto),
                ["FECHA_VENCIMIENTO"] = fechaVencimiento.ToString("dd/MM/yyyy")
            }, deudaId);
        }

        public bool EnviarRecordatorioDeudaVenceHoy(int deudaId, bool forzar = false)
        {
            var deuda = ObtenerDeuda(deudaId);
            if (deuda == null) return false;

            int clienteId = Convert.ToInt32(deuda["ClienteId"]);
            if (!forzar && dal.NotificacionYaEnviada(clienteId, "DEUDA_VENCE_HOY", deudaId))
                return true;

            string concepto = deuda["Concepto"]?.ToString() ?? "Deuda";
            decimal saldo = Convert.ToDecimal(deuda["Saldo"]);
            DateTime fechaVencimiento = Convert.ToDateTime(deuda["FechaVencimiento"]);

            return EnviarMensajeTemplado(clienteId, "DEUDA_VENCE_HOY", new Dictionary<string, string>
            {
                ["CONCEPTO"] = concepto,
                ["SALDO"] = FormatearMonto(saldo),
                ["FECHA_VENCIMIENTO"] = fechaVencimiento.ToString("dd/MM/yyyy")
            }, deudaId);
        }

        public bool EnviarRecordatorioDeuda(int deudaId, bool forzar = false)
        {
            var deuda = ObtenerDeuda(deudaId);
            if (deuda == null) return false;

            int clienteId = Convert.ToInt32(deuda["ClienteId"]);
            if (!forzar && dal.NotificacionYaEnviada(clienteId, "RECORDATORIO_VENCIMIENTO_DEUDA", deudaId))
                return true;

            string concepto = deuda["Concepto"]?.ToString() ?? "Deuda";
            decimal saldo = Convert.ToDecimal(deuda["Saldo"]);
            DateTime fechaVencimiento = Convert.ToDateTime(deuda["FechaVencimiento"]);
            int diasRestantes = (fechaVencimiento.Date - DateTime.Today).Days;

            return EnviarMensajeTemplado(clienteId, "RECORDATORIO_VENCIMIENTO_DEUDA", new Dictionary<string, string>
            {
                ["CONCEPTO"] = concepto,
                ["SALDO"] = FormatearMonto(saldo),
                ["DIAS_RESTANTES"] = diasRestantes.ToString(),
                ["FECHA_VENCIMIENTO"] = fechaVencimiento.ToString("dd/MM/yyyy")
            }, deudaId);
        }

        public bool EnviarNotificacionDeudaVencida(int deudaId, bool forzar = false)
        {
            var deuda = ObtenerDeuda(deudaId);
            if (deuda == null) return false;

            int clienteId = Convert.ToInt32(deuda["ClienteId"]);
            if (!forzar && dal.NotificacionYaEnviada(clienteId, "DEUDA_VENCIDA", deudaId))
                return true;

            string concepto = deuda["Concepto"]?.ToString() ?? "Deuda";
            decimal saldo = Convert.ToDecimal(deuda["Saldo"]);
            DateTime fechaVencimiento = Convert.ToDateTime(deuda["FechaVencimiento"]);

            return EnviarMensajeTemplado(clienteId, "DEUDA_VENCIDA", new Dictionary<string, string>
            {
                ["CONCEPTO"] = concepto,
                ["SALDO"] = FormatearMonto(saldo),
                ["FECHA_VENCIMIENTO"] = fechaVencimiento.ToString("dd/MM/yyyy")
            }, deudaId);
        }

        public bool EnviarNotificacionPagoDeudaRecibido(int clienteId, decimal montoPago, decimal saldoRestante, int? deudaId = null)
        {
            return EnviarMensajeTemplado(clienteId, "PAGO_DEUDA_RECIBIDO", new Dictionary<string, string>
            {
                ["MONTO_PAGO"] = FormatearMonto(montoPago),
                ["SALDO"] = FormatearMonto(saldoRestante)
            }, deudaId);
        }

        public bool EnviarNotificacionDeudaPagadaCompleta(int clienteId, decimal montoTotal, int? deudaId = null)
        {
            return EnviarMensajeTemplado(clienteId, "DEUDA_PAGADA_COMPLETA", new Dictionary<string, string>
            {
                ["MONTO_TOTAL"] = FormatearMonto(montoTotal)
            }, deudaId);
        }

        // ===============================
        // AUTOMATIZACIÓN
        // ===============================
        public int EjecutarAutomatizaciones()
        {
            if (!TwilioSettings.WhatsAppHabilitado)
                return 0;

            int enviados = 0;
            enviados += ReintentarMensajesFallidos();
            enviados += VerificarRecordatoriosDeuda();
            enviados += VerificarDeudasVencenHoy();
            enviados += VerificarDeudasVencidas();
            enviados += VerificarMembresiasPorVencer();
            enviados += VerificarMembresiasVencenHoy();
            enviados += VerificarMembresiasRecienVencidas();
            return enviados;
        }

        public int ReintentarMensajesFallidos()
        {
            int reintentados = 0;
            var pendientes = dal.ObtenerMensajesParaReintento(
                TwilioSettings.MaxReintentosMensajes,
                TwilioSettings.MaxIntentosPorMensaje);

            foreach (DataRow row in pendientes.Rows)
            {
                int mensajeId = Convert.ToInt32(row["Id"]);
                string numero = row["NumeroTelefono"]?.ToString() ?? string.Empty;
                string mensaje = row["Mensaje"]?.ToString() ?? string.Empty;
                string tipo = row["Tipo"]?.ToString() ?? string.Empty;
                string? respuestaAnterior = row.Table.Columns.Contains("Respuesta")
                    ? row["Respuesta"]?.ToString()
                    : null;

                if (string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(mensaje))
                    continue;

                if (EsErrorPermanente(respuestaAnterior))
                    continue;

                string mensajeTwilio = WhatsAppContentVariableHelper.PrepararCuerpoPlantilla(mensaje, null);
                var resultado = twilioClient.Enviar(numero, mensajeTwilio, null);
                string detalleRespuesta = FormatearDetalleRespuesta(resultado);

                dal.ActualizarEstadoMensaje(
                    mensajeId,
                    resultado.Entregado ? "ENVIADO" : "ERROR",
                    detalleRespuesta);

                if (!resultado.Entregado)
                    dal.IncrementarIntentos(mensajeId);

                if (resultado.Entregado)
                    reintentados++;
            }

            return reintentados;
        }

        public int VerificarRecordatoriosDeuda()
        {
            int enviados = 0;
            int diasAntes = TwilioSettings.DiasRecordatorioDeuda;
            var deudas = deudaDAL.ObtenerDeudas();
            DateTime hoy = DateTime.Today;

            foreach (DataRow row in deudas.Rows)
            {
                if (!string.Equals(row["Estado"]?.ToString(), "ACTIVA", StringComparison.OrdinalIgnoreCase))
                    continue;

                int deudaId = Convert.ToInt32(row["Id"]);
                DateTime fechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]).Date;
                int diasHastaVencimiento = (fechaVencimiento - hoy).Days;

                if (diasHastaVencimiento == diasAntes && EnviarRecordatorioDeuda(deudaId))
                    enviados++;
            }

            return enviados;
        }

        public int VerificarDeudasVencenHoy()
        {
            int enviados = 0;
            var deudas = deudaDAL.ObtenerDeudas();
            DateTime hoy = DateTime.Today;

            foreach (DataRow row in deudas.Rows)
            {
                if (!string.Equals(row["Estado"]?.ToString(), "ACTIVA", StringComparison.OrdinalIgnoreCase))
                    continue;

                int deudaId = Convert.ToInt32(row["Id"]);
                DateTime fechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]).Date;

                if (fechaVencimiento == hoy && EnviarRecordatorioDeudaVenceHoy(deudaId))
                    enviados++;
            }

            return enviados;
        }

        public int VerificarDeudasVencidas()
        {
            int enviados = 0;
            var deudas = deudaDAL.ObtenerDeudas();
            DateTime hoy = DateTime.Today;

            foreach (DataRow row in deudas.Rows)
            {
                if (!string.Equals(row["Estado"]?.ToString(), "ACTIVA", StringComparison.OrdinalIgnoreCase))
                    continue;

                int deudaId = Convert.ToInt32(row["Id"]);
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                DateTime fechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]).Date;
                int diasHastaVencimiento = (fechaVencimiento - hoy).Days;

                if (diasHastaVencimiento >= 0)
                    continue;

                int diasVencida = Math.Abs(diasHastaVencimiento);
                bool debeNotificar = diasVencida == 1 || diasVencida % 7 == 0;

                if (!debeNotificar)
                    continue;

                if (dal.NotificacionEnviadaEnDias(clienteId, "DEUDA_VENCIDA", 6, deudaId))
                    continue;

                if (EnviarNotificacionDeudaVencida(deudaId))
                    enviados++;
            }

            return enviados;
        }

        public int VerificarMembresiasPorVencer()
        {
            int enviados = 0;
            int diasAntes = TwilioSettings.DiasRecordatorioMembresia;
            var membresias = dal.ObtenerMembresiasPorVencer(diasAntes);

            foreach (DataRow row in membresias.Rows)
            {
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                int membresiaId = Convert.ToInt32(row["MembresiaId"]);
                DateTime fechaFin = Convert.ToDateTime(row["FechaFin"]);

                if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_PROXIMO", membresiaId))
                    continue;

                EnviarMensajeVencimientoProximo(clienteId, fechaFin, membresiaId);
                enviados++;
            }

            int diasUrgente = TwilioSettings.DiasRecordatorioMembresiaUrgente;
            if (diasUrgente > 0 && diasUrgente != diasAntes)
            {
                var urgentes = dal.ObtenerMembresiasPorVencer(diasUrgente);
                foreach (DataRow row in urgentes.Rows)
                {
                    int clienteId = Convert.ToInt32(row["ClienteId"]);
                    int membresiaId = Convert.ToInt32(row["MembresiaId"]);
                    DateTime fechaFin = Convert.ToDateTime(row["FechaFin"]);

                    if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_PROXIMO", membresiaId))
                        continue;

                    EnviarMensajeVencimientoProximo(clienteId, fechaFin, membresiaId);
                    enviados++;
                }
            }

            return enviados;
        }

        public int VerificarMembresiasVencenHoy()
        {
            int enviados = 0;
            var membresias = dal.ObtenerMembresiasVencenHoy();

            foreach (DataRow row in membresias.Rows)
            {
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                int membresiaId = Convert.ToInt32(row["MembresiaId"]);
                DateTime fechaFin = Convert.ToDateTime(row["FechaFin"]);
                string plan = row["Plan"]?.ToString() ?? "Membresia";

                if (EnviarMensajeVencimientoHoy(clienteId, fechaFin, plan, membresiaId))
                    enviados++;
            }

            return enviados;
        }

        public int VerificarMembresiasRecienVencidas()
        {
            int enviados = 0;
            var membresias = dal.ObtenerMembresiasVencidasActivas();

            foreach (DataRow row in membresias.Rows)
            {
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                int membresiaId = Convert.ToInt32(row["MembresiaId"]);
                DateTime fechaFin = Convert.ToDateTime(row["FechaFin"]);
                string plan = row["Plan"]?.ToString() ?? "Membresia";

                if (EnviarMensajeMembresiaVencida(clienteId, fechaFin, plan, membresiaId))
                    enviados++;
            }

            return enviados;
        }

        // ===============================
        // CONSULTAS / ESTADO
        // ===============================
        public DataTable ObtenerMensajesPendientes() => dal.ObtenerMensajesPendientes();

        public void ActualizarEstadoMensaje(int id, string estado, string? respuesta = null) =>
            dal.ActualizarEstadoMensaje(id, estado, respuesta);

        public string? ObtenerUltimoErrorCliente(int clienteId) =>
            dal.ObtenerUltimaRespuestaError(clienteId);

        // ===============================
        // HELPERS
        // ===============================
        private DataRow? ObtenerDeuda(int deudaId)
        {
            var deudas = deudaDAL.ObtenerDeudas();
            foreach (DataRow row in deudas.Rows)
            {
                if (Convert.ToInt32(row["Id"]) == deudaId)
                    return row;
            }

            return null;
        }

        private bool TryObtenerTelefonoCliente(int clienteId, out string numeroTelefono, out string nombreCliente)
        {
            numeroTelefono = string.Empty;
            nombreCliente = "Cliente";

            var cliente = clienteDAL.ObtenerClientePorId(clienteId);
            if (cliente == null)
                return false;

            nombreCliente = cliente["Nombre"]?.ToString() ?? "Cliente";
            numeroTelefono = NormalizarTelefono(cliente["Telefono"]?.ToString());
            return !string.IsNullOrWhiteSpace(numeroTelefono);
        }

        public static string NormalizarTelefono(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            telefono = telefono.Trim().Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            if (telefono.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
                telefono = telefono.Substring(9);

            if (telefono.StartsWith("+"))
                return telefono;

            if (telefono.StartsWith("1") && telefono.Length == 11)
                return "+" + telefono;

            if (telefono.Length == 10)
                return "+1" + telefono;

            return "+" + telefono;
        }

        private static string AplicarVariables(string plantilla, Dictionary<string, string> variables)
        {
            foreach (var par in variables)
                plantilla = plantilla.Replace("{" + par.Key + "}", par.Value ?? string.Empty);

            return plantilla;
        }

        private static string FormatearMonto(decimal monto) =>
            $"RD {monto.ToString("0.00", CultureInfo.InvariantCulture)}";

        private static string FormatearDetalleRespuesta(WhatsAppEnvioResult resultado)
        {
            string detalle = resultado.Detalle ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(resultado.MessageSid))
                detalle = $"{detalle} [SID:{resultado.MessageSid}]";
            if (!string.IsNullOrWhiteSpace(resultado.StatusFinal))
                detalle = $"{detalle} (status={resultado.StatusFinal})";
            return detalle.Trim();
        }

        private static bool EsErrorPermanente(string? respuesta)
        {
            if (string.IsNullOrWhiteSpace(respuesta))
                return false;

            return respuesta.Contains("63007", StringComparison.OrdinalIgnoreCase)
                || respuesta.Contains("Telefono destino invalido", StringComparison.OrdinalIgnoreCase);
        }
    }
}
