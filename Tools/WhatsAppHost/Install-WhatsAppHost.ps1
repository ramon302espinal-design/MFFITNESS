# Compatibilidad: redirige al instalador completo del stack.
# Ejecutar PowerShell como Administrador:
#   powershell -ExecutionPolicy Bypass -File .\Install-WhatsAppStack.ps1 -Environment Production

$here = $PSScriptRoot
& (Join-Path $here 'Install-WhatsAppStack.ps1') -Environment Production @args
