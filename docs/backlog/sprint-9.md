# Sprint 9 — Relatórios, volume, evolução e recordes

## ETAPA:
✅ Implementação concluída (Pós-implementação)

## OBJETIVO:
Prover ao aluno (e ao professor vinculado / admin) relatórios analíticos completos sobre seu histórico de treino: frequência (calendário), duração total e ativa, quantidade de treinos e séries, volume total (carga × repetições), evolução ao longo do tempo (séries temporais semanais/mensais), distribuição muscular por grupo, exercícios mais realizados (top N por volume e frequência), taxa de conclusão de séries e recordes pessoais (PRs) por exercício. Tudo agregando apenas séries válidas e concluídas, sem misturar métricas incompatíveis (cardio vs força), conforme PROJECT_SPEC.md — seção "Relatórios".

## ESCOPO (Entregue):
- ✅ US-0901 — Resumo agregado de um período (summary dashboard)
- ✅ US-0902 — Calendário de frequência (heatmap diário)
- ✅ US-0903 — Evolução ao longo do tempo (séries temporais por dia/semana/mês)
- ✅ US-0904 — Distribuição de volume por grupo muscular (com peso por MuscleRole e ActivationPercent)
- ✅ US-0905 — Exercícios mais realizados (ranking top N por Volume | Frequência | Sets)
- ✅ US-0906 — Recordes pessoais (PRs) por exercício: MaxLoad, MaxVolume, MaxReps, MaxDistance, MaxDuration
- ✅ US-0907 — Visão do professor / admin sobre relatórios de aluno vinculado
- ✅ Filtros transversais: período (from/to), TrainingPlanId (opcional), granularidade, top/rankBy
- ✅ Autorização: aluno vê seus dados; professor com permissão CanViewWorkoutHistory vê aluno vinculado; Admin/GymManager vê qualquer aluno.

---

## Progresso da Implementação

- [x] PASSO 1: Criar documento sprint-9.md com escopo pré-implementação
- [x] PASSO 2: Criar Contracts DTOs (ReportResponses.cs)
- [x] PASSO 3: Criar Application Queries/Handlers (ReportQueries.cs)
- [x] PASSO 4: Criar ReportsController com 12 endpoints
- [x] PASSO 5: (Opcional) Adicionar índices / migration — **Não necessário nesta sprint** (consultas SELECT-only sobre tabelas já indexadas; otimizações de índice serão validadas em ambiente de teste/carga)
- [x] PASSO 6: Build & validação de compilação (0 erros, 0 warnings)
- [x] PASSO 7: Atualizar relatório pós-implementação

---

## RESUMO PÓS-IMPLEMENTAÇÃO:

## RESUMO:
Sprint 9 entregue com sucesso. Implementado módulo completo de Relatórios analíticos do histórico de treino, composto por 6 consultas principais (cada uma com versão "My" para o aluno e "Student" para professor/admin vinculado), totalizando 12 MediatR Queries + 12 Handlers, expostos por 12 endpoints HTTP GET no novo `ReportsController`. Todos os relatórios seguem rigorosamente a regra do PROJECT_SPEC.md: apenas séries `Completed` e com `VolumeKg.HasValue` entram em cálculos de distribuição muscular; streaks de frequência são calculados por dias consecutivos; recordes pessoais são determinados por exercício considerando valores históricos de séries concluídas. Build de solução: **0 erros, 0 warnings**.

