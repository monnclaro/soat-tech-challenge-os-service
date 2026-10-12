using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Consome o evento publicado pelo Execução Service ao abrir a fila de execução
// (confirmação do comando IniciarDiagnostico) — só então a OS avança de
// Recebida para EmDiagnostico.
public class DiagnosticoIniciadoConsumer : IConsumer<DiagnosticoIniciado>
{
    private readonly IOrdemServicoGateway _gateway;

    public DiagnosticoIniciadoConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<DiagnosticoIniciado> context)
    {
        var outputPort = new CapturingOutputPort();
        var useCase = new IniciarDiagnosticoUseCase(_gateway, outputPort);

        await useCase.Execute(new IniciarDiagnosticoInput(context.Message.IdOrdemServico), context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{context.Message.IdOrdemServico}' não encontrada ao confirmar início do diagnóstico.");
        }
    }

    private sealed class CapturingOutputPort : IIniciarDiagnosticoOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
