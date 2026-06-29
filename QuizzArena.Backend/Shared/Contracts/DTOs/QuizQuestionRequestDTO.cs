namespace Shared.Contracts.DTOs;

public record QuizQuestionRequestDTO
{
    public Guid QuestionId { get; set; }
    public int Position { get; set; }
    public int ValueScore { get; set; } = 1;
}
