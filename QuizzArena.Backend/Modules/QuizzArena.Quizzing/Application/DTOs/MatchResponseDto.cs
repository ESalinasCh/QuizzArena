namespace QuizzArena.Quizzing.Application.DTOs;

public record MatchResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string CourseName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }

}
