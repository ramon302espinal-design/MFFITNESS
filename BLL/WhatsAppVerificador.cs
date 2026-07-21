using CORE;
using DL;
using System;
using System.Data;
using System.Text;

namespace BLL
{
    public class WhatsAppVerificador
    {
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly MensajeAutomaticoDAL dal = new MensajeAutomaticoDAL();
        private readonly ClienteDAL clienteDAL = new ClienteDAL();

        public string VerificarYEnviarPrueba(int clienteId = 1)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DIAGNOSTICO WHATSAPP MFFITNESS ===");
            sb.AppendLine();

            string? advertencia = TwilioSettings.ObtenerAdvertenciaConfiguracion();
            if (!string.IsNullOrWhiteSpace(advertencia))
                sb.AppendLine($"AVISO CRITICO: {advertencia}");

            sb.AppendLine($"Modo produccion: {TwilioSettings.ModoProduccion}");
            sb.AppendLine($"Numero origen: {TwilioSettings.PhoneNumber}");
            sb.AppendLine($"ContentSid generico: {(string.IsNullOrWhiteSpace(TwilioSettings.ContentSidGenerico) ? "(NO CONFIGURADO)" : TwilioSettings.ContentSidGenerico)}");
            sb.AppendLine($"Exigir entrega confirmada: {TwilioSettings.ExigirEntregaConfirmada}");
            sb.AppendLine();

            var cliente = clienteDAL.ObtenerClientePorId(clienteId);
            if (cliente == null)
            {
                sb.AppendLine($"ERROR: Cliente {clienteId} no encontrado.");
                return sb.ToString();
            }

            string nombre = cliente["Nombre"]?.ToString() ?? "Cliente";
            string telefono = MensajeAutomaticoBLL.NormalizarTelefono(cliente["Telefono"]?.ToString());

            sb.AppendLine($"Cliente: {nombre} (ID {clienteId})");
            sb.AppendLine($"Telefono normalizado: {telefono}");
            sb.AppendLine($"Twilio habilitado: {TwilioSettings.WhatsAppHabilitado}");
            sb.AppendLine($"Credenciales OK: {TwilioSettings.CredencialesConfiguradas}");
            sb.AppendLine();

            if (string.IsNullOrWhiteSpace(TwilioSettings.ContentSidGenerico))
            {
                sb.AppendLine("PASO REQUERIDO ANTES DE PRODUCCION:");
                sb.AppendLine("1. Twilio Console -> Messaging -> Content Template Builder");
                sb.AppendLine("2. Crear plantilla WhatsApp en espanol con cuerpo: {{1}}");
                sb.AppendLine("3. Enviar a aprobacion Meta/WhatsApp");
                sb.AppendLine("4. Copiar ContentSid (HX...) a TwilioContentSidGenerico en App.config");
                sb.AppendLine("5. Recompilar y probar de nuevo");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("Plantilla configurada. Numero verificado por Meta: reintente envio de prueba.");
                sb.AppendLine("Si falla con 63016, en Twilio verifique:");
                sb.AppendLine("- WhatsApp approval status = Approved");
                sb.AppendLine("- Channel eligibility: 'WhatsApp business initiated' con check verde");
                sb.AppendLine();
            }

            sb.AppendLine("Enviando mensaje de prueba directo (texto minimo)...");
            var twilio = new WhatsAppTwilioClient();
            var pruebaDirecta = twilio.Enviar(telefono, "Prueba MFFITNESS. Su membresia esta al dia.", null);
            sb.AppendLine(pruebaDirecta.Entregado
                ? "Prueba directa: ENTREGADO."
                : $"Prueba directa: FALLO. {pruebaDirecta.Detalle}");
            sb.AppendLine();

            sb.AppendLine("Enviando mensaje de prueba via plantilla BD...");
            bool enviado = mensajeBLL.EnviarMensajeTemplado(clienteId, "PRUEBA_SISTEMA", new System.Collections.Generic.Dictionary<string, string>
            {
                ["FECHA"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });

            sb.AppendLine(enviado
                ? "Resultado plantilla BD: ENTREGADO (WhatsApp confirmo recepcion)."
                : "Resultado plantilla BD: NO ENTREGADO.");

            if (!enviado)
            {
                string? ultimoError = mensajeBLL.ObtenerUltimoErrorCliente(clienteId);
                if (!string.IsNullOrWhiteSpace(ultimoError))
                    sb.AppendLine($"Detalle: {ultimoError}");
            }

            sb.AppendLine();
            sb.AppendLine("Ultimos 5 mensajes en BD:");
            var ultimos = dal.ObtenerUltimosMensajes(clienteId, 5);
            foreach (DataRow row in ultimos.Rows)
            {
                sb.AppendLine($"- [{row["Estado"]}] {row["Tipo"]} ({row["FechaEnvio"]})");
                string respuesta = row["Respuesta"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(respuesta))
                    sb.AppendLine($"  {Truncar(respuesta, 400)}");
            }

            return sb.ToString();
        }

        private static string Truncar(string texto, int max)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= max)
                return texto;

            return texto.Substring(0, max) + "...";
        }
    }
}
