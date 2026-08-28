"""Transcribe WAV/MP3 to Spanish text with faster-whisper (stdout only)."""
import sys

def main() -> int:
    if len(sys.argv) < 2:
        print("usage: transcribe_es.py <audio> [model_size]", file=sys.stderr)
        return 2

    audio_path = sys.argv[1]
    model_size = sys.argv[2] if len(sys.argv) > 2 else "small"

    try:
        from faster_whisper import WhisperModel
    except ImportError:
        print("faster-whisper not installed", file=sys.stderr)
        return 3

    model = WhisperModel(model_size, device="cpu", compute_type="int8")
    segments, _info = model.transcribe(
        audio_path,
        language="es",
        beam_size=5,
        vad_filter=True,
        condition_on_previous_text=False,
    )
    text = " ".join(s.text.strip() for s in segments).strip()
    # Solo stdout: el helper C# lee la transcripción.
    print(text, end="")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
