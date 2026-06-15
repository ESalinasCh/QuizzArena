using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IQuizQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<QuizQuestion> questions);
}
