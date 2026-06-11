namespace QuizzArena.Quizzing.Application.DTOs.Option;

public class BaseOptionDto
{
    public string Description { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Position { get; set; }
    public Guid QuestionId { get; set; }
}
