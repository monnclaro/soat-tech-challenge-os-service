using Application.OrdensServico.UseCases.Cancelar;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Compensação: o Billing Service não conseguiu gerar o orçamento (ex.: Mercado
// Pago fora do ar) — cancela a OS em vez de deixá-la travada esperando um
// orçamento que nunca chega.
public class OrcamentoFalhouConsumer : IConsumer<OrcamentoFalhou>
{
    private readonly IOrdemServicoGateway _gateway;

    public OrcamentoFalhouConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<OrcamentoFalhou> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new CancelarUseCase(_gateway, outputPort);

        await useCase.Execute(new CancelarInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao cancelar por falha na geração do orçamento.");
        }
    }

    private sealed class CapturingOutputPort : ICancelarOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
