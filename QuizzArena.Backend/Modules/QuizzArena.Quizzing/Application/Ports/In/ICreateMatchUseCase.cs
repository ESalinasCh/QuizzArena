using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ICreateMatchUseCase
{
    Task<MatchCreatedResponseDto> Execute(MatchCreateDto dto);
}
