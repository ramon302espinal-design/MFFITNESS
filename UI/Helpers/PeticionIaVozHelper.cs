using System.Globalization;
using System.Speech.Recognition;
using System.Text;

namespace UI.Helpers
{
    /// <summary>
    /// Voz en español (prioridad es-DO / acento dominicano): activación manual, una frase por pulsación.
    /// Usa Whisper local si está instalado; si no, Windows SAPI (es-MX / es-ES).
    /// </summary>
    internal sealed class PeticionIaVozHelper : IDisposable
    {
        private const string GramaticaFrases = "FotoProductoFrases";
        private const string GramaticaOraciones = "FotoProductoOraciones";
        private const string GramaticaDominicana = "FotoProductoDominicano";

        private SpeechRecognitionEngine? _engine;
        private AudioGrabacionMicro? _grabacion;
        private CancellationTokenSource? _whisperCts;
        private bool _escuchando;
        private bool _disposed;
        private bool _usaWhisper;

        public event Action<string>? TextoReconocido;
        public event Action<string>? TextoParcial;
        public event Action<int>? NivelAudio;
        public event Action<bool>? EscuchandoCambiado;
        public event Action<string>? Aviso;

        public bool Preparado => _usaWhisper || _engine != null;
        public bool Escuchando => _escuchando;
        public bool UsaWhisper => _usaWhisper;
        public string? NombreMicrofono { get; private set; }
        public string? CulturaReconocimiento { get; private set; }
        public string? MotorDescripcion { get; private set; }

        /// <summary>Prepara motor. NO escucha hasta <see cref="IniciarEscucha"/>.</summary>
        public bool TryPreparar(out string? error)
        {
            error = null;
            if (Preparado)
                return true;

            if (!AudioEntradaWindows.TryObtenerMicrofonoPredeterminado(out string? mic, out string? micError))
            {
                error = micError;
                return false;
            }

            NombreMicrofono = mic;

            // Whisper entiende mejor acentos dominicanos que SAPI (Windows no tiene pack es-DO).
            if (PeticionIaWhisperHelper.IsAvailable())
            {
                _usaWhisper = true;
                CulturaReconocimiento = "Español (Whisper · es-DO)";
                MotorDescripcion =
                    "Whisper local — optimizado para español dominicano y latino";
                return true;
            }

            try
            {
                RecognizerInfo? info = ResolverReconocedorEspanol(out bool esFallback);
                if (info == null)
                {
                    error =
                        "Instala reconocimiento de voz en ESPAÑOL en Windows:\n" +
                        "Configuración → Hora e idioma → Voz → Reconocimiento de voz → Español (México).\n\n" +
                        "Para máxima precisión con acento dominicano ejecuta:\n" +
                        "Tools\\Whisper\\Install-Whisper.ps1";
                    return false;
                }

                CulturaReconocimiento = info.Culture.DisplayName;
                MotorDescripcion = esFallback
                    ? $"Windows SAPI · {info.Culture.DisplayName} (Windows no tiene pack es-DO; usa es-MX)"
                    : $"Windows SAPI · {info.Culture.DisplayName}";

                _engine = new SpeechRecognitionEngine(info);
                AudioEntradaWindows.EnlazarMicrofonoPredeterminado(_engine);

                _engine.BabbleTimeout = TimeSpan.FromSeconds(0);
                _engine.InitialSilenceTimeout = TimeSpan.FromSeconds(8);
                _engine.EndSilenceTimeout = TimeSpan.FromSeconds(0.9);
                _engine.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(1.4);

                _engine.LoadGrammar(CrearGramaticaFrases(info.Culture));
                _engine.LoadGrammar(CrearGramaticaDominicana(info.Culture));
                _engine.LoadGrammar(CrearGramaticaOraciones(info.Culture));
                _engine.LoadGrammar(new DictationGrammar());

                _engine.SpeechRecognized += Engine_SpeechRecognized;
                _engine.SpeechHypothesized += Engine_SpeechHypothesized;
                _engine.AudioLevelUpdated += Engine_AudioLevelUpdated;
                _engine.RecognizeCompleted += Engine_RecognizeCompleted;

                return true;
            }
            catch (Exception ex)
            {
                error = ex.GetBaseException().Message;
                LiberarEngine();
                return false;
            }
        }

