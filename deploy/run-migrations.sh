#!/usr/bin/env bash
set -euo pipefail

echo -e "\033[36m==> TreviaApp - Aplicando Migrações PostgreSQL (Bash)\033[0m"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$SOLUTION_ROOT"

if [ ! -f "src/TreviaApp.Api/TreviaApp.Api.csproj" ]; then
    echo -e "\033[31mERRO: TreviaApp.Api.csproj não encontrado.\033[0m"
    exit 1
fi

echo -e "\033[33m==> Restaurando/instalando dotnet-ef...\033[0m"
dotnet tool restore 2>&1 || echo "dotnet-ef não instalado: usando fallback..."

export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Production}"
echo "  Ambiente: $ASPNETCORE_ENVIRONMENT"

echo -e "\033[33m==> Executando migrations --migrate-only...\033[0m"
dotnet run --project src/TreviaApp.Api/TreviaApp.Api.csproj -c Release -- --migrate-only

echo ""
echo -e "\033[32m==> Migrations aplicadas com SUCESSO!\033[0m"
