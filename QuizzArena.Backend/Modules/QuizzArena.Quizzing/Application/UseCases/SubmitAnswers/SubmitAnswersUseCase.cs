
using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class SubmitAnswersUseCase(
    SubmitAnswersRequestValidator submitAnswersValidator,
    SqlOptionRepository optionRepository,
    IMatchAttemptRepository matchAttemptRepository,
    IMapper mapper
) : ISubmitAnswersUseCase
{
    public async Task<SubmitAnswersResponseDto> Execute(Guid matchAttemptId, SubmitAnswersRequestDto dto)
    {
        // Validate incoming object
        await submitAnswersValidator.ValidateAndThrowAsync(dto);

        // Map every answer to Answer
        List<Answer> answers = mapper.Map<List<Answer>>(dto.Answers);

        // Get options to know correct answers
        List<Guid> optionsIds = dto.Answers.Select(answer => answer.SelectedOptionId).ToList();
        List<Option> options = await optionRepository.GetByIdsAsync(optionsIds);
        Dictionary<Guid, Option> optionsById = options.ToDictionary(option => option.Id);

        // Calculate score, correctCount, incorrectCount, totalQuestions
        int correctCount = 0;
        int incorrectCount = 0;
        int totalQuestions = answers.Count;

        foreach (Answer answer in answers)
        {
            answer.MatchAttemptId = matchAttemptId;
            answer.IsCorrect = optionsById.TryGetValue(answer.OptionId, out Option? option)
                && option.IsCorrect;
            if (answer.IsCorrect)
            {
                correctCount++;
            }
            else
            {
                incorrectCount++;
            }
        }

        int score = correctCount / totalQuestions * 100;

        // Update MatchAttempt with Answers[]
        MatchAttempt matchAttempt = await matchAttemptRepository.GetByIdAsync(matchAttemptId)
            ?? throw new InvalidOperationException($"MatchAttempt {matchAttemptId} not found.");

        matchAttempt.Answers = answers;

        matchAttempt.Score = score;

        await matchAttemptRepository.UpdateAsync(matchAttempt);

        // Build response object
        SubmitAnswersResponseDto response = new SubmitAnswersResponseDto
        {
            AttemptId = matchAttemptId,
            ScorePercentage = score,
            CorrectCount = correctCount,
            IncorrectCount = incorrectCount,
            TotalQuestions = totalQuestions,
Questions =
            {
                
            }
        };


        // Send response object

        // TODO
        throw new NotImplementedException();
    }
}
