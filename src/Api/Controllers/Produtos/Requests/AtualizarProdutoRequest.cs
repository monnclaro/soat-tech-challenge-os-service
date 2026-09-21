using System.Text.Json.Serialization;

namespace Api.Controllers.Produtos.Requests;

public record AtualizarProdutoRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);
