using QuizzArena.Quizzing.Application.DTOs;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetMatchesUseCase
{
    Task<List<MatchResponseDto>> Execute(MatchQueryParametersDto query);
}
