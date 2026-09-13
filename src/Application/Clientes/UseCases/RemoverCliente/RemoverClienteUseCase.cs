using Application.Common.Interfaces;
using Domain.Clientes.Gateways;

namespace Application.Clientes.UseCases.RemoverCliente;

public class RemoverClienteUseCase : IUseCase
{
    private readonly IClienteGateway _gateway;
    private readonly IRemoverClienteOutputPort _outputPort;

    public RemoverClienteUseCase(IClienteGateway gateway, IRemoverClienteOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverClienteInput input, CancellationToken ct = default)
    {
        var cliente = await _gateway.BuscarPorId(input.Id, ct);

        if (cliente is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        await _gateway.Remover(cliente, ct);
        _outputPort.Ok();
    }
}
