
namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchUpdatedResponseDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int TimeMinutes { get; set; }
    public int? QuestionsAmount { get; set; }
    public int AttemptsAmount { get; set; }
    public bool ShuffleQuestion { get; set; }
    public bool ShuffleOptions { get; set; }
    public Guid CourseId { get; set; }
    public Guid QuizId { get; set; }
}
