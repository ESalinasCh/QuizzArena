namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands;

public class IndexTranscriptCommand
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
