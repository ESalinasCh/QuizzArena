namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands;

public class TranscriptionRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}
