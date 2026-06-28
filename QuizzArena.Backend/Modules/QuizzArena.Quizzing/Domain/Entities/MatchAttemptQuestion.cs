
namespace QuizzArena.Quizzing.Domain.Entities;

public class MatchAttemptQuestion
{
    public Guid Id { get; set; }
    public decimal? ValueScore { get; set; }
    public Guid MatchAttemptId { get; set; }
    public Guid QuestionId { get; set; }

    public MatchAttempt? MatchAttempt { get; set; }
    public Question? Question { get; set; }
}
