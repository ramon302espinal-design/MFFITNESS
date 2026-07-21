# Instala WhatsAppHost como tarea programada Windows (inicio al encender + reinicio si cae).
# Ejecutar PowerShell como Administrador:
#   powershell -ExecutionPolicy Bypass -File .\Install-WhatsAppHost.ps1

$ErrorActionPreference = "Stop"
$TaskName = "MFFITNESS-WhatsAppHost"

# Preferir exe junto al script (output Release); si se ejecuta desde el repo, usar bin\Release.
$ExePath = Join-Path $PSScriptRoot "WhatsAppHost.exe"
if (-not (Test-Path $ExePath)) {
    $alt = Join-Path $PSScriptRoot "bin\Release\net10.0\WhatsAppHost.exe"
    if (Test-Path $alt) { $ExePath = $alt }
}

if (-not (Test-Path $ExePath)) {
    Write-Host "No se encontro WhatsAppHost.exe en: $ExePath"
    Write-Host "Compila primero: dotnet build Tools\WhatsAppHost\WhatsAppHost.csproj -c Release"
    exit 1
}

$WorkDir = Split-Path -Parent $ExePath

$existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($existing) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Host "Tarea anterior eliminada."
}

$action = New-ScheduledTaskAction -Execute $ExePath -WorkingDirectory $WorkDir
$triggerStartup = New-ScheduledTaskTrigger -AtStartup
$triggerLogon = New-ScheduledTaskTrigger -AtLogOn
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -ExecutionTimeLimit ([TimeSpan]::Zero)

$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Highest

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger @($triggerStartup, $triggerLogon) `
    -Settings $settings `
    -Principal $principal `
    -Description "MFFITNESS WhatsApp automatizacion 24/7 (deudas y membresias)" | Out-Null

Start-ScheduledTask -TaskName $TaskName
Write-Host "OK. Tarea '$TaskName' instalada y arrancada."
Write-Host "Exe: $ExePath"
Write-Host "Logs: $WorkDir\logs\"
Write-Host "UI: TwilioTimerUIHabilitado=true es un buen respaldo mientras la app esta abierta."
