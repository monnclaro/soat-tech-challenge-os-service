using Application.Common.Interfaces;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;

namespace Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;

public class AtualizarVeiculoUseCase : IUseCase
{
    private readonly IVeiculoGateway _gateway;
    private readonly IAtualizarVeiculoOutputPort _outputPort;

    public AtualizarVeiculoUseCase(
        IVeiculoGateway gateway,
        IAtualizarVeiculoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AtualizarVeiculoInput input, CancellationToken ct = default)
    {
        var placa = Placa.Criar(input.Placa);

        var veiculo = await _gateway.BuscarPorId(input.Id, ct);
        if (veiculo is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        var placaEmUso = await _gateway.ExisteComPlacaExcetoId(placa.Valor, input.Id, ct);
        if (placaEmUso)
        {
            _outputPort.PlacaDuplicada($"Já existe um veículo cadastrado com a placa '{placa}'.");
            return;
        }

        veiculo.Atualizar(placa, input.Marca, input.Modelo, input.Ano);
        await _gateway.Atualizar(veiculo, ct);

        _outputPort.Ok(new VeiculoOutput(
            veiculo.Id, veiculo.IdCliente, veiculo.Placa,
            veiculo.Marca, veiculo.Modelo, veiculo.Ano, veiculo.DataCriacao));
    }
}
