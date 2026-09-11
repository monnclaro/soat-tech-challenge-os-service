using Application.Common.Interfaces;
using Domain.Clientes.Veiculos.Gateways;
using SharedKernel.DTOs;

namespace Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;

public class BuscarListaPaginadaVeiculoUseCase : IUseCase
{
    private readonly IVeiculoGateway _gateway;
    private readonly IBuscarListaPaginadaVeiculoOutputPort _outputPort;

    public BuscarListaPaginadaVeiculoUseCase(IVeiculoGateway gateway, IBuscarListaPaginadaVeiculoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaVeiculoInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginadoPorCliente(input.IdCliente, input.Paginacao, ct);

        var output = new PagedResult<VeiculoOutput>(
            items.Select(v => new VeiculoOutput(v.Id, v.IdCliente, v.Placa, v.Marca, v.Modelo, v.Ano, v.DataCriacao)).ToList(),
            total,
            input.Paginacao.Pagina,
            input.Paginacao.Tamanho);

        _outputPort.Ok(output);
    }
}
