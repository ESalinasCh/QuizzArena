using QuizzArena.Quizzing.Application.Ports.Out;
﻿using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlOptionRepository(QuizzingDbContext context) : IOptionRepository
{
    public async Task<List<Option>> GetByIdsAsync(List<Guid> optionIds)
    {
        return await context.Options
            .Where(o => optionIds.Contains(o.Id))
            .ToListAsync();
    }

    public async Task CreateMultipleAsync(IEnumerable<Option> options)
    {
        context.Options.AddRange(options);
        await context.SaveChangesAsync();
    }
}
