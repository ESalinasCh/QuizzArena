using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchResponseDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string? Title { get; set; } = "";
    public string CourseName { get; set; } = "";
    public Guid CourseId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? QuestionCount { get; set; }
    public string ProfessorName { get; set; } = "";
    public int Duration { get; set; }
    public MatchStatus Status { get; set; }
    public MatchMode Mode { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int AttemptsAmount { get; set; }
    public int? AttemptsUsed { get; set; }
    public bool? HasActiveAttempt { get; set; }
}
