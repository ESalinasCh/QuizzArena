using System.Net.Http.Json;
using System.Text.Json.Serialization;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

/// <summary>
/// Service to embed any collections of strings and return their vector representation
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private const string EmbeddingModel = "bge-m3"; // could be an env variable
    private const int BatchSize = 32;
    private readonly HttpClient _httpClient;

    private sealed record OllamaEmbedsRequestBody(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] List<string> Input
        );
    private sealed record OllamaEmbedsResponseBody(
        [property: JsonPropertyName("embeddings")] float[][] Embeddings
    );

    public EmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<float[]>> EmbedInBatchesAsync(IReadOnlyList<string> sentences)
    {
        if (sentences.Count == 0)
        {
            return [];
        }

        List<float[]> embeddings = [];


        // I need to loop through the sentences, batch by batch, to genereate their vectors
        for (int i = 0; i < sentences.Count; i += BatchSize)
        {
            var currentBatch = sentences.Skip(i).Take(BatchSize).ToList();

            try
            {
                float[][] sentencesEmbeddings = await EmbedBatchAsync(currentBatch);
                embeddings.AddRange(sentencesEmbeddings);
            }
            catch
            {
                // Embedding model could emit NaN vector for degenerate inputs. This catch is to check the batch sentence by sentence in case a part of the batch is corrupted.
                foreach (string text in currentBatch)
                {
                    embeddings.AddRange(await EmbedBatchAsync([text]));
                }
            }
        }

        return embeddings;

    }

    private async Task<float[][]> EmbedBatchAsync(List<string> currentBatch)
    {
        // Sending the sentences in batches to the Embedding model to optimze the number of calls
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/embed", new OllamaEmbedsRequestBody(EmbeddingModel, currentBatch));

        response.EnsureSuccessStatusCode();

        OllamaEmbedsResponseBody? processedResponse = await response.Content.ReadFromJsonAsync<OllamaEmbedsResponseBody>();

        return processedResponse?.Embeddings ?? throw new InvalidOperationException("Ollama returned no embeddings.");

    }

}
