using System;

namespace UI.Helpers
{
    /// <summary>
    /// Evita procesar dos lecturas del wedge en menos de N ms (rebotes / doble Enter).
    /// </summary>
    internal sealed class PosScannerIntervalGate
    {
        public const int DefaultIntervalMs = 10;

        private DateTime _lastAcceptedUtc = DateTime.MinValue;
        private readonly int _intervalMs;

        public PosScannerIntervalGate(int intervalMs = DefaultIntervalMs)
        {
            _intervalMs = intervalMs > 0 ? intervalMs : DefaultIntervalMs;
        }

        public bool TryAcceptScan()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastAcceptedUtc).TotalMilliseconds < _intervalMs)
                return false;

            _lastAcceptedUtc = now;
            return true;
        }
    }
}
