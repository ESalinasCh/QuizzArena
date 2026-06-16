using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlOptionRepository(QuizzingDbContext context) : IOptionRepository
{
    public async Task CreateMultipleAsync(IEnumerable<Option> options)
    {
        context.Options.AddRange(options);
        await context.SaveChangesAsync();
    }
}
