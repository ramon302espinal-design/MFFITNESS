using System.Media;

namespace UI.Helpers
{
    /// <summary>
    /// Sonido corto al recibir mensajes WhatsApp entrantes.
    /// </summary>
    public static class ChatSoundHelper
    {
        public static void ReproducirMensajeEntrante()
        {
            try
            {
                SystemSounds.Exclamation.Play();
            }
            catch
            {
                try { SystemSounds.Beep.Play(); }
                catch { /* ignore */ }
            }
        }
    }
}
