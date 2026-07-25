namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;

public class IndexTranscriptCommand
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
