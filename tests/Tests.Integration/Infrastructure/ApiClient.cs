using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tests.Integration.Infrastructure;

// Cliente HTTP "cru" (sem SDK/ProjectReference de nenhum dos 3 serviços — este projeto
// testa só via rede, como um cliente de verdade faria) usado pelos step definitions do
// cenário de BDD real (ver Features/SagaCompletaReal.feature).
internal sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public ApiClient(string baseUrl) => _http = new HttpClient { BaseAddress = new Uri(baseUrl) };

    public async Task<TResponse> PostAsync<TResponse>(string path, object body, string? token = null)
    {
        using var response = await Send(HttpMethod.Post, path, token, body);
        await EnsureSuccess(response, path);
        return await Deserialize<TResponse>(response);
    }

    public async Task PostAsync(string path, object body, string? token = null)
    {
        using var response = await Send(HttpMethod.Post, path, token, body);
        await EnsureSuccess(response, path);
    }

    public async Task PatchAsync(string path, string? token = null, object? body = null)
    {
        using var response = await Send(HttpMethod.Patch, path, token, body);
        await EnsureSuccess(response, path);
    }

    public async Task<TResponse> GetAsync<TResponse>(string path, string? token = null)
    {
        using var response = await Send(HttpMethod.Get, path, token);
        await EnsureSuccess(response, path);
        return await Deserialize<TResponse>(response);
    }

    // EnsureSuccessStatusCode() sozinho não mostra o corpo do erro (ex.: a mensagem de
    // exceção real por trás de um 500) — inclui aqui pra não precisar vasculhar log de
    // container pra descobrir o que quebrou.
    private static async Task EnsureSuccess(HttpResponseMessage response, string path)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode} em \"{path}\": {body}");
    }

    public async Task<HttpStatusCode> GetStatusAsync(string path, string? token = null)
    {
        using var response = await Send(HttpMethod.Get, path, token);
        return response.StatusCode;
    }

    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, string? token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _http.SendAsync(request);
    }

    private static async Task<TResponse> Deserialize<TResponse>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(json, JsonOptions)!;
    }
}
