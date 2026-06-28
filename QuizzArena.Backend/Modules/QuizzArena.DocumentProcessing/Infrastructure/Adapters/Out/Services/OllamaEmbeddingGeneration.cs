using System.Net.Http.Json;
using System.Text.Json;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal class OllamaEmbeddingGeneration : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaEmbeddingGeneration(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private sealed record OllamaEmbeddingResponse(
        string Model,
        float[][] Embeddings
    );

    public async Task<float[]> GenerateSingleEmbeddingAsync(string model, string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));
        }

        float[][] result = await SendEmbeddingRequestAsync(model, [prompt]);

        return result.Length == 0 ? throw new InvalidOperationException("Ollama returned an empty embedding array.") : result[0];
    }

    public async Task<float[][]> GenerateMultipleEmbeddingsAsync(string model, string[] prompts)
    {
        return prompts == null || prompts.Length == 0
            ? throw new ArgumentException("Prompts array cannot be empty.", nameof(prompts))
            : await SendEmbeddingRequestAsync(model, prompts);
    }

    private async Task<float[][]> SendEmbeddingRequestAsync(string model, string[] inputs)
    {
        var payload = new
        {
            model = model,
            input = inputs
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/embed", payload);

        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var ollamaResponse = await JsonSerializer.DeserializeAsync<OllamaEmbeddingResponse>(
            responseStream,
            _caseInsensitiveOptions
        );

        if (ollamaResponse?.Embeddings == null)
        {
            throw new InvalidOperationException("Failed to extract embeddings from Ollama response.");
        }

        return ollamaResponse.Embeddings;
    }
}
