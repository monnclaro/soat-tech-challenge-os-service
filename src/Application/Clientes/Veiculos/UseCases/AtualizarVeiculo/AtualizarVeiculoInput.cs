namespace Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;

public record AtualizarVeiculoInput(Guid Id, string Placa, string Marca, string Modelo, int Ano);
