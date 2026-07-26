namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;

public class IndexingRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
