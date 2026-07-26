using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;
using QuizzArena.Workers.Contracts;
using QuizzArena.Workers.Contracts.Jobs;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Compression;

public class CompressionSaga : MassTransitStateMachine<CompressionSagaState>
{
    public State CompressionInProgress { get; private set; } = null!;
    public State CompressionPersistent { get; private set; } = null!;
    public State CompressionSuccess { get; private set; } = null!;
    public State CompressionFailed { get; private set; } = null!;


    public Event<CompressionRequestEvent> RequestEvent { get; private set; } = null!;
    public Event<IJobCompleted> JobSucceededEvent { get; private set; } = null!;
    public Event<IJobFaulted> JobFailedEvent { get; private set; } = null!;
    public Event<CompressionSuccessEvent> SuccessEvent { get; private set; } = null!;
    public Event<CompressionFailedEvent> FailedEvent { get; private set; } = null!;


    public CompressionSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RequestEvent, e =>
        {
            e.CorrelateBy(state => state.CompressionIdKey, ctx => ctx.Message.ClassSourceId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => JobSucceededEvent, e =>
            e.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => JobFailedEvent, e =>
            e.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => SuccessEvent, e =>
            e.CorrelateBy(state => state.CompressionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Event(() => FailedEvent, e =>
            e.CorrelateBy(state => state.CompressionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));


        Initially(
            When(RequestEvent)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.CompressionIdKey = ctx.Message.ClassSourceId.ToString();
                    Console.WriteLine($"[SAGA] Compression request received for ClassSourceId: {ctx.Message.ClassSourceId}. Starting compression process...");
                })
                .ThenAsync(async ctx => await ctx.Publish<ICompressAudioJob>(new
                {
                    JobId = Guid.NewGuid(),
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceFileUrl = ctx.Message.FileUrl,
                    MaxOutputSizeMb = 25.0,
                    OutputBlobContainer = "quiz-sources",
                    OutputFileName = $"class_{ctx.Saga.ClassSourceId}/CompressAudio.mp3"
                }))
                .TransitionTo(CompressionInProgress)
        );

        During(CompressionInProgress,
            When(JobSucceededEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Compression for ClassSourceId: {ctx.Saga.ClassSourceId}. Compression completed in URL: {ctx.Message.OutputBlobUrl}.")
                )
                .Publish(ctx => new CompressionStoringCommand
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    CompressedFileUrl = ctx.Message.OutputBlobUrl
                })
                .TransitionTo(CompressionPersistent),

            When(JobFailedEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Compression for ClassSourceId: {ctx.Saga.ClassSourceId}. Compression failed with error: {ctx.Message.Reason}.")
                )
                .Publish(ctx => new CompressionFailedCommand
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    ErrorMessage = ctx.Message.Reason
                })
                .TransitionTo(CompressionFailed)
                .Finalize()
        );

        During(CompressionPersistent,
            When(SuccessEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Compression for ClassSourceId: {ctx.Saga.ClassSourceId}. Compression stored successfully in URL: {ctx.Message.CompressedFileUrl}. Requesting transcription...")
                )
                .Publish(ctx => new TranscriptionRequestEvent
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    FileUrl = ctx.Message.CompressedFileUrl
                })
                .Finalize(),

            When(FailedEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Compression for ClassSourceId: {ctx.Saga.ClassSourceId}. Storing compression failed with error: {ctx.Message.ErrorMessage}.")
                )
                .Publish(ctx => new CompressionFailedCommand
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    ErrorMessage = ctx.Message.ErrorMessage
                })
                .TransitionTo(CompressionFailed)
                .Finalize()
        );

        SetCompletedWhenFinalized();

    }


}
