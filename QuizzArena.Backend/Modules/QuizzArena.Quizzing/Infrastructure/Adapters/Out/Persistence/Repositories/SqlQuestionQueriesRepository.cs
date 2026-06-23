using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuestionQueriesRepository(QuizzingDbContext context) : IQuestionQueriesRepository
{
    public async Task<List<Question>> GetQuestionsByIds(List<Guid> ids)
    {
        return await context.Questions.Include(x => x.Options).Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
