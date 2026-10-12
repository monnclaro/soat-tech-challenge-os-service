using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.IniciarDiagnostico;

// Consumido a partir do evento DiagnosticoIniciado (Execução Service) — a OS só
// avança de Recebida para EmDiagnostico quando o Execução Service confirma que
// a fila de execução abriu de verdade (ver DiagnosticoIniciadoConsumer).
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
