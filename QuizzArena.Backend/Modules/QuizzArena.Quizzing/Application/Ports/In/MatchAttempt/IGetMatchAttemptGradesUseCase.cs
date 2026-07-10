using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Filters;

namespace QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

public interface IGetMatchAttemptGradesUseCase
{
    Task<List<MatchAttemptGradesResponseDto>> Execute(Guid matchId, MatchAttemptFilters filters);
}
