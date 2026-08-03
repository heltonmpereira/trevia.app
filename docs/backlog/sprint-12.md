# Sprint 12 — PWA, Offline, Segurança, Testes e Beta

> **Data de início:** 2026-08-03
> **Base:** ROADMAP.md linha 17 + PROJECT_SPEC.md seções "PWA e offline" (item #11 ordem), "Deploy" (item #12 ordem), "Testes", "LGPD" (segurança), "Render.com"

---

## 📋 Pré-Implementação (Escopo para Aprovação)

### ETAPA
Sprint 12 — Consolidação para Beta: PWA com Instalação/Service Worker, Offline com Persistência Local de Treino, Fila de Sincronização, Headers de Segurança e Rate Limiting refinado, suíte completa de Testes (Unitários, Integração, Arquitetura) e Deploy automatizado para Beta no Render.

### OBJETIVO
Preparar a plataforma para **release beta pública**:
1. **PWA instalável** pelo usuário (manifest + service worker + ícones múltiplos) com responsividade mobile-first (PROJECT_SPEC: "PWA responsiva").
2. **Modo offline real**: treino em andamento salvo em IndexedDB, recuperação após recarregar/fechar aba, sem perda de dados.
3. **Fila de sincronização (Sync Queue)**: ações offline enfileiradas, enviadas ao voltar online com idempotência (anti-duplicidade via ClientRequestId).
4. **Segurança em produção**: CSP, X-Frame-Options, HSTS, CORS restrito, rate limiting por endpoint, health checks completos (DB + disco).
5. **Cobertura de testes**: regras de domínio puras (unitários), fluxos críticos (integração), architecture tests para manter Clean Architecture.
6. **Deploy automatizado**: migrations não automáticas (comando/job separado), Dockerfiles otimizados (multi-stage), blueprints do Render documentados.

---

### ESCOPO

#### ✅ Entra no escopo

1. **US-1201 — PWA Instalável (Manifest + Service Worker + Ícones)**
   - `manifest.json`: name, short_name, description, start_url="/", display="standalone", background_color, theme_color, icons 512/192/maskable.
   - Service Worker (PWA Builder/Microsoft padrão): cache estático assets do Blazor (dlls, css, js, imagens), cache de navegação "network-first" para rotas, fallback SPA offline.html.
   - Ícones: 192x192, 512x512 (PNG), 180 Apple touch, favicon múltiplos tamanhos.
   - `index.html` atualizado com tags `<link rel="manifest">`, `<meta theme-color>`, apple-touch-icon, manifest em `<Base />`.
   - `TreviaApp.Client.csproj`: `<ServiceWorkerAssetsManifest>true</ServiceWorkerAssetsManifest>`.
   - Responsividade: CSS container queries + bootstrap grid refinado para telas <400px (celulares pequenos).

2. **US-1202 — Offline: Persistência Local de Treino em Andamento**
   - Modelo `WorkoutInProgressStorage`: SessionId, StartedAt, Exercises (serializadas), CurrentExerciseIndex, PausedAt, ElapsedSeconds, Version.
   - Interface `IWorkoutOfflineStorage` com métodos: SaveCurrentWorkout, LoadCurrentWorkout, ClearCurrentWorkout, HasSavedWorkout.
   - Implementação Blazor `IndexedDbWorkoutStorage` via `IJSRuntime` chamando biblioteca leve `idb.js` (embedded como JS interop).
   - Hook automático: ao iniciar/pausar/executar série → salva no IndexedDB com Debounce 1s.
   - Ao recarregar página/App.razor inicializado → detecta `HasSavedWorkout()` → modal: "Deseja retomar treino de {HH:mm}? [Cancelar] [Retomar]".
   - Anti-corrupção: `Version` no schema; se schema versão antiga (diferente) → log warning + clear (não crasha).

3. **US-1203 — Fila de Sincronização (Sync Queue) com Idempotência**
   - Entidade `SyncQueueItem`: Id (Guid=ClientRequestId), OperationType (CompleteSet, FinishWorkout, SaveWeight etc), Payload (JSON), CreatedAt, RetryCount, LastError (nullable), Status (Pending/Processing/Completed/Failed).
   - Interface `ISyncQueue` com: Enqueue, ProcessPending, GetStatusCount, ClearCompleted.
   - Implementação `IndexedDbSyncQueue` + background timer `SyncBackgroundService` (Blazor WASM IDispatcher timer 15s quando online).
   - **Idempotência Server**: API recebe header `X-Client-Request-Id: <guid>`; tabela `ProcessedClientRequests` (PK RequestId, UserId, ProcessedAt) → se já existe retorna 200 idempotentemente sem reprocessar.
   - Indicador visual topo página: "🔄 3 pendentes" / "✅ Sincronizado" / "⚠ Erro de sincronização" com retry manual.

4. **US-1204 — Segurança em Produção**
   - `SecurityHeadersMiddleware` (API): adiciona headers:
     - `Content-Security-Policy`: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self' *; frame-ancestors 'none';
     - `X-Content-Type-Options: nosniff`
     - `X-Frame-Options: DENY`
     - `Referrer-Policy: strict-origin-when-cross-origin`
     - `Permissions-Policy: camera=(), microphone=(), geolocation=()`
     - `Strict-Transport-Security: max-age=31536000; includeSubDomains` (apenas produção)
   - Rate limiting refinado: políticas separadas
     - `AuthEndpoint`: login/register → 5 req/min por IP
     - `WorkoutWrite`: ações de execução → 120 req/min por usuário
     - `ReadEndpoint`: queries gerais → 300 req/min por usuário
     - `AdminEndpoint`: 60 req/min por usuário
   - **Health checks completos**:
     - `db`: Npgsql connection (já existe? garantido)
     - `disk`: FileStorage available space > 100MB
     - `memory`: GC memory < 80%
     - Endpoint `/health` + `/health/ready` + `/health/live` (padrão Render)
   - CORS: `AllowedOrigins` obrigatório; Production recusa wildcard (validate em startup); Origins lidos de IConfiguration[] e validados.

5. **US-1205 — Unit Tests (TreviaApp.UnitTests)**
   - **Gamificação**:
     - `LevelCurveTests`: xpRequiredForLevel(1..10) = valores fixos tabelados esperados; CalculateLevelFromTotalXp() = mapeamento correto.
     - `PointAntiExcessBehaviorTests`: 2x WorkoutCompleted mesmo dia → 2º retorna warning/sem pontos; 40 SetCompleted mesmo dia → pontos capados em 30.
   - **Domínio WorkoutExecution**:
     - `WorkoutSetVolumeTests`: Volume = carga * reps (válido Completed=true); inválido (incomplete) → não soma.
     - `WorkoutSessionDurationTests`: Pausa não contabiliza; Finish sem Start = throw DomainException.
   - **Domínio TrainingPlans**:
     - `SessionExerciseOrderTests`: Reorder preserva ids e corrige ordem (1,2,3 → swap 1↔3 = 3,2,1).
   - **Domínio Coaching**:
     - `CoachStudentLinkPermissionsTests`: CanViewWorkoutHistory = true quando PermissionFlag ativo.
   - **Framework**: xUnit + FluentAssertions + NSubstitute (verificar pacotes já instalados; senão adicionar).

6. **US-1206 — Integration Tests (TreviaApp.IntegrationTests)**
   - **Auth**: todos endpoints já cobrem parcial; adicionar:
     - `Register + ConfirmEmail + Login + Refresh + RevokeRefreshToken` (happy path completo).
     - `Register duplicate email` → 400.
     - `ForgotPassword + ResetPassword` flow.
   - **Exercises**: CreateExercise → Submit → Approve (Admin flow, 2 roles) → SearchApproved encontra.
   - **TrainingPlans**: Create → AddSession → AddExerciseToSession → AssignToStudent → Student pode Get.
   - **WorkoutExecution + Gamificação**:
     - StartWorkout → CompleteAllSets → FinishWorkout → AwardWorkoutPoints → Dashboard reflete pontos corretos.
   - **Coaching**: SendCoachInvite → AcceptCoachInvite → Coach pode GetStudentDashboard (policy).
   - Usa `TestWebApplicationFactory` com PostgreSQL Docker (testcontainers opcional; fallback SQLite in-memory se não disponível).

7. **US-1207 — Architecture Tests (TreviaApp.ArchitectureTests)**
   - **Layer Tests** (já existe? expandir):
     - Domain → **não referencia** Infrastructure, Api, Client.
     - Application → **não referencia** Api, Client.
     - Api → **não referencia** Client.
     - Client → **não referencia** Infrastructure, Domain (apenas Contracts + Shared).
   - **Controller Rules**:
     - Controllers herdam `ApiControllerBase`.
     - Controllers **não injetam** DbContext diretamente (só IMediator/ICommandHandler abstrações).
     - Actions **não retornam** entidades de domínio diretamente (retornam DTOs do Contracts).
   - **Naming Tests**:
     - CommandHandlers terminam com `CommandHandler`.
     - QueryHandlers terminam com `QueryHandler`.
     - Validators terminam com `Validator`.
   - **Dependency Tests**:
     - Services concretos (Infrastructure) implementam interfaces (Application/Domain).
     - Policies nomeadas existem em `AppPolicies.cs` e AuthorizationHandlers correspondem.

8. **US-1208 — Deploy Beta Automatizado**
   - **Migration controlada**:
     - Projeto console opcional `TreviaApp.Migrator` OU comando CLI `dotnet run --project TreviaApp.Api -- --migrate-only` (switch em Program.cs).
     - Script `deploy/run-migrations.ps1` e `.sh` para executar migration manualmente.
   - **Dockerfiles otimizados multi-stage**:
     - API: stage build SDK → publish → runtime aspnet Alpine (reduz imagem ~60%).
     - Client: stage build SDK → publish → nginx:alpine com `nginx.conf` fallback SPA + gzip.
   - **Render blueprints**:
     - `render.yaml` (opcional infra-as-code) com Postgres, Api Web Service, Client Static Site.
     - Atualização `deploy/RENDER.md` com passo a passo exato: criar serviços, variáveis, comandos, health check paths.
   - **Health check paths no Render**:
     - API: `/health/ready` → porta `PORT` da env.
     - Client: Static Site → `/index.html` (200).

#### ❌ Fora do escopo
- **SignalR em tempo real** (preparado estrutura Sync Queue, sem hubs reais).
- **Smartwatches/HealthKit** (PROJECT_SPEC classifica Fora do MVP).
- **Pagamentos/Marketplace** (fora MVP).
- **Ranking público de gamificação** (fora MVP).
- **PWA nativa** (.NET MAUI fica para roadmap futuro; PROJECT_SPEC já documenta limitações PiP/tela bloqueada).

---

### ARQUIVOS QUE SERÃO CRIADOS

**Client PWA:**
```
src/TreviaApp.Client/wwwroot/
  ├── manifest.json                    (US-1201)
  ├── service-worker.js                (US-1201)
  ├── service-worker.published.js      (US-1201, versão publish assets)
  ├── offline.html                     (US-1201, fallback offline)
  ├── icon-512.png + icon-maskable.png (US-1201, placeholder ou gerado)
  ├── apple-touch-icon.png             (US-1201)
  ├── js/
  │   ├── idb-storage.js               (US-1202/1203, interop IndexedDB)
  │   └── sync-indicator.js            (US-1203)
src/TreviaApp.Client/Services/
  ├── PwaInstallPromptService.cs       (US-1201)
  ├── IWorkoutOfflineStorage.cs        (US-1202)
  ├── IndexedDbWorkoutStorage.cs       (US-1202)
  ├── ISyncQueue.cs                    (US-1203)
  ├── IndexedDbSyncQueue.cs            (US-1203)
  └── SyncBackgroundService.cs         (US-1203, timer WASM)
src/TreviaApp.Client/Components/
  ├── SyncStatusIndicator.razor        (US-1203, componente visual status sync)
  ├── ResumeWorkoutModal.razor         (US-1202, modal retomar treino)
  └── PwaInstallBanner.razor           (US-1201, banner instalar app)
```

**API Segurança e Idempotência:**
```
src/TreviaApp.Api/Middlewares/
  └── SecurityHeadersMiddleware.cs     (US-1204)
src/TreviaApp.Api/Filters/
  └── IdempotencyFilter.cs             (US-1203, X-Client-Request-Id)
src/TreviaApp.Domain/Identity/
  └── ProcessedClientRequest.cs        (US-1203, tabela idempotência)
src/TreviaApp.Infrastructure/Persistence/Configurations/
  └── ProcessedClientRequestConfiguration.cs
src/TreviaApp.Shared/Constants/
  └── RateLimitPolicies.cs             (US-1204, nomes políticas)
src/TreviaApp.Api/HealthChecks/
  ├── DiskStorageHealthCheck.cs        (US-1204)
  └── MemoryHealthCheck.cs             (US-1204)
```

**Testes:**
```
tests/TreviaApp.UnitTests/
  ├── Gamification/
  │   ├── LevelCurveTests.cs           (US-1205)
  │   └── PointAntiExcessTests.cs      (US-1205)
  ├── WorkoutExecution/
  │   ├── WorkoutSetVolumeTests.cs     (US-1205)
  │   └── WorkoutSessionDurationTests.cs
  ├── TrainingPlans/
  │   └── SessionExerciseOrderTests.cs (US-1205)
  ├── Coaching/
  │   └── CoachPermissionsTests.cs     (US-1205)
  └── Usings.cs (atualizar)
tests/TreviaApp.IntegrationTests/
  ├── Auth/
  │   └── FullAuthFlowTests.cs         (US-1206)
  ├── Exercises/
  │   └── ExerciseApprovalFlowTests.cs (US-1206)
  ├── TrainingPlans/
  │   └── TrainingPlanCrudTests.cs     (US-1206)
  ├── WorkoutExecution/
  │   └── WorkoutWithGamificationTests.cs (US-1206)
  └── Coaching/
      └── CoachInviteAndDashboardTests.cs (US-1206)
tests/TreviaApp.ArchitectureTests/
  ├── ControllerRulesTests.cs          (US-1207)
  ├── NamingConventionTests.cs         (US-1207)
  └── ServiceRegistrationTests.cs      (US-1207)
```

**Deploy:**
```
deploy/
  ├── run-migrations.ps1               (US-1208)
  ├── run-migrations.sh                (US-1208)
  └── render.yaml                      (US-1208, blueprint)
```

---

### ARQUIVOS QUE SERÃO ALTERADOS

| Arquivo | Alteração |
|---|---|
| `src/TreviaApp.Client/Program.cs` | Registrar Services (IWorkoutOfflineStorage, ISyncQueue, SyncBackgroundService, PwaInstallPrompt) + `AddAuthorization`/`AddApiAuthorization` para JWT |
| `src/TreviaApp.Client/wwwroot/index.html` | `<link manifest>`, `<meta theme-color>`, apple-touch-icon, service-worker register script |
| `src/TreviaApp.Client/TreviaApp.Client.csproj` | `<ServiceWorkerAssetsManifest>true</ServiceWorkerAssetsManifest>` |
| `src/TreviaApp.Client/Layout/MainLayout.razor` | Adicionar `<SyncStatusIndicator />` e `<PwaInstallBanner />` |
| `src/TreviaApp.Client/App.razor` | Hook inicial LoadCurrentWorkout → modal `ResumeWorkoutModal` |
| `src/TreviaApp.Client/_Imports.razor` | Using Services/Components novos |
| `src/TreviaApp.Api/Program.cs` | UseSecurityHeadersMiddleware, MapHealthChecks (/health, /health/ready, /health/live), Rate limiting policies separadas + CORS validation Production, `--migrate-only` switch, IdempotencyFilter global |
| `src/TreviaApp.Api/appsettings.json` | Rate limiting policies novas, CORS, Health check UI (opcional) |
| `src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs` | +1 DbSet `ProcessedClientRequests` + configuration |
| `src/TreviaApp.Application/Abstractions/Data/IApplicationDbContext.cs` | +DbSet/Set<ProcessedClientRequest> |
| `src/TreviaApp.Infrastructure/Persistence/Migrations/` | `AddIdempotencyClientRequestTable` migration (1 tabela) |
| `src/TreviaApp.Api/Dockerfile` | Multi-stage otimizado |
| `src/TreviaApp.Client/Dockerfile` | Multi-stage publish + nginx gzip |
| `deploy/RENDER.md` | Passo a passo detalhado Beta + blueprint, comandos migrate |
| `TreviaApp.slnx` | Se existir, incluir arquivos novos (projetos já estão inclusos) |
| `tests/TreviaApp.UnitTests/TreviaApp.UnitTests.csproj` | PackageReferences xUnit/FluentAssertions/NSubstitute/Microsoft.NET.Test.Sdk |
| `tests/TreviaApp.IntegrationTests/TreviaApp.IntegrationTests.csproj` | Atualizar packages |
| `tests/TreviaApp.ArchitectureTests/LayerDependencyTests.cs` | Expandir regras |
| `sprint-12.md` (este arquivo) | Marcar passos como [x] progressivamente |

---

### MIGRATIONS

1 migration nova: `AddIdempotencyClientRequestTable`:
- **Tabela `ProcessedClientRequests`**:
  - `RequestId (Guid, PK)` → id do `X-Client-Request-Id`
  - `UserId (Guid, FK → AppUser, ON DELETE CASCADE, index)`
  - `OperationType (varchar 100)` → tipo de operação (CompleteSet etc)
  - `ResponsePayload (jsonb null)` → resposta serializada original para re-enviar idempotentemente
  - `ProcessedAt (timestamptz DEFAULT now())`
  - Índice único `(UserId, RequestId)` (garante 1 requestId por usuário, dupla proteção)

Outras entidades (WorkoutOfflineStorage, SyncQueue) são **client-side apenas** (IndexedDB no navegador) → sem migrations.

---

### ENDPOINTS (Alterações/Adições)

| Método | Rota | Descrição | US |
|---|---|---|---|
| (middleware) | todos POST/PUT | Header `X-Client-Request-Id` → verifica tabela ProcessedClientRequests | 1203 |
| GET | `/health` | Health check geral (db + disco + memória) | 1204 |
| GET | `/health/ready` | Readiness probe (Render) | 1204 |
| GET | `/health/live` | Liveness probe (Render) | 1204 |
| CLI | `--migrate-only` | Roda migrations e sai (sem startar Kestrel) | 1208 |

*(Demais endpoints já existem, apenas recebem middleware de segurança e rate limiting refinado).*

---

### TELAS (Client PWA)

1. **PWA Install Banner** (topo, só aparece se instalável): botão "Instalar TreviaApp" → dispara prompt navegador (US-1201).
2. **Resume Workout Modal** (ao abrir app se há treino salvo): "Você tem um treino em andamento iniciado às 15:32. Deseja retomar ou descartar?" (US-1202).
3. **Sync Status Indicator** (sempre visível no header):
   - Verde "✅ Sincronizado"
   - Azul "🔄 2 operações pendentes"
   - Amarelo "⚠ 1 erro. Tentar novamente"
4. **Offline.html** (fallback navegador puro quando sem rede e sem cache): tela amigável "Sem conexão. Seus treinos serão salvos localmente e sincronizados depois."
5. **Responsividade refinada**: todas telas (Login, Dashboard, Execução) reorganizam corretamente em telas 360px.

---

### TESTES (Planos)

| Projeto | Classes de teste | Quantidade mínima |
|---|---|---|
| UnitTests | LevelCurve, PointAntiExcess, WorkoutSetVolume, WorkoutSessionDuration, SessionExerciseOrder, CoachPermissions | 6 classes, ~30 testes |
| IntegrationTests | FullAuthFlow, ExerciseApproval, TrainingPlanCrud, Workout+Gamification, CoachInviteFlow | 5 classes, ~25 testes |
| ArchitectureTests | LayerRules (expandido), ControllerRules, Naming, Services/Policies | 4 arquivos, ~20 testes |
| **Total** | | **~75 testes** |

Todos devem passar com `dotnet test TreviaApp.slnx -c Release` em ambiente CI/CD.

---

### RISCOS e MITIGAÇÕES

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Service Worker cacheando API calls acidentalmente (stale data) | Alta | Alto | Cache strategy: `network-first` para `/api/*`, cache apenas assets estáticos (`framework/`, `css/`, `icon*`); regex exclusão /api no SW |
| IndexedDB não disponível (modo privado Safari) | Média | Médio | Fallback para `localStorage` com warning de capacidade menor; serviço detecta e ajusta |
| Idempotência com concorrência (mesmo requestId chegando 2x simultâneo) | Média | Médio | UNIQUE constraint no banco `(UserId, RequestId)` → catch DbUpdateException → retorna sucesso (assume processed) |
| Migration `--migrate-only` rodando múltiplas instâncias simultaneamente (Deploy paralelo) | Médio | Alto | Aplicar advisory lock PostgreSQL no início da migração (EF Core migration lock built-in já resolve na maioria; garantir `MAX_POOL_SIZE=1` no job migrate) |
| Security Headers quebram estilos/funcionalidade (CSP `unsafe-inline`) | Alta | Médio | Começar com `Content-Security-Policy-Report-Only` em staging por 2 dias; validar erros no console antes de ativar restrito |
| Rate limiting falso positivo p/ usuários de academia (muitos usuários NAT mesmo IP) | Média | Médio | `AuthEndpoint` usar combo IP+User quando autenticado; aumentar limite para `WorkoutWrite` (120/min é suficiente para 2 séries/segundo) |

---

## 🛠️ Implementação Passo a Passo (Atualizar conforme progresso)

### PASSO 0 — Criar pré-implementação e marcar escopo (este arquivo)
- [x] Definir US-1201 a US-1208 com escopo detalhado
- [x] Listar Arquivos criados/alterados, Migrations, Endpoints, Telas, Testes, Riscos
- [x] Atualizar este arquivo → marcar PASSO 0 = [x]

### PASSO 1 — US-1201: PWA manifest, service-worker, ícones e instalação
- [ ] manifest.json + ícones 512/maskable/apple-touch
- [ ] service-worker.js (assets cache + network-first api exclusão) + offline.html fallback
- [ ] index.html: meta tags, link manifest, SW register script
- [ ] TreviaApp.Client.csproj: ServiceWorkerAssetsManifest=true
- [ ] Componentes: PwaInstallBanner.razor + Service PwaInstallPromptService.cs
- [ ] Responsividade CSS refinada (mobile <400px)
- [ ] Atualizar este arquivo → marcar PASSO 1 = [x]

### PASSO 2 — US-1202: Offline persistência treino em andamento
- [ ] Criar js/idb-storage.js (interop IndexedDB CRUD genérico)
- [ ] Interfaces Services: IWorkoutOfflineStorage.cs
- [ ] Implementação IndexedDbWorkoutStorage.cs (com fallback localStorage)
- [ ] Component ResumeWorkoutModal.razor (modal retomar/descartar)
- [ ] App.razor inicializa LoadCurrentWorkout → mostra modal
- [ ] Hook save automático (debounce) nas ações de treino
- [ ] Atualizar este arquivo → marcar PASSO 2 = [x]

### PASSO 3 — US-1203: Sync Queue + Idempotência (Client + Server)
- [ ] ISyncQueue.cs + IndexedDbSyncQueue.cs (enqueue/process)
- [ ] SyncBackgroundService.cs (WASM timer 15s + online detection)
- [ ] Component SyncStatusIndicator.razor visual
- [ ] Domain ProcessedClientRequest.cs + EF Configuration
- [ ] ApplicationDbContext + IApplicationDbContext adicionar DbSet
- [ ] IdempotencyFilter.cs (API, lê X-Client-Request-Id, retorna cached se existe)
- [ ] Migration AddIdempotencyClientRequestTable gerada
- [ ] Program.cs registra filter global
- [ ] Atualizar este arquivo → marcar PASSO 3 = [x]

### PASSO 4 — US-1204: Segurança Headers + Rate Limiting + Health Checks
- [ ] SecurityHeadersMiddleware.cs (CSP, X-Frame, HSTS, etc)
- [ ] RateLimitPolicies.cs constants + Program.cs policies separadas (Auth, WorkoutWrite, Read, Admin)
- [ ] HealthChecks: DiskStorageHealthCheck.cs + MemoryHealthCheck.cs
- [ ] Program.cs: MapHealthChecks (/health, ready, live), UseSecurityHeaders, CORS production validation
- [ ] Atualizar este arquivo → marcar PASSO 4 = [x]

### PASSO 5 — US-1205: Unit Tests
- [x] Atualizar UnitTests.csproj com xUnit/FluentAssertions/NSubstitute packages
- [x] Gamification: LevelCurveTests + PointAntiExcessTests (UserLevelTests)
- [x] WorkoutExecution: WorkoutSetVolumeTests + WorkoutSessionDurationTests
- [x] TrainingPlans: SessionExerciseOrderTests
- [x] Coaching: CoachPermissionsTests
- [x] Atualizar este arquivo → marcar PASSO 5 = [x]

### PASSO 6 — US-1206: Integration Tests
- [x] Auth: FullAuthFlowTests (Register→Confirm→Login→Refresh→Revoke→Forgot→Reset)
- [x] Exercises: ExerciseApprovalFlowTests (Criar→Submit→Aprovar→Buscar)
- [x] TrainingPlans: TrainingPlanCrudTests (CRUD + AddSession + AddExercise + Assign)
- [x] WorkoutExecution: WorkoutWithGamificationTests (Start→Sets→Finish→Award→Dashboard)
- [x] Coaching: CoachInviteAndDashboardTests (Invite→Accept→CoachVêAluno)
- [x] Atualizar este arquivo → marcar PASSO 6 = [x]

### PASSO 7 — US-1207: Architecture Tests
- [x] ControllerRulesTests: herdam ApiControllerBase, não injetam DbContext, não retornam Entity
- [x] NamingConventionTests: CommandHandlers/QueryHandlers/Validators sufixos corretos
- [x] ServiceRegistrationTests: services implementam interfaces; policies possuem handlers
- [x] Expandir LayerDependencyTests (Domain not ref Infrastructure etc)
- [x] Atualizar este arquivo → marcar PASSO 7 = [x]

### PASSO 8 — US-1208: Deploy Beta Automatizado
- [ ] Program.cs: switch `--migrate-only` (run migrations + exit sem Kestrel)
- [ ] Dockerfile API multi-stage otimizado (SDK build → Alpine runtime)
- [ ] Dockerfile Client multi-stage (build → publish → nginx alpine + gzip)
- [ ] deploy/run-migrations.ps1 + .sh
- [ ] deploy/render.yaml blueprint
- [ ] deploy/RENDER.md passo a passo detalhado beta
- [ ] Atualizar este arquivo → marcar PASSO 8 = [x]

### PASSO 9 — Build + Teste final (0 erros, 0 testes falhando)
- [ ] `dotnet build TreviaApp.slnx -c Release` → 0 erros
- [ ] `dotnet test tests/TreviaApp.UnitTests/TreviaApp.UnitTests.csproj -c Release --no-build` → todos testes passam
- [ ] `dotnet test tests/TreviaApp.ArchitectureTests/TreviaApp.ArchitectureTests.csproj -c Release --no-build` → todos testes passam
- [ ] Corrigir erros/diagnostics de compilação ou testes
- [ ] Atualizar este arquivo → marcar PASSO 9 = [x]

### PASSO 10 — Relatório Pós-Implementação
- [ ] Preencher seção ✅ PÓS-IMPLEMENTAÇÃO abaixo (RESUMO, ARQUIVOS CRIADOS/ALTERADOS, ENDPOINTS, TELAS, COMO EXECUTAR, COMO TESTAR, MIGRATIONS, VARIÁVEIS, PENDÊNCIAS, PRÓXIMO PASSO)
- [ ] Atualizar este arquivo → marcar PASSO 10 = [x]

---

## ✅ PÓS-IMPLEMENTAÇÃO (Resumo, Arquivos, Endpoints disponíveis)

*(A ser preenchido ao final de todos os passos acima)*

### RESUMO:

### ARQUIVOS CRIADOS:

### ARQUIVOS ALTERADOS:

### ENDPOINTS DISPONÍVEIS:

### TELAS DISPONÍVEIS:

### COMO EXECUTAR:

### COMO TESTAR:

### MIGRATIONS:

### VARIÁVEIS DE AMBIENTE:

### PENDÊNCIAS E MELHORIAS FUTURAS:

### PRÓXIMA ETAPA RECOMENDADA:
