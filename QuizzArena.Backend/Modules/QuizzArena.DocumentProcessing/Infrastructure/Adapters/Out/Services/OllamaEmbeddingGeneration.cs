using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal sealed partial class OllamaEmbeddingGeneration : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaEmbeddingGeneration> _logger;
    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaEmbeddingGeneration(HttpClient httpClient, ILogger<OllamaEmbeddingGeneration> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

        float[][]? result = await TrySendEmbeddingRequestAsync(model, [prompt]) ?? throw new InvalidOperationException($"Ollama failed to generate an embedding for the given prompt using model '{model}'.");
        return result[0];
    }

    public async Task<EmbeddingBatchResult> GenerateMultipleEmbeddingsAsync(string model, string[] prompts, int? batchSize = 32)
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
        HashSet<int> skippedIndices = [];

        for (int i = 0; i < prompts.Length; i += actualBatchSize)
        {
            string[] batch = prompts.Skip(i).Take(actualBatchSize).ToArray();
            float[][]? batchResult = await TrySendEmbeddingRequestAsync(model, batch);

            if (batchResult is not null)
            {
                allEmbeddings.AddRange(batchResult);
                continue;
            }

            for (int j = 0; j < batch.Length; j++)
            {
                float[][]? singleResult = await TrySendEmbeddingRequestAsync(model, [batch[j]]);

                if (singleResult is not null)
                {
                    allEmbeddings.Add(singleResult[0]);
                }
                else
                {
                    skippedIndices.Add(i + j);
                    LogSkippedInput(_logger, i + j, model);
                }
            }
        }

        return new EmbeddingBatchResult(allEmbeddings.ToArray(), skippedIndices);
    }

    private async Task<float[][]?> TrySendEmbeddingRequestAsync(string model, string[] inputs)
    {
        var payload = new
        {
            model = model,
            input = inputs
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/embed", payload);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ollama could not produce an embedding for input at index {Index} using model '{Model}'; skipping it.")]
    private static partial void LogSkippedInput(ILogger logger, int index, string model);
}
