using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using BLL.Models;
using UI.DISEÑO.CHAT;

namespace UI.Helpers
{
    /// <summary>
    /// Detecta mensajes WhatsApp entrantes en toda la aplicación y muestra toast + sonido.
    /// </summary>
    public static class ChatNotificationHost
    {
        private static readonly object Sync = new();
        private static readonly ChatBLL Chat = new();
        private static System.Windows.Forms.Timer? _timer;
        private static int _ultimoIdConocido = -1;
        private static int _revisando;
        private static bool _iniciado;

        public static void Start()
        {
            lock (Sync)
            {
                if (_iniciado)
                    return;

                try
                {
                    _ultimoIdConocido = Chat.ObtenerMaxIdMensajeEntrada();
                }
                catch
                {
                    _ultimoIdConocido = 0;
                }

                _timer = new System.Windows.Forms.Timer { Interval = 2500 };
                _timer.Tick += (_, _) => RevisarNuevosMensajes();
                _timer.Start();
                _iniciado = true;
            }
        }

        /// <summary>Se dispara cuando llega un mensaje ENTRADA (clienteId, mensajeId).</summary>
        public static event Action<int, int>? MensajeEntranteRecibido;

        public static void Stop()
        {
            lock (Sync)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }

                _iniciado = false;
                _ultimoIdConocido = -1;
            }
        }

        private static async void RevisarNuevosMensajes()
        {
            if (Interlocked.CompareExchange(ref _revisando, 1, 0) != 0)
                return;

            try
            {
                int desde = _ultimoIdConocido;
                IReadOnlyList<ChatNotificacionDto> nuevos = await Task.Run(() => Chat.ListarEntradasDesdeId(desde));
                if (nuevos.Count == 0)
                    return;

                int maxId = _ultimoIdConocido;
                bool notificar = false;

                foreach (ChatNotificacionDto msg in nuevos)
                {
                    maxId = Math.Max(maxId, msg.MensajeId);
                    MensajeEntranteRecibido?.Invoke(msg.ClienteId, msg.MensajeId);

                    if (FrmChat.EstaViendoCliente(msg.ClienteId))
                        continue;

                    notificar = true;
                    FrmChatMensajeToast.Mostrar(msg);
                }

                _ultimoIdConocido = maxId;

                if (notificar)
                    ChatSoundHelper.ReproducirMensajeEntrante();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChatNotificationHost: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref _revisando, 0);
            }
        }
    }
}
