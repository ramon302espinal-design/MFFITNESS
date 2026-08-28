using NAudio.CoreAudioApi;

namespace UI.Helpers
{
    /// <summary>Dispositivo de captura (micrófono) predeterminado de Windows.</summary>
    internal static class AudioEntradaWindows
    {
        public static bool TryObtenerMicrofonoPredeterminado(out string? nombre, out string? error)
        {
            nombre = null;
            error = null;
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                using MMDevice device = enumerator.GetDefaultAudioEndpoint(
                    DataFlow.Capture,
                    Role.Communications);

                nombre = device.FriendlyName;
                if (device.State != DeviceState.Active)
                {
                    error = $"El micrófono «{nombre}» no está activo ({device.State}).";
                    return false;
                }

                return !string.IsNullOrWhiteSpace(nombre);
            }
            catch (Exception ex)
            {
                error =
                    "No se detectó micrófono en Windows.\n" +
                    "Configuración → Sistema → Sonido → Entrada → elige un micrófono.\n\n" +
                    ex.Message;
                return false;
            }
        }

        /// <summary>Re-enlaza el motor SAPI al micrófono predeterminado actual.</summary>
        public static void EnlazarMicrofonoPredeterminado(System.Speech.Recognition.SpeechRecognitionEngine engine)
        {
            ArgumentNullException.ThrowIfNull(engine);
            engine.SetInputToDefaultAudioDevice();
        }
    }
}
