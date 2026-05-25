using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IDocumentChunkRepository
{
    public interface IWhisperTranscriptionRepository
    {
        Task<String> Transcribe();
    }
}
