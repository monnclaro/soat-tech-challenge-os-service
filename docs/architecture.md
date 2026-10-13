# Arquitetura — OS Service

Documento complementar ao [README](../README.md), com mais profundidade sobre camadas,
modelo de domínio e o fluxo completo da saga (incluindo compensação). Decisão de design
específica (por que orquestração sem saga state machine separado): [ADR 0001](./adr/0001-saga-orquestrada-sem-state-machine-separado.md).

## Camadas (Clean Architecture)

```mermaid
graph TD
    Api["Api<br/>Controllers · Presenters · Middlewares"]
    App["Application<br/>UseCases · Controllers de aplicação · EventHandlers · Ports"]
    Dom["Domain<br/>OrdemServico, Cliente, Veiculo, Produto, Servico, Usuario · Domain Events"]
    Infra["Infrastructure<br/>EF Core/Postgres · MassTransit/RabbitMQ · JWT/BCrypt"]
    SK["SharedKernel<br/>Entity, IDomainEvent, marcadores de DI"]

    Api --> App
    App --> Dom
    Infra -.implementa ports.-> App
    Infra --> Dom
    Api -.-> SK
    App -.-> SK
    Dom -.-> SK
    Infra -.-> SK
```

Setas cheias = dependência de compilação. Setas pontilhadas = implementação de
interface (Infrastructure nunca é referenciada por Application/Domain — a regra é
garantida por testes de arquitetura, NetArchTest, em `tests/Tests/Camadas`).

## Modelo de domínio

```mermaid
classDiagram
    class OrdemServico {
        +Guid Id
        +Guid IdCliente
        +Guid IdVeiculo
        +StatusOrdemServico Status
        +decimal ValorTotal
        +Inserir(idCliente, idVeiculo)
        +IniciarDiagnostico()
        +RegistrarDiagnostico(servicos, produtos)
        +AprovarPagamento()
        +IniciarExecucao()
        +Cancelar()
        +Finalizar()
        +Entregar()
    }
    class HistoricoStatusOrdemServico {
        +StatusOrdemServico Status
        +DateTime AlteradoEm
    }
    class Cliente
    class Veiculo
    class Produto
    class Servico
    class Usuario

    OrdemServico "1" --> "*" HistoricoStatusOrdemServico : histórico
    OrdemServico --> Cliente : IdCliente
    OrdemServico --> Veiculo : IdVeiculo
```

`OrdemServico` é o único agregado que participa da saga — os demais (`Cliente`,
`Veiculo`, `Produto`, `Servico`, `Usuario`) são cadastro/catálogo, sem máquina de
estados distribuída.

### Máquina de estados de `OrdemServico`

```mermaid
stateDiagram-v2
    [*] --> Recebida: Inserir()
    Recebida --> EmDiagnostico: IniciarDiagnostico()
    EmDiagnostico --> AguardandoAprovacao: RegistrarDiagnostico()
    AguardandoAprovacao --> AguardandoExecucao: AprovarPagamento()
    AguardandoExecucao --> EmExecucao: IniciarExecucao()
    EmExecucao --> Finalizada: Finalizar()
    Finalizada --> Entregue: Entregar()

    EmDiagnostico --> Cancelada: Cancelar() [DiagnosticoFalhou]
    AguardandoAprovacao --> Cancelada: Cancelar() [OrcamentoFalhou / PagamentoRecusado]
    AguardandoExecucao --> Cancelada: Cancelar() [ExecucaoFalhou]
    EmExecucao --> Cancelada: Cancelar() [ExecucaoFalhou]
```

`Cancelada` é alcançável a partir de qualquer estado não-terminal — é o único estado
de compensação da saga (ver seção seguinte).

## Fluxo completo da saga (com compensação)

