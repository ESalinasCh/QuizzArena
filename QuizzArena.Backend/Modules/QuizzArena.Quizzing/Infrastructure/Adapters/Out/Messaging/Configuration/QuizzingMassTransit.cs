using MassTransit;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Messaging.Consumers;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Messaging.Configuration;

public class QuizzingMassTransit
{
    public static void AddConsumers(IBusRegistrationConfigurator x)
    {
        x.AddConsumer<GenerateQuizConsumer>();
    }
}
