using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.Entregar;

public class EntregarUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IEntregarOutputPort _outputPort;

    public EntregarUseCase(IOrdemServicoGateway gateway, IEntregarOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(EntregarInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.Entregar();
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
