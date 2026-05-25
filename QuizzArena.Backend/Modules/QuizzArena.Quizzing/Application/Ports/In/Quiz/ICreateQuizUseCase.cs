using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz;

public interface ICreateQuizUseCase
{
    Task<CreateQuizResponseDto> Execute(CreateQuizRequestDto dto);
}
