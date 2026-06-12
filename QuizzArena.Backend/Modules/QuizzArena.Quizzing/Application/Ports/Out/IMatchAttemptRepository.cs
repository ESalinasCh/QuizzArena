using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IMatchAttemptRepository
{
    Task<MatchAttempt> AddMatchAttemptAsync(MatchAttempt matchAttempt);

    Task<int> GetMatchAttemptCountByMatchIdAsync(Guid matchId);

    Task<bool> HasActiveAttemptByMatchIdAsync(Guid matchId, Guid userId);
}
