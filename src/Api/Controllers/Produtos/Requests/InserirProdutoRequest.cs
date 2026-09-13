using System.Text.Json.Serialization;

namespace Api.Controllers.Produtos.Requests;

public record InserirProdutoRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("quantidadeEmEstoque")] int QuantidadeEmEstoque);