```mermaid
sequenceDiagram
    actor Cliente
    participant OS as OS Service
    participant Exec as Execução Service
    participant Bill as Billing Service
    participant MP as Mercado Pago

    Cliente->>OS: POST /api/v1/ordens-servico
    OS->>OS: Inserir() → Status=Recebida
    Note over Exec: Ação manual no Execução Service<br/>(PATCH .../diagnostico/iniciar) — sem<br/>disparo automático ao inserir a OS
    Exec->>Exec: Abrir() + IniciarDiagnostico()
    Exec-->>OS: DiagnosticoIniciado (evento)
    OS->>OS: IniciarDiagnostico() → EmDiagnostico

    alt Diagnóstico identifica o problema
        Exec-->>OS: DiagnosticoFinalizado (evento)
        OS->>OS: RegistrarDiagnostico() → AguardandoAprovacao
        OS-->>Bill: GerarOrcamento (comando)

        alt Mercado Pago responde
            Bill->>MP: Criar Preference (Checkout Pro)
            MP-->>Bill: init_point
            Bill-->>OS: OrcamentoGerado (evento)
            Cliente->>MP: Paga via link do Checkout Pro
            MP-->>Bill: Webhook de pagamento

            alt Pagamento aprovado
                Bill-->>OS: PagamentoAprovado (evento)
                OS->>OS: AprovarPagamento() → AguardandoExecucao
                Note over Exec: Ação manual, item a item<br/>(PATCH .../servicos/{id}/iniciar-execucao) — sem<br/>disparo automático ao aprovar o pagamento
                Exec->>Exec: IniciarExecucaoServico() no 1º item
                Exec-->>OS: ExecucaoIniciada (evento)
                OS->>OS: IniciarExecucao() → EmExecucao
                Exec-->>OS: ExecucaoFinalizada (evento)
                OS->>OS: Finalizar() → Finalizada
            else Pagamento recusado
                Bill-->>OS: PagamentoRecusado (evento)
                Note over OS: Compensação
                OS->>OS: Cancelar() → Cancelada
            end
        else Mercado Pago falha (rede, API fora do ar)
            Bill-->>OS: OrcamentoFalhou (evento)
            Note over OS: Compensação
            OS->>OS: Cancelar() → Cancelada
        end

        opt Falha durante a execução (ex.: peça indisponível)
            Exec-->>OS: ExecucaoFalhou (evento)
            Note over OS: Compensação
            OS->>OS: Cancelar() → Cancelada
        end
    else Veículo não atendível
        Exec-->>OS: DiagnosticoFalhou (evento)
        Note over OS: Compensação
        OS->>OS: Cancelar() → Cancelada
    end
```

Os 4 caminhos de compensação (`DiagnosticoFalhou`, `OrcamentoFalhou`,
`PagamentoRecusado`, `ExecucaoFalhou`) convergem todos no mesmo `CancelarUseCase` —
tabela completa e justificativa de design no [ADR 0001](./adr/0001-saga-orquestrada-sem-state-machine-separado.md).

## Mensageria — comandos e consumers

| Mensagem | Tipo | Direção | Consumer/Handler |
|---|---|---|---|
| `GerarOrcamento` | Comando | OS → Billing | (consumido no Billing Service) |
| `DiagnosticoIniciado` | Evento | Execução → OS | `DiagnosticoIniciadoConsumer` |
| `DiagnosticoFinalizado` | Evento | Execução → OS | `DiagnosticoFinalizadoConsumer` |
| `DiagnosticoFalhou` | Evento (compensação) | Execução → OS | `DiagnosticoFalhouConsumer` |
| `OrcamentoGerado` | Evento | Billing → OS | (não consumido pelo OS hoje — informativo) |
| `OrcamentoFalhou` | Evento (compensação) | Billing → OS | `OrcamentoFalhouConsumer` |
| `PagamentoAprovado` | Evento | Billing → OS | `PagamentoAprovadoConsumer` |
| `PagamentoRecusado` | Evento (compensação) | Billing → OS | `PagamentoRecusadoConsumer` |
| `ExecucaoIniciada` | Evento | Execução → OS | `ExecucaoIniciadaConsumer` |
| `ExecucaoFinalizada` | Evento | Execução → OS | `ExecucaoFinalizadaConsumer` |
| `ExecucaoFalhou` | Evento (compensação) | Execução → OS | `ExecucaoFalhouConsumer` |

Diagnóstico e execução são exceção ao padrão comando/evento: nenhum dos dois é
disparado por um comando automático da OS — ambos são sempre ação manual no
Execução Service, só confirmados de volta via evento (`DiagnosticoIniciado` /
`ExecucaoIniciada`).

Todos os consumers são adaptadores finos: constroem o use case correspondente (o
mesmo usado pelos endpoints REST internos) e traduzem "não encontrada" em exceção
(o MassTransit trata isso como falha de entrega/retry). Nenhuma regra de negócio
vive na camada de mensageria.

## Persistência

PostgreSQL via EF Core, banco lógico `soat_os` isolado (ver README > "Banco de
dados"). Migração inicial gerada em `src/Infrastructure/Database/Migrations`. O
histórico de status (`HistoricoStatusOrdemServico`) é gravado a cada transição —
usado tanto para auditoria quanto para o endpoint `GET .../historico`.

## Segurança

Único serviço dos 3 que emite JWT (login do back-office). Os outros dois validam o
mesmo token com o segredo simétrico compartilhado — ver README > "Autenticação".
