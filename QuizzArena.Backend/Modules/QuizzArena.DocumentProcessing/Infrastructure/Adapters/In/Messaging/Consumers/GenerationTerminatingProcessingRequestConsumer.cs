using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

public class GenerationTerminatingProcessingRequestConsumer(
    IProcessingJobRepository processingJobRepository,
    IClassSourceRepository classSourceRepository,
    IMatchContract matchContract
) : IConsumer<GenerationTerminatingProcessingRequestCommand>
{
    public async Task Consume(ConsumeContext<GenerationTerminatingProcessingRequestCommand> context)
    {
        if (context.Message.CreateMatch)
        {
            ClassSource? classSource = await classSourceRepository.GetByIdAsync(context.Message.ClassSourceId) ??
                throw new InvalidOperationException("Invalid ClassSourceId");

            await matchContract.CreateAutomaticMatch(new MatchCreationAutomaticRequestDTO()
            {
                Title = context.Message.Title,
                QuestionAmount = context.Message.QuestionAmount,
                QuizId = context.Message.QuizId,
                CourseId = classSource.CourseId
            });
        }

        ProcessingJob? job = await processingJobRepository.GetByIdAsync(context.Message.ProcessingJobId) ??
            throw new InvalidOperationException("Invalid ProcessingJobId");

        job.FinishedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        job.Status = JobStatus.Completed;

        await processingJobRepository.UpdateAsync(job);

    }
}
