using Application.Common.Interfaces;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;

namespace Application.Clientes.UseCases.InserirCliente;

public class InserirClienteUseCase : IUseCase
{
    private readonly IClienteGateway _gateway;
    private readonly IInserirClienteOutputPort _outputPort;

    public InserirClienteUseCase(
        IClienteGateway gateway,
        IInserirClienteOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirClienteInput input, CancellationToken ct = default)
    {
        var documento = DocumentoCliente.Criar(input.Documento);

        var existe = await _gateway.ExisteComDocumento(documento.Numero, ct);
        if (existe)
        {
            _outputPort.DocumentoDuplicado($"{documento.Tipo} '{documento.Numero}' já cadastrado.");
            return;
        }

        var cliente = new Cliente();
        cliente.Inserir(input.Nome, documento);

        await _gateway.Salvar(cliente, ct);

        _outputPort.Ok(new ClienteOutput(
            cliente.Id,
            cliente.Nome,
            cliente.Documento,
            cliente.Ativo,
            cliente.DataCriacao));
    }
}
