using Application.Common.Interfaces;
using Application.OrdensServico.UseCases.Common;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos.Gateways;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.BuscarOrdemServico;

public class BuscarOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IBuscarOrdemServicoOutputPort _outputPort;

    public BuscarOrdemServicoUseCase(
        IOrdemServicoGateway gateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway,
        IBuscarOrdemServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarOrdemServicoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComItens(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        var cliente = await _clienteGateway.BuscarPorId(ordemServico.IdCliente, ct);
        var veiculo = await _veiculoGateway.BuscarPorId(ordemServico.IdVeiculo, ct);

        _outputPort.Ok(OrdemServicoOutputMapper.Map(ordemServico, cliente!, veiculo!));
    }
}
