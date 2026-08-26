namespace CORE.Update
{
    /// <summary>
    /// Señal de apagado forzado para OTA. UpdateManager la dispara; la UI sale completo.
    /// Evita el proceso zombie (FrmLogin oculto tras cerrar FrmPresentacion).
    /// </summary>
    public static class UpdateExitSignal
    {
        public const string EventName = @"Local\MFFITNESS_POS_EXIT_FOR_UPDATE";

        /// <summary>Marca que la UI debe permitir salida inmediata (sin validar caja).</summary>
        public static volatile bool ForceExitRequested;
    }
}
