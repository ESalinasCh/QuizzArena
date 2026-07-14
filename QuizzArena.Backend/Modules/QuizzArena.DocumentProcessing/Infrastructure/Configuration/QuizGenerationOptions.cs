namespace QuizzArena.DocumentProcessing.Infrastructure.Configuration;

public record QuizGenerationOptions
{
    public const string SectionName = "QuizGeneration";
    public float CosineSimilarityThreshold { get; set; } = 0.92f;
    public float JudgementThreshold { get; set; } = 0.75f;
    public string QuestionEmbeddingModel { get; set; } = "bge-m3";
    public string QuizGenerationModel { get; set; } = "qwen2.5:7b-instruct"; //GROQ: openai/gpt-oss-20b
    public string QuizJudgementModel { get; set; } = "llama3.1:8b-instruct-q4_K_M";
}
