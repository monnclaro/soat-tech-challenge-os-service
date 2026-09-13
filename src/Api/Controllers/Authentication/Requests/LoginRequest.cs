using System.Text.Json.Serialization;

namespace Api.Controllers.Authentication.Requests;

public record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("senha")] string Senha);
