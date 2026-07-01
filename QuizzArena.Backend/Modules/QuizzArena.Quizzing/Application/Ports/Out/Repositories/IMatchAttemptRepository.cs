using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IMatchAttemptRepository
{
    Task<MatchAttempt> AddMatchAttemptAsync(MatchAttempt matchAttempt);
    Task<int> GetMatchAttemptCountByMatchIdAndUserIdAsync(Guid matchId, Guid userId);
    Task<bool> HasActiveAttemptByMatchIdAsync(Guid matchId, Guid userId);
    Task<MatchAttempt?> GetByIdAsync(Guid matchAttemptId);
    Task<MatchAttempt> UpdateAsync(MatchAttempt matchAttempt);

}
