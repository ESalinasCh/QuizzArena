namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IDocumentChunkRepository
{
    public interface ITranscriptionService
    {
        Task<string> TranscribeAudioAsync(string fileUrl);
    }
}
