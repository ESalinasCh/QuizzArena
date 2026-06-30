using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

public class GenerationSaga : MassTransitStateMachine<GenerationSagaState>
{
    public State CreatingProcessingJob { get; private set; } = null!;
    public State GeneratingQuiz { get; private set; } = null!;
    public State TerminatingProcessing { get; private set; } = null!;
    public State QuizGenerationSuccess { get; private set; } = null!;
    public State QuizGenerationFailed { get; private set; } = null!;

    public Event<GenerationProcessingJobRequestEvent> QuizGenerationProcessingJobRequest { get; private set; } = null!;
    public Event<GenerationRequestEvent> QuizGenerationRequest { get; private set; } = null!;
    public Event<GenerationFinalizeProcessingRequestEvent> FinalizeProcessingRequest { get; private set; } = null!;
    public Event<GenerationCompletedEvent> QuizGenerationCompleted { get; private set; } = null!;
    public Event<GenerationFailedEvent> QuizGenerationError { get; private set; } = null!;


    public GenerationSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => QuizGenerationProcessingJobRequest, e =>
        {
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => QuizGenerationRequest, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => FinalizeProcessingRequest, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => QuizGenerationCompleted, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => QuizGenerationError, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));


        Initially(
            When(QuizGenerationProcessingJobRequest)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.ProcessingJobId = ctx.Message.ProcessingJobId.ToString();
                    ctx.Saga.DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId.ToString();
                    ctx.Saga.GenerationIdKey = ctx.Message.DocumentProcessingJobId.ToString();
                })
                .Publish(ctx => new GenerationProcessingJobRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = ctx.Message.ProcessingJobId,
                    DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId,
                    NumberOfQuestions = ctx.Message.NumberOfQuestions,
                    MinNumberOfOptions = ctx.Message.MinNumberOfOptions,
                    MaxNumberOfOptions = ctx.Message.MaxNumberOfOptions,
                    CreateMatch = ctx.Message.CreateMatch,
                    BloomTaxonomy = ctx.Message.BloomTaxonomy
                })
                .TransitionTo(CreatingProcessingJob)
        );



        During(CreatingProcessingJob,
            When(QuizGenerationRequest)
                .Then(ctx => Console.WriteLine($"[Saga] Quiz generation #{ctx.Saga.ClassSourceId} started → Waiting for quiz generation."))
                .Publish(ctx => new GenerationRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = ctx.Message.ProcessingJobId,
                    DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId,
                    NumberOfQuestions = ctx.Message.NumberOfQuestions,
                    MinNumberOfOptions = ctx.Message.MinNumberOfOptions,
                    MaxNumberOfOptions = ctx.Message.MaxNumberOfOptions,
                    CreateMatch = ctx.Message.CreateMatch,
                    BloomTaxonomy = ctx.Message.BloomTaxonomy
                })
                .TransitionTo(GeneratingQuiz)
        );

        During(GeneratingQuiz,
            When(FinalizeProcessingRequest)
                .Then(ctx => Console.WriteLine($"[Saga] Quiz generation #{ctx.Saga.ClassSourceId} is finishing."))
                .Publish(ctx => new GenerationTerminatingProcessingRequestCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = ctx.Message.ProcessingJobId,
                    DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId,
                    CreateMatch = ctx.Message.CreateMatch,
                    Title = ctx.Message.Title,
                    QuestionAmount = ctx.Message.QuestionAmount,
                    QuizId = ctx.Message.QuizId
                })
                .TransitionTo(TerminatingProcessing)
        );

        During(TerminatingProcessing,
            When(QuizGenerationCompleted)
                .Then(ctx => Console.WriteLine($"[Saga] Quiz generation #{ctx.Saga.ClassSourceId} completed."))
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