        /// <summary>Una frase por pulsación de micrófono.</summary>
        public void IniciarEscucha()
        {
            if (!Preparado || _disposed || _escuchando)
                return;

            if (_usaWhisper)
            {
                IniciarEscuchaWhisper();
                return;
            }

            if (_engine == null)
                return;

            try
            {
                AudioEntradaWindows.EnlazarMicrofonoPredeterminado(_engine);
                _engine.RecognizeAsync(RecognizeMode.Single);
                _escuchando = true;
                EscuchandoCambiado?.Invoke(true);
            }
            catch (Exception ex)
            {
                Aviso?.Invoke("No se pudo activar el micrófono.\n" + ex.Message);
            }
        }

        private void IniciarEscuchaWhisper()
        {
            _grabacion?.Dispose();
            _grabacion = new AudioGrabacionMicro();
            _grabacion.NivelAudio += nivel => NivelAudio?.Invoke(nivel);

            try
            {
                _grabacion.Iniciar();
                _escuchando = true;
                EscuchandoCambiado?.Invoke(true);
                TextoParcial?.Invoke("(habla… pulsa ■ cuando termines)");
            }
            catch (Exception ex)
            {
                Aviso?.Invoke("No se pudo grabar el micrófono.\n" + ex.Message);
                _grabacion.Dispose();
                _grabacion = null;
            }
        }

        public void DetenerEscucha()
        {
            if (!_escuchando)
                return;

            if (_usaWhisper)
            {
                DetenerEscuchaWhisper();
                return;
            }

            if (_engine == null)
                return;

            try
            {
                _engine.RecognizeAsyncCancel();
            }
            catch
            {
                try { _engine.RecognizeAsyncStop(); } catch { /* ignore */ }
            }

            _escuchando = false;
            EscuchandoCambiado?.Invoke(false);
        }

        private void DetenerEscuchaWhisper()
        {
            if (_grabacion == null)
            {
                _escuchando = false;
                EscuchandoCambiado?.Invoke(false);
                return;
            }

            string? wav = _grabacion.Detener();
            _escuchando = false;
            EscuchandoCambiado?.Invoke(false);

            if (string.IsNullOrWhiteSpace(wav) || !File.Exists(wav))
            {
                Aviso?.Invoke("No se grabó audio. Pulsa 🎤 y habla de nuevo.");
                _grabacion.Dispose();
                _grabacion = null;
                return;
            }

            _whisperCts?.Cancel();
            _whisperCts?.Dispose();
            _whisperCts = new CancellationTokenSource();
            CancellationToken ct = _whisperCts.Token;

            TextoParcial?.Invoke("(transcribiendo…)");
            _ = Task.Run(async () =>
            {
                try
                {
                    string? raw = await PeticionIaWhisperHelper.TranscribirAsync(wav, ct)
                        .ConfigureAwait(false);
                    try { File.Delete(wav); } catch { /* ignore */ }

                    if (ct.IsCancellationRequested)
                        return;

                    string text = NormalizarTextoEspanol(raw);
                    if (text.Length == 0)
                    {
                        Aviso?.Invoke("No se entendió. Habla claro y pulsa ■ al terminar.");
                        return;
                    }

                    TextoReconocido?.Invoke(text);
                }
                catch (Exception ex)
                {
                    if (!ct.IsCancellationRequested)
                        Aviso?.Invoke("Whisper: " + ex.Message);
                }
                finally
                {
                    _grabacion?.Dispose();
                    _grabacion = null;
                }
            }, ct);
        }

        public void AlternarEscucha()
        {
            if (_escuchando)
                DetenerEscucha();
            else
                IniciarEscucha();
        }

        private void Engine_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            string text = NormalizarTextoEspanol(e.Result?.Text);
            if (text.Length == 0)
                return;

            string? grammar = e.Result?.Grammar?.Name;
            float conf = e.Result?.Confidence ?? 0f;

            bool esDominio = grammar is GramaticaFrases or GramaticaOraciones or GramaticaDominicana;
            if (!esDominio && conf < 0.55f)
                return;
            if (esDominio && conf < 0.32f)
                return;

            TextoReconocido?.Invoke(text);
            DetenerEscucha();
        }

        private void Engine_SpeechHypothesized(object? sender, SpeechHypothesizedEventArgs e)
        {
            string? parcial = NormalizarTextoEspanol(e.Result?.Text);
            if (!string.IsNullOrWhiteSpace(parcial))
                TextoParcial?.Invoke(parcial!);
        }

