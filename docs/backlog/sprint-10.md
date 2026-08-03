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

### PASSO 4 — Infrastructure Layer (EF + Migration) ✅ (2026-08-03)
- [x] Adicionar 4 novos DbSets em `IApplicationDbContext` e `ApplicationDbContext` (pré-existente: [ApplicationDbContext.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs#L44-L47))
- [x] 4 Configuration classes (índices, FK behaviors, max lengths, default values): [WorkoutFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/WorkoutFeedbackConfiguration.cs), [ExerciseFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/ExerciseFeedbackConfiguration.cs), [SetFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/SetFeedbackConfiguration.cs), [NotificationConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs)
- [x] Global Query Filters nas 4 entidades (pré-existente: [ApplicationDbContext.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs#L96-L99))
- [x] Gerar migration `AddFeedbacksAndNotifications`: [20260803124451_AddFeedbacksAndNotifications.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Migrations/20260803124451_AddFeedbacksAndNotifications.cs)
- [x] Correções de build: `PaginatedResponse<T>` inicializador de objeto (não construtor 4-args) + `WorkoutSet.SetNumber` (não `OrderNumber`) em [FeedbackUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Feedbacks/FeedbackUseCases.cs) e [NotificationUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Notifications/NotificationUseCases.cs)
- [x] Atualizar este arquivo → marcar PASSO 4 = [x]

### PASSO 5 — Controllers (API Layer) ✅ (2026-08-03)
- [x] FeedbacksController.cs (9 endpoints + 1 extra: responder exercise feedback): [FeedbacksController.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/FeedbacksController.cs)
- [x] NotificationsController.cs (6 endpoints: unread-count, list paginated, detail, mark-1-read, mark-all-read, delete): [NotificationsController.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/NotificationsController.cs)
- [x] Atualizar este arquivo → marcar PASSO 5 = [x]

### PASSO 6 — Build + Validação ✅ (2026-08-03)
- [x] `dotnet build TreviaApp.slnx -c Debug` → 0 erros, 1 warning (CS0618 TestContainers obsoleto em IntegrationTests — não relacionado à sprint 10)
- [x] Correções pós-build identificadas e aplicadas:
  - Correção construtor `PaginatedResponse<T>` → inicializador de objeto com `Items`, `TotalCount`, `PageIndex`, `PageSize`, `HasNextPage`
  - Correção `WorkoutSet.OrderNumber` → `WorkoutSet.SetNumber` (3 ocorrências: CreateSetHandler, QueryBuilder Concat, GetBySession)
- [x] Atualizar este arquivo → marcar PASSO 6 = [x]

### PASSO 7 — Pós-Implementação (Relatório Final) ✅ (2026-08-03)
- [x] Preencher seção "PÓS-IMPLEMENTAÇÃO" abaixo com todos itens obrigatórios do PROJECT_SPEC.md
- [x] Atualizar este arquivo → marcar PASSO 7 = [x]

---

## ✅ PÓS-IMPLEMENTAÇÃO (Resumo, Arquivos, Endpoints disponíveis)

*(Concluído após PASSO 6 em 2026-08-03)*

### RESUMO:

Sprint 10 entregue com **Módulo de Feedbacks e Notificações** completo na API, cobrindo as 7 User Stories planejadas (US-1001 a US-1007):

- **US-1001 / 1002 / 1003 (Feedback em 3 níveis)**: Criação de comentários orientativos em `WorkoutSession` (sessão completa), `WorkoutExercise` (exercício específico) e `WorkoutSet` (série individual) com `FeedbackTone` (Incentivo/Construtivo/Correção/Neutro), flag pública, texto até 4000 chars, e campo preparatório `MediaReferenceUrl` para vídeos futuros em nível de série.
- **US-1004 (Listagens)**: Aluno lista seus feedbacks recebidos paginados (`/my`); Coach lista feedbacks enviados para um aluno específico (`/students/{studentId}`). Suporte a filtros por `sessionId`, `onlyUnread`, `Level`.
- **US-1005 (Notificações + Hook)**: Entidade `Notification` persistida em banco com 6 tipos (`FeedbackReceived`, `WorkoutCompleted`, `PlanAssigned`, `LinkAccepted`, `LinkRevoked`, `CoachMessage`). **Side-effect atômico**: cada `Create*FeedbackCommandHandler` insere o feedback E a notificação no mesmo `SaveChangesAsync`, garantindo consistência transacional.
- **US-1006 (Autorização resource-based)**: Reuso do bitwise flag `CoachPermissions.CanSendFeedback (1<<5 = 32)` + checagem `CoachStudentLink.IsActive` via `FeedbackAuthHelpers.EnsureCoachCanSendFeedbackAsync`. Edição/exclusão restritas ao autor original (`CreatedByCoachId == currentUser`) ou Admin. Aluno só visualiza feedbacks direcionados a ele.
- **US-1007 (Badge não-lidas)**: `GET /api/notifications/unread-count` com índice composto otimizado `(UserId, IsRead, CreatedAt DESC)`; endpoints de marcar 1 como lida, marcar todas, listar paginada, detalhe, excluir.

### ARQUIVOS CRIADOS:

| Arquivo | Conteúdo |
|---|---|
| [FeedbackAndNotificationEnums.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Shared/Enums/FeedbackAndNotificationEnums.cs) | 4 enums: `FeedbackTone`, `NotificationType`, `NotificationReferenceType`, `FeedbackLevel` |
| [WorkoutFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/WorkoutFeedback.cs) | Agregado raiz — feedback de nível de sessão (US-1001) |
| [ExerciseFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/ExerciseFeedback.cs) | Agregado raiz — feedback de nível exercício com `StudentResponseText/StudentRespondedAt` (US-1002) |
| [SetFeedback.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/WorkoutExecution/Feedback/SetFeedback.cs) | Agregado raiz — feedback de nível série com `MediaReferenceUrl` preparatório (US-1003) |
| [Notification.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Domain/Notifications/Notification.cs) | Agregado raiz — notificação interna persistida (US-1005/1007) |
| [FeedbackRequests.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Feedbacks/Requests/FeedbackRequests.cs) | 5 records: `CreateWorkout/Exercise/Set`, `Update`, `RespondToExercise` |
| [FeedbackResponses.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Feedbacks/Responses/FeedbackResponses.cs) | `Workout/Exercise/SetFeedbackResponse`, `FeedbacksBySessionBundleResponse`, `UnifiedFeedbackItemResponse` |
| [NotificationResponses.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Notifications/Responses/NotificationResponses.cs) | `NotificationResponse`, `UnreadCountResponse`, `MarkManyResultResponse` |
| [FeedbackUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Feedbacks/FeedbackUseCases.cs) | 7 Commands + 5 Validators + 3 Queries + 10 Handlers + helpers de autorização (`FeedbackAuthHelpers`) e build de query unificada (`FeedbackQueryBuilders.UnifiedFeedbacks`) |
| [NotificationUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Notifications/NotificationUseCases.cs) | 3 Commands + 2 Validators + 3 Queries + 6 Handlers (Marcar/Listar/Contar notificações) |
| [WorkoutFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/WorkoutFeedbackConfiguration.cs) | EF config: índices, FK Restrict/Cascade, max lengths, default `IsPublic=true` |
| [ExerciseFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/ExerciseFeedbackConfiguration.cs) | EF config: índices, FKs, `StudentResponseText varchar(4000)` |
| [SetFeedbackConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/WorkoutExecution/SetFeedbackConfiguration.cs) | EF config: índices, FKs Cascade p/ WorkoutSet, `MediaReferenceUrl varchar(2048)` |
| [NotificationConfiguration.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs) | EF config: índice crítico `(UserId, IsRead, CreatedAt DESC)` + FK Cascade p/ User |
| [20260803124451_AddFeedbacksAndNotifications.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/Migrations/20260803124451_AddFeedbacksAndNotifications.cs) | Migration: 4 tabelas novas + respectivos Designer/ModelSnapshot |
| [FeedbacksController.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/FeedbacksController.cs) | 10 endpoints: 3 Creates, Update, Delete, GetBySession, MarkRead, GetMy (Student), GetStudent (Coach/Admin), RespondExercise |
| [NotificationsController.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/NotificationsController.cs) | 6 endpoints: UnreadCount, GetMyPaginated, GetById, Mark1Read, MarkAllRead, Delete |

### ARQUIVOS ALTERADOS:

| Arquivo | Alteração |
|---|---|
| [ApplicationDbContext.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Infrastructure/Persistence/ApplicationDbContext.cs) | 4 novos DbSets (`WorkoutFeedbacks`, `ExerciseFeedbacks`, `SetFeedbacks`, `Notifications`) + 4 `HasQueryFilter(x => !x.IsDeleted)` no `OnModelCreating` |
| [ErrorCodes.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Shared/Constants/ErrorCodes.cs#L261-L289) | 7 novas constantes: `FeedbackNotFound`, `FeedbackForbidden`, `FeedbackCannotSendNoPermission`, `FeedbackTextTooLong`, `FeedbackEmpty`, `NotificationNotFound`, `NotificationNotOwner` |
| [FeedbackUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Feedbacks/FeedbackUseCases.cs) | Correções pós-build: `OrderNumber → SetNumber` (3 ocorrências) + `PaginatedResponse` → inicializador de objeto ao invés de construtor 4-args |
| [NotificationUseCases.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Notifications/NotificationUseCases.cs) | Correção `PaginatedResponse` → inicializador de objeto |

### ENDPOINTS DISPONÍVEIS:

#### FeedbacksController → `api/feedbacks`

| Método | Rota | Autorização | Request | Response |
|---|---|---|---|---|
| POST | `/workout-sessions/{sessionId}` | Trainer/GymManager/Admin | `{ text, tone, isPublic }` | `201 Created` + `WorkoutFeedbackResponse` |
| POST | `/workout-exercises/{exerciseId}` | Trainer/GymManager/Admin | `{ text, tone, isPublic }` | `201 Created` + `ExerciseFeedbackResponse` |
| POST | `/workout-sets/{setId}` | Trainer/GymManager/Admin | `{ text, tone, isPublic, mediaReferenceUrl? }` | `201 Created` + `SetFeedbackResponse` |
| PUT | `/{feedbackId}?level=Session\|Exercise\|Set` | Autor ou Admin | `{ text, tone, isPublic?, mediaReferenceUrl? }` | `200 OK` + `UnifiedFeedbackItemResponse` |
| DELETE | `/{feedbackId}?level=Session\|Exercise\|Set` | Autor ou Admin | — | `204 NoContent` |
| GET | `/workout-sessions/{sessionId}` | Estudante dono / Trainer vínculado / Admin | — | `200 OK` + `FeedbacksBySessionBundleResponse` (3 listas) |
| PUT | `/{feedbackId}/read?level=Session\|Exercise\|Set` | Student | — | `204 NoContent` |
| GET | `/my?page=&pageSize=&sessionId=&onlyUnread=&level=` | Student | — | `200 OK` + `PaginatedResponse<UnifiedFeedbackItemResponse>` |
| GET | `/students/{studentId}?page=&pageSize=&sessionId=&level=` | Trainer/GymManager/Admin | — | `200 OK` + `PaginatedResponse<UnifiedFeedbackItemResponse>` |
| POST | `/exercise-feedbacks/{feedbackId}/respond` | Student | `{ responseText }` | `200 OK` + `ExerciseFeedbackResponse` |

**Exemplo: Criar feedback em sessão**
```http
POST /api/feedbacks/workout-sessions/11111111-1111-1111-1111-111111111111
Authorization: Bearer <token-trainer>
Content-Type: application/json

{ "text": "Excelente sessão! Amanhã aumente 2,5kg no supino inclinado.", "tone": 1, "isPublic": true }
```
→ `201 Created` → dispara automaticamente `Notification` tipo `FeedbackReceived` para o aluno da sessão (mesmo `SaveChanges`).

#### NotificationsController → `api/notifications`

| Método | Rota | Autorização | Descrição | Response |
|---|---|---|---|---|
| GET | `/unread-count` | Qualquer autenticado | Badge topo app | `{ unreadCount, lastNotificationAt }` |
| GET | `?page=1&pageSize=50&onlyUnread=false` | Qualquer autenticado | Lista paginada | `PaginatedResponse<NotificationResponse>` |
| GET | `/{id}` | Dono | Detalhe (marca lido auto) | `NotificationResponse` |
| PUT | `/{id}/read` | Dono | Marca 1 como lida | `NotificationResponse` |
| PUT | `/read-all` | Dono | Batch marcar todas lidas | `{ affectedCount }` |
| DELETE | `/{id}` | Dono | Soft-delete 1 notificação | `204 NoContent` |

### TELAS DISPONÍVEIS:

(Não aplicável — sprint somente API; telas client são sprints separadas. Endpoints suportam: 1) Modal de feedback Coach, 2) Painel Feedbacks recebidos Aluno, 3) Central de notificações dropdown, 4) Timeline inline em sessão.)

### COMO EXECUTAR:

```powershell
cd c:\dev\github\trevia.app
dotnet ef database update -p src\TreviaApp.Infrastructure\TreviaApp.Infrastructure.csproj -s src\TreviaApp.Api\TreviaApp.Api.csproj
dotnet run -c Debug --project src\TreviaApp.Api\TreviaApp.Api.csproj
# Swagger: https://localhost:5001/swagger → coleções "Feedbacks" e "Notifications"
```

### COMO TESTAR:

1. **Autenticar como Trainer**: `POST /api/auth/login` → pegar token
2. **Garantir vínculo com CanSendFeedback**: `PUT /api/coach/links/{studentId}/permissions` com payload `{ grantedPermissions = 32 }` (= `CanSendFeedback`, bit 1<<5)
3. **Executar treino de teste (ou usar existente)** → pegar `workoutSessionId`, `workoutExerciseId`, `workoutSetId`
4. **Criar feedback em sessão**:
   ```json
   POST /api/feedbacks/workout-sessions/{sessionId}
   { "text": "Excelente sessão!", "tone": 1 }
   ```
5. **Verificar notificação criada automaticamente** (hook): `GET /api/notifications/unread-count` logado como Student → `{ unreadCount: 1 }`
6. **Listar feedbacks da sessão** (ambos os lados): `GET /api/feedbacks/workout-sessions/{sessionId}` → retorna bundle com Session + Exercise + Set feedbacks
7. **Listar feedbacks recebidos (Aluno)**: `GET /api/feedbacks/my?onlyUnread=true`
8. **Listar feedbacks para um aluno (Coach)**: `GET /api/feedbacks/students/{studentId}`
9. **Responder feedback de exercício (Aluno)**: `POST /api/feedbacks/exercise-feedbacks/{id}/respond { "responseText": "Vou ajustar!" }`
10. **Marcar feedback lido / notificação lida**: PUT endpoints correspondentes.

### MIGRATIONS:

- **1 migration**: `20260803124451_AddFeedbacksAndNotifications` criando 4 tabelas:
  - `WorkoutFeedbacks` (PK, FKs: CoachId/StudentId/Restrict + WorkoutSessionId/Cascade, índices, Tone string, Text varchar(4000))
  - `ExerciseFeedbacks` (+ colunas `StudentResponseText` varchar(4000) null + índice em WorkoutExerciseId)
  - `SetFeedbacks` (+ `MediaReferenceUrl` varchar(2048) null + índices em ExerciseId e SetId)
  - `Notifications` (FK UserId Cascade, índice composto `(UserId, IsRead, CreatedAt DESC)` para badge performance)

### VARIÁVEIS DE AMBIENTE:

Nenhuma nova — usa as mesmas já existentes (`ConnectionStrings__DefaultConnection`, `Jwt`, `Serilog` etc).

### PENDÊNCIAS E MELHORIAS FUTURAS:

1. **TESTES:** Unit + Integration não implementados nesta sprint (reserva para Sprint 12). Recomendado: teste transacionalidade Feedback+Notification; teste resource-based autorização CanSendFeedback; teste aluno tentando editar/excluir feedback de outro → 403.
2. **SignalR / Realtime:** Endpoints HTTP prontos; adicionar Hub (`/hubs/notifications`) e serviço de broadcast em `Create*FeedbackCommandHandler` e demais eventos (WorkoutCompleted etc).
3. **Push mobile (FCM/APNs):** Serviço `IPushNotificationService` integrado ao Notification side-effect — fora do MVP.
4. **Mídia em Feedback:** Campo `MediaReferenceUrl` preparado; conectar com `IFileStorageService` (Sprint 11/12) + endpoint `POST /api/feedbacks/media` para upload.
5. **Resposta do Aluno em Nível Sessão/Série:** Atualmente só ExerciseFeedback tem resposta; estender `StudentResponseText` para WorkoutFeedback e SetFeedback se necessário.
6. **Batch de Feedback:** Endpoint `POST /api/feedbacks/bulk` para comentar múltiplas séries/exercícios de uma vez.
7. **Notificações para outros eventos:** Implementar hooks em `FinishWorkoutSessionCommandHandler` (tipo `WorkoutCompleted`) e `AssignToStudentTrainingPlanCommandHandler` (tipo `PlanAssigned`).

### PRÓXIMA ETAPA RECOMENDADA:

**Sprint 11 — Gamificação: Pontos, Níveis, Conquistas e Sequências (Streaks avançadas)** — alinhado a ROADMAP.md linha 16 e PROJECT_SPEC.md seção "Gamificação" (item 7 do núcleo MVP). Histórias de referência:

- **US-1101** Transação de pontos por ações (concluir treino, completar série, ler feedback, 3 dias consecutivos etc.) → integração opcional com evento de leitura de feedback já capturado por `MarkFeedbackRead`
- **US-1102** Níveis e XP com curva parametrizada
- **US-1103** Conquistas/achievements (primeiro treino, 10 treinos, primeira semana completa, record pessoal etc.)
- **US-1104** Streaks integrados com gamificação (streak 7 dias = pontos bonus, 30 dias = conquista)
- **US-1105** Missões diárias/semanais automáticas + pontuação
- **US-1106** Tela de perfil com nível, XP bar, conquistas recentes
