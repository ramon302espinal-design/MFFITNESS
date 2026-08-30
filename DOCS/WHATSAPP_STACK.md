# WhatsApp Stack — desarrollo y producción

Kestrel (`WhatsAppHost`, puerto **5088**) recibe el webhook inbound de Twilio y sirve PDFs de chat si no hay Supabase. Twilio exige una **URL HTTPS pública**; el túnel la expone hacia `127.0.0.1:5088`.

## Arquitectura

```
Twilio WhatsApp  --HTTPS POST-->  [Ngrok / Cloudflared / proxy]  -->  :5088  WhatsAppHost (Kestrel)
                                                                               |
                                                                               v
                                                                         SQL Server (ChatMensajes)
UI (FrmChat)  -------------------- poll / envío manual --------------------^
```

## Configuración (prioridad)

| Orden | Origen |
|-------|--------|
| 1 | Variable `WHATSAPP_PUBLIC_BASE_URL` |
| 2 | `%LocalAppData%\MFFITNESS\whatsapp.stack.config` |
| 3 | `UI\App.config` → `WhatsAppPublicBaseUrl` |

Plantilla: `whatsapp.stack.config.example` (copiar a LocalAppData).

Credenciales Twilio: `%LocalAppData%\MFFITNESS\twilio.secrets.config` (ver `DOCS/TWILIO_SETUP.md`).

---

## Desarrollo (Visual Studio)

### Requisitos

- `ngrok` en PATH (`ngrok config add-authtoken TU_TOKEN`)
- Opcional: dominio ngrok reservado en `whatsapp.stack.config` → `NgrokDomain`

### Arranque rápido

```powershell
cd Tools\WhatsAppHost
powershell -ExecutionPolicy Bypass -File .\Start-WhatsAppStack.ps1 -Profile Dev
```

Qué hace:

1. Compila `WhatsAppHost` (Debug) si hace falta
2. Arranca Kestrel `--media-only` en `:5088`
3. Arranca ngrok (efímero o dominio fijo)
4. Escribe la URL HTTPS en `whatsapp.stack.config` y `UI\App.config`
5. Imprime la URL para pegar en Twilio Console

Luego abre la UI con perfil **Development** y entra al chat. La barra ámbar desaparece cuando la URL está configurada.

### Verificar

```powershell
.\Test-WhatsAppStack.ps1
```

### Automatización 24/7 en dev (opcional)

```powershell
# PowerShell como Administrador
.\Install-WhatsAppStack.ps1 -Environment Development
```

Tareas al **inicio de sesión** (no al boot), útil en la PC de desarrollo.

---

## Producción (PC del gym)

### Opción A — Ngrok dominio reservado (recomendado si ya usan ngrok)

1. Copiar plantilla:
   ```
   %LocalAppData%\MFFITNESS\whatsapp.stack.config
   ```
2. Editar:
   ```xml
   <add key="WhatsAppPublicBaseUrl" value="https://TU-DOMINIO.ngrok-free.app" />
   <add key="NgrokDomain" value="TU-DOMINIO.ngrok-free.app" />
   <add key="TunnelProvider" value="Ngrok" />
   ```
3. Instalar (Administrador):
   ```powershell
   cd C:\...\MFFITNESS\artifacts\pos\WhatsAppHost
   powershell -ExecutionPolicy Bypass -File .\Install-WhatsAppStack.ps1 -Environment Production
   ```
4. Twilio Console → WhatsApp Sender → **When a message comes in** (POST):
   ```
   https://TU-DOMINIO.ngrok-free.app/webhook/twilio/whatsapp
   ```

### Opción B — Cloudflare Tunnel (URL estable, sin ngrok)

1. Cloudflare Zero Trust → Networks → Tunnels → crear túnel → copiar **token**
2. En `whatsapp.stack.config`:
   ```xml
   <add key="WhatsAppPublicBaseUrl" value="https://whatsapp.tudominio.com" />
   <add key="TunnelProvider" value="Cloudflared" />
   <add key="CloudflaredToken" value="eyJ..." />
   ```
3. Mapear el hostname público al servicio `http://127.0.0.1:5088`
4. `Install-WhatsAppStack.ps1 -Environment Production`

### Opción C — Proxy propio (IIS / nginx)

```xml
<add key="WhatsAppPublicBaseUrl" value="https://whatsapp.tudominio.com" />
<add key="TunnelProvider" value="None" />
```

El reverse proxy debe reenviar POST a `http://127.0.0.1:5088/webhook/twilio/whatsapp`.

Solo instalar el host (sin tarea de túnel):

```powershell
.\Install-WhatsAppStack.ps1 -Environment Production -SkipTunnel
```

---

## Tareas Windows creadas

| Tarea | Función |
|-------|---------|
| `MFFITNESS-WhatsAppHost` | Loop automatización + Kestrel |
| `MFFITNESS-WhatsAppTunnel` | ngrok o cloudflared (si aplica) |

Desinstalar:

```powershell
powershell -ExecutionPolicy Bypass -File .\Uninstall-WhatsAppStack.ps1
```

---

## Publicación con el POS

`Scripts\Publish-Pos.ps1` incluye `artifacts\pos\WhatsAppHost\` con exe y scripts.

Tras `Deploy-Pos.ps1`, instalar el stack desde la carpeta desplegada.

---

## UI vs WhatsAppHost

| Componente | Rol |
|------------|-----|
| **WhatsAppHost 24/7** | Recordatorios deuda/membresía + webhook inbound + media fallback |
| **Timer UI** (`TwilioTimerUIHabilitado`) | Respaldo mientras la app está abierta; desactivar en prod si el host corre siempre |
| **Supabase** | PDFs factura en producción; no reemplaza el webhook de chat |

---

## Troubleshooting

| Síntoma | Acción |
|---------|--------|
| Barra ámbar en chat | Ejecutar `Start-WhatsAppStack.ps1 -Profile Dev` o configurar URL fija |
| `/health` local falla | Verificar puerto 5088 libre; revisar logs en `%LocalAppData%\MFFITNESS\WhatsAppHost\logs` |
| `/health` público falla | Túnel caído; reiniciar tarea `MFFITNESS-WhatsAppTunnel` |
| Twilio no entrega inbound | URL webhook exacta POST HTTPS; firma requiere misma URL en config |
| Mensajes salen pero no entran | Solo falta webhook; envío usa API Twilio directa |

Diagnóstico completo: `Test-WhatsAppStack.ps1`
