namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;

public class IndexingRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
