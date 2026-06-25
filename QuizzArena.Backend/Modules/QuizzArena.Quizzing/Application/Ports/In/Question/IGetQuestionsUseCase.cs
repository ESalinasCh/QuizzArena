using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface IGetQuestionsUseCase
{
    Task<List<ResponseQuestionDto>> Execute(QuestionFilters filters);
}
