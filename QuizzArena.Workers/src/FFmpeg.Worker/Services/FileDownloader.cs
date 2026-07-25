namespace FFmpeg.Worker.Services;

public class FileDownloader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileDownloader> _logger;

    public FileDownloader(HttpClient httpClient, ILogger<FileDownloader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> DownloadToTempFileAsync(string sourceUrl, CancellationToken ct)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_input");
        _logger.LogInformation("Downloading {Url} -> {Path}", sourceUrl, tempPath);

        using var response = await _httpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var httpStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await httpStream.CopyToAsync(fileStream, ct);

        return tempPath;
    }
}