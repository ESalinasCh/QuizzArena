using MassTransit;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class FinishMatchTrackedUseCase(IMatchRepository matchRepository,
    IMatchAttemptRepository matchAttemptRepository,
    IQuestionRepository questionRepository,
    ICurrentUser currentUser) : IFinishMatchTrackedUseCase
{
    public async Task<FinishedMatchTrackedDto> Execute(Guid attemptId)
    {
        decimal maxScore = 100;
        #region Validations
        if (!Guid.TryParse(currentUser.UserId, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity.");
        }

        MatchAttempt? attempt = await matchRepository.GetMatchAttemptsDetailById(attemptId) ?? throw new InvalidOperationException();
        if (attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("User doesn't belong to this match attempt.");
        }

        if (attempt.Status == QuizAttemptStatus.Completed)
        {
            throw new UnauthorizedAccessException("User already completed this match attempt.");
        }

        Match match = await matchRepository.GetMatchByIdAsync(attempt.MatchId)
            ?? throw new InvalidOperationException($"Match not found for {attempt.MatchId}.");

        int totalAttempts = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAndUserIdAsync(attempt.MatchId, userId);
        if (totalAttempts > match.AttemptsAmount)
        {
            throw new InvalidOperationException("Maximum number of attempts reached for this match.");
        }
        #endregion

        List<Question> questions = await questionRepository.GetByIdsAsync(attempt.MatchAttemptQuestions.Select(x => x.QuestionId));
        var matchQuestionDictionary = attempt.MatchAttemptQuestions.ToDictionary(x => x.QuestionId);
        var answersDictionary = attempt.Answers.ToDictionary(x => x.QuestionId);

        decimal defaultValue = maxScore / matchQuestionDictionary.Count;
        var totalScore = attempt.Answers.Sum(x => x.IsCorrect ? matchQuestionDictionary[x.QuestionId].ValueScore ?? defaultValue : 0);

        attempt.Score = totalScore;
        attempt.Status = QuizAttemptStatus.Completed;
        attempt.EndDateTime = DateTimeOffset.UtcNow;

        await matchAttemptRepository.UpdateAsync(attempt);

        FinishedMatchTrackedDto finishedMatchTrackedDto = new FinishedMatchTrackedDto()
        {
            AttemptId = attemptId,
            AnsweredQuestions = attempt.Answers.Count,
            TotalQuestions = attempt.MatchAttemptQuestions.Count,
            Answers = questions.Select((x, index) =>
            {
                answersDictionary.TryGetValue(x.Id, out var answer);
                return new AnswerTrackedDto()
                {
                    Id = x.Id,
                    Number = index + 1,
                    Text = x.Content,
                    SelectedOptionId = answer?.OptionId ?? null
                };
            }
            ).ToList()

        };
        return finishedMatchTrackedDto;
    }
}
