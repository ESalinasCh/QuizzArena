using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Ingestion;

public class IngestionSaga : MassTransitStateMachine<IngestionSagaState>
{

    public State TranscriptionInProgress { get; private set; } = null!;
    public State TranscriptionSuccess { get; private set; } = null!;
    public State TranscriptionFailed { get; private set; } = null!;


    public Event<TranscriptionRequestEvent> RequestEvent { get; private set; } = null!;
    public Event<TranscriptionSuccessEvent> SuccessEvent { get; private set; } = null!;
    public Event<TranscriptionFailedEvent> FailedEvent { get; private set; } = null!;


    public IngestionSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RequestEvent, e =>
        {
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => SuccessEvent, e =>
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Event(() => FailedEvent, e =>
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));


        Initially(
            When(RequestEvent)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.IngestionIdKey = ctx.Message.ClassSourceId.ToString();
                    Console.WriteLine($"[SAGA] Transcription request received for ClassSourceId: {ctx.Message.ClassSourceId}. Starting transcription process...");
                })
                .Publish(ctx => new TranscriptionRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    FileUrl = ctx.Message.FileUrl
                })
                .TransitionTo(TranscriptionInProgress)
        );

        During(TranscriptionInProgress,
            When(SuccessEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Transcription success received for ClassSourceId: {ctx.Saga.ClassSourceId}. Requesting indexation...")
                )
                .Publish(ctx => new IndexingRequestEvent
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    TranscriptUrl = ctx.Message.TranscriptUrl
                })
                .TransitionTo(TranscriptionSuccess)
                .Finalize(),

            When(FailedEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Transcription failed received for ClassSourceId: {ctx.Saga.ClassSourceId}. Updating status...")
                )
                .Publish(ctx => new TranscriptionFailedCommand
                {
                    ClassSourceId = Guid.Parse(ctx.Saga.ClassSourceId),
                    ErrorMessage = ctx.Message.ErrorMessage
                })
                .TransitionTo(TranscriptionFailed)
                .Finalize()
        );

        SetCompletedWhenFinalized();

    }


}
