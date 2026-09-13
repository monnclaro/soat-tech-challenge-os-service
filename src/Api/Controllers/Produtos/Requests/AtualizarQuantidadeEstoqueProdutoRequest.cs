using System.Text.Json.Serialization;

namespace Api.Controllers.Produtos.Requests;

public record AtualizarQuantidadeEstoqueProdutoRequest(
    [property: JsonPropertyName("quantidade")] decimal Quantidade);
