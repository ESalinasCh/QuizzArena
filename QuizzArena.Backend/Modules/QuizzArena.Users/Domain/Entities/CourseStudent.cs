namespace QuizzArena.Users.Domain.Entities;

public class CourseStudent
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }

    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
