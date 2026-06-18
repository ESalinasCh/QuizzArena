using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.Out;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string providerId);
    Task Register(User user);
}
