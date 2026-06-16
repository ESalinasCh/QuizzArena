using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlQuestionRepository(QuizzingDbContext context) : IQuestionRepository
{
    public async Task CreateMultipleAsync(IEnumerable<Question> questions)
    {
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }
}
