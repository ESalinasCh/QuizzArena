using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class TrackAnswerUseCase(IAnswerRepository answerRepository, IOptionRepository optionRepository,
    IMatchRepository matchRepository,
    ICurrentUser currentUser) : ITrackAnswerUseCase
{
    public async Task<MatchAttemptSmallProgressDto> Execute(Guid attemptId, Guid questionId, TrackAnswerRequestDto trackAnswerRequestDto)
    {

        MatchAttempt? attempt = await matchRepository.GetMatchAttemptsDetailById(attemptId) ?? throw new InvalidOperationException();
        if (!Guid.TryParse(currentUser.UserId, out Guid userId) ||
             attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        if (attempt.Status != QuizAttemptStatus.InProgress)
        {
            throw new InvalidOperationException();
        }

        Option? option = await optionRepository.GetByIdAsync(trackAnswerRequestDto.SelectedOptionId);
        if (option == null || option.QuestionId != questionId)
        {
            throw new InvalidOperationException();
        }

        Answer? answer = await answerRepository.GetByAttemptAndQuestionAsync(attemptId, questionId);

        bool isNew = answer == null;
        if (isNew)
        {
            answer = new Answer()
            {
                QuestionId = questionId,
                MatchAttemptId = attemptId
            };
        }
        answer!.AnsweredAt = DateTimeOffset.UtcNow;
        answer.OptionId = trackAnswerRequestDto.SelectedOptionId;
        answer.IsCorrect = option.IsCorrect;

        if (isNew)
        {
            await answerRepository.CreateAnswerAsync(answer);
        }
        else
        {
            await answerRepository.UpdateAnswerAsync(answer);
        }

        return new MatchAttemptSmallProgressDto() { AnsweredQuestions = attempt.Answers.Count, TotalQuestions = attempt.MatchAttemptQuestions.Count };
    }
}
