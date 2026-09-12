# SOAT — OS Service

Microsserviço responsável pela **Ordem de Serviço (OS)** dentro da arquitetura de microsserviços da Fase 4 do Tech Challenge (FIAP). Extraído do monolito [`soat-tech-challenge`](https://github.com/monnclaro/soat-tech-challenge), que permanece como referência histórica das Fases 1-3.

## Responsabilidades

- Abertura de Ordem de Serviço.
- Consulta de status e histórico.
- **Orquestração da saga** (abrir OS → diagnóstico → orçamento → aprovação de pagamento → execução → finalização), coordenando o [Billing Service](https://github.com/monnclaro/soat-tech-challenge-billing-service) e o [Execution Service](https://github.com/monnclaro/soat-tech-challenge-execution-service) via RabbitMQ/MassTransit — orquestração feita por reação a domain events (não um saga state machine separado): o próprio agregado `OrdemServico` guarda o estado da saga, handlers de domain event publicam os comandos (`IniciarDiagnostico`, `GerarOrcamento`, `IniciarExecucao`) e consumers MassTransit traduzem os eventos recebidos (`DiagnosticoFinalizado`, `DiagnosticoFalhou`, `PagamentoAprovado`, `PagamentoRecusado`, `ExecucaoFinalizada`) diretamente para os use cases já existentes.
- Cadastro de Clientes, Veículos, Produtos e Serviços (catálogo) — mantido aqui por ser o "dono" do agregado raiz `OrdemServico` e o autenticador do back-office.

## Papel na saga (orquestrador)

Este serviço é o **orquestrador da saga**, mas sem um saga state machine separado: o próprio agregado `OrdemServico` guarda o estado (`Status`), handlers de domain event publicam os comandos e consumers MassTransit traduzem os eventos recebidos diretamente para os use cases já existentes (mesma regra de negócio usada pelos endpoints REST internos — nenhuma lógica duplicada entre as duas vias de entrada).

| Passo | Mensagem publicada por este serviço | Consumida por | Reação deste serviço ao evento de volta |
|---|---|---|---|
| 1 | `IniciarDiagnostico` (comando) | Execução Service | — |
| 2 | — | — | `DiagnosticoFinalizado`/`DiagnosticoFalhou` → registra o diagnóstico e publica `GerarOrcamento` |
| 3 | `GerarOrcamento` (comando) | Billing Service | — |
| 4 | — | — | `PagamentoAprovado` → publica `IniciarExecucao`; `PagamentoRecusado` → cancela a OS (compensação) |
| 5 | `IniciarExecucao` (comando) | Execução Service | — |
| 6 | — | — | `ExecucaoFinalizada` → finaliza a OS |

Justificativa completa dessa escolha de design (por que não um saga state machine dedicado, como funciona a compensação): [ADR 0001](./docs/adr/0001-saga-orquestrada-sem-state-machine-separado.md).

## Entidades principais

- **OrdemServico**: agregado raiz da saga — `IdCliente`, `IdVeiculo`, `Status` (`Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, com `Cancelada` como caminho de compensação a partir de qualquer ponto), `ValorTotal`, histórico de status (`HistoricoStatusOrdemServico`, uma linha por transição, para auditoria/consulta).
- **Cliente**/**Veiculo**/**Produto**/**Servico**: catálogo/cadastro, sem relação com a saga — mantidos aqui por serem consultados na abertura da OS e por este ser o único serviço com back-office autenticado.
- **Usuario**: credenciais do back-office (login que emite o JWT usado pelos 3 serviços).

## Arquitetura

Clean Architecture, mesmo padrão validado no monolito de origem:

```
src/
  Domain/         # Entidades, regras de negócio, sem dependências externas
  Application/     # Casos de uso, ports, orquestração da saga
  Infrastructure/  # EF Core (PostgreSQL), mensageria (RabbitMQ/MassTransit), segurança
  Api/             # ASP.NET Core host, controllers, presenters, middlewares
  SharedKernel/     # Tipos cross-cutting (paginação, marcadores de DI)
```

Regras de dependência entre camadas garantidas por testes de arquitetura (NetArchTest) em `tests/Tests/Camadas`.

## Banco de dados

PostgreSQL (`soat_os`) — instância compartilhada provisionada pelo repositório [`infra-database`](https://github.com/monnclaro/soat-tech-challenge-infra-database), banco lógico e usuário próprios e isolados (nenhum outro serviço acessa este banco diretamente).

## Autenticação

Este é o **único serviço que emite tokens**: `POST /api/auth/login` valida as credenciais do back-office e emite um JWT com o segredo simétrico compartilhado (`JwtSettings:Secret`) — Billing Service e Execução Service apenas validam esse mesmo token (resource servers puros, sem login próprio).

## Endpoints principais

- `POST /api/auth/login` — login do back-office (email/senha).
- `api/v1/clientes`, `api/v1/clientes/{idCliente}/veiculos`, `api/v1/produtos`, `api/v1/servicos` — CRUD completo (catálogo), `[Authorize(Roles = "Admin")]`.
- `api/v1/ordens-servico` — abertura, consulta (por id, paginada, paginada por documento do cliente), histórico de status, entrega e remoção.
- `api/v1/ordens-servico/{id}/{iniciar-diagnostico,diagnostico,pagamento/aprovacao,cancelamento,finalizacao}` — os mesmos passos que os consumers RabbitMQ disparam automaticamente, também expostos como endpoints internos (Admin-only) para depuração/teste manual.

## Mensageria (RabbitMQ/MassTransit)

Contratos em `Soat.Contracts.Saga` (`src/Application/Messaging/Contracts/SagaContracts.cs`) — cópia idêntica mantida em cada um dos 3 repositórios (sem pacote NuGet compartilhado, para evitar a complexidade de um feed privado nesta fase do projeto). Fluxo:

1. Abrir OS → publica `IniciarDiagnostico` (consumido pelo Execução Service).
2. Execução Service publica `DiagnosticoFinalizado` (ou `DiagnosticoFalhou`) → este serviço consome, registra o diagnóstico e publica `GerarOrcamento` (consumido pelo Billing Service).
3. Billing Service publica `PagamentoAprovado`/`PagamentoRecusado` → este serviço consome e avança/cancela a OS; ao aprovar, publica `IniciarExecucao` (consumido pelo Execução Service).
4. Execução Service publica `ExecucaoFinalizada` → este serviço consome e finaliza a OS.

**Verificado de ponta a ponta com infraestrutura real** (RabbitMQ + PostgreSQL locais, sem mocks): um publisher standalone simulando Billing/Execução publicou `DiagnosticoFinalizado` → `PagamentoAprovado` → `ExecucaoFinalizada` na ordem, e a OS transitou corretamente `Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, com histórico de status persistido a cada passo — confirmando o consumo real das mensagens (não apenas os testes unitários com mocks).

## Status

- ✅ Domínio, EF Core (Postgres) e CRUD completo de Cliente/Veiculo/Produto/Servico/OrdemServico (Application + Api).
- ✅ Login do back-office (JWT).
- ✅ Mensageria RabbitMQ/MassTransit (consumers + publishers da saga), verificada de ponta a ponta contra broker e banco reais.
- ✅ Testes unitários de domínio, use cases da saga e handlers de mensageria + testes de arquitetura (NetArchTest).
- ✅ Manifests Kubernetes (`k8s/`) e pipeline CI/CD (build, testes com gate de cobertura ≥80%, SonarCloud, deploy).
- ⏳ BDD do fluxo completo da saga (em andamento).

## Rodando localmente

```bash
cp .env.example .env   # ajuste a senha do Postgres
docker compose up --build
```

API em `http://localhost:8081`, documentação OpenAPI (Scalar) em `/scalar` (ambiente de desenvolvimento), health check em `/health`.

## Testes

```bash
dotnet test
```

62/62 testes passando (unit + arquitetura). Cobertura de linha em ~80%+, com gate no CI (ver abaixo).

## CI/CD

`.github/workflows/ci-cd.yml`, mesmo padrão do monolito de origem (`soat-tech-challenge`), com 2 adições:

1. **Gate de cobertura (80%)** — `dotnet test` roda com Coverlet (`/p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line`), e o próprio comando falha (para o job) se a cobertura de linha total ficar abaixo de 80%.
2. **Quality Gate do SonarCloud** — `dotnet-sonarscanner begin/end` envolve o build e consome o relatório OpenCover gerado pelo Coverlet; `sonar.qualitygate.wait=true` faz o job falhar se o Quality Gate (bugs, vulnerabilidades, code smells, duplicação) for reprovado.

Em `pull_request`, roda só o job `build-test` (branches protegidas exigem esse check passando antes do merge). Em `push` para `main`, roda também o `deploy`: build da imagem, push no ECR, e `kubectl apply` dos manifests em `k8s/` no EKS (namespace `soat-os`, NodePort `30081`).

### Secrets necessários no repositório GitHub

| Secret | Uso |
|---|---|
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN` | Credenciais de sessão temporária da AWS Academy (deploy) |
| `SONAR_TOKEN` | Autenticação no SonarCloud (org `monnclaro`) |
| `NEW_RELIC_LICENSE_KEY` | Injetada no Secret do deployment |

### Proteção da branch `main`

Configuração manual no GitHub (Settings > Branches > Branch protection rules): exigir PR antes do merge, exigir que o check `Build, Test & Quality Gate` passe, sem push direto.
