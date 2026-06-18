namespace QuizzArena.Quizzing.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public bool IsCorrect { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }
    public int TimeMs { get; set; }

    public Guid OptionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid MatchAttemptId { get; set; }
}
