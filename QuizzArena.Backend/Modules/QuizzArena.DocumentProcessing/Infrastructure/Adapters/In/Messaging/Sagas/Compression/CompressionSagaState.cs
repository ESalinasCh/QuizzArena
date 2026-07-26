using MassTransit;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Compression;

public class CompressionSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "Initial";
    public string CompressionIdKey { get; set; } = string.Empty;

    public string ClassSourceId { get; set; } = string.Empty;
}
