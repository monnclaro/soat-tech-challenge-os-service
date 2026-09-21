using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirOrdemServicoRequest(
    [property: JsonPropertyName("idCliente"), JsonRequired] Guid IdCliente,
    [property: JsonPropertyName("idVeiculo"), JsonRequired] Guid IdVeiculo);
