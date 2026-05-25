using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz;

public interface IDeleteQuizUseCase
{
    Task<DeleteQuizResponseDto> Execute(DeleteQuizRequestDto dto);
}
