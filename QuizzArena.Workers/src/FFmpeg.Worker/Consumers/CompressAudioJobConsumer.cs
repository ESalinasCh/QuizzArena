using FFmpeg.Worker.Services;
using QuizzArena.Workers.Contracts.Jobs;

namespace FFmpeg.Worker.Consumers;

public class CompressAudioJobConsumer : JobConsumerBase<ICompressAudioJob>
{
    // Límites de bitrate para mantener calidad mínima usable
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
            // 1. Obtener duración para calcular el bitrate necesario
            var durationSeconds = await _ffmpeg.GetDurationSecondsAsync(inputPath, ct);

            // 2. Calcular bitrate: (maxMB × 8192) / durationSeconds
            //    8192 = 8 bits/byte × 1024 bytes/KB × 1024 KB/MB / 1000 ms... simplificado:
            //    maxBytes = maxMB * 1024 * 1024, maxBits = maxBytes * 8
            //    bitrate (kbps) = maxBits / durationSeconds / 1000
            var maxSizeBytes = job.MaxOutputSizeMb * 1024 * 1024;
            var calculatedBitrate = (int)((maxSizeBytes * 8) / durationSeconds / 1000);
            var bitrateKbps = Math.Clamp(calculatedBitrate, MinBitrateKbps, MaxBitrateKbps);

            Logger.LogInformation(
                "Job {JobId}: duración={Duration:F1}s, límite={MaxMb}MB, bitrate calculado={Calculated}kbps, bitrate usado={Used}kbps",
                job.JobId, durationSeconds, job.MaxOutputSizeMb, calculatedBitrate, bitrateKbps);

            // 3. Construir y ejecutar el comando ffmpeg
            //    -vn: descarta video (si es mp4, extrae solo el audio)
            //    -ac 1: convierte a mono
            //    -ar 16000: 16kHz (suficiente para voz, compatible con Whisper)
            //    -b:a {bitrate}k: bitrate objetivo
            var arguments = $"-i \"{inputPath}\" -vn -ac 1 -ar 16000 -b:a {bitrateKbps}k \"{outputPath}\"";
            await _ffmpeg.RunAsync(arguments, ct);

            // 4. Verificar que el archivo resultante no supera el límite
            var outputSizeBytes = new FileInfo(outputPath).Length;
            var outputSizeMb = outputSizeBytes / 1024.0 / 1024.0;

            if (outputSizeMb > job.MaxOutputSizeMb)
            {
                throw new InvalidOperationException(
                    $"El archivo comprimido ({outputSizeMb:F2}MB) superó el límite de {job.MaxOutputSizeMb}MB. " +
                    $"Bitrate usado: {bitrateKbps}kbps. Duración: {durationSeconds:F1}s. " +
                    $"El cálculo de compresión no fue suficiente para este archivo.");
            }

            Logger.LogInformation(
                "Job {JobId}: compresión exitosa. Tamaño final={SizeMb:F2}MB",
                job.JobId, outputSizeMb);

            // 5. Subir a blob y retornar la URL
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
