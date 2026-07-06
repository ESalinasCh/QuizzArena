using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

/// <summary>
/// Determines if a class chunk should be kept based on its classification (academic or not) and its confidence score.
/// </summary>
public class OllamaChunkClassifier(
    IOptions<IndexingOptions> indexingConfig,
    ITextGenerationService textGenerationService
) : IChunkClassifier
{
    private readonly IndexingOptions _indexingConfig = indexingConfig.Value;
    private static readonly ChunkClassification _fallback = new(ChunkCategory.Academic, 0.8);

    public async Task<ChunkClassification> ClassifyAsync(string content)
    {
        string prompt = BuildPrompt(content);

        ClassificationResult result = await textGenerationService.GenerateAsync<ClassificationResult>(_indexingConfig.ClassificationModel, prompt);

        if (result is null)
        {
            return _fallback;
        }

        return new ChunkClassification(MapCategory(result.Categoria), result.Confidence);
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

    private sealed record ClassificationResult(
        [property: JsonPropertyName("categoria")] string Categoria,
        [property: JsonPropertyName("confidence")] double Confidence,
        [property: JsonPropertyName("razon")] string Razon);
}
