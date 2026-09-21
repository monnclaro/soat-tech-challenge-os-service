using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.Remover;

public class RemoverOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IRemoverOrdemServicoOutputPort _outputPort;

    public RemoverOrdemServicoUseCase(IOrdemServicoGateway gateway, IRemoverOrdemServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverOrdemServicoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        await _gateway.Remover(ordemServico, ct);
        _outputPort.Ok();
    }
}
