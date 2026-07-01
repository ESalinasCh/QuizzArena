using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    private sealed record OllamaEmbedsRequestBody(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] List<string> Input
        );
    private sealed record OllamaEmbedsResponseBody(
        [property: JsonPropertyName("embeddings")] float[][] Embeddings
    );

    private const string EmbeddingModel = "bge-m3"; // could be an env variable
    private const int BatchSize = 32;

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

    public async Task<IReadOnlyList<float[]>> EmbedInBatchesAsync(IReadOnlyList<string> sentences)
    {
        if (sentences.Count == 0)
        {
            return [];
        }

        float[][] embeddings = new float[sentences.Count][];
        int embeddingDimension = 0;

        // I need to loop through the sentences, batch by batch, to genereate their vectors
        for (int i = 0; i < sentences.Count; i += BatchSize)
        {
            var currentBatch = sentences.Skip(i).Take(BatchSize).ToList();

            try
            {
                float[][] sentencesEmbeddings = await EmbedBatchAsync(currentBatch);
                for (int j = 0; j < sentencesEmbeddings.Length; j++)
                {
                    embeddings[i + j] = sentencesEmbeddings[j];
                    embeddingDimension = sentencesEmbeddings[j].Length;
                }
            }
            catch
            {
                // Ollama can emit a NaN vector for a degenerate input and reject the whole
                // batch (500). Retry each input alone so one bad sentence doesn't sink the
                // rest; a still-failing input is left null and zero-filled below.
                for (int j = 0; j < currentBatch.Count; j++)
                {
                    try
                    {
                        float[] single = (await EmbedBatchAsync([currentBatch[j]]))[0];
                        embeddings[i + j] = single;
                        embeddingDimension = single.Length;
                    }
                    catch
                    {
                        // Leave null; zero-filled once we know the dimension.
                    }
                }
            }
        }

        if (embeddingDimension == 0)
        {
            // Nothing embedded at all — the model or endpoint is genuinely broken.
            throw new InvalidOperationException("Ollama returned no usable embeddings for any input.");
        }

        for (int i = 0; i < embeddings.Length; i++)
        {
            embeddings[i] ??= new float[embeddingDimension];
        }

        return embeddings;

    }

    private async Task<float[][]> EmbedBatchAsync(List<string> currentBatch)
    {
        // Sending the sentences in batches to the Embedding model to optimze the number of calls
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/embed", new OllamaEmbedsRequestBody(EmbeddingModel, currentBatch));

        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Ollama embed failed ({(int)response.StatusCode}) for {currentBatch.Count} input(s): {errorBody}");
        }

        OllamaEmbedsResponseBody? processedResponse = await response.Content.ReadFromJsonAsync<OllamaEmbedsResponseBody>();

        return processedResponse?.Embeddings ?? throw new InvalidOperationException("Ollama returned no embeddings.");

    }

}
