# SOAT — OS Service

[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=soat-tech-challenge-os-service&metric=alert_status&token=b51d0baff88e97c630f1490cf09ef5f12f1c5c3d)](https://sonarcloud.io/summary/new_code?id=soat-tech-challenge-os-service)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=soat-tech-challenge-os-service&metric=coverage&token=b51d0baff88e97c630f1490cf09ef5f12f1c5c3d)](https://sonarcloud.io/summary/new_code?id=soat-tech-challenge-os-service)

Microsserviço responsável pela **Ordem de Serviço (OS)** dentro da arquitetura de microsserviços da Fase 4 do Tech Challenge (FIAP). Extraído do monolito [`soat-tech-challenge`](https://github.com/monnclaro/soat-tech-challenge), que permanece como referência histórica das Fases 1-3.

## Responsabilidades

- Abertura de Ordem de Serviço.
- Consulta de status e histórico.
- **Orquestração da saga** (abrir OS → diagnóstico → orçamento → aprovação de pagamento → execução → finalização), coordenando o [Billing Service](https://github.com/monnclaro/soat-tech-challenge-billing-service) e o [Execution Service](https://github.com/monnclaro/soat-tech-challenge-execution-service) via RabbitMQ/MassTransit — orquestração feita por reação a domain events (não um saga state machine separado): o próprio agregado `OrdemServico` guarda o estado da saga, handlers de domain event publicam os comandos (`IniciarDiagnostico`, `GerarOrcamento`, `IniciarExecucao`) e consumers MassTransit traduzem os eventos recebidos (`DiagnosticoFinalizado`, `DiagnosticoFalhou`, `OrcamentoFalhou`, `PagamentoAprovado`, `PagamentoRecusado`, `ExecucaoFinalizada`, `ExecucaoFalhou`) diretamente para os use cases já existentes.
- Cadastro de Clientes, Veículos, Produtos e Serviços (catálogo) — mantido aqui por ser o "dono" do agregado raiz `OrdemServico` e o autenticador do back-office.

## Papel na saga (orquestrador)

Este serviço é o **orquestrador da saga**, mas sem um saga state machine separado: o próprio agregado `OrdemServico` guarda o estado (`Status`), handlers de domain event publicam os comandos e consumers MassTransit traduzem os eventos recebidos diretamente para os use cases já existentes (mesma regra de negócio usada pelos endpoints REST internos — nenhuma lógica duplicada entre as duas vias de entrada).

| Passo | Mensagem publicada por este serviço | Consumida por | Reação deste serviço ao evento de volta |
|---|---|---|---|
| 1 | `IniciarDiagnostico` (comando) | Execução Service | — |
| 2 | — | — | `DiagnosticoFinalizado` → registra o diagnóstico e publica `GerarOrcamento`; **`DiagnosticoFalhou` → cancela a OS (compensação)** |
| 3 | `GerarOrcamento` (comando) | Billing Service | — |
| 4 | — | — | `PagamentoAprovado` → publica `IniciarExecucao`; **`OrcamentoFalhou`/`PagamentoRecusado` → cancela a OS (compensação)** |
| 5 | `IniciarExecucao` (comando) | Execução Service | — |
| 6 | — | — | `ExecucaoFinalizada` → finaliza a OS; **`ExecucaoFalhou` → cancela a OS (compensação)** |

Cada evento de compensação (`DiagnosticoFalhou`, `OrcamentoFalhou`, `PagamentoRecusado`, `ExecucaoFalhou`) tem seu próprio consumer (`*Consumer.cs` em `src/Infrastructure/Messaging/Consumers/`), todos adaptadores finos que chamam o mesmo `CancelarUseCase` — nenhum precisa saber sobre os outros. Justificativa completa dessa escolha de design (por que não um saga state machine dedicado, tabela completa de compensação): [ADR 0001](./docs/adr/0001-saga-orquestrada-sem-state-machine-separado.md).

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

Documentação completa (diagramas de camadas, modelo de domínio, máquina de estados, sequência completa da saga com compensação, mensageria): [docs/architecture.md](./docs/architecture.md).

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
2. Execução Service publica `DiagnosticoFinalizado` → este serviço registra o diagnóstico e publica `GerarOrcamento` (consumido pelo Billing Service); ou publica `DiagnosticoFalhou` → este serviço cancela a OS (compensação).
3. Billing Service publica `PagamentoAprovado` → este serviço publica `IniciarExecucao` (consumido pelo Execução Service); ou publica `OrcamentoFalhou`/`PagamentoRecusado` → este serviço cancela a OS (compensação).
4. Execução Service publica `ExecucaoFinalizada` → este serviço finaliza a OS; ou publica `ExecucaoFalhou` → este serviço cancela a OS (compensação).

**Verificado de ponta a ponta com infraestrutura real** (RabbitMQ + PostgreSQL locais, sem mocks): um publisher standalone simulando Billing/Execução publicou `DiagnosticoFinalizado` → `PagamentoAprovado` → `ExecucaoFinalizada` na ordem, e a OS transitou corretamente `Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, com histórico de status persistido a cada passo — confirmando o consumo real das mensagens (não apenas os testes unitários com mocks).

## Status

- ✅ Domínio, EF Core (Postgres) e CRUD completo de Cliente/Veiculo/Produto/Servico/OrdemServico (Application + Api).
- ✅ Login do back-office (JWT).
- ✅ Mensageria RabbitMQ/MassTransit (consumers + publishers da saga), verificada de ponta a ponta contra broker e banco reais.
- ✅ Testes unitários de domínio, use cases da saga e handlers de mensageria + testes de arquitetura (NetArchTest) — 193 testes, ~91% de cobertura de linha.
- ✅ Manifests Kubernetes (`k8s/`) e pipeline CI/CD (build, testes com gate de cobertura ≥80%, SonarCloud, deploy).
- ✅ BDD do fluxo completo da saga (Reqnroll — cenário feliz e o caminho de compensação com pagamento recusado).

## Rodando localmente

```bash
cp .env.example .env   # ajuste a senha do Postgres
docker compose up --build
```

API em `http://localhost:8081`, documentação OpenAPI (Scalar) em `/scalar` (ambiente de desenvolvimento), health check em `/health`.

Especificação OpenAPI (Swagger) exportada em [`docs/openapi.json`](./docs/openapi.json) — importável direto no Postman (File > Import) ou em qualquer ferramenta compatível com OpenAPI 3. Com a API rodando localmente, a versão sempre atualizada também fica disponível em `/openapi/v1.json`.

## Testes e cobertura

```bash
dotnet test
```

BDD (Reqnroll) do fluxo completo da saga em [`tests/Tests/Features/SagaOrdemServico.feature`](tests/Tests/Features/SagaOrdemServico.feature) — cenário feliz (`Recebida → ... → Finalizada`) e caminho de compensação (pagamento recusado → `Cancelada`).

### Evidência de cobertura

**197/197 testes passando** (unit + arquitetura + BDD), gerado localmente com Coverlet
(`dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=opencover`):

| Módulo | Linha | Branch | Método |
|---|---|---|---|
| Api | 91,97% | 100% | 90,38% |
| Application | 98,15% | 100% | 94,09% |
| Domain | 93,71% | 76,27% | 98,38% |
| Infrastructure | 76,54% | 70,58% | 90,62% |
| SharedKernel | 55,55% | 100% | 55,55% |
| **Total** | **91,21%** | **84,61%** | **92,81%** |

Cobertura contínua (atualizada a cada push em `main`) nos badges no topo deste README
e no [dashboard do SonarCloud](https://sonarcloud.io/summary/new_code?id=soat-tech-challenge-os-service)
— o Quality Gate falha o CI se a cobertura de código novo cair, além do gate fixo de
80% aplicado pelo Coverlet (ver "CI/CD" abaixo).

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
