using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

internal class GenerationRequestConsumer(
    IDocumentChunkRepository documentChunkRepository,
    ITextGenerationService textGenerationService
) : IConsumer<GenerationRequestCommand>
{

    public static string GenerateQuizPrompt(
        IEnumerable<DocumentChunk> documentChunks,
        int numberOfQuestions,
        int minNumberOfOptions,
        int maxNumberOfOptions
    )
    {
        // Combine the content of all document chunks into a single string
        string combinedContent = string.Join("\n", documentChunks.Select(chunk => chunk.Content));

        // Create a prompt for the text generation service
        string prompt = $"Generate a quiz with {numberOfQuestions} questions based on the following content:\n{combinedContent}";
        return prompt;
    }

    public async Task Consume(ConsumeContext<GenerationRequestCommand> context)
    {

        // Retrieve Document Chunks related to the ClassSourceId
        IEnumerable<DocumentChunk> documentChunks = await documentChunkRepository.GetChunksByClassSourceIdAsync(context.Message.ClassSourceId);

    }
}
