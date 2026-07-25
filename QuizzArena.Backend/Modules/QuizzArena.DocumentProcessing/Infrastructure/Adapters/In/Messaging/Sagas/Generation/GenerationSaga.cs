using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Generation;

public class GenerationSaga : MassTransitStateMachine<GenerationSagaState>
{
    public State GenerationStarting { get; private set; } = null!;
    public State GenerationInProgress { get; private set; } = null!;
    public State GenerationEnding { get; private set; } = null!;
    public State GenerationSuccess { get; private set; } = null!;
    public State GenerationFailed { get; private set; } = null!;

    public Event<GenerationRequestEvent> RequestEvent { get; private set; } = null!;
    public Event<GenerationProcessEvent> ProcessEvent { get; private set; } = null!;
    public Event<GenerationEndingEvent> EndingEvent { get; private set; } = null!;
    public Event<GenerationSuccessEvent> SuccessEvent { get; private set; } = null!;
    public Event<GenerationFailedEvent> FailedEvent { get; private set; } = null!;


    public GenerationSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RequestEvent, e =>
        {
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString());
            e.SelectId(_ => NewId.NextGuid());
        });

        Event(() => ProcessEvent, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => EndingEvent, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => SuccessEvent, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));

        Event(() => FailedEvent, e =>
            e.CorrelateBy(state => state.GenerationIdKey, ctx => ctx.Message.DocumentProcessingJobId.ToString()));


        Initially(
            When(RequestEvent)
                .Then(ctx =>
                {
                    ctx.Saga.ClassSourceId = ctx.Message.ClassSourceId.ToString();
                    ctx.Saga.ProcessingJobId = ctx.Message.ProcessingJobId.ToString();
                    ctx.Saga.DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId.ToString();
                    ctx.Saga.GenerationIdKey = ctx.Message.DocumentProcessingJobId.ToString();
                    Console.WriteLine($"[SAGA] Generation request received for ClassSourceId: {ctx.Message.ClassSourceId}. Creating Processing Job...");
                })
                .Publish(ctx => new GenerationStartingCommand
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
                .TransitionTo(GenerationStarting)
        );



        During(GenerationStarting,
            When(ProcessEvent)
                .Then(ctx => Console.WriteLine($"[SAGA] Generation request received for ClassSourceId: {ctx.Saga.ClassSourceId}. Starting quiz generation process..."))
                .Publish(ctx => new GenerationProcessingCommand
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
                .TransitionTo(GenerationInProgress)
        );

        During(GenerationInProgress,
            When(EndingEvent)
                .Then(ctx => Console.WriteLine($"[SAGA] Generation request received for ClassSourceId: {ctx.Saga.ClassSourceId}. Generation is finishing."))
                .Publish(ctx => new GenerationEndingCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = ctx.Message.ProcessingJobId,
                    DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId,
                    CreateMatch = ctx.Message.CreateMatch,
                    Title = ctx.Message.Title,
                    QuestionAmount = ctx.Message.QuestionAmount,
                    QuizId = ctx.Message.QuizId
                })
                .TransitionTo(GenerationEnding)
        );

        During(GenerationEnding,
            When(SuccessEvent)
                .Then(ctx => Console.WriteLine($"[SAGA] Generation request received for ClassSourceId: {ctx.Saga.ClassSourceId}. Generation completed successfully."))
                .TransitionTo(GenerationSuccess)
                .Finalize(),

            When(FailedEvent)
                .Then(ctx => Console.WriteLine($"[SAGA] Generation request received for ClassSourceId: {ctx.Saga.ClassSourceId}. Generation failed."))
                .Publish( ctx => new GenerationFailedCommand
                {
                    ClassSourceId = ctx.Message.ClassSourceId,
                    ProcessingJobId = ctx.Message.ProcessingJobId,
                    DocumentProcessingJobId = ctx.Message.DocumentProcessingJobId,
                    ErrorMessage = ctx.Message.ErrorMessage
                })
                .TransitionTo(GenerationFailed)
                .Finalize()
        );

        SetCompletedWhenFinalized();

    }


}
