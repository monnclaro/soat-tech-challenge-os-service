using Application.Common.Interfaces;
using Domain.Clientes.Veiculos.Gateways;

namespace Application.Clientes.Veiculos.UseCases.BuscarVeiculo;

public class BuscarVeiculoUseCase : IUseCase
{
    private readonly IVeiculoGateway _gateway;
    private readonly IBuscarVeiculoOutputPort _outputPort;

    public BuscarVeiculoUseCase(IVeiculoGateway gateway, IBuscarVeiculoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarVeiculoInput input, CancellationToken ct = default)
    {
        var veiculo = await _gateway.BuscarPorId(input.Id, ct);

        if (veiculo is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        _outputPort.Ok(Map(veiculo));
    }

    private static VeiculoOutput Map(Domain.Clientes.Veiculos.Veiculo v) => new(v.Id, v.IdCliente, v.Placa, v.Marca, v.Modelo, v.Ano, v.DataCriacao);
}
