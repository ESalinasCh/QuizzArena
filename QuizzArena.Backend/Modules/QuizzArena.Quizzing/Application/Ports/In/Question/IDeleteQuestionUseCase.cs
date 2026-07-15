using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface IDeleteQuestionUseCase
{
    Task<ResponseQuestionDto> Execute(Guid questionId);
}
