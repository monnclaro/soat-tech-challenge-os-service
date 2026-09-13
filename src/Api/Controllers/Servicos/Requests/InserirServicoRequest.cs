using System.Text.Json.Serialization;

namespace Api.Controllers.Servicos.Requests;

public record InserirServicoRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);
