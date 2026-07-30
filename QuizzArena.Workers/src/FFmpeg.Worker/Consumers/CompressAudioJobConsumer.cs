using FFmpeg.Worker.Services;
using QuizzArena.Workers.Contracts.Jobs;

namespace FFmpeg.Worker.Consumers;

public class CompressAudioJobConsumer : JobConsumerBase<ICompressAudioJob>
{
    // Bitrates estándar válidos en MP3 (libmp3lame) para evitar que FFmpeg redondee hacia arriba
    private static readonly int[] ValidMp3BitratesKbps = [8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128];

    private const int MinBitrateKbps = 16;
    private const int MaxBitrateKbps = 128;

    private readonly FileDownloader _downloader;
    private readonly FfmpegProcessRunner _ffmpeg;
    private readonly BlobStorageService _blobStorage;

    public CompressAudioJobConsumer(
        FileDownloader downloader,
        FfmpegProcessRunner ffmpeg,
        BlobStorageService blobStorage,
        ILogger<CompressAudioJobConsumer> logger) : base(logger)
    {
        _downloader = downloader;
        _ffmpeg = ffmpeg;
        _blobStorage = blobStorage;
    }

    protected override async Task<string> ExecuteAsync(ICompressAudioJob job, CancellationToken ct)
    {
        var inputPath = await _downloader.DownloadToTempFileAsync(job.SourceFileUrl, ct);
        var outputPath = Path.Combine(Path.GetTempPath(), $"{job.JobId}_out.mp3");

        try
        {
            // 1. Obtener duración exacta
            var durationSeconds = await _ffmpeg.GetDurationSecondsAsync(inputPath, ct);

            // 2. Aplicar Factor de Seguridad del 90% (Margen para metadatos, cabeceras y varianza)
            const double safetyFactor = 0.90;
            var targetMaxSizeBytes = job.MaxOutputSizeMb * 1024 * 1024 * safetyFactor;

            // 3. Bitrate teórico ideal en kbps
            var theoreticalBitrateKbps = (targetMaxSizeBytes * 8) / durationSeconds / 1000;

            // 4. Seleccionar el bitrate estándar MP3 más cercano pero SIEMPRE MENOR O IGUAL (Redondeo hacia abajo)
            var selectedBitrateKbps = ValidMp3BitratesKbps
                .Where(b => b <= theoreticalBitrateKbps)
                .DefaultIfEmpty(MinBitrateKbps)
                .Max();

            var bitrateKbps = Math.Clamp(selectedBitrateKbps, MinBitrateKbps, MaxBitrateKbps);

            Logger.LogInformation(
                "Job {JobId}: duración={Duration:F1}s, límite={MaxMb}MB, bitrate teórico={Theoretical:F1}kbps, bitrate final asignado={Used}kbps",
                job.JobId, durationSeconds, job.MaxOutputSizeMb, theoreticalBitrateKbps, bitrateKbps);

            // 5. Ejecutar FFmpeg (-ac 1, -ar 16000 para optimización Whisper)
            var arguments = $"-i \"{inputPath}\" -vn -ac 1 -ar 16000 -b:a {bitrateKbps}k \"{outputPath}\"";
            await _ffmpeg.RunAsync(arguments, ct);

            // 6. Validación estricta final
            var outputSizeBytes = new FileInfo(outputPath).Length;
            var outputSizeMb = outputSizeBytes / 1024.0 / 1024.0;

            if (outputSizeMb > job.MaxOutputSizeMb)
            {
                throw new InvalidOperationException(
                    $"El archivo comprimido ({outputSizeMb:F2}MB) superó el límite estricto de {job.MaxOutputSizeMb}MB. " +
                    $"Bitrate usado: {bitrateKbps}kbps. Duración: {durationSeconds:F1}s.");
            }

            Logger.LogInformation(
                "Job {JobId}: compresión exitosa. Tamaño final={SizeMb:F2}MB / {MaxMb}MB",
                job.JobId, outputSizeMb, job.MaxOutputSizeMb);

            // 7. Subir a Blob Storage
            return await _blobStorage.UploadAsync(outputPath, job.OutputBlobContainer, job.OutputFileName, ct);
        }
        finally
        {
            TryDelete(inputPath);
            TryDelete(outputPath);
        }
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { /* best effort */ }
        }
    }
}