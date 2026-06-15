using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Ports.In;

internal interface ICreateQuestionsUseCase
{
    Task Execute(IEnumerable<CreateQuestionDto> dtos, Guid classSourceId, Guid quizId);
}

