using Application.OrdensServico.UseCases.Cancelar;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Compensação: o Execução Service concluiu que o veículo não é atendível
// durante o diagnóstico — cancela a OS antes de qualquer orçamento ser gerado.
public class DiagnosticoFalhouConsumer : IConsumer<DiagnosticoFalhou>
{
    private readonly IOrdemServicoGateway _gateway;

    public DiagnosticoFalhouConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<DiagnosticoFalhou> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new CancelarUseCase(_gateway, outputPort);

        await useCase.Execute(new CancelarInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao cancelar por falha de diagnóstico.");
        }
    }

    private sealed class CapturingOutputPort : ICancelarOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
