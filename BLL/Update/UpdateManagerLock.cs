namespace BLL.Update
{
    /// <summary>
    /// Resultado de intentar adquirir el lock global de UpdateManager.
    /// </summary>
    public sealed class UpdateManagerLockResult
    {
        public bool Acquired { get; init; }
        public bool Blocked { get; init; }
        public string Message { get; init; } = string.Empty;
        public UpdateManagerLock? Lock { get; init; }

        public static UpdateManagerLockResult Success(UpdateManagerLock handle) => new()
        {
            Acquired = true,
            Blocked = false,
            Message = "Lock adquirido.",
            Lock = handle
        };

        public static UpdateManagerLockResult Failure(string message) => new()
        {
            Acquired = false,
            Blocked = true,
            Message = message,
            Lock = null
        };
    }

    /// <summary>
    /// Named Mutex global: Global\MFFITNESS_UpdateManager.
    /// Solo un UpdateManager.exe puede ejecutar side effects a la vez.
    /// Nota: el mutex de Windows es reentrante en el mismo hilo; la exclusión
    /// real se valida entre procesos/hilos distintos.
    /// </summary>
    public sealed class UpdateManagerLock : IDisposable
    {
        public const string MutexName = @"Global\MFFITNESS_UpdateManager";

        private Mutex? _mutex;
        private bool _ownsMutex;
        private bool _disposed;

        private UpdateManagerLock(Mutex mutex, bool ownsMutex)
        {
            _mutex = mutex;
            _ownsMutex = ownsMutex;
        }

        /// <summary>
        /// Intenta adquirir el mutex. Si otro proceso lo tiene, retorna Blocked sin side effects.
        /// </summary>
        public static UpdateManagerLockResult TryAcquire(TimeSpan? timeout = null)
        {
            Mutex? mutex = null;
            try
            {
                mutex = new Mutex(initiallyOwned: false, name: MutexName, createdNew: out _);

                TimeSpan wait = timeout ?? TimeSpan.Zero;
                bool acquired;
                try
                {
                    acquired = mutex.WaitOne(wait);
                }
                catch (AbandonedMutexException)
                {
                    // Proceso anterior murió sin liberar: tomamos ownership de forma segura.
                    acquired = true;
                }

                if (!acquired)
                {
                    mutex.Dispose();
                    return UpdateManagerLockResult.Failure(
                        "Otro UpdateManager está activo. Actualización bloqueada (mutex).");
                }

                return UpdateManagerLockResult.Success(new UpdateManagerLock(mutex, ownsMutex: true));
            }
            catch (Exception ex)
            {
                try { mutex?.Dispose(); } catch { /* ignore */ }
                return UpdateManagerLockResult.Failure("No se pudo adquirir el lock de UpdateManager: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_mutex != null)
            {
                try
                {
                    if (_ownsMutex)
                        _mutex.ReleaseMutex();
                }
                catch (ApplicationException)
                {
                    // No somos owners.
                }
                finally
                {
                    _mutex.Dispose();
                    _mutex = null;
                    _ownsMutex = false;
                }
            }
        }
    }
}
