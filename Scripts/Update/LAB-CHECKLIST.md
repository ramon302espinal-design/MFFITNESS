# Lab checklist — actualización real (pre-producción)

Ejecutar en una máquina de prueba con LocalDB `MF CYBER DB` y **caja cerrada**.

## A. Artefactos

```powershell
.\Scripts\Build-UpdatePackage.ps1
```

Verificar:

- [ ] `artifacts/pos/UI.exe` existe
- [ ] `artifacts/pos/UpdateManager\UpdateManager.exe` existe
- [ ] `artifacts/update-package/MFFITNESS-*.zip` existe
- [ ] `artifacts/update-package/manifest.json` existe
- [ ] ZIP **no** contiene `UpdateManager.exe` ni carpeta `UpdateManager/`
- [ ] `manifest.packageSha256` coincide con el ZIP

```powershell
.\Scripts\Verify-UpdatePackage.ps1
```

## B. Despliegue lab (simula POS instalado)

1. Copiar `artifacts/pos\` → carpeta lab (ej. `C:\MFFITNESS-Lab\`)
2. Abrir UI, confirmar login y versión en pantalla
3. Anotar `SchemaVersion` actual (query o log de migración)

## C. Prueba E2E real (manual)

1. Caja **cerrada**
2. Preparar `request.json` apuntando al ZIP + manifest SHA256 + `InstallDirectory` = carpeta lab
3. Ejecutar:

```text
artifacts\pos\UpdateManager\UpdateManager.exe --request C:\ruta\request.json
```

4. Esperar exit code `0`
5. Verificar:
   - [ ] AppVersion == manifest.AppVersion
   - [ ] SchemaVersion == manifest.TargetDbVersion
   - [ ] UI arranca
   - [ ] Datos de negocio intactos (spot-check clientes/ventas)

## D. Recuperación

1. Simular crash (matar UpdateManager a mitad) o usar sesión stale
2. `artifacts\pos\UpdateManager\UpdateManager.exe --recover`
3. Verificar estado terminal seguro (OLD+OLD o RecoveryRequired), nunca Completed inválido

## D2. Limpieza de sesiones de lab

Tras labs fallidos, sesiones `Completed` / `Blocked` / `FailedRecovered` se pueden archivar:

```powershell
.\Scripts\Quarantine-UpdateSessions.ps1
```

`FailedRecoveryRequired` / `RecoveryRequired` **no** se mueven (bloqueo intencional).

## E. Fallos controlados

| Escenario | Esperado |
|-----------|----------|
| Caja abierta | Blocked |
| SHA256 incorrecto | Blocked |
| ZIP sin migración requerida | Failed / FailedRecovered |
| Kill mid-install | FailedRecovered vía --recover |

## F. No hacer en lab de prod-data

- No apuntar a la BD de producción real del gimnasio
- No borrar backups
- No forzar Kill de UI salvo timeout documentado
