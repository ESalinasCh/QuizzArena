using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlUserQueriesRepository(UserDbContext context) : IUserQueriesRepository
{
    public async Task<List<User>> GetByIds(List<Guid> userIds)
    {
        List<User> reponse = await context.Users.Where(x => userIds.Contains(x.Id)).ToListAsync();
        return reponse;
    }
}
