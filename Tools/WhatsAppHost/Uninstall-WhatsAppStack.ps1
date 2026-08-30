#Requires -RunAsAdministrator
#Requires -Version 5.1
# Desinstala tareas del stack WhatsApp MFFITNESS.

$ErrorActionPreference = 'Stop'

foreach ($name in @('MFFITNESS-WhatsAppTunnel', 'MFFITNESS-WhatsAppHost')) {
    $t = Get-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue
    if ($t) {
        Stop-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue
        Unregister-ScheduledTask -TaskName $name -Confirm:$false
        Write-Host "Eliminada: $name"
    }
}

Get-Process ngrok, cloudflared, WhatsAppHost -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "OK. Stack WhatsApp desinstalado."
