using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Filters;

namespace QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

public interface IGetMatchAttemptsByStudent
{
    Task<List<GetMatchAttemptDTO>> Execute(MatchAttemptFilters filters);
}
