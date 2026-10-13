using Soat.Contracts.Saga;

namespace Application.Common.Interfaces;

// Port de saída da saga (Application não conhece RabbitMQ/MassTransit — só
// este contrato; a implementação real mora em Infrastructure.Messaging).
public interface ISagaCommandBus
{
    Task EnviarGerarOrcamento(
        Guid idOrdemServico,
        IReadOnlyList<ItemServicoDiagnosticado> servicos,
        IReadOnlyList<ItemProdutoDiagnosticado> produtos,
        decimal valorTotal,
        CancellationToken ct = default);
}
