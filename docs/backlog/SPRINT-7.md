# Sprint 7 — Execução, séries, cargas e cronômetro

## Objetivo

Permitir que o aluno (dono da ficha atribuída) execute suas sessões de treino de forma
controlada, registrando cada série executada com a sua realidade (carga, repetições,
tempo, distância, velocidade, inclinação, observações e dificuldade), além de controlar
o ciclo de vida da sessão (iniciar, pausar, retomar, finalizar) com cronômetro e
distinção entre tempo total e tempo efetivo de atividade. Separar prescrição (o que o
professor planejou) da execução (o que o aluno realmente fez).

## Histórias (User Stories)

### US-0701 — Aluno inicia sessão de treino com base em TrainingSession

**Como** Aluno (dono de uma ficha atribuída ou do próprio plano)  
**Quero** iniciar a execução de uma `TrainingSession` da minha ficha  
**Para** começar o treino com cronômetro e lista de exercícios carregados.

Critérios de aceite:
- [x] Somente o aluno dono do plano (ou dono da ficha atribuída a ele) pode iniciar;
- [x] `TrainingSession` deve existir;
- [x] Se plano atribuído a outro usuário — nega;
- [x] Só pode existir **no máximo 1 sessão ativa** (status InProgress ou Paused) por aluno ao mesmo tempo;
- [x] Ao iniciar, WorkoutSession entra `InProgress` e `StartedAt` é gravado;
- [x] São clonados os exercícios e prescrições de séries da `TrainingSession` → `WorkoutExercises`/`WorkoutSets`;
- [x] Campos `WeekNumberInPlan` (padrão 1), `TrainingPlanId`, `TrainingSessionId`, `Name`, `StudentId` gravados;
- [x] Resposta `201 Created` com cabeçalho para `GetById`.

---

### US-0702 — Aluno pausa sessão em andamento

**Como** Aluno  
**Quero** pausar o cronômetro  
**Para** beber água, atender o celular, etc.

Critérios:
- [x] Somente status `InProgress` pode pausar;
- [x] Cria `WorkoutPause` com `StartedAt`;
- [x] Status passa para `Paused`;
- [x] Re-pausar já pausado → erro de transição.

---

### US-0703 — Aluno retoma sessão pausada

**Como** Aluno  
**Quero** retomar de onde parei  
**Para** continuar o treino.

Critérios:
- [x] Somente `Paused` pode retomar;
- [x] O `WorkoutPause` aberto mais recente é encerrado com `EndedAt=Now`;
- [x] Status volta para `InProgress`.

---

### US-0704 — Aluno finaliza sessão com avaliação

**Como** Aluno  
**Quero** finalizar uma sessão InProgress/Paused com rating de esforço e notas  
**Para** finalizar o treino do dia.

Critérios:
- [x] Sessão deve estar `InProgress` ou `Paused`;
- [x] Ao finalizar:
  - encerra pauses em aberto;
  - grava `FinishedAt`;
  - calcula `TotalDurationElapsed = FinishedAt - StartedAt`;
  - calcula `ActiveTime = Total - Sum(WorkoutPause.Duration)` (não-negativo);
  - `CaloriesBurned` opcional informado pelo usuário ou wearable no futuro;
  - `OverallRating` enum: VeryLight..VeryIntense, Interrupted;
  - `Status = Completed` ou `Interrupted` (caso OverallRating=Interrupted);
  - `GeneralNotes` varchar(2000) opcional.

---

### US-0705 — Aluno registra série executada (carga, reps, tempo, distância, velocidade, inclinação, calorias, dificuldade, nota)

**Como** Aluno  
**Quero** registrar o que fiz em cada série (incluindo drop-set, rest-pause, isometria, cardio)  
**Para** ter meu histórico fiel de treino.

