# Plataforma Fitness Gamificada — Especificação para o TRAE

## Papel

Atue como arquiteto de software, analista de sistemas, desenvolvedor C#/.NET, especialista em PostgreSQL, UX/UI e DevOps.

Trabalhe incrementalmente, seguindo Agile, backlog, histórias de usuário, critérios de aceite e entregas verificáveis. Não desenvolva o produto inteiro de uma única vez.

## Produto

Construir uma plataforma fitness para alunos avulsos, alunos acompanhados, personal trainers, professores, academias e administradores.

O núcleo do MVP é:

1. Professor e aluno criam contas;
2. Exercícios são cadastrados;
3. Professor cria e atribui uma ficha;
4. Aluno executa e registra o treino;
5. Aluno avalia a dificuldade;
6. Professor acompanha e envia feedback;
7. Aluno acompanha evolução e recebe pontos.

## Tecnologias obrigatórias

### Server

- C# e ASP.NET Core na versão LTS atual;
- ASP.NET Core Web API;
- Entity Framework Core;
- PostgreSQL com Npgsql;
- ASP.NET Core Identity;
- JWT de curta duração e Refresh Token rotativo;
- Swagger/OpenAPI;
- SignalR para recursos em tempo real;
- Problem Details;
- validação de entrada;
- logging estruturado;
- rate limiting;
- health checks.

### Client

- Blazor WebAssembly;
- PWA responsiva;
- autenticação por JWT;
- persistência local do treino em andamento;
- recuperação e sincronização posterior;
- publicação independente do Server.

### Infraestrutura

- Git;
- Docker;
- Dockerfile separado para Client e Server;
- PostgreSQL gerenciado;
- Render.com;
- variáveis de ambiente;
- Development e Production.

## Arquitetura

Use monólito modular e Clean Architecture de forma pragmática. Não use microsserviços inicialmente.

Estrutura sugerida:

```text
src/
  FitnessApp.Api
  FitnessApp.Application
  FitnessApp.Domain
  FitnessApp.Infrastructure
  FitnessApp.Contracts
  FitnessApp.Shared
  FitnessApp.Client

tests/
  FitnessApp.UnitTests
  FitnessApp.IntegrationTests
  FitnessApp.ArchitectureTests
```

Módulos planejados:

- Identity;
- Profiles;
- Exercises;
- TrainingPlans;
- WorkoutExecution;
- Coaching;
- Gamification;
- Messaging;
- Assessments;
- Nutrition;
- Payments;
- Community;
- Administration.

## Padrões

- Código em inglês e interface em português do Brasil;
- nullable habilitado;
- async/await e CancellationToken quando aplicável;
- DTOs, mapeamentos explícitos e paginação;
- autorização baseada em policies;
- regras de negócio fora dos controllers;
- não retornar entidades diretamente pela API;
- não usar repository genérico sem necessidade;
- não registrar senhas, tokens ou dados sensíveis.

## Perfis

- Administrator: usuários, biblioteca global, moderação e auditoria;
- Student: perfil, treinos, histórico, metas e solicitações de acompanhamento;
- Trainer: alunos, fichas, modelos, exercícios próprios e feedbacks;
- GymManager: academia, professores, alunos, grupos e modelos institucionais.

Uma conta poderá possuir mais de um perfil.

## Autenticação

Implementar cadastro, login, logout, confirmação de e-mail, recuperação de senha, JWT, Refresh Token com rotação, revogação, controle de sessões, roles, claims, policies, bloqueio temporário, rate limiting e auditoria básica.

Use ASP.NET Core Identity. Nunca implemente hash de senha manualmente.

## Perfis e dados físicos

Nome, sobrenome, nome de exibição, nascimento, gênero opcional, altura, histórico de peso, foto, cidade, estado, objetivo, experiência, ambiente de treino, equipamentos disponíveis e privacidade.

Objetivos: perda de peso, ganho de massa, condicionamento, força, mobilidade, saúde, esporte e outros.

## Biblioteca de exercícios

Cada exercício deverá possuir nome, slug, descrição, instruções, cuidados, ambiente, modalidade, dificuldade, tipo de medição, músculo principal, músculos secundários, equipamentos, mídias, autor, visibilidade, aprovação e datas.

Ambientes: academia, casa, ar livre, estúdio, piscina e outros.

Modalidades: musculação, cardio, alongamento, mobilidade, aquecimento, Pilates, CrossFit, funcional, corrida, ciclismo e peso corporal.

Medições: repetições, tempo, distância, calorias, carga e repetições, carga e tempo, peso corporal, carga adicional e assistência.

## Fichas

Uma ficha possui nome, descrição, objetivo, modalidade, período, nível, autor, aluno, status, sessões e observações.

Cada sessão possui nome, ordem, objetivo, dia sugerido, duração e exercícios.

A prescrição permite séries, repetições mínimas e máximas, carga, tempo, distância, velocidade, inclinação, descanso, cadência, RPE, RIR, observações e técnicas como superset, circuito, drop-set e rest-pause.

Separe sempre prescrição do que foi realmente executado.

## Execução

O aluno poderá iniciar, retomar, pausar e finalizar uma sessão; visualizar mídia; registrar carga, repetições, tempo e distância; concluir séries; usar cronômetro; pular exercícios; adicionar séries e observações.

