using System.Collections.Generic;

namespace BLL
{
    public sealed class WhatsAppPlantillaDefinicion
    {
        public WhatsAppPlantillaDefinicion(
            string tipo,
            string titulo,
            string evento,
            string condicion,
            string predeterminada,
            IReadOnlyList<string> variables,
            bool programada = false)
        {
            Tipo = tipo;
            Titulo = titulo;
            Evento = evento;
            Condicion = condicion;
            Predeterminada = predeterminada;
            Variables = variables;
            Programada = programada;
        }

        public string Tipo { get; }
        public string Titulo { get; }
        public string Evento { get; }
        public string Condicion { get; }
        public string Predeterminada { get; }
        public IReadOnlyList<string> Variables { get; }
        /// <summary>True solo en avisos del reloj. El POS no se apaga con esta bandera.</summary>
        public bool Programada { get; }
    }

    /// <summary>
    /// Tipos que el centro edita. El envío sigue usando MensajeAutomaticoBLL y Twilio existentes.
    /// </summary>
    public static class WhatsAppPlantillaCatalogo
    {
        public static IReadOnlyList<WhatsAppPlantillaDefinicion> Centro { get; } = new[]
        {
            new WhatsAppPlantillaDefinicion(
                "VENCIMIENTO_PROXIMO",
                "Membresía próxima a vencer",
                "FechaFin de la membresía activa",
                "Los días salen de la configuración actual. Se conservan los dos plazos. Un aviso por día y membresía.",
                "Hola {CLIENTE}! Tu membresía {PLAN} vence en {DIAS_RESTANTES} días. Fecha final: {FECHA_VENCE}",
                new[] { "CLIENTE", "PLAN", "DIAS_RESTANTES", "FECHA_VENCE", "FECHA_VENCE_COMPLETA" },
                programada: true),
            new WhatsAppPlantillaDefinicion(
                "MEMBRESIA_VENCIDA",
                "Membresía vencida",
                "Membresía vencida y aún activa",
                "Cada 3 días, a partir de las 9:00. No cambia el acceso al gimnasio.",
                "Hola {CLIENTE}! Tu membresía {PLAN} venció el {FECHA_VENCE}. {MOTIVO}\n[[DETALLE_DEUDA]]{DETALLE_DEUDA}\n[[/DETALLE_DEUDA]]",
                new[] { "CLIENTE", "PLAN", "FECHA_VENCE", "MOTIVO", "SALDO", "DETALLE_DEUDA" },
                programada: true),
            new WhatsAppPlantillaDefinicion(
                "RECORDATORIO_VENCIMIENTO_DEUDA",
                "Recordatorio de deuda",
                "Deuda activa con saldo",
                "Los días salen de la configuración actual. Se conservan los dos plazos.",
                "{DETALLE}",
                new[] { "CLIENTE", "DETALLE", "ASUNTO" },
                programada: true),
            new WhatsAppPlantillaDefinicion(
                "DEUDA_CREADA",
                "Venta a crédito",
                "Se registra una deuda o un préstamo",
                "Al crear la venta a crédito. No reenvía deudas antiguas al guardar esta pantalla.",
                "{DETALLE}",
                new[] { "CLIENTE", "DETALLE", "ASUNTO" }),
            new WhatsAppPlantillaDefinicion(
                "PAGO_MEMBRESIA",
                "Confirmación de pago",
                "Pago de membresía registrado",
                "Cada pago nuevo. Guardar la plantilla no reenvía pagos anteriores.",
                "Hola {CLIENTE}! Tu membresía ha sido activada. Fecha de vencimiento: {FECHA_VENCE}",
                new[] { "CLIENTE", "FECHA_VENCE", "FECHA_VENCE_COMPLETA", "ASUNTO" }),
            new WhatsAppPlantillaDefinicion(
                "PROGRAMACION_ACTIVADA",
                "Renovación de membresía",
                "Se activa una membresía programada",
                "Al activar la programación ya pagada. No modifica fechas ni caja.",
                "Hola {CLIENTE}! Tu membresía {PLAN} quedó activa desde {FECHA_INICIO} hasta {FECHA_VENCE}.",
                new[] { "CLIENTE", "PLAN", "FECHA_INICIO", "FECHA_VENCE", "ASUNTO" })
        };

        public static WhatsAppPlantillaDefinicion? Buscar(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return null;

            foreach (var item in Centro)
            {
                if (string.Equals(item.Tipo, tipo.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        public static IReadOnlyDictionary<string, string> DatosEjemplo { get; } =
            new Dictionary<string, string>
            {
                ["CLIENTE"] = "Ana Pérez",
                ["PLAN"] = "MENSUALIDAD",
                ["DIAS_RESTANTES"] = "3",
                ["FECHA_VENCE"] = "24/10/2026",
                ["FECHA_VENCE_COMPLETA"] = "sábado, 24 octubre 2026",
                ["FECHA_INICIO"] = "24/09/2026",
                ["MOTIVO"] = "Puedes renovar en recepción.",
                ["SALDO"] = "RD$1,500.00",
                ["DETALLE_DEUDA"] = "Financiamiento MENSUALIDAD · saldo RD$1,500.00",
                ["DETALLE"] = "Cuota 2 de 4 · vence 24/10/2026 · RD$1,500.00",
                ["ASUNTO"] = "Actualización de cuenta"
            };
    }
}
