namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;

public class TranscriptionFailedCommand
{
    public Guid ClassSourceId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
