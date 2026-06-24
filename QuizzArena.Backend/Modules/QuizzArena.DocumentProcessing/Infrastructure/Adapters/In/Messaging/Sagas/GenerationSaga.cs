using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

public class GenerationSaga : MassTransitStateMachine<GenerationSagaState>
{

    public State GeneratingQuiz { get; private set; } = null!;
    public State QuizGenerationSuccess { get; private set; } = null!;
    public State QuizGenerationFailed { get; private set; } = null!;

    public Event<GenerationRequestEvent> QuizGenerationRequest { get; private set; } = null!;
    public Event<GenerationCompletedEvent> QuizGenerationCompleted { get; private set; } = null!;
    public Event<GenerationFailedEvent> QuizGenerationError { get; private set; } = null!;


    public GenerationSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => QuizGenerationRequest, e =>
        {
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => QuizGenerationCompleted, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => QuizGenerationError, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));


        Initially(
            When(QuizGenerationRequest)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.ProcessingJobId = ctx.Message.ProcessingJobId.ToString();
                    ctx.Saga.DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId.ToString();
                    ctx.Saga.GenerationIdKey = ctx.Message.DocumentProcessingJobId.ToString();
                })
                .Publish(ctx => new GenerationRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                })
                .TransitionTo(GeneratingQuiz)
        );

        During(GeneratingQuiz,
            When(QuizGenerationCompleted)
                .Then(ctx => Console.WriteLine($"[Saga] Quiz generation #{ctx.Saga.ClassSourceId} completed → Waiting for quiz generation."))
                .TransitionTo(QuizGenerationSuccess)
                .Finalize(),

            When(QuizGenerationError)
                .Then(ctx => Console.WriteLine($"[Saga] Quiz generation #{ctx.Saga.ClassSourceId} failed."))
                .TransitionTo(QuizGenerationFailed)
                .Finalize()
        );

        SetCompletedWhenFinalized();

    }


}
