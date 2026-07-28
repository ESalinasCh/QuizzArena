namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;

public class TranscriptionRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}
