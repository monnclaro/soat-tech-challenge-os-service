using Application.Common.Interfaces;
using Application.OrdensServico.UseCases.Common;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.OrdensServico.Gateways;
using SharedKernel.DTOs;

namespace Application.OrdensServico.UseCases.BuscarListaPaginada;

public class BuscarListaPaginadaOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IBuscarListaPaginadaOrdemServicoOutputPort _outputPort;

    public BuscarListaPaginadaOrdemServicoUseCase(
        IOrdemServicoGateway gateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway,
        IBuscarListaPaginadaOrdemServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaOrdemServicoInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginado(input.Paginacao, ct);

        var clientesCache = new Dictionary<Guid, Cliente>();
        var veiculosCache = new Dictionary<Guid, Veiculo>();

        var outputs = new List<OrdemServicoOutput>();
        foreach (var os in items)
        {
            if (!clientesCache.TryGetValue(os.IdCliente, out var cliente))
            {
                cliente = (await _clienteGateway.BuscarPorId(os.IdCliente, ct))!;
                clientesCache[os.IdCliente] = cliente;
            }

            if (!veiculosCache.TryGetValue(os.IdVeiculo, out var veiculo))
            {
                veiculo = (await _veiculoGateway.BuscarPorId(os.IdVeiculo, ct))!;
                veiculosCache[os.IdVeiculo] = veiculo;
            }

            outputs.Add(OrdemServicoOutputMapper.Map(os, cliente, veiculo));
        }

        _outputPort.Ok(new PagedResult<OrdemServicoOutput>(outputs, total, input.Paginacao.Pagina, input.Paginacao.Tamanho));
    }
}
