<#
.SYNOPSIS
  Instala faster-whisper (CPU) para dictado en español con acento dominicano/latino.
.DESCRIPTION
  Crea %LOCALAPPDATA%\MFFITNESS\whisper-venv e instala faster-whisper.
  El POS usará este motor automáticamente si está disponible (más preciso que SAPI).
#>
[CmdletBinding()]
param(
    [ValidateSet("tiny", "base", "small", "medium")]
    [string]$ModelSize = "small",
    [switch]$ForceRecreate
)

$ErrorActionPreference = "Stop"

function Find-PythonLauncher {
    $candidates = @(
        @{ Cmd = "py"; Args = @("-3") },
        @{ Cmd = "python"; Args = @() },
        @{ Cmd = "python3"; Args = @() }
    )
    foreach ($c in $candidates) {
        $cmd = Get-Command $c.Cmd -ErrorAction SilentlyContinue
        if (-not $cmd) { continue }
        try {
            $verArgs = $c.Args + @("--version")
            $null = & $c.Cmd @verArgs 2>&1
            if ($LASTEXITCODE -eq 0 -or $?) {
                return @{ Exe = $c.Cmd; Args = $c.Args }
            }
        } catch { }
    }
    throw "No Python 3. Install Python 3 and ensure py/python is on PATH."
}

$venvPath = Join-Path $env:LOCALAPPDATA "MFFITNESS\whisper-venv"
Write-Host "Target venv: $venvPath"

if ($ForceRecreate -and (Test-Path $venvPath)) {
    Write-Host "Removing existing venv (-ForceRecreate)..."
    Remove-Item -Recurse -Force $venvPath
}

$parent = Split-Path $venvPath -Parent
if (-not (Test-Path $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$py = Find-PythonLauncher
Write-Host "Using: $($py.Exe) $($py.Args -join ' ')"

if (-not (Test-Path (Join-Path $venvPath "Scripts\python.exe"))) {
    Write-Host "Creating virtual environment..."
    & $py.Exe @($py.Args + @("-m", "venv", $venvPath))
    if ($LASTEXITCODE -ne 0) { throw "Failed to create venv (exit $LASTEXITCODE)" }
} else {
    Write-Host "Virtual environment already exists."
}

$venvPython = Join-Path $venvPath "Scripts\python.exe"
Write-Host "Upgrading pip..."
& $venvPython -m pip install --upgrade pip
if ($LASTEXITCODE -ne 0) { throw "pip upgrade failed (exit $LASTEXITCODE)" }

Write-Host "Installing faster-whisper (CPU)..."
& $venvPython -m pip install "faster-whisper"
if ($LASTEXITCODE -ne 0) { throw "faster-whisper install failed (exit $LASTEXITCODE)" }

$scriptDest = Join-Path $env:LOCALAPPDATA "MFFITNESS\Whisper\transcribe_es.py"
$scriptSrc = Join-Path $PSScriptRoot "transcribe_es.py"
$scriptDir = Split-Path $scriptDest -Parent
if (-not (Test-Path $scriptDir)) {
    New-Item -ItemType Directory -Path $scriptDir -Force | Out-Null
}
Copy-Item -Path $scriptSrc -Destination $scriptDest -Force

Write-Host ""
Write-Host "Pre-downloading Whisper model '$ModelSize' (first run may take a few minutes)..."
& $venvPython -c @"
from faster_whisper import WhisperModel
WhisperModel('$ModelSize', device='cpu', compute_type='int8')
print('Model $ModelSize ready.')
"@

Write-Host ""
Write-Host "Success."
Write-Host "  python.exe      : $venvPython"
Write-Host "  transcribe_es.py: $scriptDest"
Write-Host "  model           : $ModelSize"
Write-Host ""
Write-Host "Reinicia el POS. El micrófono usará Whisper automáticamente."
