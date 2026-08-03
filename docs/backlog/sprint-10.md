# Sprint 10 — Feedbacks e Notificações

> **Data de início:** 2026-08-03
> **Base:** ROADMAP.md linha 15 + PROJECT_SPEC.md seções "Professor e Aluno" e item 9 da ordem ("Feedback")

---

## 📋 Pré-Implementação (Escopo para Aprovação)

### ETAPA
Sprint 10 — Módulo de Feedbacks e Notificações

### OBJETIVO
Permitir que Professores/Treinadores com vínculo ativo e permissão `CanSendFeedback` (já existente no enum `CoachPermissions`) enviem comentários orientativos em três níveis de granularidade de uma sessão de treino executada: **Sessão Completa**, **Exercício Específico** ou **Série Individual**. Além disso, entregar um sistema básico de Notificações internas (push via API, não websocket) para avisar o aluno quando receber feedback, o professor quando um aluno concluir um treino e outros eventos futuros.

### ESCOPO

#### ✅ Entra no escopo

1. **US-1001 — Feedback na Sessão de Treino (nível sessão)**
   - Coach com `CanSendFeedback` + vínculo ativo cria um comentário em um `WorkoutSession` de seu aluno.
   - Campos: texto (obrigatório, máx 4000 caracteres), sentimento do feedback (`FeedbackTone`: Incentivo / Construtivo / Correção técnica / Neutro — enum), flag pública (padrão true — indica se aparece no dashboard do aluno).
   - Criar, editar (só autor), listar por sessão, visualizar detalhe, soft-delete (só autor ou admin).
   - Aluno vê os feedbacks enviados em sua sessão (lista por `sessionId`).

2. **US-1002 — Feedback em Exercício (nível WorkoutExercise)**
   - Opcionalmente associado a um `WorkoutExercise` dentro da sessão.
   - Mesma estrutura de US-1001 mas referenciando `WorkoutExerciseId`.
   - Relação 1:N: um exercício executado pode ter múltiplos feedbacks ao longo do tempo (ex: revisão posterior).
   - Resposta do aluno opcional (`StudentResponseText`, `StudentRespondedAt`).

3. **US-1003 — Feedback em Série Individual (nível WorkoutSet)**
   - Associado a um `WorkoutSet` específico (para comentar técnica numa série em particular).
   - Herda campos base + referência a `WorkoutSetId`.
   - Suporta referência a vídeo/mídia futura via `MediaReferenceUrl` string nullable (fora do MVP, campo preparatório).

4. **US-1004 — Listagem de Feedbacks recebidos (Aluno) e enviados (Coach)**
   - Aluno: GET `/api/feedbacks/my?from=&to=&sessionId=&read=` — lista seus feedbacks recebidos, paginação.
   - Coach: GET `/api/feedbacks/students/{studentId}?from=&to=&sessionId=` — lista feedbacks que ele enviou p/ um aluno.
   - Marcação de "lido" quando aluno abre o feedback (`ReadAt = DateTimeOffset.UtcNow`).

5. **US-1005 — Notificações Internas (Banco de Dados)**
   - Entidade `Notification`: `UserId` (destinatário), `Type` (enum: `FeedbackReceived`, `WorkoutCompleted`, `PlanAssigned`, `LinkAccepted`, `LinkRevoked`, `CoachMessage`), `Title`, `Message`, `ReferenceType` (Session/Exercise/Set/Plan/Link), `ReferenceId`, `IsRead`, `ReadAt`, `CreatedAt`, `IsDeleted`.
   - Endpoints: list minhas notificações (contador de não lidas separado), marcar 1 como lida, marcar todas como lidas, apagar (soft).
   - Hook (side-effect no handler): ao criar um Feedback (qualquer nível), disparar automaticamente uma `Notification` para o aluno do tipo `FeedbackReceived`, com referência a Sessão.

6. **US-1006 — Autorização e resource-based**
   - Reutiliza `CoachPermissions.CanSendFeedback` (existe no enum desde Sprint 6).
   - Professor só pode enviar feedback para alunos com vínculo ativo (`CoachStudentLink.IsActive = true`) e permissão concedida.
   - Professor só pode editar/excluir feedbacks de sua autoria (`CreatedByCoachId == currentUser.Id`).
   - Aluno só vê feedbacks direcionados a ele (`StudentId == currentUser.Id`).
   - Admin/GymManager tem acesso irrestrito (auditoria).

