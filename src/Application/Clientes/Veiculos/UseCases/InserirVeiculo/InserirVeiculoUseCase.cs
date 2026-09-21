using Application.Common.Interfaces;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;

namespace Application.Clientes.Veiculos.UseCases.InserirVeiculo;

public class InserirVeiculoUseCase : IUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IInserirVeiculoOutputPort _outputPort;

    public InserirVeiculoUseCase(
        IVeiculoGateway veiculoGateway,
        IClienteGateway clienteGateway,
        IInserirVeiculoOutputPort outputPort)
    {
        _veiculoGateway = veiculoGateway;
        _clienteGateway = clienteGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirVeiculoInput input, CancellationToken ct = default)
    {
        var placa = Placa.Criar(input.Placa);

        var clienteExiste = await _clienteGateway.BuscarPorId(input.IdCliente, ct);
        if (clienteExiste is null)
        {
            _outputPort.ClienteNaoEncontrado();
            return;
        }

        var placaEmUso = await _veiculoGateway.ExisteComPlaca(placa.Valor, ct);
        if (placaEmUso)
        {
            _outputPort.PlacaDuplicada($"Já existe um veículo com a placa '{placa}'.");
            return;
        }

        var veiculo = new Veiculo();
        veiculo.Inserir(input.IdCliente, placa, input.Marca, input.Modelo, input.Ano);

        await _veiculoGateway.Inserir(veiculo, ct);

        _outputPort.Ok(new VeiculoOutput(
            veiculo.Id, veiculo.IdCliente, veiculo.Placa,
            veiculo.Marca, veiculo.Modelo, veiculo.Ano, veiculo.DataCriacao));
    }
}
