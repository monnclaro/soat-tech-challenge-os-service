using Application.Common.Interfaces;
using Domain.Clientes.Veiculos.Gateways;

namespace Application.Clientes.Veiculos.UseCases.RemoverVeiculo;

public class RemoverVeiculoUseCase : IUseCase
{
    private readonly IVeiculoGateway _gateway;
    private readonly IRemoverVeiculoOutputPort _outputPort;

    public RemoverVeiculoUseCase(IVeiculoGateway gateway, IRemoverVeiculoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverVeiculoInput input, CancellationToken ct = default)
    {
        var veiculo = await _gateway.BuscarPorId(input.Id, ct);

        if (veiculo is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        await _gateway.Remover(veiculo, ct);
        _outputPort.Ok();
    }
}
