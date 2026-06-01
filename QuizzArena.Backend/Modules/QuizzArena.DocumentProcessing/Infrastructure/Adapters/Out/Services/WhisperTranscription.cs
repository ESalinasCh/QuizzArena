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
        // 1. Descargar el archivo usando el _httpClient inyectado (Evita Socket Exhaustion)
        using var fileStream = await _httpClient.GetStreamAsync(fileUrl);

        // 2. Preparar la petición Multipart para la API de Whisper
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

        // El contenedor espera el archivo bajo la clave "audio_file"
        content.Add(streamContent, "audio_file", "audio.wav");

        // 3. Enviar al endpoint usando ruta relativa (ya que BaseAddress se configuró en DI)
        var response = await _httpClient.PostAsync("asr?task=transcribe&output=txt", content);

        response.EnsureSuccessStatusCode();

        // 4. Retornar el texto plano generado por Whisper
        return await response.Content.ReadAsStringAsync();
    }
}
