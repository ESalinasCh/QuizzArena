using QuizzArena.Quizzing.Application.DTOs.Answer;

namespace QuizzArena.Quizzing.Application.Ports.In.Answer;

public interface ISetAnswerUseCase
{
    Task<SetAnswerResponseDto> Execute(SetAnswerRequestDto dto);
}
