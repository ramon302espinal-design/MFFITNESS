<#
.SYNOPSIS
  Installs rembg (CPU) into a local venv for MFFITNESS.
.DESCRIPTION
  Creates %LOCALAPPDATA%\MFFITNESS\rembg-venv, upgrades pip, and installs rembg[cpu,cli].
#>
[CmdletBinding()]
param(
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
    throw "No Python 3 interpreter found. Install Python 3 and ensure py/python is on PATH."
}

$venvPath = Join-Path $env:LOCALAPPDATA "MFFITNESS\rembg-venv"
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

Write-Host "Installing rembg[cpu,cli]..."
& $venvPython -m pip install "rembg[cpu,cli]"
if ($LASTEXITCODE -ne 0) { throw "rembg install failed (exit $LASTEXITCODE)" }

$rembgExe = Join-Path $venvPath "Scripts\rembg.exe"
Write-Host ""
Write-Host "Success."
Write-Host "  python.exe : $venvPython"
Write-Host "  rembg.exe  : $rembgExe"
& $venvPython -c "import rembg; print('  rembg version:', rembg.__version__)"


