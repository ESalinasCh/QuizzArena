using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IMatchRepository
{
    Task<Match?> GetMatchByIdAsync(Guid matchId);
    Task<List<Match>> GetMatchesAsync(List<Guid> courseIds, MatchQueryParametersDto? query = null);
}
