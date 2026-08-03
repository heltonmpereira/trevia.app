Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "==> TreviaApp - Aplicando Migracoes PostgreSQL (PowerShell) <==" -ForegroundColor Cyan

$SolutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $SolutionRoot

if (-not (Test-Path ".\src\TreviaApp.Api\TreviaApp.Api.csproj")) {
    throw "TreviaApp.Api.csproj nao encontrado no diretorio esperado."
}

Write-Host ""
Write-Host "==> Restaurando ferramentas dotnet-ef..." -ForegroundColor Yellow
dotnet tool restore 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Instalando dotnet-ef global fallback..." -ForegroundColor DarkYellow
    dotnet tool install --global dotnet-ef 2>&1 | Out-Null
}

Write-Host ""
Write-Host "==> Executando migrations --migrate-only (TreviaApp.Api)..." -ForegroundColor Yellow

$env:ASPNETCORE_ENVIRONMENT = if ($env:ASPNETCORE_ENVIRONMENT) { $env:ASPNETCORE_ENVIRONMENT } else { "Production" }
Write-Host "  Ambiente: $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Gray

dotnet run --project src\TreviaApp.Api\TreviaApp.Api.csproj -c Release -- --migrate-only

if ($LASTEXITCODE -ne 0) {
    throw "Falha ao aplicar migrations (exit code $LASTEXITCODE)."
}

Write-Host ""
Write-Host "==> Migrations aplicadas com SUCESSO!" -ForegroundColor Green
Write-Host ""
