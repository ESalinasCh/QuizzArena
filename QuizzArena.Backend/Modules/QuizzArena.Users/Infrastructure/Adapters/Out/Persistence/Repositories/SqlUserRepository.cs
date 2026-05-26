using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlUserRepository : IUserRepository
{
    public async Task<User> GetById(Guid id)
    {
        //throw new NotImplementedException();
        return new User();
    }

    public async Task Register(User quiz)
    {
        //throw new NotImplementedException();
        return;
    }

    public async Task Delete(Guid id)
    {
        //throw new NotImplementedException();
        return;
    }
}
