using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<Question> questions);
}
