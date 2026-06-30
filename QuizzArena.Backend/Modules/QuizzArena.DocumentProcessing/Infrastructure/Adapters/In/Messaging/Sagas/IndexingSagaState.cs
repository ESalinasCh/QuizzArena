using MassTransit;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

public class IndexingSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Initial";
    public string IndexingIdKey { get; set; } = string.Empty;

    public string ClassSourceId { get; set; } = string.Empty;
}
