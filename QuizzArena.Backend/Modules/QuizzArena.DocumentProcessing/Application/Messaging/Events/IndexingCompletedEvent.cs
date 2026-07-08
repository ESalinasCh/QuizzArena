namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class IndexingCompletedEvent
{
    public Guid ClassSourceId { get; set; }
    public int StoredChunkCount { get; set; }
}
