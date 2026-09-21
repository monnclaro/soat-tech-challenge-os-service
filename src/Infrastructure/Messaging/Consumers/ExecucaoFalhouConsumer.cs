using Application.OrdensServico.UseCases.Cancelar;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Compensação: o Execução Service não conseguiu concluir a execução (ex.: peça
// indisponível) — cancela a OS. Diferente de DiagnosticoFalhouConsumer, que
// cobre a falha ainda na fase de diagnóstico.
public class ExecucaoFalhouConsumer : IConsumer<ExecucaoFalhou>
{
    private readonly IOrdemServicoGateway _gateway;

    public ExecucaoFalhouConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<ExecucaoFalhou> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new CancelarUseCase(_gateway, outputPort);

        await useCase.Execute(new CancelarInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao cancelar por falha na execução.");
        }
    }

    private sealed class CapturingOutputPort : ICancelarOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
