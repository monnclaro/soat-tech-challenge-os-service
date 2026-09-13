using Application.Common.Interfaces;
using Domain.Clientes.Gateways;

namespace Application.Clientes.UseCases.AtualizarCliente;

public class AtualizarClienteUseCase : IUseCase
{
    private readonly IClienteGateway _gateway;
    private readonly IAtualizarClienteOutputPort _outputPort;

    public AtualizarClienteUseCase(IClienteGateway gateway, IAtualizarClienteOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AtualizarClienteInput input, CancellationToken ct = default)
    {
        var cliente = await _gateway.BuscarPorId(input.Id, ct);

        if (cliente is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        cliente.Atualizar(input.Nome);

        if (input.Ativo)
            cliente.Ativar();
        else
            cliente.Inativar();

        await _gateway.Atualizar(cliente, ct);

        _outputPort.Ok(new ClienteOutput(cliente.Id, cliente.Nome, cliente.Documento, cliente.Ativo, cliente.DataCriacao));
    }
}
