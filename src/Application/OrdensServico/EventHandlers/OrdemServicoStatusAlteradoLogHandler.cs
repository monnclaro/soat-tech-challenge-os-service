using Application.Common.Interfaces;
using Domain.OrdensServico.Events;
using Microsoft.Extensions.Logging;

namespace Application.OrdensServico.EventHandlers;

// Log estruturado a cada transição de status — coletado pelo Fluent Bit do
// nri-bundle (infra-k8s) e correlacionável no New Relic via NRQL (FROM Log),
// mesmo padrão já validado no monolito de origem.
internal sealed class OrdemServicoStatusAlteradoLogHandler : IDomainEventHandler<OrdemServicoStatusAlteradoDomainEvent>
{
    private readonly ILogger<OrdemServicoStatusAlteradoLogHandler> _logger;

    public OrdemServicoStatusAlteradoLogHandler(ILogger<OrdemServicoStatusAlteradoLogHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrdemServicoStatusAlteradoDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "OrdemServicoStatusAlterado {idOrdemServico} {status}",
            domainEvent.IdOrdemServico,
            domainEvent.Status);

        return Task.CompletedTask;
    }
}
