using System.Text.Json.Serialization;

namespace Api.Controllers.Servicos.Requests;

public record AtualizarServicoRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor"), JsonRequired] decimal Valor);
