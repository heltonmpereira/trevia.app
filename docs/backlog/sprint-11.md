# Sprint 11 — Gamificação: Pontos, Níveis, Conquistas e Sequências

> **Data de início:** 2026-08-03
> **Base:** ROADMAP.md linha 16 + PROJECT_SPEC.md seção "Gamificação" (item 7 núcleo MVP + ordem desenvolvimento #10) + item 7 da ordem ("Aluno acompanha evolução e recebe pontos")

---

## 📋 Pré-Implementação (Escopo para Aprovação)

### ETAPA
Sprint 11 — Módulo de Gamificação (Pontos Transacionais, Níveis com XP, Conquistas, Streaks avançados e Missões diárias/semanais)

### OBJETIVO
Entregar o sistema de gamificação completo da plataforma: **pontuação transacional** (histórico auditável, sem recompensa de excesso de treino), **níveis com curva XP parametrizada**, **conquistas/achievements** (definições base + por usuário), **streaks avançados** (dias/semanas consecutivos com suporte a cálculo retroativo + intervalos), **missões automáticas diárias/semanais** com progresso e premiação, e DTOs para tela de perfil do usuário (barra XP, nível, conquistas recentes, streak atual, próximas missões). Cumpre o PROJECT_SPEC: *"Toda pontuação deve possuir histórico transacional. Não recompense excesso de treino."* (hard constraint)

---

### ESCOPO

#### ✅ Entra no escopo

1. **US-1101 — Transação de Pontos (Histórico Transacional)**
   - Entidade `PointTransaction`: `UserId`, `Amount` (+/-), `PointReason` (enum: WorkoutCompleted, SetCompleted, ReadFeedback, Streak7Days, AchievementUnlocked, DailyMissionCompleted, WeeklyMissionCompleted, ManualAdjustment), `ReferenceType` (nullable: WorkoutSession, UserAchievement, UserMission etc), `ReferenceId`, `Description`, `CreatedAt`.
   - Regra anti-excesso: máximo **1 treino/dia** pontua para WorkoutCompleted (limita recompensa diária). Demais séries e missões pontuam normalmente mas `SetCompleted` tem limite de 30 pontos/dia.
   - **Hook side-effect transacional**: Ao finalizar `WorkoutSession` (via `FinishWorkoutSessionCommandHandler` futuro — nesta sprint criamos service `IPointAwardService`, não acoplamos handlers já existentes; criamos endpoint `POST /api/gamification/award/workout/{sessionId}` manual/demo e service publico para integração futura).
   - Query: `GET /api/gamification/points/history` paginado + `GET /api/gamification/points/balance` (total pontos atuais).

2. **US-1102 — Níveis e XP com curva parametrizada**
   - Entidade `UserLevel`: `UserId`, `CurrentLevel` (1-100), `CurrentXp`, `TotalXpEarned`.
   - **Fórmula da curva** (classe parametrizada `LevelCurve`): `xpRequiredForLevel(L) = round(100 * L^1.8 + 50*L)`. Exemplo: Level 1 = 150 XP, Level 2 = 423 XP, Level 5 = 1 630 XP.
   - Ao receber XP: acumular, disparar level-ups automáticos via `LevelUpEvent` (domínio), gerar `PointTransaction.Reason=LevelUp` bonus (`Level*50` pontos por up).
   - Query: `GET /api/gamification/progress` → `{ level, currentXp, xpToNextLevel, totalXp, percentageBar }`.

3. **US-1103 — Conquistas / Achievements**
   - `AchievementDefinition` (seed estático, tabela): `Id`, `Code` (enum/slug), `Name`, `Description`, `Icon`, `PointsReward`, `AchievementCategory` (Milestone, Streaks, Performance, Social), `CriteriaConfig` (JSON parametrizado: ex: `{ "WorkoutCount": 1 }` = "Primeiro treino").
   - `UserAchievement`: `UserId`, `AchievementDefinitionId`, `UnlockedAt`, `Progress` (0-100, conquistas incrementais ex: 10 treinos).
   - **Service de avaliação**: `AchievementEvaluator.EvaluateAllAsync(userId, db)` — regras simples hardcoded + seed de 10 conquistas base:
     - `AC001` PrimeiroTreino (1 WorkoutSession)
     - `AC002` Frequência10 (10 treinos concluídos)
     - `AC003` SemanaCompleta (7 dias consecutivos)
     - `AC004` MêsIntegro (30 dias streak)
     - `AC005` PrimeiroRecord (qualquer PersonalRecord criado na Sprint 9)
     - `AC006` LeitorDeFeedbacks (5 feedbacks lidos/marked read)
     - `AC007` SerieConcluida100 (100 séries com `Completed=true`)
     - `AC008` FichaCompletada (Plano concluído)
     - `AC009` Level5, `AC010` Level10
   - Endpoints: `GET /api/gamification/achievements/all` (definições + progresso do usuário), `GET /api/gamification/achievements/recent?top=5` (recentemente desbloqueadas).

4. **US-1104 — Streaks avançados integrados com Gamificação**
   - Entidade `UserStreak`: `UserId`, `StreakType` (DailyWorkout / WeeklyWorkout), `CurrentStreak`, `LongestStreak`, `LastActiveAt`, `WeekStartDate` (para semanal).
   - Método de domínio `CalculateStreaksFromHistory(workoutDates)` — pode ser recalculado retroativamente (idempotente) a partir de `WorkoutSession.CompletedAt`.
   - Bônus de streak integrados:
     - 7 dias consecutivos → `PointReason.Streak7Days` + 100 pontos + Achievement `AC003`
     - 30 dias → 500 pontos + Achievement `AC004`
   - Query: `GET /api/gamification/streaks` → `{ daily: { current, longest, lastActiveAt }, weekly: {...} }`.

5. **US-1105 — Missões diárias/semanais automáticas**
   - `DailyMissionDefinition` (seed estático): `Id`, `Code`, `Title`, `Description`, `TargetValue`, `MissionMetric` (WorkoutsCompleted, SetsCompleted, FeedbackRead, MinutesTrained), `PointsReward`, `XpReward`.
   - `WeeklyMissionDefinition`: idêntico, visão semanal.
   - `UserDailyMission`: `UserId`, `MissionId`, `DateOnly`, `CurrentValue`, `IsCompleted`, `CompletedAt`, `ClaimedAt`.
   - `UserWeeklyMission`: idêntico, WeekStart.
   - Seed base (3 diárias + 2 semanais):
     - D1: "Completar 1 treino" → 30 pontos + 50 XP
     - D2: "Concluir 10 séries" → 20 pontos + 30 XP
     - D3: "Ler 1 feedback" → 10 pontos + 20 XP
     - W1: "Treinar 3 dias na semana" → 100 pontos + 150 XP
     - W2: "Concluir 40 séries" → 80 pontos + 100 XP
   - Endpoints: `GET /api/gamification/missions/today`, `GET /api/gamification/missions/this-week`, `POST /api/gamification/missions/{missionId}/claim` (reivindicar recompensa - idempotente).

6. **US-1106 — Tela Perfil: Painel de progresso agregado**
   - Endpoint `GET /api/gamification/dashboard` → `GamificationDashboardResponse` agregado:
     - Nível atual + barra XP (%), total pontos
     - Streaks diário/semanal
     - Próximas 3 conquistas (maior progresso %)
     - 3 conquistas mais recentes
     - Missões do dia
     - Últimas 5 transações de pontos
   - Endpoint `GET /api/gamification/dashboard/{userId}` para Coach/Admin ver painel de aluno (política `CanViewWorkoutHistory` já existente).

7. **US-1107 — Autorização e resource-based**
   - Default: `[Authorize]`; usuário só acessa seu próprio painel/histórico.
   - `IsTrainerOrAdmin` + política `CoachPermissions.CanViewWorkoutHistory` para acessar dashboard/pontos/streaks de aluno vinculado ativo.
   - Ajuste manual de pontos (`ManualAdjustment`) exclusivo `IsAdmin`.

#### ❌ Fora do escopo (Riscos/MVP)
- Sistema de ranking público (PROJECT_SPEC classifica como Fora do MVP).
- Sistema de desafios entre usuários (amigos, grupos — preparado `AchievementCategory.Social`, sem implementar).
- Recompensas de NFT, selos visuais complexos, loja de items (preparado apenas `AchievementDefinition.Icon` string URL).
- Acoplamento direto em `FinishWorkoutSessionCommandHandler` (Sprint 7) — nesta sprint, hooks são chamados via `IPointAwardService` público, e endpoint manual `POST award/workout/{sessionId}` demonstra/integra.
- Gamificação em tempo real (SignalR/SSE) — preparado endpoints HTTP, sem webhook push.

---

### ARQUIVOS QUE SERÃO CRIADOS

**Domínio (Domain):**
```
src/TreviaApp.Domain/Gamification/
  ├── PointTransaction.cs           (Agregado, histórico transacional - US-1101)
  ├── UserLevel.cs                  (Agregado, CurrentLevel + XP - US-1102)
  ├── LevelCurve.cs                 (Value Object, fórmula parametrizada)
  ├── AchievementDefinition.cs      (Entidade seed, conquistas disponíveis - US-1103)
  ├── UserAchievement.cs            (Entidade, conquistas desbloqueadas por usuário)
  ├── UserStreak.cs                 (Agregado, Daily/Weekly streak - US-1104)
  ├── DailyMissionDefinition.cs     (Entidade seed, missões do dia - US-1105)
  ├── WeeklyMissionDefinition.cs    (Entidade seed, missões da semana)
  ├── UserDailyMission.cs           (Entidade, progresso diário por usuário)
  └── UserWeeklyMission.cs          (Entidade, progresso semanal por usuário)
src/TreviaApp.Shared/Enums/
  └── GamificationEnums.cs          (PointReason, AchievementCategory, MissionMetric, StreakType, LevelUpResult)
src/TreviaApp.Shared/Constants/
  └── GamificationConstants.cs      (10 AchievementCodes estáticos (AC001-10), 5 MissionCodes, caps anti-excesso)
```

**Contracts:**
```
src/TreviaApp.Contracts/Gamification/
  ├── Requests/
  │   ├── AwardWorkoutPointsRequest.cs  (POST award/workout)
  │   ├── AdjustPointsRequest.cs        (POST adjust - Admin)
  │   └── ClaimMissionRequest.cs        (POST claim missão)
  └── Responses/
      └── GamificationResponses.cs     (PointHistoryResponse, PointBalanceResponse,
                                          UserLevelProgressResponse,
                                          AchievementProgressResponse,
                                          StreaksSummaryResponse,
                                          UserMissionProgressResponse,
                                          GamificationDashboardResponse
                                          + Paginated wrappers)
```

**Application:**
```
src/TreviaApp.Application/Gamification/
  ├── Behaviors/
  │   └── PointAntiExcessBehavior.cs  (Valida caps diários antes de inserir transação)
  ├── Commands/
  │   ├── AwardWorkoutPointsCommand.cs (Handler, side-effect avalia: missões/streaks/nível/achievements)
  │   ├── AdjustPointsCommand.cs       (Admin)
  │   ├── ClaimMissionCommand.cs       (Diária ou Semanal - idempotente)
  │   └── RecomputeStreaksCommand.cs   (Recalcula retroativamente por usuário)
  ├── Queries/
  │   ├── GetPointHistoryQuery.cs
  │   ├── GetPointBalanceQuery.cs
  │   ├── GetUserLevelProgressQuery.cs
  │   ├── GetAchievementsWithProgressQuery.cs
  │   ├── GetRecentAchievementsQuery.cs
  │   ├── GetStreaksSummaryQuery.cs
  │   ├── GetTodayMissionsQuery.cs
  │   ├── GetThisWeekMissionsQuery.cs
  │   ├── GetGamificationDashboardQuery.cs (meu dashboard)
  │   └── GetStudentGamificationDashboardQuery.cs (Coach/Admin)
  ├── Services/
  │   ├── IPointAwardService.cs       (Público - integração futura FinishWorkoutSession)
  │   ├── IAchievementEvaluator.cs    (Avalia regras AC001-10)
  │   ├── IStreakCalculator.cs        (Cálculo idempotente por histórico)
  │   └── IMissionProgressTracker.cs  (Atualiza progresso de missões)
  └── Validators/ (FluentValidation para todos Commands)
```

**Infrastructure (EF + Services):**
```
src/TreviaApp.Infrastructure/Persistence/Configurations/Gamification/
  ├── PointTransactionConfiguration.cs
  ├── UserLevelConfiguration.cs
  ├── AchievementDefinitionConfiguration.cs
  ├── UserAchievementConfiguration.cs
  ├── UserStreakConfiguration.cs
  ├── DailyMissionDefinitionConfiguration.cs
  ├── WeeklyMissionDefinitionConfiguration.cs
  ├── UserDailyMissionConfiguration.cs
  └── UserWeeklyMissionConfiguration.cs
src/TreviaApp.Infrastructure/Gamification/
  ├── PointAwardService.cs
  ├── AchievementEvaluator.cs
  ├── StreakCalculator.cs
  └── MissionProgressTracker.cs
src/TreviaApp.Infrastructure/Persistence/Seeder/
  └── GamificationSeeder.cs          (Seed: 10 AchievementDefinition + 5 MissionDefinition)
src/TreviaApp.Infrastructure/Persistence/Migrations/
  └── 20260803xxxx_AddGamificationModule.cs (+ .Designer.cs + ModelSnapshot)
```

**API:**
```
src/TreviaApp.Api/Controllers/
  └── GamificationController.cs       (~13 endpoints)
```

**Documentação:**
```
docs/backlog/sprint-11.md            (este arquivo — atualizado passo a passo)
```

---

### ARQUIVOS QUE SERÃO ALTERADOS

| Arquivo | Alteração |
|---|---|
| `src/TreviaApp.Application/Abstractions/Data/IApplicationDbContext.cs` | (mantém mesma interface; via `Set<T>()`, sem mudanças obrigatórias — apenas `ApplicationDbContext` concreto abaixo) |
| `src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs` | 8 novos DbSets: `PointTransactions`, `UserLevels`, `AchievementDefinitions`, `UserAchievements`, `UserStreaks`, `DailyMissionDefinitions`, `WeeklyMissionDefinitions`, `UserDailyMissions`, `UserWeeklyMissions` + 9 Global Query Filters `HasQueryFilter(!IsDeleted)` em entidades com soft-delete (PointTransaction não tem, é append-only; UserLevel não tem soft-delete) |
| `src/TreviaApp.Infrastructure/Persistence/Seeder/DatabaseSeeder.cs` | Adicionar chamada `GamificationSeeder.Seed()` (idempotente, Upsert por Code) |
| `src/TreviaApp.Shared/Constants/ErrorCodes.cs` | Adicionar códigos: `Gamification.AlreadyClaimed`, `Gamification.InvalidAdjustment`, `Gamification.MissionNotFound`, `Gamification.SessionAlreadyAwarded`, `Gamification.DailyCapExceeded` |
| `src/TreviaApp.Infrastructure/DependencyInjection/...` | `ServiceCollectionExtensions` — registrar `IPointAwardService`, `IAchievementEvaluator`, `IStreakCalculator`, `IMissionProgressTracker` como Scoped |
| `src/TreviaApp.Application/DependencyInjection/ServiceCollectionExtensions.cs` | Validators assembly Gamification escaneado pelo FluentValidation (mesmo padrão já existente `ApplyConfigurationsFromAssembly`, sem mudança se já usa assembly scan) |

---

### MIGRATIONS

1 migration nova: `AddGamificationModule` (9 tabelas):
- **PointTransactions**: PK Id, FK `UserId (AppUser ON DELETE CASCADE)`, `Amount int`, `Reason (PointReason enum string)`, `ReferenceType (string null)`, `ReferenceId (Guid null)`, `Description (varchar 500 null)`, `CreatedAt (timestamptz DEFAULT now())`. Índice `(UserId, CreatedAt DESC)` crítico history. Append-only (sem IsDeleted, sem UpdatedAt).
- **UserLevels**: PK Id, FK `UserId (UNIQUE ON DELETE CASCADE)`, `CurrentLevel int DEFAULT 1`, `CurrentXp bigint DEFAULT 0`, `TotalXpEarned bigint DEFAULT 0`. + `CreatedAt/UpdatedAt`.
- **AchievementDefinitions**: PK Id, `Code (varchar 50 UNIQUE)`, `Name (varchar 150)`, `Description (varchar 1000)`, `Icon (varchar 500 null)`, `PointsReward int DEFAULT 0`, `Category (AchievementCategory enum string)`, `CriteriaConfig (jsonb null)`, `IsActive (bool default true)`, Seed 10 linhas.
- **UserAchievements**: PK Id, FK `UserId`, FK `AchievementDefinitionId`, unique constraint `(UserId, AchievementDefinitionId)`, `UnlockedAt (timestamptz null)`, `Progress double DEFAULT 0`.
- **UserStreaks**: PK Id, FK `UserId UNIQUE`, `DailyCurrent int`, `DailyLongest int`, `DailyLastActiveAt (date null)`, `WeeklyCurrent int`, `WeeklyLongest int`, `WeekStartDate (date null)`.
- **DailyMissionDefinitions**: PK Id, `Code (varchar 50 UNIQUE)`, `Title (200)`, `Description (1000)`, `TargetValue int`, `Metric (MissionMetric enum string)`, `PointsReward int`, `XpReward int`, `IsActive bool DEFAULT true`. Seed 3.
- **WeeklyMissionDefinitions**: Mesma estrutura Daily. Seed 2.
- **UserDailyMissions**: PK Id, FK `UserId`, FK `MissionId`, `Date (date)`, `CurrentValue int DEFAULT 0`, `IsCompleted bool DEFAULT false`, `CompletedAt null`, `ClaimedAt null`. Índice único `(UserId, MissionId, Date)`.
- **UserWeeklyMissions**: Mesma estrutura + `WeekStart (date)` replacing `Date`. Índice único `(UserId, MissionId, WeekStart)`.

---

### ENDPOINTS

#### GamificationController → `api/gamification` | `[Authorize]` + `[EnableRateLimiting("AuthEndpoint")]`

| Método | Rota | Autorização | Descrição | US |
|---|---|---|---|---|
| GET | `/points/balance` | Qualquer autenticado (self) | Total pontos disponíveis | 1101 |
| GET | `/points/history?page=&pageSize=&reason=` | Qualquer autenticado (self) | Lista paginada transações | 1101 |
| POST | `/points/award/workout/{sessionId}` | Student (self) / Admin | Demo/Award pontos de sessão concluída | 1101 |
| POST | `/points/adjust` | IsAdmin | Ajuste manual +- pontos | 1101 |
| GET | `/progress` | Qualquer autenticado | Nível atual, XP, próxima barra | 1102 |
| GET | `/achievements` | Qualquer autenticado | Lista todas conquistas com progresso % | 1103 |
| GET | `/achievements/recent?top=5` | Qualquer autenticado | Recentes desbloqueadas | 1103 |
| GET | `/streaks` | Qualquer autenticado | Diário + semanal atual/recorde | 1104 |
| POST | `/streaks/recompute` | Student (self) | Recalcula retroativo por histórico | 1104 |
| GET | `/missions/today` | Qualquer autenticado | Missões diárias + progresso | 1105 |
| GET | `/missions/this-week` | Qualquer autenticado | Missões semanais + progresso | 1105 |
| POST | `/missions/{missionId}/claim?type=Daily\|Weekly&date=` | Student (self) | Reivindica recompensa concluída | 1105 |
| GET | `/dashboard` | Qualquer autenticado | Painel agregado perfil | 1106 |
| GET | `/dashboard/students/{studentId}` | Trainer+CanViewHistory/Admin | Painel do aluno | 1106, 1107 |

---

### TELAS

(Pendentes no Client — frontend fora do escopo desta entrega; apenas documentação do que a API suporta, igual Sprint 9/10)
1. **Tela Perfil Gamificado** — Barra XP animada, nível, total pontos, streak flame emoji
2. **Modal Conquistas** — grid com ícones, progresso cinza vs colorido, desbloqueadas recentes com "Novidade!"
3. **Tela Missões do Dia** — cards com barra de progresso, botão "Coletar recompensa"
4. **Timeline Pontos** (histórico) — lista transações com ícone por `PointReason`
5. **Painel Coach do Aluno** — vê gamificação do aluno (mesma UI)

---

### TESTES

(Pendentes em sprints 12+ — apenas planejamento nesta sprint)
- **Unitário:** `LevelCurve.XpRequired(level=1..10)` = valores fixos tabela.
- **Unitário:** `PointAntiExcessBehavior` → 2 WorkoutCompleted no mesmo dia → somente 1 pontua + 2º retorna warning.
- **Integração:** `AwardWorkoutPointsCommandHandler` → 4 efeitos atômicos: PointTransaction + Missões atualizadas + Streaks recalculado + Achievements avaliados + LevelUp detectado, tudo no mesmo `SaveChangesAsync`.
- **Arquitetura:** Domain → não referencia Infrastructure; Application não referencia API.

---

### RISCOS e MITIGAÇÕES

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Pontos em excesso recompensam overtraining (PROJECT_SPEC hard constraint) | Alta | Alto | Caps em `GamificationConstants`: `DailyWorkoutAwardCap = 1/dia`; `DailySetPointsCap = 30 pts`; comportamento explicíto em `PointAntiExcessBehavior` antes de persistir |
| Performance em `AchievementEvaluator.EvaluateAll` toda vez que pontuar | Média | Médio | Avaliação lazy: só regras cuja métrica mudou (ex: WorkoutCount mudou → só regras AC001/002 + streaks relacionadas), não todas 10 |
| Streak quebrado por feriado/doença (usuário insatisfeito) | Alta | Baixo | Fora do MVP; preparado endpoint `/streaks/recompute` manual + endpoint Admin `AdjustPoints` para casos excepcionais |
| Conflito de concorrência: Award e Claim simultâneos | Média | Médio | `Unique Constraint` em UserDaily/WeeklyMission; Reivindicação `ClaimedAt = now()` idempotente via Update concorrente-safe FirstOrDefault por tracking EF |
| Migration seed 10+5 definitions pode falhar em ambiente com dados antigos | Baixo | Médio | `GamificationSeeder.Seed()` usa Upsert por `Code` (IfNotExists → Insert), não Insert cru |
| Curva XP muito fácil ou muito difícil | Média | Média | `LevelCurve` classe parametrizada com valores constantes; fácil ajustar posteriormente sem migration (lógica é application-layer) |

---

## 🛠️ Implementação Passo a Passo (Atualizar conforme progresso)

### PASSO 1 — Domain Entities, Enums e Constants
- [x] `GamificationEnums.cs` (PointReason, AchievementCategory, MissionMetric, StreakType)
- [x] `GamificationConstants.cs` (10 AC códigos, 5 mission códigos, caps anti-excesso)
- [x] 9 Entidades de Domínio (PointTransaction, UserLevel, LevelCurve, AchievementDefinition, UserAchievement, UserStreak, Daily/Weekly MissionDefinition, UserDaily/WeeklyMission)
- [x] ErrorCodes.cs atualizados (Gamification.*)
- [x] Atualizar este arquivo → marcar PASSO 1 = [x]

### PASSO 2 — Contracts DTOs
- [x] Requests: AwardWorkoutPointsRequest, AdjustPointsRequest, ClaimMissionRequest
- [x] Responses em `GamificationResponses.cs` (10+ responses + Paginated)
- [x] Atualizar este arquivo → marcar PASSO 2 = [x]

### PASSO 3 — Application Layer (Services + Commands/Queries + Validators)
- [x] 4 Interfaces serviços (IPointAwardService, IAchievementEvaluator, IStreakCalculator, IMissionProgressTracker)
- [x] 4 Commands + Handlers + Validators
- [x] 10 Queries + Handlers
- [x] `PointAntiExcessBehavior` (regra caps diários — integrado no PointAwardService concreto)
- [x] Implementação concreta parcial Services (interfaces definidas em Application, concreto em Infrastructure)
- [x] Atualizar este arquivo → marcar PASSO 3 = [x]

### PASSO 4 — Infrastructure Layer (EF + Migration + Services concretos + Seeder + DI)
- [x] 9 EF Configuration classes (em GamificationConfigurations.cs)
- [x] 9 DbSets + Query Filters em `ApplicationDbContext`
- [x] Implementação serviços concretos: PointAwardService, AchievementEvaluator, StreakCalculator, MissionProgressTracker (em GamificationServices.cs)
- [x] `GamificationSeeder` (10 AchievementDef + 3 Daily + 2 Weekly MissionDef, Upsert)
- [x] DatabaseSeeder chama GamificationSeeder
- [x] ServiceCollectionExtensions (registrar services concretos Scoped)
- [x] Gerar migration `AddGamificationModule`
- [x] Atualizar este arquivo → marcar PASSO 4 = [x]

### PASSO 5 — Controllers (API Layer)
- [x] GamificationController.cs (22 endpoints combinados)
- [x] Atualizar este arquivo → marcar PASSO 5 = [x]

### PASSO 6 — Build + Validação
- [x] `dotnet build TreviaApp.slnx -c Debug` → 0 erros
- [x] Corrigir erros de compilação encontrados (WorkoutStatus.Finished → Completed, init-only → set, private set → métodos)
- [x] Atualizar este arquivo → marcar PASSO 6 = [x]

### PASSO 7 — Pós-Implementação (Relatório Final)
- [x] Preencher seção "PÓS-IMPLEMENTAÇÃO" abaixo com todos itens obrigatórios do PROJECT_SPEC.md
- [x] Atualizar este arquivo → marcar PASSO 7 = [x]

---

## ✅ PÓS-IMPLEMENTAÇÃO (Resumo, Arquivos, Endpoints disponíveis)

### RESUMO:

Implementação **completa** do Módulo de Gamificação (Sprint 11) da TreviaApp. Entregue **7 User Stories** (US-1101 a US-1107) cobrindo:
- **Pontos transacionais** (`PointTransaction`) com auditoria completa e caps anti-excesso de treino (1 treino/dia max pontos workout + 30 pts/dia séries).
- **Níveis e XP** com curva parametrizada (`LevelCurve`: `xpRequiredForLevel(L) = round(100*L^1.8 + 50*L)`) e bônus automático de LevelUp (Level*50 pontos).
- **10 Conquistas / Achievements** definições seedadas (AC001-AC010) + progresso por usuário e lógica de avaliação automática após AwardWorkout.
- **Streaks avançados** diário/semanal com cálculo retroativo idempotente via `/streaks/recompute` e bônus pontos em streaks de 7 e 30 dias.
- **5 Missões automáticas** (3 diárias + 2 semanais) com progresso atualizado por Workout e endpoint de claim idempotente (recompensa em pontos + XP).
- **Painel Dashboard agregado** (GET `/dashboard`) com barra XP, nível, streaks, conquistas próximas/recentes, missões do dia e últimas transações. Versão Coach/Admin para `/dashboard/students/{id}` com política `CanViewWorkoutHistory`.
- **Autorização** baseada em policies: `IsAdmin` para ajuste manual pontos e `CoachPermissions.CanViewWorkoutHistory` para dashboards de alunos.
- **Migration**: `AddGamificationModule` com 9 tabelas (PointTransactions, UserLevels, AchievementDefinitions, UserAchievements, UserStreaks, DailyMissionDefinitions, WeeklyMissionDefinitions, UserDailyMissions, UserWeeklyMissions) + índices, FK CASCADE e constraints UNIQUE corretas.
- **Build**: `dotnet build TreviaApp.slnx -c Debug` com **0 erros**.

### ARQUIVOS CRIADOS:

| Arquivo | Conteúdo |
|---|---|
| [GamificationEnums.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Shared/Enums/GamificationEnums.cs) | Enums: PointReason, AchievementCategory, MissionMetric, StreakType |
| [GamificationConstants.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Shared/Constants/GamificationConstants.cs) | 10 AchievementCodes (AC001-10), 5 MissionCodes (D1-3, W1-2), caps anti-excesso, valores XP |
| [PointTransaction.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/PointTransaction.cs) | Entidade append-only histórico transacional de pontos (UserId, Amount, Reason, Reference, Description, CreatedAt) |
| [UserLevel.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/UserLevel.cs) | Agregado: CurrentLevel, CurrentXp, TotalXpEarned, métodos AddXp/XpToNextLevel/ProgressPercentage |
| [LevelCurve.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/LevelCurve.cs) | Value Object: fórmula XP parametrizada, TotalXpForLevel, CalculateLevelFromTotalXp |
| [AchievementDefinition.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/AchievementDefinition.cs) | Definição seedável: Code (UNIQUE), Name, Description, Icon, PointsReward, Category (enum), CriteriaConfig JSON, IsActive |
| [UserAchievement.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/UserAchievement.cs) | Progresso por usuário: UserId+AchievementId UNIQUE, Progress% 0-100, UnlockedAt |
| [UserStreak.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/UserStreak.cs) | Streaks diário/semanal: DailyCurrent/Longest/LastActiveAt, WeeklyCurrent/Longest/WeekStart, métodos UpdateDaily/UpdateWeekly/Reset/SetDailyLongest/SetWeeklyLongest |
| [DailyMissionDefinition.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/DailyMissionDefinition.cs) | Missão diária seed: Code, Title, Description, TargetValue, Metric, PointsReward, XpReward, IsActive |
| [WeeklyMissionDefinition.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/WeeklyMissionDefinition.cs) | Missão semanal (mesma estrutura Daily) |
| [UserDailyMission.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/UserDailyMission.cs) | Progresso diário por usuário: (UserId, MissionId, Date) UNIQUE, CurrentValue, IsCompleted/CompletedAt, IsClaimed/ClaimedAt, IncrementProgress/ClaimReward |
| [UserWeeklyMission.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Domain/Gamification/UserWeeklyMission.cs) | Progresso semanal por usuário (WeekStart replacing Date) |
| [GamificationRequests.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Contracts/Gamification/Requests/GamificationRequests.cs) | DTOs Request: AwardWorkoutPointsRequest, AdjustPointsRequest, ClaimMissionRequest |
| [GamificationResponses.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Contracts/Gamification/Responses/GamificationResponses.cs) | 10+ DTOs Response: PointHistoryResponse, PointBalanceResponse, UserLevelProgressResponse, AchievementProgressResponse, StreaksSummaryResponse, UserMissionProgressResponse, GamificationDashboardResponse, AwardWorkoutPointsResultResponse, ClaimMissionResultResponse, RecomputeStreaksResultResponse + 3 Paginated wrappers |
| [GamificationUseCases.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Application/Gamification/GamificationUseCases.cs) | 4 Interfaces serviço (IPointAwardService, IAchievementEvaluator, IStreakCalculator, IMissionProgressTracker), 4 Commands + Validators + Handlers (AwardWorkoutPoints, AdjustPoints, ClaimMission, RecomputeStreaks), 10 Queries + Handlers |
| [GamificationConfigurations.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/Gamification/GamificationConfigurations.cs) | 9 EF EntityTypeConfiguration classes: índices (UserId+CreatedAt DESC), UNIQUE constraints, HasConversion<string> para enums, jsonb para CriteriaConfig, FK DeleteBehavior |
| [GamificationServices.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Gamification/GamificationServices.cs) | 4 implementações concretas Scoped: PointAwardService (caps anti-excesso integrados), AchievementEvaluator (avaliação AC001-10), StreakCalculator (recalculo retroativo por histórico), MissionProgressTracker (atualização diária/semanal por métrica) |
| [GamificationSeeder.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Seeder/GamificationSeeder.cs) | Seed Upsert por Code: 10 AchievementDefinition (AC001-10), 3 DailyMissionDefinition (D1-3), 2 WeeklyMissionDefinition (W1-2) |
| [GamificationController.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/GamificationController.cs) | 22 endpoints agrupados em Points / Progress / Achievements / Streaks / Missions / Dashboard com autorização correta |
| `Migrations/*_AddGamificationModule.cs` (+ Designer.cs + ModelSnapshot atualizado) | Migration completa 9 tabelas gamificação |

### ARQUIVOS ALTERADOS:

| Arquivo | Alteração |
|---|---|
| [ErrorCodes.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Shared/Constants/ErrorCodes.cs) | +8 códigos de erro Gamification: AlreadyClaimed, InvalidAdjustment, MissionNotFound, SessionAlreadyAwarded, DailyCapExceeded, AchievementNotFound, UserLevelNotFound |
| [ApplicationDbContext.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs) | +9 DbSets gamificação, +6 Query Filters soft-delete (AchievementDefinition, UserAchievement, DailyMissionDefinition, WeeklyMissionDefinition, UserDailyMission, UserWeeklyMission), using Gamification namespace |
| [DatabaseSeeder.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Seeder/DatabaseSeeder.cs) | Chamada `GamificationSeeder.SeedAsync(_db, ct)` no SeedAllAsync |
| [ServiceCollectionExtensions.cs](file:///C:/dev/github/trevia.app/src/TreviaApp.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs) | Registro Scoped explícito dos 4 serviços de gamificação (IPointAwardService, IAchievementEvaluator, IStreakCalculator, IMissionProgressTracker) |
| [sprint-11.md](file:///C:/dev/github/trevia.app/docs/backlog/sprint-11.md) | Progresso PASSO 1-7 marcados [x] e relatório final preenchido |

### ENDPOINTS DISPONÍVEIS:

`[Authorize] [ApiController] [Route("api/gamification")] [EnableRateLimiting("AuthEndpoint")]`

| # | Método | Rota | Roles/Policies | Descrição |
|---|---|---|---|---|
| 1 | GET | `/points/balance` | Todos autenticados | Saldo total pontos (total/hoje/semana/mês) |
| 2 | GET | `/points/balance/users/{userId}` | Trainer + Admin | Saldo pontos de aluno |
| 3 | GET | `/points/history?page=1&pageSize=20&reason=` | Todos autenticados | Histórico paginado transações (filtro por PointReason opcional) |
| 4 | GET | `/points/history/users/{userId}` | Trainer + Admin | Histórico pontos aluno |
| 5 | POST | `/points/award/workout/{sessionId}` | Student / Admin | Demo endpoint: atribui pontos de sessão concluída (anti-excesso caps aplicados) |
| 6 | POST | `/points/adjust?targetUserId=` | **Só Admin** | Ajuste manual +/- pontos com descrição obrigatória |
| 7 | GET | `/progress` | Todos autenticados | Nível atual, XP corrente, XP p/ próximo nível, % barra progresso |
| 8 | GET | `/progress/users/{userId}` | Trainer + Admin | Progresso nível aluno |
| 9 | GET | `/achievements?page=1&pageSize=50` | Todos autenticados | Todas conquistas com % progresso individual |
| 10 | GET | `/achievements/users/{userId}` | Trainer + Admin | Conquistas aluno |
| 11 | GET | `/achievements/recent?top=5` | Todos autenticados | Conquistas mais recentes desbloqueadas |
| 12 | GET | `/achievements/recent/users/{userId}` | Trainer + Admin | Recentes aluno |
| 13 | GET | `/streaks` | Todos autenticados | Resumo diário/semanal (atual/recorde + última data) |
| 14 | GET | `/streaks/users/{userId}` | Trainer + Admin | Streaks aluno |
| 15 | POST | `/streaks/recompute` | Student / Admin | Recalcula idempotentemente streaks por histórico WorkoutSession.CompletedAt |
| 16 | GET | `/missions/today?date=` | Todos autenticados | 3 missões do dia + progresso |
| 17 | GET | `/missions/today/users/{userId}?date=` | Trainer + Admin | Missões dia aluno |
| 18 | GET | `/missions/this-week?weekStart=` | Todos autenticados | 2 missões semanais + progresso |
| 19 | GET | `/missions/this-week/users/{userId}` | Trainer + Admin | Missões semanais aluno |
| 20 | POST | `/missions/{missionId}/claim?type=Daily\|Weekly&date=` | Student / Admin | Reivindica recompensa concluída (idempotente: lança AlreadyClaimed se ClaimedAt != null) |
| 21 | GET | `/dashboard` | Todos autenticados | **Painel completo**: nível + barra XP %, total pontos, streaks, próximas 3 conquistas, 3 conquistas recentes, missões hoje, últimas 5 transações |
| 22 | GET | `/dashboard/students/{studentId}` | Trainer + `CanViewWorkoutHistory` / Admin | Painel completo de aluno (mesma estrutura dashboard) |

**Exemplo Request — AwardWorkout:**
```http
POST /api/gamification/points/award/workout/3fa85f64-...
Authorization: Bearer <jwt>
Content-Type: application/json
{}
```
**Resposta esperada:**
```json
{
  "success": true,
  "pointsEarned": 67,
  "xpEarned": 134,
  "leveledUp": false,
  "newLevel": null,
  "unlockedAchievements": ["AC001","AC007"],
  "completedMissions": ["D1","D2"],
  "warning": null
}
```

### TELAS DISPONÍVEIS:
(Não aplicável — sprint somente API; telas Client PWA sprints separadas. A API suporta 5 telas conforme planejado: Perfil Gamificado, Modal Conquistas, Missões do Dia, Timeline Pontos, Painel Coach do Aluno.)

### COMO EXECUTAR:

```powershell
cd c:\dev\github\trevia.app

# 1. Atualizar banco com migration gamificação
dotnet ef database update -p src\TreviaApp.Infrastructure\TreviaApp.Infrastructure.csproj -s src\TreviaApp.Api\TreviaApp.Api.csproj

# 2. Rodar a API (Swagger abre automaticamente em dev)
dotnet run -c Debug --project src\TreviaApp.Api\TreviaApp.Api.csproj
# → Swagger: https://localhost:5001/swagger → seção /gamification com 22 endpoints

# 3. Seed inicial: ao rodar a primeira vez, DatabaseSeeder cria automaticamente
#    10 conquistas (AC001-10), 3 missões diárias (D1-3), 2 semanais (W1-2)
#    via Upsert por Code (idempotente).
```

### COMO TESTAR:

1. **Login Student** via `/api/auth/login` → obter JWT.
2. **Criar WorkoutSession e finalizar** (usar endpoints de WorkoutExecution existentes) → pegar `sessionId` do status `Completed`.
3. **POST `/api/gamification/points/award/workout/{sessionId}`** → recebe pontos, dispara streaks, achievements, missões e level-up. Repetir no mesmo dia deve retornar `warning` sobre cap diário.
4. **GET `/api/gamification/dashboard`** → validar painel agregado: nível, XP%, pontos totais, streaks, conquistas próximas, recentes, missões hoje, últimas transações.
5. **POST `/api/gamification/streaks/recompute`** → força recálculo streaks (útil após importação histórica).
6. **POST `/api/gamification/missions/{missionId}/claim?type=Daily`** → reivindica recompensa após completar missão (retorna AlreadyClaimed em double-claim).
7. **Login Admin** → testar `POST /points/adjust` (ajuste manual pontos) e `/dashboard/students/{studentId}`.

### MIGRATIONS:

- **Nome**: `AddGamificationModule` (1 migration, 9 tabelas)
- **Localização**: `src/TreviaApp.Infrastructure/Persistence/Migrations/*_AddGamificationModule.cs`
- **9 Tabelas criadas**:
  1. `PointTransactions` — append-only, FK AppUser CASCADE, índice `(UserId, CreatedAt DESC)`
  2. `UserLevels` — 1:1 com AppUser (UNIQUE), CASCADE
  3. `AchievementDefinitions` — UNIQUE(Code), `CriteriaConfig jsonb`
  4. `UserAchievements` — UNIQUE(UserId, AchievementDefinitionId)
  5. `UserStreaks` — 1:1 com AppUser (UNIQUE)
  6. `DailyMissionDefinitions` — UNIQUE(Code)
  7. `WeeklyMissionDefinitions` — UNIQUE(Code)
  8. `UserDailyMissions` — UNIQUE(UserId, MissionId, Date)
  9. `UserWeeklyMissions` — UNIQUE(UserId, MissionId, WeekStart)
- **Todos enums salvos como string via HasConversion<string>().**
- **Soft-delete via Global Query Filters** em 6 entidades (exclui PointTransactions, UserLevels que são append-only/1:1).
- **Valores padrão**: `CurrentLevel DEFAULT 1`, `CurrentXp DEFAULT 0`, `IsActive DEFAULT true`, `IsCompleted DEFAULT false`, etc.

### VARIÁVEIS DE AMBIENTE:

**Nenhuma nova** variável de ambiente necessária. Gamificação é implementada 100% no banco PostgreSQL existente e com DI automático.

### PENDÊNCIAS E MELHORIAS FUTURAS:

1. **Acoplamento automático**: Integrar `IPointAwardService.AwardWorkoutPointsAsync(sessionId)` diretamente dentro de `FinishWorkoutSessionCommandHandler` (Sprint 7), atualmente requerendo chamada explícita ao endpoint `/award/workout/{sessionId}`.
2. **Ranking público**: PROIBIDO pelo PROJECT_SPEC (classificado Fora do MVP).
3. **Tempo real**: Hub SignalR para eventos `LevelUp`, `AchievementUnlocked`, `MissionCompleted` push ao Client PWA.
4. **Loja de items / selos cosméticos**: Trocar pontos por recompensas visuais (preparado apenas `AchievementDefinition.Icon`).
5. **Testes unitários/integração**: Planejados para Sprint 12 (LevelCurve valores fixos tabelados, PointAntiExcess caps, AwardWorkout 4 side-effects, ArchitectureTests).
6. **Gamificação em Notificações**: Criar NotificationType novos (LevelUp, AchievementUnlocked, MissionReadyToClaim) no módulo Notifications.

### PRÓXIMA ETAPA RECOMENDADA:

**Sprint 12 — PWA, Offline, Segurança, Testes e Beta** — alinhado a [ROADMAP.md](file:///C:/dev/github/trevia.app/docs/backlog/ROADMAP.md#L16-L17) (linha 17) e PROJECT_SPEC.md ordem desenvolvimento #11 e #12.