Avaliação de série: muito fácil, fácil, adequada, difícil, muito difícil ou não concluída.

Avaliação do treino: muito leve, leve, moderado, intenso, muito intenso ou interrompido.

## Relatórios

Frequência, calendário, tempo, quantidade de treinos e séries, volume, evolução, recordes, distribuição muscular, exercícios mais realizados e taxa de conclusão.

`Volume da série = carga × repetições`.

Somar somente séries válidas e concluídas. Não misture métricas incompatíveis.

## Professor e aluno

Convites, solicitações, aceite, vínculo, permissões, atribuição de fichas, acesso a dados autorizados, feedback em treino/exercício/série e encerramento do vínculo.

## Avaliação física, metas e hidratação

Preparar estrutura para peso, medidas, percentual de gordura informado, fotos privadas, metas e registros de água. Fotos corporais nunca devem ser públicas por padrão.

## Gamificação

Pontos, níveis, conquistas, sequências, desafios, recordes e missões. Toda pontuação deve possuir histórico transacional. Não recompense excesso de treino.

## Fora do MVP

Marketplace, pagamentos, rede social, chat completo, smartwatches, HealthKit, Health Connect, dietas individualizadas, ranking público e aplicativo nativo.

Prepare interfaces e limites arquiteturais, mas não implemente esses módulos prematuramente.

## PWA e offline

Implementar instalação, responsividade, persistência do treino em andamento, recuperação após recarregar, fila de sincronização, proteção contra duplicidade e indicador de sincronização.

Documentar limitações de PiP, tela bloqueada e execução em segundo plano. Recursos nativos futuros poderão usar .NET MAUI.

## LGPD

Consentimentos versionados, finalidade, revogação, privacidade por padrão, visibilidade, autorização por recurso, auditoria, exclusão de conta, proteção de mídias, URLs temporárias e logs sem dados sensíveis.

## Arquivos

Não armazenar fotos ou vídeos no PostgreSQL. Criar `IFileStorageService` com upload, download autorizado, exclusão, URL temporária, validação, metadados e propriedade.

Não tratar o filesystem do Render como armazenamento persistente.

## PostgreSQL

Use UUID quando apropriado, timestamptz, índices, constraints, precisão decimal explícita, delete behavior explícito, paginação, migrations e seed inicial.

Seed: roles, músculos, equipamentos, modalidades e configurações. Credenciais reais nunca entram no código.

## Render.com

Criar recursos separados:

- PostgreSQL gerenciado;
- Server/API;
- Client/PWA;
- armazenamento externo de mídias no futuro.

O Server deve usar a porta fornecida por `PORT`, health check, connection string via ambiente, CORS, logs e migrations controladas.

O Client deve possuir fallback de SPA e `ApiBaseUrl` configurável.

Variáveis mínimas:

```text
ASPNETCORE_ENVIRONMENT
PORT
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__Key
Jwt__AccessTokenMinutes
Jwt__RefreshTokenDays
Cors__AllowedOrigins__0
AdminSeed__Email
AdminSeed__Password
FileStorage__Provider
```

Não executar migrations automaticamente em toda inicialização. Proponha comando ou job controlado.

## Testes

Unitários: volume, recordes, pontos, metas, permissões, vínculos e execução.

Integração: cadastro, login, refresh token, PostgreSQL, exercícios, fichas, atribuição, execução, autorização e relatórios.

Arquitetura: Domain não depende de Infrastructure; Application não depende de API; Client não acessa banco; controllers não possuem regras de negócio.

## Ordem de desenvolvimento

1. Fundação;
2. Identidade;
3. Perfis;
4. Exercícios;
5. Fichas;
6. Vínculo professor-aluno;
7. Execução;
8. Histórico;
9. Feedback;
10. Gamificação;
11. PWA/offline;
12. Deploy.

## Formato obrigatório de entrega

Antes de implementar:

```text
ETAPA:
OBJETIVO:
ESCOPO:
ARQUIVOS QUE SERÃO CRIADOS:
ARQUIVOS QUE SERÃO ALTERADOS:
MIGRATIONS:
ENDPOINTS:
TELAS:
TESTES:
RISCOS:
```

Depois de implementar:

```text
RESUMO:
ARQUIVOS CRIADOS:
ARQUIVOS ALTERADOS:
ENDPOINTS DISPONÍVEIS:
TELAS DISPONÍVEIS:
COMO EXECUTAR:
COMO TESTAR:
MIGRATIONS:
VARIÁVEIS DE AMBIENTE:
PENDÊNCIAS:
PRÓXIMA ETAPA RECOMENDADA:
```

## Primeira tarefa do TRAE

Não implemente todo o sistema.

Execute apenas:

1. análise dos requisitos;
2. definição detalhada do MVP;
3. proposta arquitetural;
4. estrutura da solution;
5. módulos;
6. backlog;
7. histórias da Sprint 0 e Sprint 1;
8. critérios de aceite;
9. entidades iniciais;
10. diagrama textual;
11. estratégia de autenticação;
12. estratégia de deploy no Render;
13. estratégia de migrations;
14. variáveis de ambiente;
15. riscos;
16. decisões pendentes.

Ao final, solicite autorização para criar a solution e a infraestrutura inicial.
