# ADR 0001 — Saga Orquestrada sem Saga State Machine Separado

**Status:** Aceito

## Contexto

A Ordem de Serviço (OS) atravessa 3 microsserviços independentes até ser finalizada:

1. **OS Service** (este repositório) — abre a OS, é o dono do agregado `OrdemServico` e do seu ciclo de vida.
2. **Execução Service** — diagnostica o veículo e executa os serviços/produtos identificados.
3. **Billing Service** — gera o orçamento e processa o pagamento (Mercado Pago).

O enunciado exige um **Saga Pattern** (orquestrado ou coreografado) com rollback/compensação para coordenar essa transação distribuída, já que não existe uma transação de banco de dados única cruzando os 3 serviços.

A implementação mais comum de saga orquestrada usa um **saga state machine** dedicado (ex.: `MassTransit.StateMachine`/`MassTransitStateMachineSaga`, ou uma tabela própria "SagaState"), com uma tabela de persistência própria para o estado da saga, separada das entidades de domínio. Consideramos essa abordagem e optamos por **não** usá-la.

## Decisão

O **próprio agregado `OrdemServico`** (Domain) é o estado da saga — seu campo `Status` (`Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue`, com `Cancelada` como compensação) já registra exatamente em que ponto do fluxo distribuído aquela OS está. Não existe uma tabela/entidade de saga separada.

A orquestração acontece assim:

1. Os **use cases existentes** (`InserirOrdemServicoUseCase`, `RegistrarDiagnosticoUseCase`, `AprovarPagamentoUseCase`, etc.) continuam sendo a única fonte de verdade da regra de negócio — chamados tanto por controllers REST quanto por consumers de mensageria.
2. Ao mudar de estado, o agregado levanta **domain events** (`OrdemServicoAbertaDomainEvent`, `DiagnosticoRegistradoDomainEvent`, `OrdemServicoStatusAlteradoDomainEvent`, ...).
3. **Handlers de domain event** (`src/Application/OrdensServico/EventHandlers/*`) reagem a esses eventos publicando os **comandos da saga** (`IniciarDiagnostico`, `GerarOrcamento`, `IniciarExecucao`) via `ISagaCommandBus` (implementado com MassTransit/RabbitMQ).
4. Os outros 2 serviços reagem a esses comandos e publicam **eventos da saga** de volta (`DiagnosticoFinalizado`/`DiagnosticoFalhou`, `OrcamentoGerado`, `PagamentoAprovado`/`PagamentoRecusado`, `ExecucaoFinalizada`).
5. **Consumers MassTransit** (`src/Infrastructure/Messaging/Consumers/*`) traduzem esses eventos recebidos diretamente em chamadas aos use cases já existentes — nenhuma lógica de negócio duplicada entre a via REST e a via mensageria.

Os contratos de mensagem (`src/Application/Messaging/Contracts/SagaContracts.cs`) marcam explicitamente cada tipo com `ISagaCommand` ou `ISagaEvent` — interfaces vazias, sem efeito em runtime, cujo único propósito é deixar a intenção de cada mensagem explícita no próprio tipo (comando imperativo vs. evento já ocorrido), em vez de depender só de convenção de nome ou comentário.

### Compensação (rollback)

`PagamentoRecusado` é o caminho de compensação: o OS Service reage cancelando a OS (`CancelarUseCase`, `Status = Cancelada`) em vez de avançar para `IniciarExecucao`. Não há necessidade de desfazer nada no Execução Service (o diagnóstico já registrado continua válido como histórico) nem no Billing Service (o `Pagamento`/`Orcamento` ficam com `Status = Recusado`/`Reprovado`, não são apagados) — a compensação é **avançar a OS para um estado terminal de cancelamento**, não literalmente reverter dados dos outros serviços.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Saga state machine dedicado (MassTransit `MassTransitStateMachine`) com tabela própria | Duplicaria o estado (o `Status` de `OrdemServico` já é o estado da saga) e criaria 2 fontes de verdade para a mesma coisa — risco de dessincronia entre "estado da saga" e "estado da OS" |
| Saga coreografada (cada serviço decide sozinho o próximo passo, sem orquestrador central) | O enunciado permite ambas, mas coreografia espalha a lógica de "o que acontece depois" pelos 3 serviços — mais difícil de auditar/entender o fluxo completo lendo um único lugar |

## Consequências

- O OS Service é o único lugar onde o fluxo completo da saga pode ser lido de ponta a ponta (os handlers de domain event + os consumers, todos neste repositório) — Execução Service e Billing Service só conhecem os 2 passos que lhes dizem respeito, não a saga inteira.
- Reaproveitar os use cases existentes como consumers de mensageria significa que a mesma regra de negócio é exercitada tanto pelos endpoints REST internos (mantidos para depuração/teste manual) quanto pela mensageria real — sem duplicação, mas também sem uma abstração dedicada de "saga" que apareça como tal no código: quem não conhece o fluxo completo (este ADR) pode não perceber de imediato que os handlers de domain event + consumers, somados, implementam uma saga.
- Adicionar um novo passo à saga (ex.: uma nova etapa de aprovação) significa adicionar um novo `Status`, um novo domain event, um novo handler que publica o comando correspondente, e um novo consumer no serviço de destino — mais pontos de código tocados do que uma state machine centralizada, mas cada um pequeno e testável isoladamente.
