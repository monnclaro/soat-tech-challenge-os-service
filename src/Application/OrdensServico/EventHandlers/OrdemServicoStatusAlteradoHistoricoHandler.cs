using Application.Common.Interfaces;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Historico;
using Domain.OrdensServico.Historico.Gateways;

namespace Application.OrdensServico.EventHandlers;

// Novo na Fase 4 — persiste cada transição de status (além do log estruturado
// já emitido por OrdemServicoStatusAlteradoLogHandler), alimentando o novo
// endpoint de consulta de histórico (GET /ordens-servico/{id}/historico).
internal sealed class OrdemServicoStatusAlteradoHistoricoHandler : IDomainEventHandler<OrdemServicoStatusAlteradoDomainEvent>
{
    private readonly IHistoricoStatusOrdemServicoGateway _gateway;

    public OrdemServicoStatusAlteradoHistoricoHandler(IHistoricoStatusOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Handle(OrdemServicoStatusAlteradoDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var historico = new HistoricoStatusOrdemServico(
            domainEvent.IdOrdemServico,
            domainEvent.Status,
            domainEvent.OcurredAt);

        await _gateway.Salvar(historico, cancellationToken);
    }
}
