namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record TrackAnswerRequestDto
{
    public List<Guid> SelectedOptionIds { get; set; } = [];
}
