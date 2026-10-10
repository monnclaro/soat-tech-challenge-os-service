using Application.OrdensServico.UseCases.RegistrarLinkPagamento;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Consome o evento publicado pelo Billing Service ao gerar o orçamento com sucesso —
// guarda o link de pagamento (Mercado Pago) na OS, já que o Billing Service não o
// persiste em lugar nenhum consultável depois da criação (só a preferência).
public class OrcamentoGeradoConsumer : IConsumer<OrcamentoGerado>
{
    private readonly IOrdemServicoGateway _gateway;

    public OrcamentoGeradoConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<OrcamentoGerado> context)
    {
        var msg = context.Message;
        var outputPort = new CapturingOutputPort();
        var useCase = new RegistrarLinkPagamentoUseCase(_gateway, outputPort);

        await useCase.Execute(new RegistrarLinkPagamentoInput(msg.IdOrdemServico, msg.LinkPagamento), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{msg.IdOrdemServico}' não encontrada ao registrar o link de pagamento.");
        }
    }

    private sealed class CapturingOutputPort : IRegistrarLinkPagamentoOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
