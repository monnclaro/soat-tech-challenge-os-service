namespace Application.Clientes.Veiculos.UseCases;

public record VeiculoOutput(
    Guid Id,
    Guid IdCliente,
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    DateTime DataCriacao);
