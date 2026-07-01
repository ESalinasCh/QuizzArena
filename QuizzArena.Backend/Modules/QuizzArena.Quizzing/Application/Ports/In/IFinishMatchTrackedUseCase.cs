using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IFinishMatchTrackedUseCase
{
    public Task<FinishedMatchTrackedDto> Execute(Guid attemptId);
}
