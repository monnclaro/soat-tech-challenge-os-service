# SOAT — OS Service

Microsserviço responsável pela **Ordem de Serviço (OS)** dentro da arquitetura de microsserviços da Fase 4 do Tech Challenge (FIAP). Extraído do monolito [`soat-tech-challenge`](https://github.com/monnclaro/soat-tech-challenge), que permanece como referência histórica das Fases 1-3.

Plano completo da migração (arquitetura, saga, infraestrutura, ordem de execução): [`PLANO-FASE-4-MICROSSERVICOS.md`](../PLANO-FASE-4-MICROSSERVICOS.md) na raiz do workspace.

## Responsabilidades

- Abertura de Ordem de Serviço.
- Consulta de status e histórico.
- **Orquestração da saga** (abrir OS → diagnóstico → orçamento → aprovação de pagamento → execução → finalização), coordenando o [Billing Service](https://github.com/monnclaro/soat-tech-challenge-billing-service) e o [Execution Service](https://github.com/monnclaro/soat-tech-challenge-execution-service) via RabbitMQ/MassTransit — orquestração feita por reação a domain events (não um saga state machine separado): o próprio agregado `OrdemServico` guarda o estado da saga, handlers de domain event publicam os comandos (`IniciarDiagnostico`, `GerarOrcamento`, `IniciarExecucao`) e consumers MassTransit traduzem os eventos recebidos (`DiagnosticoFinalizado`, `DiagnosticoFalhou`, `PagamentoAprovado`, `PagamentoRecusado`, `ExecucaoFinalizada`) diretamente para os use cases já existentes.
- Cadastro de Clientes, Veículos, Produtos e Serviços (catálogo) — mantido aqui por ser o "dono" do agregado raiz `OrdemServico` e o autenticador do back-office.

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

## Endpoints principais

- `POST /api/auth/login` — login do back-office (email/senha).
- `api/v1/clientes`, `api/v1/clientes/{idCliente}/veiculos`, `api/v1/produtos`, `api/v1/servicos` — CRUD completo (catálogo), `[Authorize(Roles = "Admin")]`.
- `api/v1/ordens-servico` — abertura, consulta (por id, paginada, paginada por documento do cliente), histórico de status, entrega e remoção.
- `api/v1/ordens-servico/{id}/{iniciar-diagnostico,diagnostico,pagamento/aprovacao,cancelamento,finalizacao}` — os mesmos passos que os consumers RabbitMQ disparam automaticamente, também expostos como endpoints internos (Admin-only) para depuração/teste manual.

## Mensageria (RabbitMQ/MassTransit)

Contratos em `Soat.Contracts.Saga` (`src/Application/Messaging/Contracts/SagaContracts.cs`) — cópia idêntica mantida em cada um dos 3 repositórios (sem pacote NuGet compartilhado, decisão documentada no plano). Fluxo:

1. Abrir OS → publica `IniciarDiagnostico` (consumido pelo Execução Service).
2. Execução Service publica `DiagnosticoFinalizado` (ou `DiagnosticoFalhou`) → este serviço consome, registra o diagnóstico e publica `GerarOrcamento` (consumido pelo Billing Service).
3. Billing Service publica `PagamentoAprovado`/`PagamentoRecusado` → este serviço consome e avança/cancela a OS; ao aprovar, publica `IniciarExecucao` (consumido pelo Execução Service).
4. Execução Service publica `ExecucaoFinalizada` → este serviço consome e finaliza a OS.

**Verificado de ponta a ponta com infraestrutura real** (RabbitMQ + PostgreSQL locais, sem mocks): um publisher standalone simulando Billing/Execução publicou `DiagnosticoFinalizado` → `PagamentoAprovado` → `ExecucaoFinalizada` na ordem, e a OS transitou corretamente `Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, com histórico de status persistido a cada passo — confirmando o consumo real das mensagens (não apenas os testes unitários com mocks).

## Status

- ✅ Domínio, EF Core (Postgres) e CRUD completo de Cliente/Veiculo/Produto/Servico/OrdemServico (Application + Api).
- ✅ Login do back-office (JWT).
- ✅ Mensageria RabbitMQ/MassTransit (consumers + publishers da saga), verificada de ponta a ponta contra broker e banco reais.
- ✅ Testes unitários de domínio, use cases da saga e handlers de mensageria + testes de arquitetura (NetArchTest) — 62/62 passando.
- ⏳ BDD do fluxo completo, gate de cobertura ≥80%, SonarCloud, k8s manifests, CI/CD.
- ⏳ Billing Service e Execução Service ainda não têm sua própria mensageria ligada (scaffold completo, ver seus respectivos READMEs) — a saga completa entre os 3 serviços depende disso.

Ver [`PLANO-FASE-4-MICROSSERVICOS.md`](../PLANO-FASE-4-MICROSSERVICOS.md) para a ordem de execução completa.

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
