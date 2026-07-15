using System.Net.Http.Json;
using System.Text.Json;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal class GeminiEmbeddingGeneration : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeminiEmbeddingGeneration(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private sealed record GeminiEmbeddingResponse(
        GeminiEmbedding Embedding
    );

    private sealed record GeminiBatchEmbeddingResponse(
        GeminiEmbedding[] Embeddings
    );

    private sealed record GeminiEmbedding(
        float[] Values
    );

    public async Task<float[]> GenerateSingleEmbeddingAsync(string model, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Prompt cannot be empty.", nameof(text));
        }

        float[][] result = await SendSingleEmbeddingRequestAsync(model, text);

        return result[0];
    }

    public async Task<float[][]> GenerateMultipleEmbeddingsAsync(string model, string[] texts, int? batchSize = 32)
    {
        if (texts == null || texts.Length == 0)
        {
            throw new ArgumentException("Prompts array cannot be empty.", nameof(texts));
        }

        if (batchSize.HasValue && batchSize.Value <= 0)
        {
            throw new ArgumentException("Batch size must be a positive integer.", nameof(batchSize));
        }

        int actualBatchSize = batchSize ?? texts.Length;
        List<float[]> allEmbeddings = new(texts.Length);

        for (int i = 0; i < texts.Length; i += actualBatchSize)
        {
            string[] batch = texts.Skip(i).Take(actualBatchSize).ToArray();
            float[][] batchResult = await SendBatchEmbeddingRequestAsync(model, batch);
            allEmbeddings.AddRange(batchResult);
        }

        return allEmbeddings.ToArray();
    }

    private async Task<float[][]> SendSingleEmbeddingRequestAsync(string model, string prompt)
    {
        var payload = new
        {
            model,
            taskType = "RETRIEVAL_DOCUMENT",
            outputDimensionality = 1024,
            content = new
            {
                parts = new[]
                {
                    new
                    {
                        text = prompt
                    }
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/v1beta/models/gemini-embedding-2:embedContent",
            payload);

        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();

        var geminiResponse = await JsonSerializer.DeserializeAsync<GeminiEmbeddingResponse>(
            responseStream,
            _caseInsensitiveOptions);

        if (geminiResponse?.Embedding?.Values == null)
        {
            throw new InvalidOperationException("Failed to extract embedding from Gemini response.");
        }

        ValidateEmbeddings([geminiResponse.Embedding.Values]);

        return [geminiResponse.Embedding.Values];
    }

    private async Task<float[][]> SendBatchEmbeddingRequestAsync(string model, string[] prompts)
    {
        var payload = new
        {
            requests = prompts.Select(prompt => new
            {
                model,
                taskType = "RETRIEVAL_DOCUMENT",
                outputDimensionality = 1024,
                content = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            })
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/v1beta/models/gemini-embedding-2:batchEmbedContents",
            payload);

        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();

        var geminiResponse = await JsonSerializer.DeserializeAsync<GeminiBatchEmbeddingResponse>(
            responseStream,
            _caseInsensitiveOptions);

        if (geminiResponse?.Embeddings == null)
        {
            throw new InvalidOperationException("Failed to extract embeddings from Gemini response.");
        }

        float[][] embeddings = geminiResponse.Embeddings
            .Select(e => e.Values)
            .ToArray();

        ValidateEmbeddings(embeddings);

        return embeddings;
    }

    private static void ValidateEmbeddings(float[][] embeddings)
    {
        if (embeddings.Length == 0)
        {
            throw new InvalidOperationException("Gemini returned an empty embedding array.");
        }

        int? expectedDimension = null;

        foreach (float[] embedding in embeddings)
        {
            if (embedding == null || embedding.Length == 0)
            {
                throw new InvalidOperationException("Gemini returned an invalid embedding: null or empty vector.");
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