7. **US-1007 — Contador e badge de não-lidas**
   - `GET /api/notifications/unread-count` — retorna `{ unreadCount, lastNotificationAt }` — barra top do app.
   - `GET /api/notifications?pageSize=&page=&onlyUnread=true|false` — lista paginada.
   - `PUT /api/notifications/{id}/read` — marcar 1.
   - `PUT /api/notifications/read-all` — marcar todas.

#### ❌ Fora do escopo
- SignalR / WebSockets / push realtime — preparado mas não implementado (campos existem).
- Upload de mídia em feedback (campo `MediaReferenceUrl` preparatório, mas sem endpoint de upload).
- Chat completo (PROJECT_SPEC.md já classifica como fora do MVP).
- FCM / Push notifications mobile (somente notificações persistidas em DB).

---

### ARQUIVOS QUE SERÃO CRIADOS

**Domínio (Domain):**
```
src/TreviaApp.Domain/WorkoutExecution/Feedback/
  ├── WorkoutFeedback.cs              (agregado raiz, referência a Sessão)
  ├── ExerciseFeedback.cs             (entidade filho, referência a WorkoutExercise)
  └── SetFeedback.cs                  (entidade filho, referência a WorkoutSet)
src/TreviaApp.Domain/Notifications/
  └── Notification.cs                 (agregado raiz, usuário destinatário)
src/TreviaApp.Shared/Enums/
  └── FeedbackAndNotificationEnums.cs (FeedbackTone, NotificationType, NotificationReferenceType)
```

**Contracts:**
```
src/TreviaApp.Contracts/Feedbacks/
  ├── Requests/
  │   ├── CreateWorkoutFeedbackRequest.cs
  │   ├── CreateExerciseFeedbackRequest.cs
  │   ├── CreateSetFeedbackRequest.cs
  │   ├── UpdateFeedbackRequest.cs
  │   └── MarkFeedbackReadRequest.cs
  └── Responses/
      └── FeedbackResponses.cs        (WorkoutFeedbackResponse, ExerciseFeedbackResponse, SetFeedbackResponse, Paged<T>)
src/TreviaApp.Contracts/Notifications/
  └── Responses/
      └── NotificationResponses.cs    (NotificationResponse, UnreadCountResponse)
```

**Application:**
```
src/TreviaApp.Application/Feedbacks/
  ├── Commands/
  │   ├── CreateWorkoutFeedbackCommand.cs  (+ Handler + Validator)
  │   ├── CreateExerciseFeedbackCommand.cs (+ Handler + Validator)
  │   ├── CreateSetFeedbackCommand.cs      (+ Handler + Validator)
  │   ├── UpdateFeedbackCommand.cs         (+ Handler)
  │   ├── DeleteFeedbackCommand.cs         (+ Handler)
  │   └── MarkFeedbackReadCommand.cs       (+ Handler)
  └── Queries/
      ├── GetFeedbacksBySessionQuery.cs    (+ Handler)
      ├── GetMyFeedbacksQuery.cs           (+ Handler)
      └── GetStudentFeedbacksQuery.cs      (+ Handler)
src/TreviaApp.Application/Notifications/
  ├── Commands/
  │   ├── MarkNotificationReadCommand.cs   (+ Handler)
  │   ├── MarkAllNotificationsReadCommand.cs (+ Handler)
  │   └── DeleteNotificationCommand.cs     (+ Handler)
  └── Queries/
      ├── GetMyNotificationsQuery.cs       (+ Handler)
      └── GetUnreadCountQuery.cs           (+ Handler)
```

**Infrastructure (EF):**
```
src/TreviaApp.Infrastructure/Persistence/Configurations/
  ├── WorkoutFeedbackConfiguration.cs
  ├── ExerciseFeedbackConfiguration.cs
  ├── SetFeedbackConfiguration.cs
  └── NotificationConfiguration.cs
src/TreviaApp.Infrastructure/Persistence/Migrations/
  └── YYYYMMDDHHMMSS_AddFeedbacksAndNotifications.cs   (+ .Designer.cs)
```

