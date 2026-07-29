namespace Shared.Contracts;

public interface ICurrentUser
{
    string UserId { get; }
    string UserName { get; }
    string Role { get; }
}
