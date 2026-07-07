using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.ExternalServices;

public class QuestionContract(
    IQuestionRepository questionRepository
) : IQuestionContract
{
    public async Task<List<Guid>> CreateQuestions(List<QuestionCreationRequestDTO> questionCreationRequestDTOs)
    {
        List<Question> questions = questionCreationRequestDTOs.Select(q =>
        {
            Guid questionId = Guid.NewGuid();
            return new Question
            {
                Id = questionId,
                Content = q.Content,
                Justification = q.Justification,
                Status = QuestionStatus.Draft,
                Origin = QuestionOrigin.AiGenerated,
                Type = QuestionType.SingleChoice,
                Deleted = false,
                ProcessingJobId = q.ProcessingJobId,
                Options = q.Options.Select((option, index) => new Option
                {
                    Id = Guid.NewGuid(),
                    Description = option,
                    IsCorrect = index == q.CorrectAnswer,
                    Position = index,
                    Deleted = false,
                    QuestionId = questionId,
                }).ToList()
            };
        }).ToList();

        await questionRepository.CreateMultipleAsync(questions);

        return questions.Select(q => q.Id).ToList();
    }
}
