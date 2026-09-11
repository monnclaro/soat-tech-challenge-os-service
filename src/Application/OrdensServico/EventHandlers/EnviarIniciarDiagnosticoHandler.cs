using Application.Common.Interfaces;
using Domain.OrdensServico.Events;

namespace Application.OrdensServico.EventHandlers;

// Passo 1 -> 2 da saga: ao abrir a OS, comanda o Execução Service a iniciar o
// diagnóstico — ver PLANO-FASE-4-MICROSSERVICOS.md.
internal sealed class EnviarIniciarDiagnosticoHandler : IDomainEventHandler<OrdemServicoAbertaDomainEvent>
{
    private readonly ISagaCommandBus _bus;

    public EnviarIniciarDiagnosticoHandler(ISagaCommandBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(OrdemServicoAbertaDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await _bus.EnviarIniciarDiagnostico(domainEvent.IdOrdemServico, domainEvent.IdCliente, domainEvent.IdVeiculo, cancellationToken);
    }
}