        private void Engine_AudioLevelUpdated(object? sender, AudioLevelUpdatedEventArgs e)
            => NivelAudio?.Invoke(e.AudioLevel);

        private void Engine_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            _escuchando = false;
            EscuchandoCambiado?.Invoke(false);

            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                if (e.Error is InvalidOperationException)
                    Aviso?.Invoke("No se oyó nada claro. Pulsa 🎤 y habla de nuevo.");
                else
                    Aviso?.Invoke("Voz: " + e.Error.Message);
            }
        }

        /// <summary>Normaliza y mapea expresiones dominicanas a comandos estándar.</summary>
        internal static string NormalizarTextoEspanol(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string t = raw.Trim().ToLowerInvariant();
            var sb = new StringBuilder(t.Length);
            foreach (char c in t)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0 && sb[^1] != ' ')
                        sb.Append(' ');
                }
                else
                    sb.Append(c);
            }

            t = sb.ToString().Trim();
            t = AplicarSinonimosDominicanos(t);
            return t;
        }

        private static string AplicarSinonimosDominicanos(string t)
        {
            // Frases completas (orden: más específicas primero).
            (string buscar, string reemplazo)[] frases =
            {
                ("quítale esa vaina de atrás", "quítale el fondo"),
                ("quita esa vaina de atrás", "quítale el fondo"),
                ("quítale lo de atrás", "quítale el fondo"),
                ("quita lo de atrás", "quítale el fondo"),
                ("quítale lo de atras", "quítale el fondo"),
                ("quita lo de atras", "quítale el fondo"),
                ("tírale el fondo", "quítale el fondo"),
                ("tirale el fondo", "quítale el fondo"),
                ("sácale el fondo", "quítale el fondo"),
                ("sacale el fondo", "quítale el fondo"),
                ("quita el fondo de atrás", "quítale el fondo"),
                ("sin lo de atrás", "sin fondo"),
                ("sin lo de atras", "sin fondo"),
                ("ponla finita", "ponla nítida"),
                ("ponla fina", "ponla nítida"),
                ("dale brillo", "más brillo"),
                ("subele el brillo", "súbele el brillo"),
                ("subele brillo", "súbele el brillo"),
                ("hazla más clara", "más claridad"),
                ("hazla mas clara", "más claridad"),
                ("arregla esa foto", "arregla la foto"),
                ("mejora esa foto", "mejora la foto"),
                ("endereza eso", "endereza la foto"),
                ("recorta eso", "recorta bordes"),
                ("blanquea el fondo", "fondo blanco"),
                ("blanqueala", "fondo blanco"),
                ("quita el ruido", "reduce ruido"),
                ("quita ruido", "reduce ruido"),
            };

            foreach ((string buscar, string reemplazo) in frases)
            {
                if (t.Contains(buscar, StringComparison.Ordinal))
                    t = t.Replace(buscar, reemplazo, StringComparison.Ordinal);
            }

            t = t
                .Replace("quita le", "quítale", StringComparison.Ordinal)
                .Replace("quitale", "quítale", StringComparison.Ordinal)
                .Replace("pon la", "ponla", StringComparison.Ordinal)
                .Replace("ajusta la", "ajústala", StringComparison.Ordinal)
                .Replace("ajustala", "ajústala", StringComparison.Ordinal)
                .Replace("sube le", "súbele", StringComparison.Ordinal)
                .Replace("subele", "súbele", StringComparison.Ordinal)
                .Replace("mas nitida", "más nítida", StringComparison.Ordinal)
                .Replace("mas contraste", "más contraste", StringComparison.Ordinal)
                .Replace("mas brillo", "más brillo", StringComparison.Ordinal)
                .Replace("mas claridad", "más claridad", StringComparison.Ordinal);

            return t;
        }

        private static RecognizerInfo? ResolverReconocedorEspanol(out bool esFallback)
        {
            esFallback = false;
            var instalados = SpeechRecognitionEngine.InstalledRecognizers();
            if (instalados == null || instalados.Count == 0)
                return null;

            // App es-DO: priorizar es-DO si algún día Windows lo ofrece; es-MX es el mejor sustituto caribeño.
            string[] preferidos =
            {
                "es-DO",
                "es-MX",
                "es-419",
                "es-US",
                "es-ES",
                "es"
            };

            foreach (string cultura in preferidos)
            {
                RecognizerInfo? hit = instalados.FirstOrDefault(r =>
                    string.Equals(r.Culture.Name, cultura, StringComparison.OrdinalIgnoreCase));
                if (hit == null)
                    continue;

                esFallback = !string.Equals(cultura, "es-DO", StringComparison.OrdinalIgnoreCase);
                return hit;
            }

            return instalados.FirstOrDefault(r =>
                r.Culture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase));
        }

        private static Grammar CrearGramaticaFrases(CultureInfo cultura)
        {
            string[] frases =
            {
                "ponla nítida", "ponla nitida", "más nítida", "mas nitida",
                "ponla fina", "ponla finita",
                "mejora la calidad", "mejorar calidad", "mejora la foto", "arregla la foto",
                "arregla esa foto", "mejora esa foto",
                "quítale el fondo", "quitale el fondo", "quitar el fondo", "elimina el fondo",
                "quítale lo de atrás", "quita lo de atrás", "tírale el fondo", "tirale el fondo",
                "sin fondo", "fondo blanco", "background blanco", "blanquea el fondo",
                "ajústala al lienzo", "ajustala al lienzo", "ajustar al lienzo",
                "endereza la foto", "enderezar foto", "endereza eso", "recorta bordes", "recorta eso",
                "más contraste", "mas contraste", "más brillo", "mas brillo",
                "súbele el brillo", "subele el brillo", "dale brillo", "más claridad", "mas claridad",
                "reduce ruido", "quita ruido", "sin cambiar el producto",
                "mejora nitidez", "ponla vertical", "gira la foto", "hazla más clara"
            };

            var gb = new GrammarBuilder { Culture = cultura };
            gb.Append(new Choices(frases));
            return new Grammar(gb) { Name = GramaticaFrases };
        }

        private static Grammar CrearGramaticaDominicana(CultureInfo cultura)
        {
            string[] frases =
            {
                "quítale esa vaina de atrás", "quita esa vaina de atrás",
                "quítale lo de atrás", "quita lo de atrás",
                "sácale el fondo", "sacale el fondo",
                "sin lo de atrás", "sin lo de atras",
                "ponla finita", "ponla fina",
                "dale brillo", "subele el brillo",
                "arregla esa foto", "mejora esa foto",
                "endereza eso", "recorta eso",
                "blanquéala", "blanqueala", "blanquea el fondo",
                "por favor quítale el fondo", "por favor ponla nítida",
                "déjala nítida", "dejala nitida"
            };

            var gb = new GrammarBuilder { Culture = cultura };
            gb.Append(new Choices(frases));
            return new Grammar(gb) { Name = GramaticaDominicana };
        }

        private static Grammar CrearGramaticaOraciones(CultureInfo cultura)
        {
            var gb = new GrammarBuilder { Culture = cultura };
            var opcional = new GrammarBuilder();
            opcional.Append(new Choices("por favor", "ahora", ""));

            gb.Append(opcional);
            gb.Append(new Choices(
                "ponla", "pon la", "deja", "haz", "quítale", "quitale", "elimina", "sácale", "sacale",
                "mejora", "arregla", "ajusta", "ajústala", "endereza", "recorta", "tírale", "tirale"));
            gb.Append(new Choices(
                "nítida", "nitida", "finita", "fina", "más nítida", "mas nitida",
                "el fondo", "fondo blanco", "lo de atrás", "lo de atras", "la calidad", "la foto",
                "al lienzo", "el brillo", "el contraste", "los bordes", "esa vaina de atrás"));
            return new Grammar(gb) { Name = GramaticaOraciones };
        }

        private void LiberarEngine()
        {
            _whisperCts?.Cancel();
            _whisperCts?.Dispose();
            _whisperCts = null;

            _grabacion?.Dispose();
            _grabacion = null;

            if (_engine == null)
                return;

            try
            {
                _engine.SpeechRecognized -= Engine_SpeechRecognized;
                _engine.SpeechHypothesized -= Engine_SpeechHypothesized;
                _engine.AudioLevelUpdated -= Engine_AudioLevelUpdated;
                _engine.RecognizeCompleted -= Engine_RecognizeCompleted;
                if (_escuchando)
                {
                    try { _engine.RecognizeAsyncCancel(); } catch { /* ignore */ }
                }

                _engine.UnloadAllGrammars();
                _engine.Dispose();
            }
            catch
            {
                // ignore
            }

            _engine = null;
            _escuchando = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            LiberarEngine();
        }
    }
}
