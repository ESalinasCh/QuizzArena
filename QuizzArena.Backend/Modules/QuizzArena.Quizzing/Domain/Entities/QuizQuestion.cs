namespace QuizzArena.Quizzing.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public decimal ValueScore { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
}
