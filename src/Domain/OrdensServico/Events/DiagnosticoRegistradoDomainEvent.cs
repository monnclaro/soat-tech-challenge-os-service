using Domain.Common.Events;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;

namespace Domain.OrdensServico.Events;

// Dispara o próximo passo da saga (comando GerarOrcamento ao Billing Service)
// — ver Application/OrdensServico/EventHandlers/EnviarGerarOrcamentoHandler.
public sealed record DiagnosticoRegistradoDomainEvent(
    Guid IdOrdemServico,
    IReadOnlyList<OrdemServicoServico> Servicos,
    IReadOnlyList<OrdemServicoProduto> Produtos,
    decimal ValorTotal
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}
