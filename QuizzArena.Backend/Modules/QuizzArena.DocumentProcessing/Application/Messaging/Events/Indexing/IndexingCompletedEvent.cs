namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;

public class IndexingCompletedEvent
{
    public Guid ClassSourceId { get; set; }
    public int StoredChunkCount { get; set; }
}
