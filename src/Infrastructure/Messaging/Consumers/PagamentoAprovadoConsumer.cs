using Application.OrdensServico.UseCases.AprovarPagamento;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

public class PagamentoAprovadoConsumer : IConsumer<PagamentoAprovado>
{
    private readonly IOrdemServicoGateway _gateway;

    public PagamentoAprovadoConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<PagamentoAprovado> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new AprovarPagamentoUseCase(_gateway, outputPort);

        await useCase.Execute(new AprovarPagamentoInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao aprovar pagamento.");
        }
    }

    private sealed class CapturingOutputPort : IAprovarPagamentoOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
