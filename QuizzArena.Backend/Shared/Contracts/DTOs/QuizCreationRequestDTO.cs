namespace Shared.Contracts.DTOs;

public record QuizCreationRequestDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuizQuestionRequestDTO> Questions { get; set; } = new List<QuizQuestionRequestDTO>();
}
