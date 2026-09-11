using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record RegistrarDiagnosticoServicoRequest(
    [property: JsonPropertyName("idServico")] Guid IdServico,
    [property: JsonPropertyName("nomeServico")] string NomeServico,
    [property: JsonPropertyName("valor")] decimal Valor);

public record RegistrarDiagnosticoProdutoRequest(
    [property: JsonPropertyName("idProduto")] Guid IdProduto,
    [property: JsonPropertyName("nomeProduto")] string NomeProduto,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario,
    [property: JsonPropertyName("quantidade")] decimal Quantidade);

// Payload interno — hoje chamado diretamente (endpoint), futuramente o mesmo
// formato do evento "DiagnosticoFinalizado" consumido do Execução Service
// (ver PLANO-FASE-4-MICROSSERVICOS.md).
public record RegistrarDiagnosticoRequest(
    [property: JsonPropertyName("servicos")] List<RegistrarDiagnosticoServicoRequest> Servicos,
    [property: JsonPropertyName("produtos")] List<RegistrarDiagnosticoProdutoRequest> Produtos);
