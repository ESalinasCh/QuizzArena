namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface ITranscriptionService
{
    Task<string> TranscribeAudioAsync(string fileUrl);
}
