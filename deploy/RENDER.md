# Deploy no Render.com — Beta (Sprint 12)

## 🔗 Documentação oficial: **Sprint 12 — TreviaApp

## 🎯 Recursos

1. **PostgreSQL Gerenciado (obrigatório)
2. **Web Service** = API
3. **Static Site / Web Service = Client PWA
4. **Job Opcional: Job ou command rodar migrations

---

## 1. Passo-a-Passo (UI

### Passo 1: PostgreSQL (faça primeiro — sempre)

No painel Render → **New + → **PostgreSQL**:
- **Name**: `treviaapp-db`
- **PostgreSQL Version**: `17`
- **Region**: Oregon (US West) ou São Paulo
- **Instance Name**: `treviaapp-prod`
- **User**: `treviaapp`
- **Plan**: Free (pro) → Upgrade após beta)

Copie a **Connection String** (Internal) - formato):
```
Host=...;Port=5432;Database=...;Username=...;Password=...
```

---

### Passo 2: API (Web Service + Dockerfile)

Render → New + → Web Service:
- **Name**: `treviaapp-api`
- **Runtime**: Docker
- **Root**: raiz do repo
- **Dockerfile Path**: `src/TreviaApp.Api/Dockerfile`
- **Docker Context**: `.` (raiz)
- **Plan**: Starter ($7/mês grátis testar 750h/free tier)
- **Auto Deploy**: Sim
- **Health Check Path**: `/health/ready` (10s interval inicial → Autenticação)

#### Variáveis da API (Environment)

| Key | valor | Exemplo |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | |
| `PORT` | `10000` | **PORT fornecida pelo Render** |
| `ConnectionStrings__DefaultConnection` | *cole a do Passo 1 | |
| `Jwt__Issuer` | ex `TreviaApp.Production` | |
| `Jwt__Audience` | idem Issuer | |
| `Jwt__Key` | **64+ caracteres aleatórios | Use `openssl rand -base64 48` |
| `Jwt__AccessTokenMinutes` | `15` | |
| `Jwt__RefreshTokenDays` | `30` | |
| `Cors__AllowedOrigins__0` | URL do Client (HTTPS!) | `https://seu-client.onrender.com` |
| `AdminSeed__Email` | `admin@seudominio.com` | |
| `AdminSeed__Password` | forte 12+ chars) | Pelo menos 10 caracteres, com maiúsculas, minúsculas, números e símbolo |
| `FileStorage__Provider` | `Local` | (futuro S3/Azure) |
| `FileStorage__RootPath` | `/app_data/files` | |

---

### Passo 3: Rodar Migrations Iniciais (uma vez)

**Opção A (CLI em máquina local, usando o `--migrate-only`):**

No terminal (antes de usar a API funcionar:
```powershell
# PowerShell (Windows):
$env:ConnectionStrings__DefaultConnection = "<sua connection string>"
.\deploy\run-migrations.ps1
```
```bash
# Linux:
export ConnectionStrings__DefaultConnection="..."
chmod +x deploy/run-migrations.sh
./deploy/run-migrations.sh
```

Ou use o blueprint render.yaml (Cron Job:

```bash
# Ou crie um Shell temporário que execução de migrations** no Render:
Runtime: Docker (Dockerfile da API, command = "dotnet TreviaApp.Api.dll --migrate-only`
```

---

### Passo 4: Client PWA (Static Site **OU Web Service NGINX)

#### Opção A — Static Site + Build) (recomendado por ser mais barato)

1. New → Static Site:
- **Build Command**:
```bash
apt-get update && apt-get install -y curl
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --install-dir /opt/dotnet
export DOTNET_ROOT=/opt/dotnet
export PATH=$DOTNET_ROOT/tools:$DOTNET_ROOT:$PATH
dotnet workload install wasm-tools wasm-tools-net10 wasm-experimental workloads atualizados
dotnet publish src/TreviaApp.Client/TreviaApp.Client.csproj -c Release -o ./dist
```
- **Publish Directory**: `dist/wwwroot`
- **Environment Variable**:
  `API_BASE_URL`: `https://treviaapp-api.onrender.com` (URL da **seu serviço API)

⚠️ **Importante**: Static sites **não suportam proxy reverso em nginx. Para ter `/api → API, **precisa usar** Web Service NGINX. Use Opção B abaixo.

#### Opção B — Web Service NGINX (suporta)
Runtime → Docker)
New → Web Service, Dockerfile em `src/TreviaApp.Client/Dockerfile`, env var `API_BASE_URL`=URL da API.

---

## 2. Melhores práticas de produção:

1. **Jamais** coloque `ConnectionStrings, JWT segredos em texto** no repo. Use Environment Variables criptografadas.
2. **Habilite** SSL automáticos para não wildcard em produção**.
3. Use** não automático.
4. Sempre rode migrations **antes de deployar novas versões da API via job separado.
5. Configure Alertas de deployamento: `PORT` da API.
6. Mantenha** em `/health/ready`.

---

## 3.** Automação: Blueprint (Infra as Code)

Use [`render.yaml`:
`
Copiar na pasta `deploy/render.yaml` na raiz do repo.

Documentação: <https://render.com/docs/infrastructure-as-code>

---

## 4. Variáveis de Ambiente - Resumo

```env
# Server
- Server/Produção obrigatórias:

ASPNETCORE_ENVIRONMENT=Production
PORT=10000
ConnectionStrings__DefaultConnection=Host=...
Jwt__Issuer=TreviaApp.Production
Jwt__Audience=TreviaApp.Production
Jwt__Key=<chave-secreta-aqui-com-64+chars
Jwt__AccessTokenMinutes=15
Jwt__RefreshTokenDays=30
Cors__AllowedOrigins__0=https://seudominio.com
AdminSeed__Email=admin@seudominio.com
AdminSeed__Password=SenhaForte123!
FileStorage__Provider=Local
FileStorage__RootPath=/app_data/files
Serilog__MinimumLevel__Default=Information
ApiOptions__UseDetailedErrors=false
```

```env
# Client (Produção estática) necessárias Static Site
API_BASE_URL=https://apiseudominio.com
NGINX Docker Client
```

## 5. Migrations job ou Deploy

```powershell
#1. Criar migration de qualquer lugar (localmente. (PowerShell)
.\deploy\run-migrations.ps1
```
```bash
# Linux/bash:
./deploy/run-migrations.sh
```

ou manual:
```bash
dotnet run --project src/TreviaApp.Api/TreviaApp.Api.csproj -c Release -- --migrate-only
```
