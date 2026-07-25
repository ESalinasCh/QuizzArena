using MassTransit;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Generation;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Indexing;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Sagas.Ingestion;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Messaging.Configuration;

public class DocumentProcessingMassTransit
{
    public static void AddConsumers(IBusRegistrationConfigurator x)
    {
        x.AddSagaStateMachine<IngestionSaga, IngestionSagaState>().InMemoryRepository();
        x.AddConsumer<TranscriptionRequestConsumer>().Endpoint(e => e.PrefetchCount = 1);
        x.AddConsumer<TranscriptionFailedConsumer>();

        x.AddSagaStateMachine<IndexingSaga, IndexingSagaState>().InMemoryRepository();
        x.AddConsumer<IndexingTranscriptConsumer>().Endpoint(e => e.PrefetchCount = 1);

        x.AddSagaStateMachine<GenerationSaga, GenerationSagaState>().InMemoryRepository();
        x.AddConsumer<GenerationRequestConsumer>().Endpoint(e => e.PrefetchCount = 1);
        x.AddConsumer<GenerationProcessingJobRequestConsumer>();
        x.AddConsumer<GenerationTerminatingProcessingRequestConsumer>();
    }
}
