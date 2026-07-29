namespace QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

public interface IResetMatchAttemptUseCase
{
    Task Execute(Guid matchId, Guid userId);
}
