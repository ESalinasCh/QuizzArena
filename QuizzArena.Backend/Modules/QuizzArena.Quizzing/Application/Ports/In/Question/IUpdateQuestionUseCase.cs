using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface IUpdateQuestionUseCase
{
    Task<ResponseQuestionDto> Execute(UpdateQuestionDto dto);
}
