using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface ICreateManualQuestionUseCase
{
    Task<ResponseQuestionDto> Execute(CreateManualQuestionDto dto);
}
