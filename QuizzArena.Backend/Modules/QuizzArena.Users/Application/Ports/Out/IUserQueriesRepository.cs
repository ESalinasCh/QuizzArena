using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.Out;

public interface IUserQueriesRepository
{
    Task<List<User>> GetByIds(List<Guid> userIds);
    Task<User?> GetUserById(Guid userId);
}
