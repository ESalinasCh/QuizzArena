
using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlQuestionRepository(QuizzingDbContext context) : IQuestionRepository
{
    public async Task<List<Question>> GetByIdsAsync(List<Guid> questionIds)
    {
        return await context.Questions
        .Where(q => questionIds.Contains(q.Id))
        .ToListAsync();
    }

    public async Task<List<Question>> GetByIdsWithOptionsAsync(List<Guid> questionIds)
    {
        return await context.Questions
        .Include(q => q.Options)
        .Where(q => questionIds.Contains(q.Id))
        .ToListAsync();
    }
}
