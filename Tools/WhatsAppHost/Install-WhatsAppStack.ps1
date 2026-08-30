#Requires -RunAsAdministrator
#Requires -Version 5.1
<#
.SYNOPSIS
  Instala WhatsAppHost + túnel como tareas programadas Windows (producción o dev persistente).

.EXAMPLE
  # Producción (PC del gym): compila Release, copia a LocalAppData, tareas al inicio
  .\Install-WhatsAppStack.ps1 -Environment Production

.EXAMPLE
  # Dev: tareas al iniciar sesión (mismo usuario)
  .\Install-WhatsAppStack.ps1 -Environment Development

  Requisitos previos:
  - %LocalAppData%\MFFITNESS\whatsapp.stack.config (copiar desde whatsapp.stack.config.example)
  - PROD: WhatsAppPublicBaseUrl fija O NgrokDomain reservado O CloudflaredToken
  - ngrok / cloudflared en PATH
#>
param(
    [ValidateSet('Production', 'Development')]
    [string]$Environment = 'Production',
    [switch]$SkipBuild,
    [switch]$SkipTunnel
)

$ErrorActionPreference = 'Stop'

$TaskHost = 'MFFITNESS-WhatsAppHost'
$TaskTunnel = 'MFFITNESS-WhatsAppTunnel'
$InstallDir = Join-Path $env:LOCALAPPDATA 'MFFITNESS\WhatsAppHost'
$StackConfig = Join-Path $env:LOCALAPPDATA 'MFFITNESS\whatsapp.stack.config'
$LogDir = Join-Path $InstallDir 'logs'

function Get-RepoRoot {
    $here = $PSScriptRoot
    if (Test-Path (Join-Path $here 'WhatsAppHost.csproj')) { return $here }
    throw 'Ejecute desde Tools\WhatsAppHost'
}

function Read-StackSetting([string]$key) {
    if (-not (Test-Path $StackConfig)) { return $null }
    [xml]$xml = Get-Content $StackConfig
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq $key } | Select-Object -First 1
    if ($node -and -not [string]::IsNullOrWhiteSpace($node.value)) { return $node.value.Trim() }
    return $null
}

function Remove-TaskIfExists([string]$Name) {
    $t = Get-ScheduledTask -TaskName $Name -ErrorAction SilentlyContinue
    if ($t) {
        Stop-ScheduledTask -TaskName $Name -ErrorAction SilentlyContinue
        Unregister-ScheduledTask -TaskName $Name -Confirm:$false
        Write-Host "Tarea eliminada: $Name"
    }
}

function Register-StackTask {
    param(
        [string]$Name,
        [string]$Exe,
        [string]$Arguments,
        [string]$WorkingDirectory,
        [string[]]$TriggerTypes
    )
    $action = New-ScheduledTaskAction -Execute $Exe -Argument $Arguments -WorkingDirectory $WorkingDirectory
    $triggers = @()
    foreach ($tt in $TriggerTypes) {
        switch ($tt) {
            'Startup' { $triggers += New-ScheduledTaskTrigger -AtStartup }
            'Logon'   { $triggers += New-ScheduledTaskTrigger -AtLogOn }
        }
    }
    $settings = New-ScheduledTaskSettingsSet `
        -AllowStartIfOnBatteries `
        -DontStopIfGoingOnBatteries `
        -StartWhenAvailable `
        -RestartCount 3 `
        -RestartInterval (New-TimeSpan -Minutes 1) `
        -ExecutionTimeLimit ([TimeSpan]::Zero)
    $principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Highest
    Register-ScheduledTask -TaskName $Name -Action $action -Trigger $triggers -Settings $settings -Principal $principal `
        -Description "MFFITNESS WhatsApp stack ($Environment)" | Out-Null
}

# --- Main ---
$repo = Get-RepoRoot
$example = Join-Path $repo '..\..\whatsapp.stack.config.example'
$example = (Resolve-Path $example).Path

if (-not (Test-Path $StackConfig)) {
    New-Item -ItemType Directory -Path (Split-Path $StackConfig -Parent) -Force | Out-Null
    Copy-Item $example $StackConfig
    Write-Host "Creado $StackConfig — configure antes de producción." -ForegroundColor Yellow
}

if (-not $SkipBuild) {
    Write-Host "Publicando WhatsAppHost Release -> $InstallDir" -ForegroundColor Cyan
    if (Test-Path $InstallDir) { Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue }
    dotnet publish (Join-Path $repo 'WhatsAppHost.csproj') -c Release -o $InstallDir --nologo
}

$hostExe = Join-Path $InstallDir 'WhatsAppHost.exe'
if (-not (Test-Path $hostExe)) { throw "Falta $hostExe — compile con dotnet publish" }

New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
Copy-Item $StackConfig (Join-Path $InstallDir 'whatsapp.stack.config') -Force

$triggers = if ($Environment -eq 'Production') { @('Startup', 'Logon') } else { @('Logon') }

Remove-TaskIfExists $TaskHost
Remove-TaskIfExists $TaskTunnel

Register-StackTask -Name $TaskHost -Exe $hostExe -Arguments '' -WorkingDirectory $InstallDir -TriggerTypes $triggers
Write-Host "Tarea host: $TaskHost" -ForegroundColor Green

if (-not $SkipTunnel) {
    $publicUrl = Read-StackSetting 'WhatsAppPublicBaseUrl'
    $provider = Read-StackSetting 'TunnelProvider'
    if (-not $provider) { $provider = 'Ngrok' }
    $port = Read-StackSetting 'WhatsAppMediaListenPort'
    if (-not $port) { $port = '5088' }
    $ngrokDomain = Read-StackSetting 'NgrokDomain'
    $cfToken = Read-StackSetting 'CloudflaredToken'

    $tunnelExe = $null
    $tunnelArgs = $null

    if ($provider -eq 'Cloudflared' -and $cfToken) {
        $tunnelExe = 'cloudflared'
        $tunnelArgs = "tunnel run --token $cfToken"
    }
    elseif ($provider -eq 'Ngrok' -and $ngrokDomain) {
        $tunnelExe = 'ngrok'
        $tunnelArgs = "http --url=$ngrokDomain $port"
    }
    elseif ($provider -eq 'None' -and $publicUrl) {
        Write-Host "TunnelProvider=None y URL fija — sin tarea de túnel." -ForegroundColor Yellow
    }
    else {
        Write-Host "AVISO: configure NgrokDomain, CloudflaredToken o WhatsAppPublicBaseUrl+None en $StackConfig" -ForegroundColor Yellow
    }

    if ($tunnelExe) {
        Register-StackTask -Name $TaskTunnel -Exe $tunnelExe -Arguments $tunnelArgs -WorkingDirectory $InstallDir -TriggerTypes $triggers
        Write-Host "Tarea túnel: $TaskTunnel ($tunnelExe)" -ForegroundColor Green
    }
}

Start-ScheduledTask -TaskName $TaskHost -ErrorAction SilentlyContinue
Start-ScheduledTask -TaskName $TaskTunnel -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Instalación OK [$Environment]" -ForegroundColor Green
Write-Host "  Host:   $hostExe"
Write-Host "  Config: $StackConfig"
Write-Host "  Logs:   $LogDir"
Write-Host ""
Write-Host "Verifique: .\Test-WhatsAppStack.ps1" -ForegroundColor Cyan
Write-Host "Desinstalar: .\Uninstall-WhatsAppStack.ps1" -ForegroundColor DarkGray
