using Application.Common.Interfaces;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Events;

namespace Application.OrdensServico.EventHandlers;

// Passo 5 -> 6 da saga: ao aprovar o pagamento (PagamentoAprovado, consumido
// do Billing Service), comanda o Execução Service a iniciar a execução.
// Reaproveita o evento genérico de mudança de status (já usado para log e
// histórico) em vez de um evento dedicado, já que
// nenhum dado extra além do novo status é necessário aqui.
internal sealed class EnviarIniciarExecucaoHandler : IDomainEventHandler<OrdemServicoStatusAlteradoDomainEvent>
{
    private readonly ISagaCommandBus _bus;

    public EnviarIniciarExecucaoHandler(ISagaCommandBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(OrdemServicoStatusAlteradoDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.Status != StatusOrdemServico.EmExecucao)
        {
            return;
        }

        await _bus.EnviarIniciarExecucao(domainEvent.IdOrdemServico, cancellationToken);
    }
}
