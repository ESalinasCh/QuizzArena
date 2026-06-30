using MassTransit;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

public class GenerationSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Initial";
    public string GenerationIdKey { get; set; } = string.Empty;

    public string ClassSourceId { get; set; } = string.Empty;
    public string ProcessingJobId { get; set; } = string.Empty;
    public string DocumentProcessingJobId { get; set; } = string.Empty;
}
