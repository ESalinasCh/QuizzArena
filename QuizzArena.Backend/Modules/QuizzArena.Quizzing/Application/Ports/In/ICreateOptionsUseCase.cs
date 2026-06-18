using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ICreateOptionsUseCase
{
    Task Execute(IEnumerable<CreateOptionDto> dtos);
}
