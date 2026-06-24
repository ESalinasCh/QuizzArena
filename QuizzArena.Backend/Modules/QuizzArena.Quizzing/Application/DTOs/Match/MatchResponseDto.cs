namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchResponseDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; } = "";
    public string CourseName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public string ProfessorName { get; set; } = "";
    public int Duration { get; set; }
}
