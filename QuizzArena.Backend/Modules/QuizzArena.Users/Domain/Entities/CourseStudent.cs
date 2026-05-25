namespace QuizzArena.Users.Domain.Entities;

internal class CourseStudent
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
}
