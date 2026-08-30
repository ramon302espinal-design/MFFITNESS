# Instalación MFFITNESS (instalable)

## Para el desarrollador — crear el instalador

```powershell
cd C:\Users\Mootv\source\repos\MFFITNESS
.\Scripts\Build-Installer.ps1
```

Salida:
- **Setup.exe:** `artifacts\installer\MFFITNESS-Setup-<versión>.exe` (requiere [Inno Setup 6](https://jrsoftware.org/isdl.php))
- **Paquete manual:** `artifacts\installer\package\Instalar-MFFITNESS.cmd` (sin Inno Setup)

## Instalar en esta PC (desarrollo / gym)

```powershell
.\Scripts\Install-MFFITNESS.ps1
# o atajo:
.\Scripts\Deploy-Pos.ps1
```

Instala en:
```
%LocalAppData%\Programs\MFFITNESS\
```

Incluye:
- `UI.exe` + DLLs
- `WhatsAppHost\` (webhook WhatsApp)
- `UpdateManager\`
- Acceso directo Escritorio + Menú Inicio
- Entrada en **Configuración → Aplicaciones → MFFITNESS POS**

## Desinstalar

- Configuración de Windows → Aplicaciones → MFFITNESS POS → Desinstalar  
- O: `%LocalAppData%\Programs\MFFITNESS\Uninstall-MFFITNESS.ps1`

Los datos de usuario (`%LocalAppData%\MFFITNESS\` — secretos Twilio, facturas, ngrok config) **se conservan** al desinstalar.

## Portable vs instalable

| | Portable (antiguo) | Instalable (nuevo) |
|---|-------------------|-------------------|
| Ubicación | Carpeta suelta / OneDrive | `%LocalAppData%\Programs\MFFITNESS` |
| Acceso directo | Manual | Automático |
| Desinstalar | Borrar carpeta | Panel de Windows |
| OTA updates | Sí | Sí (misma ruta) |
| WhatsAppHost | A veces faltaba | Incluido en publish |

## Requisitos en la PC del gym

- Windows 10/11 x64
- .NET 8 (incluido en el publish self-contained si aplica; ver UI.csproj)
- SQL Server LocalDB (`MF CYBER DB`)
- **ngrok** instalado y autenticado (para recibir WhatsApp)
- Credenciales Twilio en `%LocalAppData%\MFFITNESS\twilio.secrets.config`
