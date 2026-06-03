using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.Out;

public interface IUserRepository
{
    Task<User> GetById(Guid id);
    Task<bool> ExistsAsync(string providerId);
    Task Register(User user);
    Task Delete(Guid id);

}
