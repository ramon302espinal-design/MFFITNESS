namespace BLL
{
    public readonly struct WhatsAppCompatibilidadResultado
    {
        public WhatsAppCompatibilidadResultado(bool puedeEnviar, string aviso)
        {
            PuedeEnviar = puedeEnviar;
            Aviso = aviso;
        }

        public bool PuedeEnviar { get; }
        public string Aviso { get; }
    }

    /// <summary>
    /// El texto guardado en MFFITNESS entra como variable de la plantilla aprobada en Meta.
    /// Guardarlo no actualiza esa plantilla.
    /// </summary>
    public static class WhatsAppPlantillaCompatibilidad
    {
        public static WhatsAppCompatibilidadResultado Evaluar(bool hayContentSidConfigurado, string? textoLocal)
        {
            if (!hayContentSidConfigurado)
            {
                return new WhatsAppCompatibilidadResultado(
                    false,
                    "No hay plantilla aprobada configurada (TwilioContentSidGenerico o TwilioContentSidAvisoUtility). " +
                    "No se envía texto libre fuera de la ventana de 24 horas.");
            }

            if (string.IsNullOrWhiteSpace(textoLocal))
            {
                return new WhatsAppCompatibilidadResultado(
                    false,
                    "El texto quedó vacío después de omitir datos sin valor. No se envía.");
            }

            if (textoLocal.Contains('{') && textoLocal.Contains('}'))
            {
                return new WhatsAppCompatibilidadResultado(
                    false,
                    "El texto todavía tiene variables sin valor. No se envía una plantilla incompleta.");
            }

            return new WhatsAppCompatibilidadResultado(
                true,
                "Este texto va dentro de la plantilla aprobada de Twilio (variable de detalle). " +
                "Guardarlo aquí no cambia la plantilla ya aprobada en WhatsApp.");
        }
    }
}
