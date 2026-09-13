using System.Text.Json.Serialization;

namespace Api.Controllers.Clientes.Veiculos.Requests;

public record InserirVeiculoRequest(
    [property: JsonPropertyName("placa")] string Placa,
    [property: JsonPropertyName("marca")] string Marca,
    [property: JsonPropertyName("modelo")] string Modelo,
    [property: JsonPropertyName("ano")] int Ano);

public record AtualizarVeiculoRequest(
    [property: JsonPropertyName("placa")] string Placa,
    [property: JsonPropertyName("marca")] string Marca,
    [property: JsonPropertyName("modelo")] string Modelo,
    [property: JsonPropertyName("ano")] int Ano);
