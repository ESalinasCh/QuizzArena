using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlQuizQuestionRepository(QuizzingDbContext context) : IQuizQuestionRepository
{
    public async Task CreateMultipleAsync(IEnumerable<QuizQuestion> questions)
    {
        context.QuizQuestions.AddRange(questions);
        await context.SaveChangesAsync();
    }
}
