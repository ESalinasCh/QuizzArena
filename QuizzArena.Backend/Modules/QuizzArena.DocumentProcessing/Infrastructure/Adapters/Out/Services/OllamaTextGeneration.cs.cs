using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal class OllamaTextGeneration : ITextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _caseInsensitiveOptions = new() 
    { 
        PropertyNameCaseInsensitive = true 
    };

    public OllamaTextGeneration(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private sealed record OllamaGenerateResponseString(
        string Model,
        DateTime CreatedAt,
        string Response,
        bool Done
    );

    public async Task<string> GenerateAsync(string model, string prompt)
    {
        var ollamaResponse = await GetOllamaResponseAsync(model, prompt);
        return ollamaResponse.Response;
    }

    public async Task<T> GenerateAsync<T>(string model, string prompt)
    {
        var schema = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(T));
        var ollamaResponse = await GetOllamaResponseAsync(model, prompt, schema);

        var result = JsonSerializer.Deserialize<T>(ollamaResponse.Response);

        if (result == null)
        {
            throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
        }

        return result;
    }

    private async Task<OllamaGenerateResponseString> GetOllamaResponseAsync(
        string model, 
        string prompt, 
        JsonNode? schema = null)
    {
        var payload = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            format = schema
        };

        var response = await _httpClient.PostAsync(
            "/api/generate",
            new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );

        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponseString>(
            rawContent, 
            _caseInsensitiveOptions
        );

        if (ollamaResponse == null || string.IsNullOrEmpty(ollamaResponse.Response))
        {
            throw new InvalidOperationException("No response from Ollama");
        }

        return ollamaResponse;
    }
}
