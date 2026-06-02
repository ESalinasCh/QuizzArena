using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlUserRepository(UserDbContext context) : IUserRepository
{
    public async Task<User> GetById(Guid id)
    {
        //throw new NotImplementedException();
        return new User();
    }
    public async Task<bool> ExistsAsync(string providerId)
    {
        return await context.Users.AnyAsync(user => user.ProviderId == providerId);
    }

    public async Task Register(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        //throw new NotImplementedException();
        return;
    }
}
