namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record StartAttemptRequestDto
{
    public Guid MatchId { get; set; }
}
