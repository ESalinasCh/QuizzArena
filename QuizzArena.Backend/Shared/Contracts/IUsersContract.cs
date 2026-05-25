namespace Shared.Contracts;

public interface IUsersContract
{
    Task<bool> IsProfessor(Guid userId);
}
