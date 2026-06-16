
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IMatchRepository
{
    Task<MatchAttempt?> GetMatchAttemptsDetailById(Guid matchAttemptId);
    Task<Match?> GetMatchByIdAsync(Guid matchId);
    Task<List<Match>> GetMatchesAsync(List<Guid> courseIds, MatchQueryParametersDto? query = null);
}