**API:**
```
src/TreviaApp.Api/Controllers/
  ├── FeedbacksController.cs        (≈9 endpoints)
  └── NotificationsController.cs    (≈6 endpoints)
```

**Documentação:**
```
docs/backlog/sprint-10.md            (este arquivo — atualizado passo a passo)
```

### ARQUIVOS QUE SERÃO ALTERADOS

| Arquivo | Alteração |
|---|---|
| `src/TreviaApp.Application/Abstractions/Data/IApplicationDbContext.cs` | Adicionar `DbSet<WorkoutFeedback>`, `DbSet<ExerciseFeedback>`, `DbSet<SetFeedback>`, `DbSet<Notification>` |
| `src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs` | Mesmos 4 DbSets + 4 Global Query Filters `HasQueryFilter(x => !x.IsDeleted)` |
| `src/TreviaApp.Shared/Constants/ErrorCodes.cs` | Adicionar códigos: `Feedback.Forbidden`, `Feedback.NotFound`, `Feedback.InvalidSession`, `Feedback.TextTooLong`, `Notification.NotFound` |

### MIGRATIONS
1 migration nova: `AddFeedbacksAndNotifications`
- Tabela `WorkoutFeedbacks`: PK Id, FKs (CoachId, StudentId, WorkoutSessionId), Text (varchar 4000), Tone (enum int), IsPublic (bool), ReadAt (timestamptz null), CreatedAt/UpdatedAt/IsDeleted
  - Índices: (StudentId, CreatedAt DESC), (CoachId, StudentId), (WorkoutSessionId)
  - FKs: `OnDelete(Restrict)` p/ CoachId e StudentId; `OnDelete(Cascade)` p/ WorkoutSessionId (quando sessão é deletada via soft delete, o filter já resolve — hard delete remove feedbacks em cascata)

- Tabela `ExerciseFeedbacks`: PK Id, FKs (WorkoutFeedbackId opcional? ou CoachId/StudentId direto + WorkoutExerciseId) → opção **direta**: CoachId, StudentId, WorkoutExerciseId, WorkoutSessionId (redundante para busca rápida), Text, Tone, IsPublic, StudentResponseText (varchar 4000 null), StudentRespondedAt (timestamptz null), ReadAt, CreatedAt/UpdatedAt/IsDeleted

- Tabela `SetFeedbacks`: PK Id, FKs CoachId, StudentId, WorkoutSessionId, WorkoutExerciseId, WorkoutSetId, Text, Tone, MediaReferenceUrl (varchar 2048 null), ReadAt, CreatedAt/UpdatedAt/IsDeleted

- Tabela `Notifications`: PK Id, FK UserId (AppUser), Type (enum int), Title (varchar 200), Message (varchar 1000), ReferenceType (enum int null), ReferenceId (uuid null), IsRead (bool, default false), ReadAt (timestamptz null), CreatedAt/UpdatedAt/IsDeleted
  - Índice composto: (UserId, IsRead, CreatedAt DESC) — critical for performance da badge de não-lidas.

### ENDPOINTS

#### FeedbacksController → `api/feedbacks` | `[Authorize]` + `[EnableRateLimiting("AuthEndpoint")]`

| Método | Rota | Roles/Autorização | Descrição |
|---|---|---|---|
| POST | `/workout-sessions/{sessionId}` | Trainer/GymManager/Admin (com CanSendFeedback no link) | US-1001: Criar feedback em sessão |
| POST | `/workout-exercises/{exerciseId}` | Trainer/... | US-1002: Criar feedback em exercício |
| POST | `/workout-sets/{setId}` | Trainer/... | US-1003: Criar feedback em série |
| PUT | `/{feedbackId}` | Autor original ou Admin | Editar texto/tone de feedback (qualquer tipo) |
| DELETE | `/{feedbackId}` | Autor original ou Admin | Soft-delete feedback |
| GET | `/workout-sessions/{sessionId}` | Estudante (dono) / Trainer (com permissão) / Admin | Listar feedbacks (workout + exercise + set) de uma sessão |
| PUT | `/{feedbackId}/read` | Estudante dono | US-1004: Marcar feedback como lido |
| GET | `/my` | Student | Lista paginada dos feedbacks que eu recebi |
| GET | `/students/{studentId}` | Trainer/Admin | Lista paginada dos feedbacks que enviei p/ um aluno (ou admin vê todos) |

