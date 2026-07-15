using System.Net.Http.Json;
using System.Text.Json;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal sealed class OllamaEmbeddingGeneration : IEmbeddingService
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

        return result[0];
    }

    public async Task<float[][]> GenerateMultipleEmbeddingsAsync(string model, string[] prompts, int? batchSize = 32)
    {
        if (prompts == null || prompts.Length == 0)
        {
            throw new ArgumentException("Prompts array cannot be empty.", nameof(prompts));
        }

        if (batchSize.HasValue && batchSize.Value <= 0)
        {
            throw new ArgumentException("Batch size must be a positive integer.", nameof(batchSize));
        }

        int actualBatchSize = batchSize ?? prompts.Length;
        List<float[]> allEmbeddings = new List<float[]>(prompts.Length);

        for (int i = 0; i < prompts.Length; i += actualBatchSize)
        {
            string[] batch = prompts.Skip(i).Take(actualBatchSize).ToArray();
            float[][] batchResult = await SendEmbeddingRequestAsync(model, batch);
            allEmbeddings.AddRange(batchResult);
        }

        return allEmbeddings.ToArray();
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

        ValidateEmbeddings(ollamaResponse.Embeddings);

        return ollamaResponse.Embeddings;
    }

    private static void ValidateEmbeddings(float[][] embeddings)
    {
        if (embeddings.Length == 0)
        {
            throw new InvalidOperationException("Ollama returned an empty embedding array.");
        }

        int? expectedDimension = null;

        foreach (var embedding in embeddings)
        {
            if (embedding == null || embedding.Length == 0)
            {
                throw new InvalidOperationException("Ollama returned an invalid embedding: null or empty vector.");
            }

            if (expectedDimension is null)
            {
                expectedDimension = embedding.Length;
            }
            else if (embedding.Length != expectedDimension)
            {
                throw new InvalidOperationException(
                    $"Inconsistent embedding dimensions: expected {expectedDimension}, got {embedding.Length}.");
            }
        }
    }

}
