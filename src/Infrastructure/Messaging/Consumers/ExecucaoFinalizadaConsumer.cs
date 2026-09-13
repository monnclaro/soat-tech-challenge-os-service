using Application.OrdensServico.UseCases.Finalizar;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

public class ExecucaoFinalizadaConsumer : IConsumer<ExecucaoFinalizada>
{
    private readonly IOrdemServicoGateway _gateway;

    public ExecucaoFinalizadaConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<ExecucaoFinalizada> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new FinalizarUseCase(_gateway, outputPort);

        await useCase.Execute(new FinalizarInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao finalizar.");
        }
    }

    private sealed class CapturingOutputPort : IFinalizarOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
