# SOAT — OS Service

Microsserviço responsável pela **Ordem de Serviço (OS)** dentro da arquitetura de microsserviços da Fase 4 do Tech Challenge (FIAP). Extraído do monolito [`soat-tech-challenge`](https://github.com/monnclaro/soat-tech-challenge), que permanece como referência histórica das Fases 1-3.

Plano completo da migração (arquitetura, saga, infraestrutura, ordem de execução): [`PLANO-FASE-4-MICROSSERVICOS.md`](../PLANO-FASE-4-MICROSSERVICOS.md) na raiz do workspace.

## Responsabilidades

- Abertura de Ordem de Serviço.
- Consulta de status e histórico.
- **Orquestração da saga** (abrir OS → orçamento → aprovação de pagamento → execução → finalização), coordenando o [Billing Service](https://github.com/monnclaro/soat-tech-challenge-billing-service) e o [Execution Service](https://github.com/monnclaro/soat-tech-challenge-execution-service) via RabbitMQ.
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
- `api/v1/ordens-servico/{id}/{iniciar-diagnostico,diagnostico,pagamento/aprovacao,cancelamento,finalizacao}` — passos da saga, hoje endpoints internos (Admin-only) chamados manualmente; substituídos por consumers RabbitMQ/MassTransit quando a mensageria for ligada.

## Status

- ✅ Domínio, EF Core (Postgres) e CRUD completo de Cliente/Veiculo/Produto/Servico/OrdemServico (Application + Api).
- ✅ Login do back-office (JWT).
- ✅ Testes unitários de domínio + testes de arquitetura (NetArchTest).
- ⏳ Saga real via RabbitMQ/MassTransit (hoje os passos são endpoints internos — ver comentários no `OrdemServicosController`).
- ⏳ BDD do fluxo completo, gate de cobertura, SonarCloud, Dockerfile validado em cluster, k8s manifests, CI/CD.

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
