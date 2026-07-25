using FFmpeg.Worker.Services;
using QuizzArena.Workers.Contracts.Jobs;

namespace FFmpeg.Worker.Consumers;

public class CompressAudioJobConsumer : JobConsumerBase<ICompressAudioJob>
{
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
        var outputPath = Path.Combine(Path.GetTempPath(), $"{job.JobId}_{job.OutputFileName}");

        try
        {
            await _ffmpeg.RunAsync(inputPath, outputPath, job.FfmpegArguments, ct);
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