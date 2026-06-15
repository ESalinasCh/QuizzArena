using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<Question> questions);
}
