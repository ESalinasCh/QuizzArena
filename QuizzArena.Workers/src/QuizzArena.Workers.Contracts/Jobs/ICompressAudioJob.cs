namespace QuizzArena.Workers.Contracts.Jobs;

/// <summary>
/// El primer tipo de job concreto. Los próximos (ej. ITranscodeVideoJob,
/// IGenerateThumbnailJob) van en este mismo folder "Jobs/".
/// </summary>
public interface ICompressAudioJob : IWorkerJob
{
    /// <summary>
    /// URL pública o SAS del archivo de entrada (mp3, wav, mp4, etc.)
    /// </summary>
    string SourceFileUrl { get; }

    /// <summary>
    /// Tamaño máximo permitido del audio de salida en MB.
    /// El worker calculará el bitrate necesario para no superar este límite.
    /// Si tras comprimir sigue superándolo, el job falla con IJobFaulted.
    /// </summary>
    double MaxOutputSizeMb { get; }

    string OutputBlobContainer { get; }
    string OutputFileName { get; }
}
