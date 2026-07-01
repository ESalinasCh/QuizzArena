using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

public class IngestionSaga : MassTransitStateMachine<IngestionSagaState>
{

    public State Transcribing { get; private set; } = null!;
    // public State GeneratingQuiz { get; private set; } = null!;
    public State TranscriptionSuccess { get; private set; } = null!;
    public State TranscriptionFailed { get; private set; } = null!;



    public Event<TranscriptionRequestEvent> TranscriptionRequest { get; private set; } = null!;
    public Event<TranscriptionCompletedEvent> TranscriptionCompleted { get; private set; } = null!;
    public Event<TranscriptionFailedEvent> TranscriptionError { get; private set; } = null!;

    // Quiz generation is handled elsewhere for now — likely needs to move to its own saga.
    // public Event<QuizGenerationCompletedEvent> QuizCompleted { get; private set; } = null!;
    // public Event<QuizGenerationFailedEvent> QuizFailed { get; private set; } = null!;


    public IngestionSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TranscriptionRequest, e =>
        {
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => TranscriptionCompleted, e =>
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        Event(() => TranscriptionError, e =>
            e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        // Event(() => QuizCompleted, e =>
        //     e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));

        // Event(() => QuizFailed, e =>
        //     e.CorrelateBy(state => state.IngestionIdKey, ctx => ctx.Message.ClassSourceId.ToString()));


        Initially(
            When(TranscriptionRequest)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.IngestionIdKey = ctx.Message.ClassSourceId.ToString();
                })
                .Publish(ctx => new TranscriptionRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    FileUrl = ctx.Message.FileUrl
                })
                .TransitionTo(Transcribing)
        );

        During(Transcribing,
            When(TranscriptionCompleted)
                .Then(ctx =>
                        Console.WriteLine(
                            $"[Saga] Transcription #{ctx.Saga.ClassSourceId} completed → IndexingSaga takes over."))
                .TransitionTo(TranscriptionSuccess)
                .Finalize(),

            When(TranscriptionError)
                .Then(ctx =>
                        Console.WriteLine(
                            $"[Saga] Transcription #{ctx.Saga.ClassSourceId} failed."))
                .TransitionTo(TranscriptionFailed)
                .Finalize()
        );

        // Quiz generation used to run here after transcription. Kept for reference until it
        // is moved to its own saga; the IndexingSaga now picks up TranscriptionCompleted.
        // During(GeneratingQuiz,
        //     When(QuizCompleted)
        //         .Then(ctx =>
        //             Console.WriteLine(
        //                 $"[Saga] Quiz generation for #{ctx.Saga.ClassSourceId} completed."))
        //         .TransitionTo(TranscriptionSuccess)
        //         .Finalize(),
        //
        //     When(QuizFailed)
        //         .Then(ctx =>
        //             Console.WriteLine(
        //                 $"[Saga] Quiz generation for #{ctx.Saga.ClassSourceId} failed."))
        //         .TransitionTo(TranscriptionFailed)
        //         .Finalize()
        // );

        SetCompletedWhenFinalized();

    }


}
