using System.Net.Http.Headers;
using static QuizzArena.DocumentProcessing.Application.Ports.Out.IDocumentChunkRepository;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

public class WhisperTranscription : ITranscriptionService
{
    private readonly HttpClient _httpClient;

    public WhisperTranscription(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> TranscribeAudioAsync(string fileUrl)
    {
        using var fileStream = await _httpClient.GetStreamAsync(fileUrl);

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

        content.Add(streamContent, "audio_file", "audio.wav");

        var response = await _httpClient.PostAsync("asr?task=transcribe&output=txt", content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
