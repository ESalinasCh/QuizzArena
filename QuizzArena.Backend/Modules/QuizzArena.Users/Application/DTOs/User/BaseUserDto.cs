using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Application.DTOs.User;

public class BaseUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ExternalProvider { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string ProviderId { get; set; } = string.Empty;
}
