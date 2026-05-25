using Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.Out;

public interface IUserRepository
{
    Task<User> GetById(Guid id);
    Task Register(User quiz);
    Task Delete(Guid id);

}
