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

        /// <summary>Margen para que Meta reporte el rechazo tras el "sent" inicial.</summary>
        private const int SegundosVerificacionEntrega = 25;

        /// <summary>Financiamientos que se listan uno a uno en el estado de cuenta.</summary>
        private const int MaxFinanciamientosDetallados = 6;

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
                    UltimoDetalleEnvio =
                        "Cliente sin telefono valido (revise el numero en la ficha del miembro).";
                    return false;
                }

                variables["CLIENTE"] = nombreCliente;
                variables["ASUNTO"] = AsuntoPorTipo(tipoPlantilla);

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

                bool ok = resultado.Entregado;

                dal.ActualizarEstadoMensaje(
                    mensajeId,
                    ok ? "ENVIADO" : "ERROR",
                    detalleRespuesta);

                if (!ok)
                    dal.IncrementarIntentos(mensajeId);

                // Meta puede rechazar despues de "sent": confirmar el estado real mas tarde.
                ProgramarVerificacionEntrega(mensajeId, resultado.MessageSid);

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

            // El PDF puede existir en disco sin estar publicado (subida previa fallida):
            // se verifica/republica antes de enviar para que Twilio pueda descargarlo.
            if (pagoId.HasValue && pagoId.Value > 0 && SupabaseSettings.Configurado)
                mediaUrl = Facturas.FacturaSupabaseUploader.AsegurarPublicada(pagoId.Value);
            else
                mediaUrl = pagoId.HasValue
                    ? FacturaStorage.ConstruirMediaUrlPublica(pagoId.Value)
                    : null;

            // Solo archivo PDF: sin URL pública no se envía nada (ni texto de respaldo).
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                UltimoDetalleEnvio =
                    "No se pudo publicar la factura PDF (Supabase/PublicBaseUrl). WhatsApp no enviado.";
                System.Diagnostics.Debug.WriteLine($"FACTURA_MEMBRESIA: {UltimoDetalleEnvio}");
                return UltimoDetalleEnvio;
            }

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
                    ["FECHA_PAGO"] = fechaPago.ToString(FechaHoraFormats.FechaHora),
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

        public bool EnviarMensajeVencimientoProximo(
            int clienteId,
            DateTime fechaVencimiento,
            int diasRestantes,
            string nombrePlan,
            int? membresiaId = null)
        {
            // Un envio por dia y por membresia: a los 10 dias y a los 3 dias
            // (misma plantilla, distinto DIAS_RESTANTES) salen en dias distintos.
            if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_PROXIMO", membresiaId))
                return false;

            return EnviarMensajeTemplado(clienteId, "VENCIMIENTO_PROXIMO", new Dictionary<string, string>
            {
                ["PLAN"] = string.IsNullOrWhiteSpace(nombrePlan) ? "Membresia" : nombrePlan,
                ["DIAS_RESTANTES"] = diasRestantes.ToString(),
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["FECHA_VENCE_COMPLETA"] = fechaVencimiento.ToString(
                    "dddd, dd MMMM yyyy",
                    new CultureInfo("es-DO"))
            }, membresiaId);
        }

        public bool EnviarMensajeVencimientoHoy(int clienteId, DateTime fechaVencimiento, string nombrePlan, int? membresiaId = null)
        {
            if (dal.NotificacionYaEnviada(clienteId, "VENCIMIENTO_HOY", membresiaId))
                return false;

            return EnviarMensajeTemplado(clienteId, "VENCIMIENTO_HOY", new Dictionary<string, string>
            {
                ["PLAN"] = nombrePlan,
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["FECHA_VENCE_COMPLETA"] = fechaVencimiento.ToString(
                    "dddd, dd MMMM yyyy",
                    new CultureInfo("es-DO"))
            }, membresiaId);
        }

        /// <summary>
        /// Aviso diario mientras la membresia siga vencida y activa:
        /// pide saldar la deuda pendiente (o renovar si no hay financiamiento).
        /// </summary>
        public bool EnviarMensajeMembresiaVencida(int clienteId, DateTime fechaVencimiento, string nombrePlan, int? membresiaId = null)
        {
            if (dal.NotificacionYaEnviada(clienteId, "MEMBRESIA_VENCIDA", membresiaId))
                return false;

            var (motivo, saldo, detalleDeuda) = ConstruirMotivoDeudaPendiente(clienteId);

            return EnviarMensajeTemplado(clienteId, "MEMBRESIA_VENCIDA", new Dictionary<string, string>
            {
                ["PLAN"] = nombrePlan,
                ["FECHA_VENCE"] = fechaVencimiento.ToString("dd/MM/yyyy"),
                ["MOTIVO"] = motivo,
                ["SALDO"] = FormatearMonto(saldo),
                ["DETALLE_DEUDA"] = detalleDeuda
            }, membresiaId);
        }

        /// <summary>
        /// Motivo del aviso diario: si hay financiamientos pendientes, pedirle
        /// que los salde; si no, que renueve la membresia vencida.
        /// </summary>
        private (string Motivo, decimal Saldo, string Detalle) ConstruirMotivoDeudaPendiente(int clienteId)
        {
            DataTable deudas = deudaDAL.ObtenerResumenDeudasCliente(clienteId);
            if (deudas == null || deudas.Rows.Count == 0)
            {
                return (
                    "Tu membresia esta vencida. Renueva o salda el pago pendiente en recepcion para recuperar el acceso.",
                    0m,
                    "Sin financiamientos registrados.");
            }

            decimal total = 0m;
            var detalle = new System.Text.StringBuilder();
            int n = 0;

            foreach (DataRow row in deudas.Rows)
            {
                n++;
                decimal saldo = LeerDecimal(row, "Saldo");
                total += saldo;
                string concepto = (row["Concepto"]?.ToString() ?? "Financiamiento").Trim();
                DateTime vence = LeerFecha(row, "FechaVencimiento");
                detalle.AppendLine($"{n}) {concepto} - saldo {FormatearMonto(saldo)} - vence {vence:dd/MM/yyyy}.");
            }

            string motivo = n == 1
                ? $"Tu membresia esta vencida. Debes saldar tu deuda pendiente de {FormatearMonto(total)} para reactivar el acceso."
                : $"Tu membresia esta vencida. Debes saldar tus {n} deudas pendientes (total {FormatearMonto(total)}) para reactivar el acceso.";

            return (motivo, total, detalle.ToString().TrimEnd());
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

        /// <summary>
        /// Estado de cuenta completo: un solo mensaje con todos los financiamientos
        /// pendientes del miembro (membresia y producto a credito), cada uno con la
        /// fecha en que se pactó y su fecha límite.
        /// </summary>
        public bool EnviarResumenDeudasCliente(int clienteId)
        {
            UltimoDetalleEnvio = null;

            DataTable deudas = deudaDAL.ObtenerResumenDeudasCliente(clienteId);
            if (deudas == null || deudas.Rows.Count == 0)
            {
                UltimoDetalleEnvio = "El miembro no tiene financiamientos pendientes.";
                return false;
            }

            var detalle = new System.Text.StringBuilder();
            decimal total = 0m;
            DateTime? proximoVencimiento = null;
            int numero = 0;

            foreach (DataRow row in deudas.Rows)
            {
                numero++;
                decimal saldo = LeerDecimal(row, "Saldo");
                total += saldo;

                DateTime vence = LeerFecha(row, "FechaVencimiento");
                if (proximoVencimiento == null || vence < proximoVencimiento.Value)
                    proximoVencimiento = vence;

                // El cuerpo de la plantilla Twilio se recorta a 900 caracteres:
                // se listan los primeros y el resto se resume para no cortar a medias.
                if (numero <= MaxFinanciamientosDetallados)
                    detalle.AppendLine(DescribirFinanciamiento(numero, row, saldo, vence));
            }

            int restantes = numero - MaxFinanciamientosDetallados;
            if (restantes > 0)
            {
                detalle.AppendLine(restantes == 1
                    ? "Y 1 financiamiento mas pendiente."
                    : $"Y {restantes} financiamientos mas pendientes.");
            }

            return EnviarMensajeTemplado(clienteId, "RESUMEN_DEUDAS", new Dictionary<string, string>
            {
                ["DETALLE"] = detalle.ToString().TrimEnd(),
                ["TOTAL"] = FormatearMonto(total),
                ["CANTIDAD"] = numero == 1 ? "1 financiamiento" : $"{numero} financiamientos",
                ["PROXIMO_VENCIMIENTO"] = proximoVencimiento?.ToString("dd/MM/yyyy") ?? "Sin fecha"
            });
        }

        /// <summary>
        /// Una linea por financiamiento: que se financio, cuando y cuanto queda.
        /// </summary>
        private static string DescribirFinanciamiento(
            int numero,
            DataRow row,
            decimal saldo,
            DateTime vence)
        {
            string concepto = (row["Concepto"]?.ToString() ?? string.Empty).Trim();
            string plan = row.Table.Columns.Contains("Plan")
                ? (row["Plan"]?.ToString() ?? string.Empty).Trim()
                : string.Empty;

            if (string.IsNullOrWhiteSpace(concepto))
                concepto = string.IsNullOrWhiteSpace(plan) ? "Financiamiento" : $"Plan {plan}";
            else if (!string.IsNullOrWhiteSpace(plan)
                     && concepto.IndexOf(plan, StringComparison.OrdinalIgnoreCase) < 0)
                concepto = $"{concepto} (Plan {plan})";

            DateTime financiado = LeerFecha(row, "FechaCreacion");
            decimal montoTotal = LeerDecimal(row, "MontoTotal");
            decimal pagado = LeerDecimal(row, "MontoPagado");

            return $"{numero}) {concepto} - financiado el {financiado:dd/MM/yyyy} - "
                 + $"vence el {vence:dd/MM/yyyy} - total {FormatearMonto(montoTotal)}, "
                 + $"pagado {FormatearMonto(pagado)}, saldo {FormatearMonto(saldo)}.";
        }

        private static decimal LeerDecimal(DataRow row, string columna) =>
            row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToDecimal(row[columna])
                : 0m;

        private static DateTime LeerFecha(DataRow row, string columna) =>
            row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToDateTime(row[columna])
                : DateTime.Today;

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

                // La factura necesita el PDF adjunto: reenviarla como texto plano
                // dejaria al miembro un mensaje sin comprobante.
                if (string.Equals(tipo, "FACTURA_MEMBRESIA", StringComparison.OrdinalIgnoreCase))
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

            // Ambos ciclos (pago el 15 y pago a fin de mes) se cubren con FechaFin:
            // el aviso sale exactamente N dias antes de la fecha real de vencimiento.
            foreach (int diasAntes in DiasRecordatorioMembresiaActivos())
            {
                var membresias = dal.ObtenerMembresiasPorVencer(diasAntes);
                foreach (DataRow row in membresias.Rows)
                {
                    int clienteId = Convert.ToInt32(row["ClienteId"]);
                    int membresiaId = Convert.ToInt32(row["MembresiaId"]);
                    DateTime fechaFin = Convert.ToDateTime(row["FechaFin"]);
                    string plan = row["Plan"]?.ToString() ?? "Membresia";

                    if (EnviarMensajeVencimientoProximo(clienteId, fechaFin, diasAntes, plan, membresiaId))
                        enviados++;
                }
            }

            return enviados;
        }

        /// <summary>Dias configurados para el aviso previo (p. ej. 10 y 3), sin duplicados.</summary>
        private static IEnumerable<int> DiasRecordatorioMembresiaActivos()
        {
            var vistos = new HashSet<int>();
            foreach (int dias in new[]
            {
                TwilioSettings.DiasRecordatorioMembresia,
                TwilioSettings.DiasRecordatorioMembresiaUrgente
            })
            {
                if (dias > 0 && vistos.Add(dias))
                    yield return dias;
            }
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

        /// <summary>
        /// Cada dia, mientras la membresia siga vencida y sin salida registrada,
        /// se reenvia el aviso pidiendo saldar la deuda / renovar.
        /// </summary>
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
            return EsTelefonoValido(numeroTelefono);
        }

        /// <summary>
        /// Un numero incompleto llega a Twilio y vuelve como error 63024 tras gastar el envio.
        /// E.164 admite de 8 a 15 digitos; un movil real nunca baja de 10.
        /// </summary>
        public static bool EsTelefonoValido(string? numeroE164)
        {
            if (string.IsNullOrWhiteSpace(numeroE164) || !numeroE164.StartsWith('+'))
                return false;

            string digitos = numeroE164[1..];
            foreach (char c in digitos)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return digitos.Length is >= 10 and <= 15;
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

        /// <summary>
        /// Reconsulta el estado real en Twilio y corrige el registro.
        /// Sin esto, un rechazo posterior de Meta (63016 / 63049 / 63019)
        /// quedaba guardado como ENVIADO aunque el cliente nunca recibio nada.
        /// </summary>
        private void ProgramarVerificacionEntrega(int mensajeId, string? messageSid)
        {
            if (mensajeId <= 0 || string.IsNullOrWhiteSpace(messageSid))
                return;

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await System.Threading.Tasks.Task
                        .Delay(TimeSpan.FromSeconds(SegundosVerificacionEntrega))
                        .ConfigureAwait(false);

                    var estado = twilioClient.ConsultarEstado(messageSid!);
                    if (!estado.Consultado)
                        return;

                    if (!estado.Fallido && !estado.Entregado)
                        return;

                    dal.ActualizarEstadoMensaje(
                        mensajeId,
                        estado.Fallido ? "ERROR" : "ENVIADO",
                        $"{estado.Detalle} [SID:{messageSid}]");

                    if (estado.Fallido)
                    {
                        dal.IncrementarIntentos(mensajeId);
                        System.Diagnostics.Debug.WriteLine(
                            $"WhatsApp NO entregado (mensaje {mensajeId}): {estado.Detalle}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Verificacion de entrega WhatsApp fallo: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Asunto fijo para la plantilla UTILITY de avisos ({{2}}).
        /// </summary>
        private static string AsuntoPorTipo(string tipoPlantilla) => tipoPlantilla switch
        {
            "PAGO_MEMBRESIA" => "Pago de membresia registrado",
            "FACTURA_MEMBRESIA" => "Comprobante de pago",
            "VENCIMIENTO_PROXIMO" => "Membresia proxima a vencer",
            "VENCIMIENTO_HOY" => "Membresia vence hoy",
            "MEMBRESIA_VENCIDA" => "Membresia vencida",
            "DESACTIVACION_MEMBRESIA" => "Membresia desactivada",
            "FINANCIAMIENTO" => "Financiamiento registrado",
            "DEUDA_CREADA" => "Deuda registrada",
            "DEUDA_VENCE_HOY" => "Deuda vence hoy",
            "RECORDATORIO_VENCIMIENTO_DEUDA" => "Recordatorio de pago",
            "DEUDA_VENCIDA" => "Deuda vencida",
            "PAGO_DEUDA_RECIBIDO" => "Pago recibido",
            "DEUDA_PAGADA_COMPLETA" => "Deuda saldada",
            "RESUMEN_DEUDAS" => "Estado de cuenta de financiamientos",
            "PRUEBA_SISTEMA" => "Mensaje de prueba",
            _ => "Actualizacion de cuenta"
        };

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

            // 63024: numero de destino invalido. 63049: Meta bloquea la plantilla marketing.
            // Reintentarlos igual solo repite el mismo rechazo y gasta mensajes.
            return respuesta.Contains("63007", StringComparison.OrdinalIgnoreCase)
                || respuesta.Contains("63024", StringComparison.OrdinalIgnoreCase)
                || respuesta.Contains("63049", StringComparison.OrdinalIgnoreCase)
                || respuesta.Contains("Telefono destino invalido", StringComparison.OrdinalIgnoreCase);
        }
    }
}
