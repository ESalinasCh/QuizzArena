using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Hosting;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

internal class GenerateQuizConsumer(
    ICreateQuizUseCase CreateQuizUseCase,
    IHostEnvironment environment
) : IConsumer<TranscriptionCompletedEvent>
{
    public async Task Consume(ConsumeContext<TranscriptionCompletedEvent> context)
    {
        try
        {
            if (environment.IsDevelopment())
            {
                string path = Path.Combine(AppContext.BaseDirectory, "MockData", "quiz.json");

                string json = await File.ReadAllTextAsync(path);

                CreateQuizDto? quiz = JsonSerializer.Deserialize<CreateQuizDto>(json);

                if (quiz is not null)
                {
                    await CreateQuizUseCase.Execute(quiz, context.Message.ClassSourceId);
                    Console.WriteLine("Mock quiz loaded successfully.");
                }
                else
                {
                    throw new InvalidOperationException("Mock quiz file is invalid.");
                }
            }
            else if (environment.IsProduction())
            {
                Console.WriteLine("AI quiz generation.");

                // TODO:
                // Here use the IA
            }

            await context.Publish(new QuizGenerationCompletedEvent
            {
                ClassSourceId = context.Message.ClassSourceId
            });
        }
        catch (Exception ex)
        {
            await context.Publish(new QuizGenerationFailedEvent
            {
                ClassSourceId = context.Message.ClassSourceId,

                Reason = ex.Message
            });

            throw;
        }
    }
}
