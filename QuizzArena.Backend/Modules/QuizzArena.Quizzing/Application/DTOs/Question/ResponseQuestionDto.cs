namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class ResponseQuestionDto : BaseQuestionDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