#### NotificationsController → `api/notifications` | `[Authorize]`

| Método | Rota | Descrição |
|---|---|---|
| GET | `/unread-count` | US-1007: Retorna `{ unreadCount, lastNotificationAt }` |
| GET | `?page=1&pageSize=50&onlyUnread=false` | Lista paginada minhas notificações |
| PUT | `/{id}/read` | Marca 1 notificação como lida |
| PUT | `/read-all` | Batch: marca todas minhas notificações como lidas |
| DELETE | `/{id}` | Soft-delete de 1 notificação |
| GET | `/{id}` | Detalhe de 1 notificação (marca lida automaticamente?) |

### TELAS
(Pendentes no Client — frontend fora do escopo desta entrega; apenas documentação do que a API suporta)
1. **Modal de feedback (Coach)** — Botão "Comentar" em cada Sessão / Exercício / Série no histórico do aluno
2. **Painel "Feedbacks recebidos" (Aluno)** — lista + badge não lido
3. **Central de notificações (Ambos)** — dropdown top com lista, contador, marcar tudo lido
4. **Linha do tempo da sessão** — feedbacks inline embaixo do item correspondente

### TESTES
(Pendentes em sprints 12+; apenas planejamento nesta sprint)
- **Unitários:** Handler de autorização `CanSendFeedback` para diferentes combinações de vínculo (ativo/inativo, permissão concedida/retirada).
- **Integração:** Fixture de Coach ativo → Post feedback → Notification criada automaticamente (verificada via query).
- **Integração:** Aluno tenta editar feedback de outro → deve retornar 403 Forbidden.
- **Arquitetura:** Verify Domain não referencia Infrastructure, Application não referencia API.

### RISCOS e MITIGAÇÕES

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Performance em `Notification.unread_count` com milhões de linhas | Média | Média | Índice composto `(UserId, IsRead, CreatedAt)` + `LIMIT 1` para o `LastNotificationAt` |
| Orfanato: Sessão/Exercício/Série deletada via hard-delete com feedbacks vinculados | Baixa | Baixo | Todas FKs → `OnDelete(Cascade)` no hard-delete, e soft-delete já filtra o feedback via `HasQueryFilter` e JOIN p/ WorkoutSession.IsDeleted = false |
| Coach envia feedback para aluno sem permissão `CanSendFeedback` | Baixo | Alto | Dupla checagem: `[Authorize(Roles = Trainer)]` no nível do controlador **e** handler consulta `CoachStudentLink` com bitwise flag `(link.Permissions & CoachPermissions.CanSendFeedback) != 0 && link.IsActive` antes de persistir |
| Texto muito longo em feedback | Baixo | Baixo | FluentValidation: `MaximumLength(4000)` + DB constraint `varchar(4000)` |
| Flood de feedbacks / rate limit ausente | Média | Baixo | Reuso `[EnableRateLimiting("AuthEndpoint")]` já existente em todos endpoints de gravação |
| Notification duplicada: side-effect é disparado 2x em retry (transacional) | Baixo | Baixo | A criação do Feedback + Notification é **atômica** no mesmo `SaveChangesAsync` (ambos no mesmo handler de command, mesma transaction/scope). |

---

## 🛠️ Implementação Passo a Passo (Atualizar conforme progresso)

