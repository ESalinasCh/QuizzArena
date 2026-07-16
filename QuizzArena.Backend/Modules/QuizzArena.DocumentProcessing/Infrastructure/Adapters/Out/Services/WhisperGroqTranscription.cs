using System.Net.Http;
using System.Text.Json;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

public class WhisperGroqTranscription : ITranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public WhisperGroqTranscription(HttpClient httpClient, IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> TranscribeAudioAsync(string fileUrl)
    {
        using var downloadClient = _httpClientFactory.CreateClient();
        using var fileStream = await downloadClient.GetStreamAsync(fileUrl);

        var uri = new Uri(fileUrl);
        var fileName = Path.GetFileName(uri.AbsolutePath);
        if (string.IsNullOrEmpty(fileName) || !Path.HasExtension(fileName))
        {
            fileName = "audio.mp3";
        }

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);

        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent("whisper-large-v3-turbo"), "model");
        content.Add(new StringContent("json"), "response_format");

        var response = await _httpClient.PostAsync("openai/v1/audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonResponse);

        return document.RootElement.GetProperty("text").GetString()!;
    }
}
