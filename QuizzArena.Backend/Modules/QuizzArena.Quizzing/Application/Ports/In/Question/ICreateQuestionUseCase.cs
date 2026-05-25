using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface ICreateQuestionUseCase
{
    Task<CreateQuestionResponseDto> Execute(CreateQuestionRequestDto dto);
}
