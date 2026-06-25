namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record TrackAnswerRequestDto
{
    public Guid SelectedOptionId { get; set; }
}
