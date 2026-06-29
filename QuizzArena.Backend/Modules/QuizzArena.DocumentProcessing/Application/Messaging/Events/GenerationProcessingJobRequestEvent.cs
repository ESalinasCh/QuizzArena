using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class GenerationProcessingJobRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; } = Guid.NewGuid();
    public Guid DocumentProcessingJobId { get; set; } = Guid.NewGuid();
    public int NumberOfQuestions { get; set; } = 5;
    public int MinNumberOfOptions { get; set; } = 2;
    public int MaxNumberOfOptions { get; set; } = 4;
    public bool CreateMatch { get; set; } = true;
    public BloomTaxonomyLevel BloomTaxonomy { get; set; } = BloomTaxonomyLevel.Remember;
}
