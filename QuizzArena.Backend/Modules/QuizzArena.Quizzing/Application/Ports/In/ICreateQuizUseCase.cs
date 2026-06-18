using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ICreateQuizUseCase
{
    Task Execute(CreateQuizDto dto, Guid classSourceId);
}
