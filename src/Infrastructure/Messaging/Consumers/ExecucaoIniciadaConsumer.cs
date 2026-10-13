using Application.OrdensServico.UseCases.IniciarExecucao;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Consome o evento publicado pelo Execução Service ao primeiro serviço entrar
// em execução (confirmação de uma ação manual lá) — só então a OS avança de
// AguardandoExecucao para EmExecucao.
public class ExecucaoIniciadaConsumer : IConsumer<ExecucaoIniciada>
{
    private readonly IOrdemServicoGateway _gateway;

    public ExecucaoIniciadaConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<ExecucaoIniciada> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new IniciarExecucaoUseCase(_gateway, outputPort);

        await useCase.Execute(new IniciarExecucaoInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao confirmar início da execução.");
        }
    }

    private sealed class CapturingOutputPort : IIniciarExecucaoOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
