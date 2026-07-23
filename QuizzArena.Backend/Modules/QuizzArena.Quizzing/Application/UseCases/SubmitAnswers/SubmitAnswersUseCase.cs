using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Exceptions;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class SubmitAnswersUseCase(
    SubmitAnswersRequestValidator submitAnswersValidator,
    IMatchRepository matchRepository,
    IMatchAttemptRepository matchAttemptRepository,
    IMapper mapper,
    IQuestionRepository questionRepository,
    ICurrentUser currentUser
) : ISubmitAnswersUseCase
{
    public async Task<SubmitAnswersResponseDto> Execute(Guid matchAttemptId, SubmitAnswersRequestDto dto)
    {

        // Check current user refers to the corresponding student
        if (!Guid.TryParse(currentUser.UserId, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity.");
        }

        MatchAttempt matchAttempt = await matchAttemptRepository.GetByIdAsync(matchAttemptId)
            ?? throw new InvalidOperationException($"MatchAttempt {matchAttemptId} not found.");

        if (matchAttempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("User doesn't belong to this match attempt.");
        }

        if (matchAttempt.Status == Domain.Enums.QuizAttemptStatus.Completed)
        {
            throw new AttemptAlreadyCompletedException();
        }

        Guid matchId = matchAttempt.MatchId;
        Match match = await matchRepository.GetMatchByIdAsync(matchId)
            ?? throw new InvalidOperationException($"Match not found for {matchAttemptId}.");
        int totalAttempts = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, userId);
        if (totalAttempts > match.AttemptsAmount)
        {
            throw new MaxAttemptsReachedException();
        }

        // Validate incoming object
        await submitAnswersValidator.ValidateAndThrowAsync(dto);

        // Map every answer to Answer
        List<Answer> answers = mapper.Map<List<Answer>>(dto.Answers);

        // Get questions with their options (options tell us which ones are correct)
        List<Guid> questionsIds = answers.Select(a => a.QuestionId).Distinct().ToList();
        List<Question> questions = await questionRepository.GetByIdsWithOptionsAsync(questionsIds);
        Dictionary<Guid, Question> questionsById = questions.ToDictionary(question => question.Id);

        int correctCount = 0;
        int incorrectCount = 0;
        int totalQuestions = answers.Count;

        List<QuestionResultDto> questionResultDtos = [];

        // One Answer per question; the options the student picked live in its SelectedOptions
        foreach (Answer answer in answers)
        {
            Question question = questionsById.GetValueOrDefault(answer.QuestionId)
                ?? throw new InvalidOperationException($"Question {answer.QuestionId} not found.");

            HashSet<Guid> correctOptionIds = question.Options
                .Where(option => option.IsCorrect)
                .Select(option => option.Id)
                .ToHashSet();

            if (correctOptionIds.Count == 0)
            {
                throw new InvalidOperationException($"Question {question.Id} has no correct option.");
            }

            HashSet<Guid> questionOptionIds = question.Options.Select(option => option.Id).ToHashSet();
            HashSet<Guid> selectedOptionIds = answer.SelectedOptions.Select(selected => selected.OptionId).ToHashSet();

            foreach (Guid selectedOptionId in selectedOptionIds)
            {
                if (!questionOptionIds.Contains(selectedOptionId))
                {
                    throw new InvalidSelectedOptionException(selectedOptionId, question.Id);
                }
            }

            // MultipleChoice is all-or-nothing: SetEquals rejects both partial
            // selections and over-selection. Single-answer types allow exactly one pick.
            bool isCorrect = question.Type == Domain.Enums.QuestionType.MultipleChoice
                ? selectedOptionIds.SetEquals(correctOptionIds)
                : selectedOptionIds.Count == 1 && correctOptionIds.Contains(selectedOptionIds.First());

            if (isCorrect)
            {
                correctCount++;
            }
            else
            {
                incorrectCount++;
            }

            foreach (SelectedOption selected in answer.SelectedOptions)
            {
                selected.IsCorrect = correctOptionIds.Contains(selected.OptionId);
            }

            answer.MatchAttemptId = matchAttemptId;

            // Placeholder only: answer.OptionId is still a NOT NULL FK to option, so it must
            // hold a real option id. It carries no meaning — SelectedOptions is the answer.
            // Delete this line when the column is dropped (see the note in Answer.cs).
            answer.OptionId = selectedOptionIds.First();

            questionResultDtos.Add(new QuestionResultDto(
                question.Id,
                question.Content,
                [.. selectedOptionIds],
                [.. correctOptionIds],
                isCorrect
            ));
        }

        // Multiply before dividing to avoid integer-division truncating to 0
        int score = totalQuestions == 0 ? 0 : correctCount * 100 / totalQuestions;

        // Update MatchAttempt with Answers[] and score
        matchAttempt.Answers = answers;
        matchAttempt.Score = score;
        matchAttempt.Status = Domain.Enums.QuizAttemptStatus.Completed;

        await matchAttemptRepository.UpdateAsync(matchAttempt);

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
