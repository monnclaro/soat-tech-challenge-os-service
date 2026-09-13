using Application.Common.Interfaces;
using Domain.Clientes.Gateways;

namespace Application.Clientes.UseCases.BuscarCliente;

public class BuscarClienteUseCase : IUseCase
{
    private readonly IClienteGateway _gateway;
    private readonly IBuscarClienteOutputPort _outputPort;

    public BuscarClienteUseCase(IClienteGateway gateway, IBuscarClienteOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarClienteInput input, CancellationToken ct = default)
    {
        var cliente = await _gateway.BuscarPorId(input.Id, ct);

        if (cliente is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        _outputPort.Ok(new ClienteOutput(cliente.Id, cliente.Nome, cliente.Documento, cliente.Ativo, cliente.DataCriacao));
    }
}
