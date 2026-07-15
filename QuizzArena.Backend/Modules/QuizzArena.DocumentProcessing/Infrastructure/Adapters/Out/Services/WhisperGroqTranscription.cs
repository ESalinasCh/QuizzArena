using System.Text.Json;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

public class WhisperGroqTranscription : ITranscriptionService
{
    private readonly HttpClient _httpClient;

    public WhisperGroqTranscription(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> TranscribeAudioAsync(string fileUrl)
    {
        using var fileStream = await _httpClient.GetStreamAsync(fileUrl);

        var uri = new Uri(fileUrl);
        var fileName = Path.GetFileName(uri.AbsolutePath);
        if (string.IsNullOrEmpty(fileName) || !Path.HasExtension(fileName))
        {
            fileName = "audio.mp3";
        }

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);

        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent("whisper-large-v3-turbo"), "model");
        content.Add(new StringContent("json"), "response_format");

        var response = await _httpClient.PostAsync("audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonResponse);

        return document.RootElement.GetProperty("text").GetString()!;
    }
}
