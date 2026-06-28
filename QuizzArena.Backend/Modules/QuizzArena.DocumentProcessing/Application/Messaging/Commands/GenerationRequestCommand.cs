using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands;

public class GenerationRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
    public int NumberOfQuestions { get; set; } = 5;
    public int MinNumberOfOptions { get; set; } = 2;
    public int MaxNumberOfOptions { get; set; } = 4;
    public BloomTaxonomyLevel BloomTaxonomy { get; set; } = BloomTaxonomyLevel.Remember;
}
