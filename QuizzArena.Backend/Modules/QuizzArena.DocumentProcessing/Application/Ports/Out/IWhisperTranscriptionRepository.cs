namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IDocumentChunkRepository
{
    public interface IWhisperTranscriptionRepository
    {
        Task<string> Transcribe();
    }
}
