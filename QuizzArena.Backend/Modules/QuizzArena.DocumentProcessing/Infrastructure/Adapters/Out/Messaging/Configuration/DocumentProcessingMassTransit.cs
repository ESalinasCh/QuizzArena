using MassTransit;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Messaging.Configuration;

public class DocumentProcessingMassTransit
{
    public static void AddConsumers(IBusRegistrationConfigurator x)
    {
        x.AddSagaStateMachine<IngestionSaga, IngestionSagaState>().InMemoryRepository();
        x.AddConsumer<TranscriptionRequestConsumer>();

        x.AddSagaStateMachine<GenerationSaga, GenerationSagaState>().InMemoryRepository();
        x.AddConsumer<GenerationRequestConsumer>();
    }
}
