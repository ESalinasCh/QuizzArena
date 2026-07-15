namespace QuizzArena.DocumentProcessing.Infrastructure.Configuration;

public record IndexingOptions
{

    public const string SectionName = "Indexing";
    public float CosineSimilarityThreshold { get; set; } = 0.92f;

    /*
        The embedding model must match the configured embedding provider.

        If the EmbeddingProvider in appsettings is set to "Gemini",
        here shloud be a valid Gemini embedding model (like "gemini-embedding-2").

        For local providers such as Ollama, use the corresponding local model
        name ("bge-m3").
    */
    public string EmbeddingModel { get; set; } = "bge-m3"; //Gemini: models/gemini-embedding-2
    public string ClassificationModel { get; set; } = "qwen2.5:7b-instruct";
    public double MinConfidence { get; set; } = 0.7;
    public int MinSentenceWords { get; set; } = 4;
    public int MaxSentenceWords { get; set; } = 15;
}
