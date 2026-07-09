using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

public interface IGetMatchAttemptDetail
{
    Task<GetMatchAttemptDetailDTO> Execute(Guid matchAttemptId);
}
