namespace QuizzArena.Quizzing.Application.DTOs.Question;

public record StartAttemptOptionResponseDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
}
