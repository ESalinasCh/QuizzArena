namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;

public class IndexingFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
