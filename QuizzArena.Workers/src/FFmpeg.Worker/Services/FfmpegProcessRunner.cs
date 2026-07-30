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

    /// <summary>
    /// Ejecuta: ffmpeg {arguments}
    /// El caller es responsable de construir los argumentos completos,
    /// incluyendo los paths de input/output.
    /// </summary>
    public async Task RunAsync(string arguments, CancellationToken ct)
    {
        _logger.LogInformation("Ejecutando: ffmpeg {Arguments}", arguments);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
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
            _logger.LogError("ffmpeg falló (exit {Code}): {Stderr}", process.ExitCode, stderr);
            throw new InvalidOperationException($"ffmpeg salió con código {process.ExitCode}: {stderr}");
        }
    }

    /// <summary>
    /// Usa ffprobe (incluido con ffmpeg) para obtener la duración del archivo en segundos.
    /// </summary>
    public async Task<double> GetDurationSecondsAsync(string filePath, CancellationToken ct)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0 || !double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var duration))
        {
            throw new InvalidOperationException($"ffprobe no pudo obtener la duración de: {filePath}");
        }

        _logger.LogInformation("Duración detectada: {Duration:F1}s", duration);
        return duration;
    }
}
