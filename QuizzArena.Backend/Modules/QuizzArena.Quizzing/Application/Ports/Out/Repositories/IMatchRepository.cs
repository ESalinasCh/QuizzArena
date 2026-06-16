
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

internal interface IMatchRepository
{
    Task<MatchAttempt?> GetMatchAttemptsDetailById(Guid matchAttemptId);
}
