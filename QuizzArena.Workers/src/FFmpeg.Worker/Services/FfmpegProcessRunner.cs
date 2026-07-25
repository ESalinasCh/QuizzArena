using System.Diagnostics;

namespace FFmpeg.Worker.Services;

public class FfmpegProcessRunner
{
    private readonly ILogger<FfmpegProcessRunner> _logger;
    private readonly TimeSpan _timeout;

    public FfmpegProcessRunner(ILogger<FfmpegProcessRunner> logger, IConfiguration config)
    {
        _logger = logger;
        var minutes = config.GetValue<int?>("Ffmpeg:TimeoutMinutes") ?? 60;
        _timeout = TimeSpan.FromMinutes(minutes);
    }

    public async Task RunAsync(string inputPath, string outputPath, string ffmpegArgumentsTemplate, CancellationToken ct)
    {
        var args = ffmpegArgumentsTemplate
            .Replace("{input}", inputPath)
            .Replace("{output}", outputPath);

        _logger.LogInformation("Running: ffmpeg {Args}", args);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_timeout);

        process.Start();
        var stderrTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"ffmpeg excedió el timeout de {_timeout}");
        }

        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            _logger.LogError("ffmpeg falló con código {Code}: {Stderr}", process.ExitCode, stderr);
            throw new InvalidOperationException($"ffmpeg salió con código {process.ExitCode}: {stderr}");
        }
    }
}