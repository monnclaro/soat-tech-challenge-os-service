using Domain.Common.Events;
using Domain.OrdensServico.Produtos;

namespace Domain.OrdensServico.Events;

public sealed record OrdemServicoFinalizadaDomainEvent(
    Guid IdOrdemServico,
    IReadOnlyList<OrdemServicoProduto> Produtos
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}
