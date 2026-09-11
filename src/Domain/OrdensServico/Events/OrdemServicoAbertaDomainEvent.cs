using Domain.Common.Events;

namespace Domain.OrdensServico.Events;

// Dispara o primeiro passo da saga (comando IniciarDiagnostico ao Execução
// Service) — ver Application/OrdensServico/EventHandlers/EnviarIniciarDiagnosticoHandler.
public sealed record OrdemServicoAbertaDomainEvent(
    Guid IdOrdemServico,
    Guid IdCliente,
    Guid IdVeiculo
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}
