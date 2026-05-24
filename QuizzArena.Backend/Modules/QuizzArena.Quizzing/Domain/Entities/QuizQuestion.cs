namespace QuizzArena.Quizzing.Domain.Entities;

internal class QuizQuestion
{
    public Guid Id { get; set; }
    public int Position { get; set; }
    public int ValueScore { get; set; }

    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
}
