using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuizQuestionQueriesRepository(QuizzingDbContext context) : IQuizQuestionQueriesRepository
{
    public async Task<Dictionary<Guid, int>> GetQuestionCountsByQuizIdsAsync(List<Guid> quizIds)
    {
        return await context.QuizQuestions
            .AsNoTracking()
            .Where(qq => quizIds.Contains(qq.QuizId))
            .GroupBy(qq => qq.QuizId)
            .Select(group => new
            {
                QuizId = group.Key,
                QuestionCount = group.Count()
            })
            .ToDictionaryAsync(x => x.QuizId, x => x.QuestionCount);
    }
}
