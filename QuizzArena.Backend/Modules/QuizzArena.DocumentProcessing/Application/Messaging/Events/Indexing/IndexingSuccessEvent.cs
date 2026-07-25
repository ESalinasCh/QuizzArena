namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;

public class IndexingSuccessEvent
{
    public Guid ClassSourceId { get; set; }
    public int StoredChunkCount { get; set; }
}
