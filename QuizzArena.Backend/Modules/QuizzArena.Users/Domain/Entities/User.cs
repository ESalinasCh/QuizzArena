using QuizzArena.Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ExternalProvider { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public UserRole Role { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
