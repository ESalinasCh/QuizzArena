using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

/// <summary>
/// This saga is listening to the Ingestion one. When transcript is succesfully processed and saved, this Saga starts.
/// Goes from transcript to embedded chunks in the database.
/// </summary>
public class IndexingSaga : MassTransitStateMachine<IndexingSagaState>
{
    public State Processing { get; private set; } = null!;
    public State Indexed { get; private set; } = null!;
    public State Faulted { get; private set; } = null!;

    public Event<TranscriptionCompletedEvent> TranscriptionCompleted { get; private set; } = null!;
    public Event<IndexingCompletedEvent> IndexingCompleted { get; private set; } = null!;
    public Event<IndexingFailedEvent> IndexingFailed { get; private set; } = null!;

    public IndexingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TranscriptionCompleted, e =>
        {
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => IndexingCompleted, e =>
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Event(() => IndexingFailed, e =>
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Initially(
            When(TranscriptionCompleted)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.IndexingIdKey = ctx.Message.ClassSourceId.ToString();
                })
                .Publish(ctx => new IndexTranscriptCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    TranscriptUrl = ctx.Message.TranscriptUrl,
                })
                .TransitionTo(Processing)
        );

        During(Processing,
            When(IndexingCompleted)
                .Then(ctx =>
                    Console.WriteLine(
                        $"[Saga] Indexing #{ctx.Saga.ClassSourceId} completed → stored {ctx.Message.StoredChunkCount} chunks."))
                .TransitionTo(Indexed)
                .Finalize(),

            When(IndexingFailed)
                .Then(ctx =>
                    Console.WriteLine(
                        $"[Saga] Indexing #{ctx.Saga.ClassSourceId} failed: {ctx.Message.Reason}"))
                .TransitionTo(Faulted)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
