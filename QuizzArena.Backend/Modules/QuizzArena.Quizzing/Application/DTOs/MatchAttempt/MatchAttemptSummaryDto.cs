namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record MatchAttemptSummaryDto
{
    public int Count { get; set; }
    public bool HasActiveAttempt { get; set; }
}
