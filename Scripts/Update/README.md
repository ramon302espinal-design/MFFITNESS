# Scripts de publicación y paquetes de actualización (FASE producción)

## 1. Publicar layout de instalación

```powershell
.\Scripts\Publish-Pos.ps1
```

Salida: `artifacts/pos/`

```
UI.exe
UpdateManager\UpdateManager.exe   ← runtime aislado (obligatorio en install inicial)
BLL.dll / DL.dll / DTO.dll / CORE.dll / UI.dll
Database/Migrations/*.sql
Resources/...
```

**TargetDbVersion actual: 4** (`0002`–`0004` = historial del motor; no borrar).
Próxima migración de negocio: `0005_....sql`.

## 2. Generar paquete de update (ZIP + manifest)

```powershell
.\Scripts\Build-UpdatePackage.ps1
# o con versión explícita:
.\Scripts\Build-UpdatePackage.ps1 -AppVersion 1.1.0 -MinAppVersion 1.0.0 -TargetDbVersion 4
```

Salida: `artifacts/update-package/`

| Archivo | Uso |
|---------|-----|
| `MFFITNESS-{version}.zip` | Asset del GitHub Release (sin UpdateManager.exe) |
| `manifest.json` | Asset del GitHub Release (SHA256 + SemVer + TargetDb) |
| `package-files.txt` | Auditoría local (no subir) |

## 3. Reglas de seguridad del package

- **Nunca** incluir `UpdateManager.exe` en el ZIP
- Solo rutas whitelist (`AllowedUpdatePackageFiles`)
- `manifest.PackageSha256` debe coincidir con el ZIP
- `TargetDbVersion` = máxima migración incluida (o valor explícito)

## 4. Despliegue inicial en un POS

Copiar **todo** `artifacts/pos/` (incluye `UpdateManager\UpdateManager.exe` y sus DLLs).

Los updates posteriores solo reemplazan lo del ZIP vía `UpdateManager\UpdateManager.exe --request`
(lanzado desde la subcarpeta; WorkingDirectory = esa carpeta).

## 5. UI (FASE 11 mínima)

En Login → enlace **Buscar actualizaciones** → `FrmActualizacion`:

1. Buscar (GitHub Releases + manifest)
2. Descargar e instalar (SHA256 + lanza `UpdateManager.exe --request`)
3. Cierra la UI para liberar binarios

Requisito: caja cerrada.

## 6. Publicación / despliegue (Release)

### En esta PC (recomendado)

```powershell
.\Scripts\Deploy-Pos.ps1 -CleanDesktopFolder
```

Instala en `%LocalAppData%\Programs\MFFITNESS` (fuera de OneDrive) y deja solo
`MFFITNESS.lnk` en el Escritorio + Menú Inicio. Ideal para OTA.

### Solo generar layout (lab / paquete)

```powershell
.\Scripts\Publish-Pos.ps1 -Configuration Release
# → artifacts\pos\
```

### Self-contained win-x64 (PC sin .NET Runtime; más pesado)

```powershell
dotnet publish .\UI\UI.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=false -p:DebugType=None -o .\artifacts\pos-sc --nologo
```

**No** copies la carpeta completa al Escritorio/OneDrive: el sync bloquea DLLs en updates.

Notas:
- El exe se llama `UI.exe` (contrato OTA). Nombre comercial **MFFITNESS** en metadatos/icono/acceso directo.
- No uses `PublishSingleFile=true` con ZIP OTA parcial.

## Flujo senior recomendado (1.0.0 → 1.1.0)

```powershell
# 1) Baseline 1.0.0 ya en artifacts/baselines/1.0.0 (si no, publica 1.0.0 antes del bump)
# 2) Directory.Build.props = 1.1.0
.\Scripts\Build-UpdatePackage.ps1 -AppVersion 1.1.0 -MinAppVersion 1.0.0
.\Scripts\Verify-UpdatePackage.ps1

# 3) Preparar lab (dry-run)
.\Scripts\Lab-RunUpdate.ps1

# 4) Ejecutar update REAL (caja cerrada; toca LocalDB)
.\Scripts\Lab-RunUpdate.ps1 -Execute

# 5) Publicar GitHub Release (solo cuando lab OK)
.\Scripts\Create-GitHubRelease.ps1          # dry-run
.\Scripts\Create-GitHubRelease.ps1 -Push    # crea release v1.1.0
```
