using MassTransit;
using QuizzArena.Workers.Contracts;

namespace FFmpeg.Worker.Consumers;

/// <summary>
/// Lógica común a CUALQUIER tipo de job: ejecuta, publica IJobCompleted o
/// IJobFaulted. Los consumers concretos (CompressAudioJobConsumer, y los que
/// vengan después) solo implementan ExecuteAsync.
/// </summary>
public abstract class JobConsumerBase<TJob> : IConsumer<TJob> where TJob : class, IWorkerJob
{
    protected readonly ILogger Logger;

    protected JobConsumerBase(ILogger logger) => Logger = logger;

    public async Task Consume(ConsumeContext<TJob> context)
    {
        var job = context.Message;
        var jobType = typeof(TJob).Name;

        // 1. Log inmediato al recibir el mensaje
        Logger.LogInformation(">>> Recibido trabajo {JobId} de tipo {JobType}", job.JobId, jobType);

        try
        {
            var outputBlobUrl = await ExecuteAsync(job, context.CancellationToken);

            await context.Publish<IJobCompleted>(new
            {
                job.JobId,
                job.CorrelationId,
                JobType = jobType,
                OutputBlobUrl = outputBlobUrl,
                CompletedAtUtc = DateTime.UtcNow
            });

            Logger.LogInformation("Job {JobId} ({JobType}) completado -> {BlobUrl}", job.JobId, jobType, outputBlobUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Job {JobId} ({JobType}) falló con error: {Message}", job.JobId, jobType, ex.Message);

            await context.Publish<IJobFaulted>(new
            {
                job.JobId,
                job.CorrelationId,
                JobType = jobType,
                Reason = ex.Message,
                FailedAtUtc = DateTime.UtcNow
            });

            // Re-lanzar para que MassTransit active reintentos o mueva el mensaje a _error
            throw;
        }
    }

    protected abstract Task<string> ExecuteAsync(TJob job, CancellationToken ct);
}