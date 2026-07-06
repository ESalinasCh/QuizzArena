namespace QuizzArena.DocumentProcessing.Infrastructure.Configuration;

public record IndexingOptions
{
    public const string SectionName = "Indexing";
    public float CosineSimilarityThreshold { get; set; } = 0.92f;
    public string EmbeddingModel { get; set; } = "bge-m3";
    public string ClassificationModel { get; set; } = "qwen2.5:7b-instruct";
    public double MinConfidence { get; set; } = 0.7;
}
