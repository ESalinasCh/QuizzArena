using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Indexing;

/// <summary>
/// This saga is listening to the Ingestion one. When transcript is succesfully processed and saved, this Saga starts.
/// Goes from transcript to embedded chunks in the database.
/// </summary>
public class IndexingSaga : MassTransitStateMachine<IndexingSagaState>
{
    public State IndexingInProgress { get; private set; } = null!;
    public State IndexingSuccess { get; private set; } = null!;
    public State IndexingFailed { get; private set; } = null!;

    public Event<IndexingRequestEvent> RequestEvent { get; private set; } = null!;
    public Event<IndexingSuccessEvent> SuccessEvent { get; private set; } = null!;
    public Event<IndexingFailedEvent> FailedEvent { get; private set; } = null!;

    public IndexingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RequestEvent, e =>
        {
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => SuccessEvent, e =>
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Event(() => FailedEvent, e =>
            e.CorrelateBy(state => state.IndexingIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Initially(
            When(RequestEvent)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.IndexingIdKey = ctx.Message.ClassSourceId.ToString();
                    Console.WriteLine($"[SAGA] Indexing request received for ClassSourceId: {ctx.Message.ClassSourceId}. Starting indexing process...");
                })
                .Publish(ctx => new IndexingRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    TranscriptUrl = ctx.Message.TranscriptUrl,
                })
                .TransitionTo(IndexingInProgress)
        );

        During(IndexingInProgress,
            When(SuccessEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Indexing Success received for ClassSource: {ctx.Saga.ClassSourceId}. Stored {ctx.Message.StoredChunkCount} chunks. Requesting generation...")
                )
                .TransitionTo(IndexingSuccess)
                .Publish(ctx => new GenerationRequestEvent
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = Guid.NewGuid(),
                    DocumentProcessingJobId = Guid.NewGuid(),
                    NumberOfQuestions = 5,
                    MinNumberOfOptions = 2,
                    MaxNumberOfOptions = 3,
                    CreateMatch = true,
                    BloomTaxonomy = BloomTaxonomyLevel.Remember
                })
                .Finalize(),

            When(FailedEvent)
                .Then(ctx =>
                    Console.WriteLine($"[SAGA] Indexing Failed received for ClassSource: {ctx.Saga.ClassSourceId}. Error: {ctx.Message.ErrorMessage}")
                )
                .TransitionTo(IndexingFailed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
