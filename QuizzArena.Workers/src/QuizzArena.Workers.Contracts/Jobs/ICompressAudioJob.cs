namespace QuizzArena.Workers.Contracts.Jobs;

/// <summary>
/// El primer tipo de job concreto. Los próximos (ej. ITranscodeVideoJob,
/// IGenerateThumbnailJob) van en este mismo folder "Jobs/".
/// </summary>
public interface ICompressAudioJob : IWorkerJob
{
    string SourceFileUrl { get; }

    /// <summary>
    /// Argumentos de ffmpeg con placeholders {input} y {output}.
    /// Ej: "-i {input} -b:a 64k -vn {output}"
    /// </summary>
    string FfmpegArguments { get; }

    string OutputBlobContainer { get; }
    string OutputFileName { get; }
}