using Application.OrdensServico.UseCases.Cancelar;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Compensação: pagamento recusado (ou expirado) pelo Billing Service — cancela a OS.
public class PagamentoRecusadoConsumer : IConsumer<PagamentoRecusado>
{
    private readonly IOrdemServicoGateway _gateway;

    public PagamentoRecusadoConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<PagamentoRecusado> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new CancelarUseCase(_gateway, outputPort);

        await useCase.Execute(new CancelarInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao cancelar por pagamento recusado.");
        }
    }

    private sealed class CapturingOutputPort : ICancelarOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
