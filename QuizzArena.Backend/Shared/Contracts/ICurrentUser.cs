namespace Shared.Contracts;

public interface ICurrentUser
{
    string UserId { get; }
    string Role { get; }
}
