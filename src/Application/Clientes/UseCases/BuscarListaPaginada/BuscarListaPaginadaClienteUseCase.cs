using Application.Common.Interfaces;
using Domain.Clientes.Gateways;
using SharedKernel.DTOs;

namespace Application.Clientes.UseCases.BuscarListaPaginada;

public class BuscarListaPaginadaClienteUseCase : IUseCase
{
    private readonly IClienteGateway _gateway;
    private readonly IBuscarListaPaginadaClienteOutputPort _outputPort;

    public BuscarListaPaginadaClienteUseCase(IClienteGateway gateway, IBuscarListaPaginadaClienteOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaClienteInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginado(input.Paginacao, ct);

        var output = new PagedResult<ClienteOutput>(
            items.Select(c => new ClienteOutput(c.Id, c.Nome, c.Documento, c.Ativo, c.DataCriacao)).ToList(),
            total,
            input.Paginacao.Pagina,
            input.Paginacao.Tamanho);

        _outputPort.Ok(output);
    }
}
