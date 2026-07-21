# Desinstala la tarea programada MFFITNESS-WhatsAppHost
# Ejecutar PowerShell como Administrador:
#   powershell -ExecutionPolicy Bypass -File .\Uninstall-WhatsAppHost.ps1

$ErrorActionPreference = "Stop"
$TaskName = "MFFITNESS-WhatsAppHost"

$existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if (-not $existing) {
    Write-Host "La tarea '$TaskName' no existe."
    exit 0
}

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
Write-Host "OK. Tarea '$TaskName' eliminada."
