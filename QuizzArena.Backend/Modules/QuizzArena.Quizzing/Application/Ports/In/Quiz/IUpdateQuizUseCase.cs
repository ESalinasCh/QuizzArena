using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz;

public interface IUpdateQuizUseCase
{
    Task<UpdateQuizResponseDto> Execute(UpdateQuizRequestDto dto);
}
