
using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class SubmitAnswersUseCase(
    SubmitAnswersRequestValidator submitAnswersValidator,
    IOptionRepository optionRepository,
    IMatchAttemptRepository matchAttemptRepository,
    IMapper mapper,
    IQuestionRepository questionRepository
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

        // Multiply before dividing to avoid integer-division truncating to 0
        int score = totalQuestions == 0 ? 0 : correctCount * 100 / totalQuestions;

        // Update MatchAttempt with Answers[]
        MatchAttempt matchAttempt = await matchAttemptRepository.GetByIdAsync(matchAttemptId)
            ?? throw new InvalidOperationException($"MatchAttempt {matchAttemptId} not found.");

        matchAttempt.Answers = answers;

        matchAttempt.Score = score;

        await matchAttemptRepository.UpdateAsync(matchAttempt);

        // Get questions with their options (need options to know the correct one)
        List<Guid> questionsIds = answers.Select(a => a.QuestionId).Distinct().ToList();
        List<Question> questions = await questionRepository.GetByIdsWithOptionsAsync(questionsIds);
        Dictionary<Guid, Question> questionsById = questions.ToDictionary(question => question.Id);

        // One result per answer: question text + selected vs correct option
        List<QuestionResultDto> questionResultDtos = answers.Select(answer =>
        {
            Question question = questionsById.GetValueOrDefault(answer.QuestionId)
                ?? throw new InvalidOperationException($"Question {answer.QuestionId} not found.");

            Option correctOption = question.Options.FirstOrDefault(option => option.IsCorrect)
                ?? throw new InvalidOperationException($"Question {question.Id} has no correct option.");

            return new QuestionResultDto(
                question.Id,
                question.Content,
                answer.OptionId,
                correctOption.Id,
                answer.IsCorrect
            );
        }).ToList();

        // Build response object
        SubmitAnswersResponseDto response = new SubmitAnswersResponseDto
        {
            AttemptId = matchAttemptId,
            ScorePercentage = score,
            CorrectCount = correctCount,
            IncorrectCount = incorrectCount,
            TotalQuestions = totalQuestions,
            Questions = questionResultDtos
        };

        // Send response object
        return response;
    }
}
