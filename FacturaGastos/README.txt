Carpeta de facturas automaticas — PRODUCCION (BD: MF CYBER DB).

Perfil: UI (Production)  →  MFFITNESS_ENVIRONMENT=Production
Carpeta: FacturaGastos (esta)
Development usa FacturaGastosDev (no mezclar).

Requisitos:
1) App abierta (Presentacion) — la vigilancia vive a nivel app
2) Caja ABIERTA en BD
3) Ollama con modelos (qwen2.5vl:7b, etc.)

Suelta JPG/PNG/BMP/WEBP o PDF en esta raiz (no en subcarpetas).
Exito  -> Procesadas\
Error   -> Errores\
Log     -> _auto.log
Hashes  -> _hashes_ok.txt (anti-duplicado)