## ARQUIVOS CRIADOS:
1. [ReportResponses.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Contracts/Reports/Responses/ReportResponses.cs) — 7 records de DTO (`WorkoutSummaryResponse`, `WorkoutCalendarDayResponse`, `WorkoutProgressPointResponse`, `MuscleVolumeItemResponse`, `ExerciseRankItemResponse`, `PersonalRecordItemResponse`) + 3 enums auxiliares (`ProgressGranularity`, `ExerciseRankBy`, `PersonalRecordType`).
2. [ReportQueries.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Application/Reports/Queries/ReportQueries.cs) — 12 Query records + 12 Handlers MediatR, 11 métodos estáticos auxiliares (filtros, autorização coach, cálculos de streak, agrupamento temporal, distribuição muscular, top exercícios, PRs).
3. [ReportsController.cs](file:///c:/dev/github/trevia.app/src/TreviaApp.Api/Controllers/ReportsController.cs) — 12 endpoints GET, 6 para "meus relatórios" e 6 para "relatórios de aluno" com role/permission check.
4. [sprint-9.md](file:///c:/dev/github/trevia.app/docs/backlog/sprint-9.md) — Este documento de backlog e entrega da sprint.

## ARQUIVOS ALTERADOS:
- **Nenhum arquivo existente foi alterado** (obrigado, Clean Architecture!). A camada Application reutilizou `IApplicationDbContext.Set<T>()` padrão; a camada Api reutilizou políticas de autorização/roles já existentes (`AppRoles.Administrator`, `AppRoles.GymManager`, `CoachPermissions.CanViewWorkoutHistory` via `CoachStudentLink`); `ErrorCodes.Forbidden` já existente foi suficiente para todos os casos de autorização.

## ENDPOINTS DISPONÍVEIS (12):
### Meus relatórios (autenticado como Aluno/Trainer/Admin):
| Método | Rota | Response |
|---|---|---|
| GET | `/api/reports/summary?from=&to=&trainingPlanId=` | `WorkoutSummaryResponse` |
| GET | `/api/reports/calendar?year=&month=` | `IReadOnlyList<WorkoutCalendarDayResponse>` |
| GET | `/api/reports/progress?from=&to=&granularity=Day\|Week\|Month` | `IReadOnlyList<WorkoutProgressPointResponse>` |
| GET | `/api/reports/muscles?from=&to=` | `IReadOnlyList<MuscleVolumeItemResponse>` |
| GET | `/api/reports/exercises/top?from=&to=&top=10&rankBy=Volume\|Frequency\|Sets` | `IReadOnlyList<ExerciseRankItemResponse>` |
| GET | `/api/reports/records?exerciseId=` | `IReadOnlyList<PersonalRecordItemResponse>` |

### Relatórios de Aluno (Professor vinculado / Admin / GymManager):
Mesmos relatórios acima, prefixados com `/students/{studentId}/`:
| Método | Rota |
|---|---|
| GET | `/api/reports/students/{studentId}/summary` |
| GET | `/api/reports/students/{studentId}/calendar` |
| GET | `/api/reports/students/{studentId}/progress` |
| GET | `/api/reports/students/{studentId}/muscles` |
| GET | `/api/reports/students/{studentId}/exercises/top` |
| GET | `/api/reports/students/{studentId}/records` |

**Autorização aluno-professor:** Reutiliza a tabela `CoachStudentLink` validando a flag `CoachPermissions.CanViewWorkoutHistory` (mesmo padrão do módulo Coaching/Sprint 6).

## TELAS DISPONÍVEIS:
- **Nenhuma tela Client nesta Sprint** (backlog conforme ROADMAP.md: telas do Blazor PWA serão entregues nas Sprints 11/12).
- Swagger/OpenAPI: Todos os 12 endpoints automaticamente documentados em `/swagger` da Api.

## COMO EXECUTAR:
```powershell
# 1) Garantir PostgreSQL rodando local (via docker-compose up -d) ou remoto
cd c:\dev\github\trevia.app

# 2) Rodar migrations (se ainda não rodou em ambiente)
dotnet ef database update -p src\TreviaApp.Infrastructure -s src\TreviaApp.Api

# 3) Build e rodar API
dotnet run -c Debug --project src\TreviaApp.Api\TreviaApp.Api.csproj

# 4) Abrir Swagger
Start-Process https://localhost:5001/swagger
```

## COMO TESTAR (manual via Swagger/curl):
1. **Registrar/logar** como Student → obter JWT (endpoints `/api/auth/register`, `/api/auth/login`).
2. **Executar alguns treinos** usando Sprint 7+8 (criar ficha, atribuir, iniciar sessão, logar séries com carga/reps, finalizar com rating).
3. **Chamar os relatórios**:
   - `GET /api/reports/summary` → resumo de 30 dias padrão
   - `GET /api/reports/muscles?from=2026-01-01&to=2026-12-31` → distribuição anual
   - `GET /api/reports/records` → PRs de todos os exercícios
   - `GET /api/reports/progress?granularity=Week` → série temporal semanal
4. **Testar visão professor**: Registrar 2º usuário Trainer → convidar/vincular aluno → usar endpoints `/students/{id}/…`.

## MIGRATIONS:
- **Nenhuma migration gerada nesta sprint.**
- Justificativa: Todas as consultas são `SELECT` (leitura) sobre entidades já migradas nas sprints 7 (WorkoutExecution) e 3 (Exercises).
- Pendência futura (opcional, validadar em load-test): criar migration `AddReportPerformanceIndexes` com índices compostos `WorkoutSessions(StudentId, StartedAt, Status)`, `WorkoutSets(Completed, VolumeKg)` caso seja observado slow query em volume grande de dados.

## VARIÁVEIS DE AMBIENTE:
- **Nenhuma variável nova adicionada.**
- Reutiliza `ConnectionStrings__DefaultConnection` (PostgreSQL), `Jwt__*` (autenticação), `Cors__*` já existentes da fundação/Sprint 1.

## PENDÊNCIAS / Backlog futuro:
- [ ] **Testes unitários e de integração:** Adicionar em `tests/TreviaApp.UnitTests/Reports/` e `tests/TreviaApp.IntegrationTests/Reports/` cobrindo: agregação válida (só Completed), cálculo de streaks, distribuição muscular ponderada por ActivationPercent, desempate em PRs, autorização coach.
- [ ] **Índices de performance:** Validar em banco com volume >100k sessões a necessidade de migration `AddReportPerformanceIndexes`.
- [ ] **Materialized Views / Caching:** Para dashboards em larga escala, considerar Redis OutputCache ou materialized view periódica.
- [ ] **Exportação:** Adicionar endpoints `GET /api/reports/summary/export?format=Csv|Pdf` (Sprint 12+).
- [ ] **Telas Blazor (Sprint 11+):** Consumir endpoints no Client e montar Dashboard.

## PRÓXIMA ETAPA RECOMENDADA:
### **Sprint 10 — Feedbacks e Notificações (conforme ROADMAP.md)**
Histórias alinhadas ao PROJECT_SPEC.md "Professor e aluno" e "Relatórios":
- **US-1001:** Professor envia feedback em treino/sessão (`WorkoutSession.FeedbackByCoachId`, `CoachNotes`).
- **US-1002:** Professor comenta exercício específico ou série.
- **US-1003:** Aluno visualiza feedbacks pendentes/lidos.
- **US-1004:** Notificações internas (SignalR hub opcional) + endpoint `GET /api/notifications`.
- **US-1005:** Marcar feedback como lido.
- **Base técnica:** reutilizar `CoachStudentLink` + permissões; integrar na mesma `ReportsController` ou novo `FeedbacksController` / `NotificationsController`.
