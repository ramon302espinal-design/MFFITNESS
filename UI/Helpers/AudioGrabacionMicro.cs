using NAudio.Wave;

namespace UI.Helpers
{
    /// <summary>Graba audio del micrófono Windows en WAV 16 kHz mono (formato Whisper).</summary>
    internal sealed class AudioGrabacionMicro : IDisposable
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private string? _tempWav;
        private bool _grabando;

        public bool Grabando => _grabando;
        public event Action<int>? NivelAudio;

        public void Iniciar()
        {
            if (_grabando)
                return;

            DetenerSilencioso();

            _tempWav = Path.Combine(
                Path.GetTempPath(),
                "mffitness-voz-" + Guid.NewGuid().ToString("N") + ".wav");

            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 50
            };

            _writer = new WaveFileWriter(_tempWav, _waveIn.WaveFormat);
            _waveIn.DataAvailable += WaveIn_DataAvailable;
            _waveIn.RecordingStopped += WaveIn_RecordingStopped;

            _waveIn.StartRecording();
            _grabando = true;
        }

        /// <summary>Detiene y devuelve ruta WAV temporal (caller debe borrar).</summary>
        public string? Detener()
        {
            if (!_grabando || _waveIn == null)
                return null;

            _grabando = false;
            _waveIn.StopRecording();
            return _tempWav;
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
            _writer?.Flush();

            if (e.BytesRecorded <= 0)
                return;

            int peak = 0;
            for (int i = 0; i + 1 < e.BytesRecorded; i += 2)
            {
                short sample = (short)(e.Buffer[i] | (e.Buffer[i + 1] << 8));
                int abs = Math.Abs(sample);
                if (abs > peak)
                    peak = abs;
            }

            int nivel = Math.Clamp(peak * 100 / 32768, 0, 100);
            NivelAudio?.Invoke(nivel);
        }

        private void WaveIn_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            _writer?.Dispose();
            _writer = null;
            _waveIn?.Dispose();
            _waveIn = null;
        }

        private void DetenerSilencioso()
        {
            if (_waveIn == null)
                return;

            try
            {
                if (_grabando)
                    _waveIn.StopRecording();
            }
            catch { /* ignore */ }

            _writer?.Dispose();
            _writer = null;
            _waveIn?.Dispose();
            _waveIn = null;
            _grabando = false;
        }

        public void Dispose()
        {
            DetenerSilencioso();
            if (!string.IsNullOrWhiteSpace(_tempWav) && File.Exists(_tempWav))
            {
                try { File.Delete(_tempWav); } catch { /* ignore */ }
            }

            _tempWav = null;
        }
    }
}
