using Domain.Common.Events;
using Domain.OrdensServico.Enums;

namespace Domain.OrdensServico.Events;

// OcurredAt sozinho já é suficiente para reconstruir a duração de cada fase
// (ver HistoricoStatusOrdemServico) — mesmo padrão de auditoria do monolito de origem.
public sealed record OrdemServicoStatusAlteradoDomainEvent(
    Guid IdOrdemServico,
    StatusOrdemServico Status
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}
