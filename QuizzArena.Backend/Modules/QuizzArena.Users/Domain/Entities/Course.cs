using Users.Domain.Entities;

namespace QuizzArena.Users.Domain.Entities
{
    internal class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }

        // FK
        public Guid TeacherId { get; set; }
    }
}
