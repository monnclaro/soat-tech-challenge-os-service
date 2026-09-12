using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.IniciarDiagnostico;

// Passo 2 da saga: hoje disparado manualmente via endpoint interno; quando o
// RabbitMQ/MassTransit entrar, isso vira reação a um evento próprio (ex.:
// "OrdemServicoCriada") já consumido pelo orquestrador, que então publica o
// comando "IniciarDiagnostico" para o Execução Service.
public class IniciarDiagnosticoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IIniciarDiagnosticoOutputPort _outputPort;

    public IniciarDiagnosticoUseCase(IOrdemServicoGateway gateway, IIniciarDiagnosticoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(IniciarDiagnosticoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.IniciarDiagnostico();
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
