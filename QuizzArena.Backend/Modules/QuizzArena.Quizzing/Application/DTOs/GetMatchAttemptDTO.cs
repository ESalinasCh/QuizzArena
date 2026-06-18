

using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs;

public record GetMatchAttemptDTO
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? CourseName { get; set; }
    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
    public int Score { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public int Duration { get; set; }
}
