using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

/// <summary>
/// Determines if a class chunk should be kept based on its classification (academic or not) and its confidence score.
/// </summary>
public class OllamaChunkClassifier : IChunkClassifier
{
    private const string LlmModel = "qwen2.5:7b-instruct";

    private static readonly ChunkClassification _fallback = new(ChunkCategory.Academic, 0.8);

    private readonly HttpClient _httpClient;

    public OllamaChunkClassifier(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChunkClassification> ClassifyAsync(string content)
    {
        try
        {
            OllamaChatRequest request = new(
                LlmModel,
                [new ChatMessage("user", BuildPrompt(content))],
                Stream: false,
                Format: "json");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/chat", request);
            response.EnsureSuccessStatusCode();

            OllamaChatResponse? body = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

            string? json = body?.Message?.Content;
            if (string.IsNullOrWhiteSpace(json))
            {
                return _fallback;
            }

            ClassificationResult? result = JsonSerializer.Deserialize<ClassificationResult>(json);
            if (result is null)
            {
                return _fallback;
            }

            return new ChunkClassification(MapCategory(result.Categoria), result.Confidence);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return _fallback;
        }
    }

    private static ChunkCategory MapCategory(string categoria) =>
        categoria.Trim().ToUpperInvariant() switch
        {
            "ACADEMICO" => ChunkCategory.Academic,
            "OFF_TOPIC" => ChunkCategory.OffTopic,
            "ERROR_ESTUDIANTIL" => ChunkCategory.StudentError,
            "ANECDOTA" => ChunkCategory.Anecdote,
            _ => ChunkCategory.Unknown,
        };

    private static string BuildPrompt(string content) =>
        $@"Eres un clasificador de contenido educativo universitario.
Clasifica el siguiente fragmento de clase grabada.

TEXTO: ""{content}""

Categorías:
- ACADEMICO: contenido del tema de la materia, explicaciones, definiciones, ejemplos académicos.
- OFF_TOPIC: conversación no relacionada al tema (clima, anécdotas personales, organización, saludos).
- ERROR_ESTUDIANTIL: respuesta incorrecta de un estudiante no corregida explícitamente.
- ANECDOTA: historia ilustrativa, puede ser útil como contexto pero no es conocimiento clave.

Responde únicamente con un objeto JSON válido con este formato:
{{""categoria"": ""ACADEMICO"", ""confidence"": 0.92, ""razon"": ""explicación sobre termodinámica""}}";

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] string Format);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] ChatMessage? Message);

    private sealed record ClassificationResult(
        [property: JsonPropertyName("categoria")] string Categoria,
        [property: JsonPropertyName("confidence")] double Confidence,
        [property: JsonPropertyName("razon")] string Razon);
}
