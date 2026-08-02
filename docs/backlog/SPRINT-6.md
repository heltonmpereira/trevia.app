# Sprint 6 — Convites, vínculos e atribuições

## Objetivo

Concluir o módulo de **Coaching** (Professor ↔ Aluno) e a **atribuição de fichas** a alunos
vinculados, incluindo autorizações baseadas no vínculo ativo.

## Histórias (User Stories)

### US-0601 — Professor envia convite de coaching a aluno

**Como** Professor  
**Quero** enviar um convite a um aluno com mensagem e permissões iniciais  
**Para** iniciar um vínculo de acompanhamento.

Critérios de aceite:
- [x] Professor (Trainer, Admin, GymManager) autenticado pode enviar;
- [x] Não pode convidar a si mesmo;
- [x] Aluno deve existir e não estar deletado;
- [x] Não pode haver outro convite pendente entre o par;
- [x] Não pode haver vínculo ativo entre o par;
- [x] Convite tem data de expiração (padrão 30 dias);
- [x] Resposta `201 Created` com link para `GetRelationshipById`.

---

### US-0602 — Aluno solicita acompanhamento a professor

**Como** Aluno  
**Quero** enviar uma solicitação de acompanhamento a um professor  
**Para** poder ter meu treino acompanhado.

Critérios de aceite:
- [x] Aluno (Student) autenticado pode enviar;
- [x] Não pode solicitar a si mesmo;
- [x] Professor deve existir;
- [x] Sem solicitação pendente duplicada;
- [x] Sem vínculo ativo duplicado.

---

### US-0603 — Destinatário aceita convite ou solicitação

**Como** Aluno OU Professor (destinatário)  
**Quero** aceitar um convite/solicitação pendente  
**Para** formalizar o vínculo de coaching.

Critérios de aceite:
- [x] Apenas o destinatário (ou admin) pode aceitar;
- [x] Convite deve estar `Pending` e não expirado;
- [x] Ao aceitar, cria `CoachStudentLink` ativo com as permissões concedidas;
- [x] Convite passa para `Accepted`.

---

### US-0604 — Destinatário rejeita convite

**Como** Destinatário  
**Quero** rejeitar o convite com motivo opcional  
**Para** recusar o vínculo.

Critérios:
- [x] Apenas destinatário (ou admin);
- [x] Convite `Pending`;
- [x] Motivo até 500 caracteres;
- [x] Convite passa para `Rejected`.

---

### US-0605 — Remetente cancela convite pendente

**Como** Remetente do convite  
**Quero** cancelar a solicitação ainda pendente  
**Para** desistir do contato.

Critérios:
- [x] Apenas o remetente (CoachToStudent → Coach; StudentToCoach → Student) ou admin;
- [x] Convite passa para `Cancelled`.

---

### US-0606 — Professor atualiza permissões do vínculo

**Como** Professor do vínculo  
**Quero** ajustar o flags de permissão  
**Para** controlar o que posso ver/fazer no perfil do aluno.

Critérios:
- [x] Apenas Coach do link (ou admin) pode atualizar;
- [x] Vínculo deve estar ativo;
- [x] Permissões usam `[Flags]`: `CanViewWeightHistory`, `CanViewBodyMeasurements`, `CanViewProfilePhotos`, `CanAssignTrainingPlans`, `CanViewWorkoutHistory`, `CanSendFeedback`, `CanViewAssessments`, `CanInviteToGroups`.

---

### US-0607 — Professor, Aluno ou Admin encerram o vínculo

**Como** Participante do vínculo ou Administrador  
**Quero** encerrar o relacionamento ativo com motivo  
**Para** finalizar o acompanhamento.

Critérios:
- [x] Só pode encerrar se estiver ativo;
- [x] Motivos: `MutualAgreement`, `EndedByCoach`, `EndedByStudent`, `EndedByAdmin`, `Expired`, `Other`;
- [x] Campos: `EndedAt`, `EndReason`, `EndReasonNotes` (até 1000 chars);
- [x] `IsActive = false`;
- [x] Soft delete (`IsDeleted = true`) para manter consistência do índice único.

---

### US-0608 — Professor lista seus alunos

**Como** Professor  
**Quero** listar meus alunos vinculados com paginação e filtro  
**Para** acompanhar cada aluno.