### PASSO 1 — Domain Entities e Enums ✅ (2026-08-03)
- [x] Criação de `FeedbackTone`, `NotificationType`, `NotificationReferenceType`, `FeedbackLevel` em [FeedbackAndNotificationEnums.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Shared/Enums/FeedbackAndNotificationEnums.cs)
- [x] Criação de [WorkoutFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/WorkoutFeedback.cs), [ExerciseFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/ExerciseFeedback.cs), [SetFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/SetFeedback.cs) e [Notification.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/Notifications/Notification.cs) — AggregateRoots com factory constructors e métodos de domínio (UpdateContent, MarkAsRead, SetStudentResponse).
- [x] Atualizados [ErrorCodes.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Shared/Constants/ErrorCodes.cs#L259-L290) com 7 novas constantes (Feedback.*/Notification.*)
- [x] Atualizar este arquivo → marcar PASSO 1 = [x]

### PASSO 2 — Contracts DTOs ✅ (2026-08-03)
- [x] Requests de criação/edição/resposta em [FeedbackRequests.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Feedbacks/Requests/FeedbackRequests.cs) (5 records: CreateWorkout/Exercise/Set, Update, RespondExercise)
- [x] Responses de feedbacks em [FeedbackResponses.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Feedbacks/Responses/FeedbackResponses.cs): 3 tipos level (Workout/Exercise/Set) + FeedbacksBySessionBundle + UnifiedFeedbackItem
- [x] Responses de notificações em [NotificationResponses.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Notifications/Responses/NotificationResponses.cs): NotificationResponse + UnreadCountResponse + MarkManyResultResponse
- [x] Reutilização de `PaginatedRequest`/`PaginatedResponse` já existentes em [Contracts/Common](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Common/)
- [x] Atualizar este arquivo → marcar PASSO 2 = [x]

### PASSO 3 — Application Layer (Commands/Queries + Handlers + Validators) ✅ (2026-08-03)
- [x] 3 Create commands em [FeedbackUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Feedbacks/FeedbackUseCases.cs): `CreateWorkoutFeedbackCommand`, `CreateExerciseFeedbackCommand`, `CreateSetFeedbackCommand` + 5 AbstractValidator<> FluentValidation (Create/Update/Set/Respond)
- [x] Update / Delete / MarkRead / RespondExercise commands com handlers polimórficos baseados em `FeedbackLevel`
- [x] 3 Queries: `GetFeedbacksBySessionQuery` (retorna `FeedbacksBySessionBundleResponse` com as 3 listas), `GetMyFeedbacksQuery` (paginada, filtros Level/Unread/Session), `GetStudentFeedbacksQuery` (paginada Coach)
- [x] Notifications em [NotificationUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Notifications/NotificationUseCases.cs): Commands MarkOneRead/MarkAllRead/Delete + Queries GetMyPaginated/GetById/GetUnreadCount + Validators
- [x] **Side effect atômico:** em cada Create*FeedbackCommandHandler cria `Notification` tipo `FeedbackReceived` no mesmo `SaveChangesAsync` (ambos inseridos transacionalmente)
- [x] `FeedbackAuthHelpers.EnsureCoachCanSendFeedbackAsync` + bitwise flag `CoachPermissions.CanSendFeedback` (existente, 1<<5 = 32) + `link.IsActive`
- [x] `FeedbackQueryBuilders.UnifiedFeedbacks()` concatena 3 tabelas (Session/Exercise/Set) via LINQ Concat e Join para CoachName/StudentName/SessionName/ExerciseName
- [x] Atualizar este arquivo → marcar PASSO 3 = [x]

### PASSO 4 — Infrastructure Layer (EF + Migration)
- [ ] Adicionar 4 novos DbSets em `IApplicationDbContext` e `ApplicationDbContext`
- [ ] 4 Configuration classes (índices, FK behaviors, max lengths, default values)
- [ ] Global Query Filters nas 4 entidades
- [ ] Gerar migration `AddFeedbacksAndNotifications`
- [ ] Aplicar migration local / buildar scripts
- [ ] Atualizar este arquivo → marcar PASSO 4 = [x]

### PASSO 5 — Controllers (API Layer)
- [ ] FeedbacksController.cs (9 endpoints)
- [ ] NotificationsController.cs (6 endpoints)
- [ ] Atualizar este arquivo → marcar PASSO 5 = [x]

### PASSO 6 — Build + Validação
- [ ] `dotnet build TreviaApp.slnx -c Debug` → 0 erros, 0 warnings
- [ ] `dotnet ef migrations script --idempotent -o verify.sql -p TreviaApp.Infrastructure` (opcional, gerar script de conferência)
- [ ] Atualizar este arquivo → marcar PASSO 6 = [x]

### PASSO 7 — Pós-Implementação (Relatório Final)
- [ ] Preencher seção "PÓS-IMPLEMENTAÇÃO" abaixo com todos itens obrigatórios do PROJECT_SPEC.md
- [ ] Atualizar este arquivo → marcar PASSO 7 = [x]

---

## ✅ PÓS-IMPLEMENTAÇÃO (Resumo, Arquivos, Endpoints disponíveis)

*(Preencher após conclusão do PASSO 6)*

### RESUMO:

*(Aguardando conclusão)*

### ARQUIVOS CRIADOS:

| Arquivo | Conteúdo |
|---|---|
| *(Preencher no final)* | |

### ARQUIVOS ALTERADOS:

| Arquivo | Alteração |
|---|---|
| *(Preencher no final)* | |

### ENDPOINTS DISPONÍVEIS:

*(Preencher no final com rotas finais, exemplo de request JSON e resposta esperada)*

### TELAS DISPONÍVEIS:

(Não aplicável — sprint somente API; telas client são sprints separadas)

### COMO EXECUTAR:

```powershell
cd c:\dev\github\trevia.app
dotnet ef database update -p src\TreviaApp.Infrastructure\TreviaApp.Infrastructure.csproj -s src\TreviaApp.Api\TreviaApp.Api.csproj
dotnet run -c Debug --project src\TreviaApp.Api\TreviaApp.Api.csproj
# Swagger: https://localhost:5001/swagger → coleção Feedbacks e Notifications
```

### COMO TESTAR:

1. **Autenticar como Trainer**: `POST /api/auth/login` → pegar token
2. **Garantir vínculo com CanSendFeedback**: `PUT /api/coach/links/{studentId}/permissions` com bitwise contendo `32` (= CanSendFeedback, 1<<5)
3. **Executar treino de teste (ou usar existente)** → pegar `workoutSessionId`
4. **Criar feedback**: `POST /api/feedbacks/workout-sessions/{sessionId}`
   ```json
   { "text": "Excelente sessão, amanhã aumente 2,5kg no supino!", "tone": 1 }
   ```
5. **Verificar badge notificação como aluno**: `GET /api/notifications/unread-count` → `{ unreadCount: 1 }`
6. **Listar feedbacks recebidos**: `GET /api/feedbacks/my` → retorna item criado

### MIGRATIONS:

- Nome: `AddFeedbacksAndNotifications` (1 migration, 4 tabelas)

### VARIÁVEIS DE AMBIENTE:

Nenhuma nova — usa as mesmas já existentes (ConnectionStrings, Jwt etc.)

### PENDÊNCIAS E MELHORIAS FUTURAS:

1. **TESTES:** Unit + Integration não implementados nesta sprint (reserva para a Sprint 12).
2. **SignalR / Realtime:** Endpoints HTTP prontos; adicionar hub para notificações push em tempo real.
3. **Push mobile (FCM/APNs):** Serviço de integração externa — fora do MVP.
4. **Mídia em Feedback:** Campo `MediaReferenceUrl` preparado; conectar com `IFileStorageService` na Sprint 11/12.
5. **Resposta do Aluno em feedback:** Campo `StudentResponseText` existente; adicionar endpoint `POST /api/feedbacks/{id}/respond` na Sprint 11 caso necessário.
6. **Batch de Feedback:** Atualmente 1 a 1; futuro: endpoint `POST /api/feedbacks/bulk` para comentar múltiplas séries em lote.

### PRÓXIMA ETAPA RECOMENDADA:

**Sprint 11 — Gamificação: Pontos, Níveis, Conquistas e Sequências (Streaks avançadas)** — alinhado a ROADMAP.md linha 16 e PROJECT_SPEC.md item 10. Histórias de referência:

- **US-1101** Transação de pontos por ações (concluir treino, completar série, ler feedback, 3 dias consecutivos etc.)
- **US-1102** Níveis e XP com curva parametrizada
- **US-1103** Conquistas/achievements (primeiro treino, 10 treinos, primeira semana completa, record pessoal etc.)
- **US-1104** Streaks integrados com gamificação (streak 7 dias = pontos bonus, 30 dias = conquista)
- **US-1105** Missões diárias/semanais automáticas + pontuação
- **US-1106** Tela de perfil com nível, XP bar, conquistas recentes
