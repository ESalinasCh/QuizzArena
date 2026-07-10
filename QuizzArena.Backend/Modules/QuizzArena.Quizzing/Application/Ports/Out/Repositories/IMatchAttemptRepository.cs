using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IMatchAttemptRepository
{
    Task<MatchAttempt> AddMatchAttemptAsync(MatchAttempt matchAttempt);
    Task<int> GetMatchAttemptCountByMatchIdAndUserIdAsync(Guid matchId, Guid userId);
    Task<bool> HasActiveAttemptByMatchIdAsync(Guid matchId, Guid userId);
    Task<MatchAttempt?> GetByIdAsync(Guid matchAttemptId);
    Task<MatchAttempt> UpdateAsync(MatchAttempt matchAttempt);
    Task<List<MatchAttempt>> GetAttemptsByStudentId(Guid studentId, MatchAttemptFilters filters);
    Task<List<MatchAttempt>> GetAttemptsByMatchId(Guid matchId, MatchAttemptFilters filters);
    Task<List<MatchAttempt>> GetAttemptsByUserIds(Guid matchId, List<Guid> userIds);
    Task<int> GetMatchAttemptCountByMatchIdAndStatusAsync(Guid matchId, QuizAttemptStatus status);
}
