
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class SubmitAnswersUseCase(
    SubmitAnswersRequestValidator submitAnswersValidator,
    SqlOptionRepository optionRepository
) : ISubmitAnswersUseCase
{
    public async Task<SubmitAnswersResponseDto> Execute(SubmitAnswersRequestDto dto)
    {
        // Validate incoming object
        await submitAnswersValidator.ValidateAndThrowAsync(dto);

        // Map every answer to Answer

        // Get options to know correct answers
        List<Guid> optionsIds = dto.Answers.Select(answer => answer.SelectedOptionId).ToList();
        List<Option> options = await optionRepository.GetByIdsAsync(optionsIds);

        // Calculate score, correctCount, incorrectCount, totalQuestions

        // Update MatchAttempt with Answers[]

        // Build (or map) response object

        // Send response object

        // TODO
        throw new NotImplementedException();
    }
}
