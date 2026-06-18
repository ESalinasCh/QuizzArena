
using QuizzArena.Quizzing.Application.DTOs;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetMatchAttemptDetail
{
    Task<GetMatchAttemptDetailDTO> Execute(Guid matchAttemptId);
}
