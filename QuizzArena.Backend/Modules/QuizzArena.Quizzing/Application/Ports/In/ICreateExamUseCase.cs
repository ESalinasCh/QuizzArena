using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ICreateExamUseCase
{
    Task<CreateQuizResponseDto> Execute(CreateExamDto dto);
}
