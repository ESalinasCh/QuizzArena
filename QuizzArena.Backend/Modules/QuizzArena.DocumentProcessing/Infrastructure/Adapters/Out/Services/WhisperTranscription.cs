using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services
{
    public class WhisperTranscription : IWhisperTranscriptionRepository
    {
        public async Task<string> Transcribe()
        {
            return "Transcribed text from Whisper API (Not implemented yet)";
        }
    }
}
