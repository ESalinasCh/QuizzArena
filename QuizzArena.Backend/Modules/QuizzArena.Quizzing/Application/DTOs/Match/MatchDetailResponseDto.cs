using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchDetailResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string? Title { get; set; }
    public MatchStatus Status { get; set; }
    public MatchMode Mode { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int Duration { get; set; }
    public int? QuestionCount { get; set; }
    public int AttemptsAmount { get; set; }
    public bool ShuffleQuestion { get; set; }
    public bool ShuffleOptions { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CourseId { get; set; }
    public Guid QuizId { get; set; }
}