Critérios:
- [x] Atualiza um `WorkoutSet` existente (Id conhecido por referência a SetPrescription ou série extra);
- [x] Sessão deve estar `InProgress` ou `Paused`;
- [x] `Completed` booleano para série feita / pulada dentro do exercício;
- [x] Campos:
  - `ActualReps` (>=0), `ActualLoadValue` (>=0), `ActualLoadUnit` (Kg/Lb/Porcentagem1RM/etc.);
  - `ActualDurationSeconds` (séries isométricas, cardio, time-under-tension);
  - `DistanceKm`, `SpeedKmh`, `InclinePercent` (cardio / ergômetros);
  - `Calories` (opcional por série);
  - `DifficultyRating = SetRating`;
  - `Notes` (até 500 chars);
  - campo calculado `VolumeKg = ActualLoadValue * ActualReps` (quando ambos existem).

---

### US-0706 — Aluno pula exercício com motivo

**Como** Aluno  
**Quero** marcar exercício como pulado com motivo  
**Para** o professor compreender quando ajustar o treino.

Critérios:
- [x] Exercício deve pertencer à sessão;
- [x] Sessão InProgress/Paused;
- [x] Campos: `IsSkipped = true`, `SkipReason` (max 500).

---

### US-0707 — Aluno adiciona série extra no exercício

**Como** Aluno  
**Quero** adicionar 1 ou mais séries extras no exercício (drop sets, falha, etc.)  
**Para** refletir o que realmente foi feito além do planejado.

Critérios:
- [x] Exercício da sessão em InProgress/Paused;
- [x] Campo `IsAdditionalSet = true` e `SetPrescriptionId = NULL`;
- [x] Número da série: sugere próximo número; aceita sugestão via request.

---

### US-0708 — Aluno lista minhas sessões (filtros e paginação)

**Como** Aluno  
**Quero** listar minhas sessões com paginação e filtro  
**Para** ver meu histórico.

Filtros:
- [x] `statusFilter = WorkoutStatus`;
- [x] `trainingPlanId`;
- [x] `from`, `to` (faixa StartedAt);
- [x] ordenação StartedAt desc;
- [x] agregados: ExercíciosCount, CompletedSetsCount, TotalVolumeKg.

---

### US-0709 — Aluno visualiza sessão atual em andamento

**Como** Aluno  
**Quero** carregar rapidamente a sessão ativa (InProgress/Paused)  
**Para** continuar de onde parei no App.

Critérios:
- [x] `GET /api/workouts/sessions/current-active` → detalhe completo ou 204/200 nulo.

---

### US-0710 — Aluno vê detalhe completo de sessão (por id)

**Como** Aluno (ou, futuramente, Professor vinculado)  
**Quero** visualizar todos exercícios, séries, tempos e pausas de uma sessão  
**Para** acompanhar.

Critérios:
- [x] Exercícios ordenados por Order;
- [x] Séries por SetNumber;
- [x] Pausas com duração em segundos;
- [x] Totais: TotalDurationSeconds, ActiveTimeSeconds, CaloriesBurned, VolumeKg por série e por exercício.

---

## Critérios transversais

- [x] Regras de estado e de negócio ficam nas entidades de domínio (`WorkoutSession`, `WorkoutExercise`, `WorkoutSet`, `WorkoutPause`);
- [x] Controllers são "finos", delegando para Commands/Queries com MediatR;
- [x] Validação de entrada via FluentValidation (Start);
- [x] Erros padronizados (`ErrorCodes.Workout*`) + `Result<>` pattern;
- [x] Log estruturado ("SaveChangesAsync explícito concluído: …") para auditoria;
- [x] Soft delete + query filter automáticos nas 4 entidades do módulo;
- [x] Índices: (StudentId, Status), (TrainingSessionId), (WorkoutSessionId, WorkoutExerciseId.Order), (SetPrescriptionId);
- [x] Conversões: enum como string (WorkoutStatus, Rating, Unit); TimeSpan ↔ long seconds.

---

## Próximas histórias recomendadas (Sprint 8+)

- Replay automático: pré-carregar carga da série anterior como sugestão;
- RPE/RIR por série executada (hoje está em prescrição);
- Comparativo "prescrito vs realizado" agregado;
- Professor visualiza sessões de seu aluno (IsLinkedTrainer);
- Exportação de sessão para PDF/Planilha;
- Integração com wearables (calorias, duração, heart-rate zones).
