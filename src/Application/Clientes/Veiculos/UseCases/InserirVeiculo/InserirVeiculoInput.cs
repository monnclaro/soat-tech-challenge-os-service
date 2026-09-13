namespace Application.Clientes.Veiculos.UseCases.InserirVeiculo;

public record InserirVeiculoInput(Guid IdCliente, string Placa, string Marca, string Modelo, int Ano);
