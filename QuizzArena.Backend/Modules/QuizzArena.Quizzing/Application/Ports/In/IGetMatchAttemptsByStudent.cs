using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Filters;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetMatchAttemptsByStudent
{
    Task<List<GetMatchAttemptDTO>> Execute(Guid studentId, MatchAttemptFilters filters);
}
