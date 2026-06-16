using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IMatchQueriesRepository
{
    Task<List<Match>> GetMatchesByIds(List<Guid> matchIds);

    Task<List<MatchAttempt>> GetAttemptsByStudentId(Guid studentId, MatchAttemptFilters filters);
}