Critérios:
- [x] Paginado (`page`, `pageSize`);
- [x] Filtro `onlyActive`, `searchName`;
- [x] Ordenação: `linkedSinceDesc`, `linkedSinceAsc`, `nameAsc`, `nameDesc`;
- [x] Dados: `DisplayName`, `PhotoFileId`, `Goal`, `Experience`, `LinkedSince`, `Permissions`, `ActiveTrainingPlansCount`.

---

### US-0609 — Aluno lista seus professores

**Como** Aluno  
**Quero** ver meus professores vinculados  
**Para** saber quem me acompanha.

Critérios:
- [x] Mesma estrutura de US-0608, perspectiva aluno.

---

### US-0610 — Listar convites recebidos/enviados

**Como** Usuário autenticado  
**Quero** visualizar convites/solicitações recebidos e enviados  
**Para** decidir aceitar, rejeitar, etc.

Critérios:
- [x] Paginação + filtro por `Status`;
- [x] Ordenação por `CreatedAt` (ASC/DESC);
- [x] Flag `IsExpired` calculada.

---

### US-0611 — Contagem de convites/solicitações pendentes

**Como** Usuário autenticado  
**Quero** saber quantas pendências tenho  
**Para** exibir badge/notificação.

---

### US-0612 — Detalhe do vínculo por Id

**Como** Participante do vínculo ou Admin  
**Quero** obter detalhes de um link  
**Para** conferir permissões e datas.

Critérios:
- [x] Acesso apenas para Coach, Student do link ou Admin/GymManager.

---

### US-0613 — Verificar status de vínculo com outro usuário

**Como** Usuário autenticado  
**Quero** checar se já tenho vínculo/convite pendente com um usuário específico  
**Para** decidir qual ação tomar (convidar / aceitar).

Resposta: `HasActiveLink`, `LinkId`, `IsCoachInRelationship`, `IsStudentInRelationship`, `CurrentPermissions`, `PendingInviteStatus`, `PendingInviteId`, `PendingInviteDirection`.

---

### US-0614 — Admin/GymManager visualiza alunos de um professor

**Como** Administrador ou Gerente de Academia  
**Quero** ver os alunos vinculados a um coach  
**Para** auditoria e gestão.

---

### US-0615 — Buscar alunos/professores ainda não vinculados

**Como** Professor (ou Aluno)  
**Quero** buscar por nome usuários com o perfil alvo que ainda não estão vinculados a mim e sem convite pendente  
**Para** enviar convite/solicitação.

---

### US-0616 — Professor atribui ficha a seu aluno vinculado

**Como** Professor  
**Quero** atribuir um `TrainingPlan` que criei a um dos meus alunos  
**Para** que ele possa executar o treino.

Critérios:
- [x] Plano deve existir e ser de minha autoria (ou admin);
- [x] Plano ainda não pode estar atribuído a outro aluno;
- [x] Deve existir vínculo ativo entre eu e o aluno;
- [x] Vínculo ativo deve possuir `CoachPermissions.CanAssignTrainingPlans`;
- [x] Seta `TrainingPlan.AssignedToStudentId = studentId` e `Published`/`Active`.

---

## Critérios de aceite transversais

- [x] Regras de negócio no Domain + Application (fora dos controllers);
- [x] Respostas da API usam DTOs/Responses (não retornam entidades);
- [x] Validação de entrada com validators;
- [x] `DomainException` + `ErrorCodes` coerentes;
- [x] Log estruturado (informação e debug com `SaveChangesAsync explícito concluído`);
- [x] Soft delete + query filter automáticos;
- [x] Índices no banco para desempenho: FKs, status, par (Coach,Student) único para ativos e pendentes;
- [x] Policies de autorização: `CanAssignTrainingPlans`, `IsTrainerOrAdmin`, `IsGymManagerOrAdmin`, `IsLinkedTrainer`, `IsLinkedStudent`, `IsLinkedTrainerOrAdmin`;
- [x] Handlers de autorização `LinkedTrainerAuthorizationHandler` e `LinkedStudentAuthorizationHandler`;
- [x] Endpoints em `CoachingController` sob `[EnableRateLimiting("AuthEndpoint")]`.
