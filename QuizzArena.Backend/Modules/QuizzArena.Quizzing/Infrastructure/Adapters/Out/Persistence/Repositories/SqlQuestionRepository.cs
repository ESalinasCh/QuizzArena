using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlQuestionRepository(QuizzingDbContext context) : IQuestionRepository
{
    public async Task CreateMultipleAsync(IEnumerable<Question> questions)
    {
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }

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
    public async Task<List<Question>> GetByProcessingJobIdAsync(QuestionFilters filters)
    {
        IQueryable<Question> query = context.Questions
            .AsNoTracking()
            .Where(q => q.ProcessingJobId.HasValue && filters.ProcessingJobIds.Contains(q.ProcessingJobId.Value));

        if (filters.Status.HasValue)
        {
            query = query.Where(x => x.Status == filters.Status);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();
    }
}
