<#
.SYNOPSIS
  Verifica el paquete de update generado (offline, sin SQL).
#>
[CmdletBinding()]
param(
    [string] $PackageDir = '',
    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    if (!(Test-Path (Join-Path $RepoRoot 'MFFITNESS.sln'))) {
        $RepoRoot = Split-Path -Parent $PSScriptRoot
    }
}

if ([string]::IsNullOrWhiteSpace($PackageDir)) {
    $PackageDir = Join-Path $RepoRoot 'artifacts\update-package'
}

$manifestPath = Join-Path $PackageDir 'manifest.json'
if (!(Test-Path $manifestPath)) { throw "No hay manifest.json en $PackageDir. Ejecuta Build-UpdatePackage.ps1" }

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$zipPath = Join-Path $PackageDir $manifest.packageName
if (!(Test-Path $zipPath)) { throw "ZIP no encontrado: $zipPath" }

# SHA256
$sha = (Get-FileHash -Algorithm SHA256 -Path $zipPath).Hash.ToLowerInvariant()
$expected = ([string]$manifest.packageSha256).ToLowerInvariant()
if ($sha -ne $expected) {
    throw "SHA256 mismatch. ZIP=$sha Manifest=$expected"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
$tmpUi = Join-Path $env:TEMP ("mff-verify-ui-" + [guid]::NewGuid().ToString('N') + ".dll")
try {
    $names = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    $required = @('UI.exe', 'UI.dll', 'BLL.dll', 'DL.dll', 'DTO.dll', 'CORE.dll')
    foreach ($r in $required) {
        if (-not ($names | Where-Object { $_ -eq $r -or $_.EndsWith('/' + $r) })) {
            throw "ZIP sin archivo requerido: $r"
        }
    }
    if ($names | Where-Object {
            $leaf = $_.Split('/')[-1]
            $leaf -eq 'UpdateManager.exe' -or $leaf.StartsWith('UpdateManager.')
        }) {
        throw "ZIP contiene artefactos UpdateManager.* (prohibido)"
    }

    $uiEntry = $zip.Entries | Where-Object {
        $n = $_.FullName.Replace('\', '/')
        $n -eq 'UI.dll' -or $n.EndsWith('/UI.dll')
    } | Select-Object -First 1
    if ($null -eq $uiEntry) { throw "ZIP sin UI.dll para verificar FileVersion" }

    [System.IO.Compression.ZipFileExtensions]::ExtractToFile($uiEntry, $tmpUi, $true)
    $vi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($tmpUi)
    $binary = $vi.ProductVersion
    if ([string]::IsNullOrWhiteSpace($binary)) { $binary = $vi.FileVersion }
    if ($binary -match '\+') { $binary = $binary.Split('+')[0] }
    $parts = $binary.Split('.')
    if ($parts.Length -eq 4 -and $parts[3] -eq '0') {
        $binary = ($parts[0..2] -join '.')
    }
    $expectedApp = [string]$manifest.appVersion
    if ($binary -ne $expectedApp) {
        throw "Versión de UI.dll en ZIP ($binary) != manifest.appVersion ($expectedApp). Regenera el paquete tras bump de Directory.Build.props."
    }
}
finally {
    $zip.Dispose()
    if (Test-Path $tmpUi) { Remove-Item $tmpUi -Force -ErrorAction SilentlyContinue }
}

Write-Host "Verify-UpdatePackage OK" -ForegroundColor Green
Write-Host "  Package:  $($manifest.packageName)"
Write-Host "  App:      $($manifest.appVersion)"
Write-Host "  TargetDb: $($manifest.targetDbVersion)"
Write-Host "  SHA256:   $sha"
Write-Host "  UI.dll FileVersion coincide con manifest: OK"
Write-Host "  UpdateManager.exe ausente del ZIP: OK"
