using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IMatchRepository
{
    Task<List<Match>> GetMatchesAsync(List<Guid> courseIds, MatchQueryParametersDto query);
}
